using MoonSharp.Interpreter;

namespace Qlay.Modules.LuaSocket
{
    class LuaWrapper
    {
        private Qlay plugin;

        public LuaWrapper(Qlay plugin)
        {
            this.plugin = plugin as Qlay;
        }

        public DynValue Packet(uint id)
        {
            qLuaPacket packet = new qLuaPacket(id);

            return UserData.Create(packet);
        }

        public Table Client()
        {
            qConnection client = new qConnection();

            //client.host = host;
            //client.port = port;

            client.plugin = plugin;
            client.luaTable = new Table(plugin.lua);
            client.luaTable["_object"] = client;
            client.luaTable.MetaTable = plugin.wrappedMeta;
            //Thread thread = new Thread(client.connect);
            //thread.Start();
            return client.luaTable;
        }

        /*public Table Server()
        {
            qServer server = new qServer();

            //client.host = host;
            //client.port = port;

            server.plugin = plugin;
            server.luaTable = new Table(plugin.lua);
            server.luaTable["_object"] = server;
            server.luaTable.MetaTable = plugin.wrappedMeta;
            //Thread thread = new Thread(client.connect);
            //thread.Start();
            return server.luaTable;
        }*/
    }
}
