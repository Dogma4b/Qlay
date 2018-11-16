using MoonSharp.Interpreter;
using MySql.Data.MySqlClient;
using System;

namespace Qlay.Modules
{
    [MoonSharpUserData]
    public class MySQL
    {
        private Qlay plugin;
        public MySQL(Qlay plugin)
        {
            this.plugin = plugin as Qlay;
        }
        public Table connect(string host, string username, string password, string database, int port)
        {
            MySQLConnection c = new MySQLConnection();
            c.plugin = plugin;

            c.hostname = host;
            c.user = username;
            c.password = password;
            c.database = database;
            c.port = port; 

            c.db = new Table(plugin.lua);
            c.db["_object"] = c;
            c.db.MetaTable = plugin.wrappedMeta;
            return c.db;
        }
        public string EscapeString(string var)
        {
            return MySqlHelper.EscapeString(var);
        }
    }

    [MoonSharpUserData]
    public class MySQLConnection
    {
        internal Qlay plugin;

        internal Table db;

        internal string hostname { get; set; }
        internal int port { get; set; } = 3306;
        internal string user { get; set; }
        internal string password { get; set; }
        internal string database { get; set; }

        internal MySqlConnection DBConnection; 

        public void Connect()
        {
            if (DBConnection != null) return;

            DBConnection = new MySqlConnection("server=" + this.hostname + ";user=" + this.user + ";password=" + this.password + ";database=" + this.database + ";port=" + this.port.ToString());
           
            try
            {
                DBConnection.StateChange += Connection_StateChange;
                DBConnection.Open();
            }
            catch (Exception ex)
            {
                DynValue onConnectionFailed = db.Get("onConnectionFailed");
                if (onConnectionFailed != null)
                    onConnectionFailed.Function.Call(db, ex.Message);
            }
        }

        public void Disconnect()
        {
            DBConnection.Dispose();
            DBConnection = null;
        }

        public Table Query(string sql)
        {
            if (DBConnection == null) return null;
            var q = new MySQLQuery(this, sql);
            q.table = new Table(plugin.lua);
            q.table["_object"] = q;
            q.table.MetaTable = plugin.wrappedMeta;
            return q.table; 
        }

        public DynValue GetValue(DynValue k)
        {
            return (DynValue)db[k];
        }
        public void SetValue(DynValue k, DynValue v)
        {
            db[k] = v;
        }

        [MoonSharpHidden]
        private void Connection_StateChange(object sender, System.Data.StateChangeEventArgs ev)
        {
            if (ev.CurrentState == System.Data.ConnectionState.Open)
            {
                DynValue onConnect = db.Get("onConnect");
                if (onConnect != null)
                    onConnect.Function.Call(db);
            }
        }


    }
    [MoonSharpUserData]
    public class MySQLQuery
    {
        internal Table table;
        private MySqlCommand DBCommand;
        private MySQLConnection Connection;
        public MySQLQuery(MySQLConnection parent, string sql)
        {
            Connection = parent;
            DBCommand = new MySqlCommand(sql, Connection.DBConnection);
        }
             
        public void Start()
        {
            try
            {
                Table data = new Table(Connection.plugin.lua);
                MySqlDataReader reader = DBCommand.ExecuteReader();
                while (reader.Read())
                {
                    Table row = new Table(Connection.plugin.lua);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        switch (reader.GetFieldType(i).Name)
                        {
                            case "Int32":
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : DynValue.NewNumber(reader.GetUInt32(i));
                                break;
                            case "Int64":
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : DynValue.NewNumber(reader.GetUInt64(i));
                                break;
                            case "String":
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : DynValue.NewString(reader.GetString(i));
                                break;
                            case "Boolean":
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : DynValue.NewBoolean(reader.GetBoolean(i));
                                break;
                            default:
                                row.Append(DynValue.NewString(reader.GetFieldType(i).Name));
                                break;
                        }
                    }
                    data.Append(DynValue.NewTable(row));
                }
                reader.Close();
                DynValue onSuccess = table.Get("onSuccess");
                if (onSuccess != null)
                    onSuccess.Function.Call(table, data);
                DBCommand.Dispose();
            }
            catch (Exception ex)
            {
                DynValue onError = table.Get("onError");
                if (onError != null)
                    onError.Function.Call(table, ex.Message);
            }
        } 

    }
}
