using System;
using System.IO;
using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;
using MoonSharp.Interpreter;
using Smod2.API;
using System.Collections.Generic;
#if DEBUG
using System.Linq;
#endif

namespace Qlay
{
    [PluginDetails(
        author = "Andrey",
        name = "Qlay",
        description = "Lua included",
        id = "qlay",
        version = "0.1",
        SmodMajor = 3,
        SmodMinor = 0,
        SmodRevision = 0
        )]

    public class Qlay : Plugin
    {

        private class LuaPlugin
        {
            //public Script script { get; set; }
            public string id { get; set; }
            public string absolutePath { get; set; }
            public string folder { get; set; }
            public bool reloadCooldown { get; set; }  
            public Table context { get; set; }

            public LuaPlugin(  string path)
            {
                //this.script = script;
                this.folder = path.Substring(path.LastIndexOf("lua") + 4);
                this.absolutePath = path;
            }
        }

        private List<LuaPlugin> PluginList = new List<LuaPlugin>();
        internal ServerMod2.SmodLogger log = new ServerMod2.SmodLogger();
#if DEBUG
        public Script lua;
#else
        internal Script lua;
#endif
        internal DynValue luaHookCall;
        internal FileSystemWatcher watcher;

        internal Table wrappedMeta;

        public Qlay()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        } 
        private void OnFileChanged(object source, FileSystemEventArgs ev)
        {
            //Info("file changed: " + ev.FullPath);
            string plFolder = ev.FullPath.Substring(ev.FullPath.IndexOf("lua") + 4);
            LuaPlugin plugin = PluginList.Find(pl => plFolder.Contains(pl.folder));
            //Info("plugin changed: " + plugin.folder);
            if (!plugin.reloadCooldown)
            {
                log.Info("qlay", "Reloading lua plugin " + plugin.id);
                plugin.reloadCooldown = true;
                Modules.Timer.singleton.Create("pluginreload_" + plugin.id, 1000, 1, () =>
                {
                    ReloadPlugin(plugin);
                });
            }
        }

        private void ReloadPlugin(LuaPlugin plugin)
        {
            try
            {
                plugin.reloadCooldown = false;
                LoadPluginFile(plugin.absolutePath, plugin.context);
                if (plugin.context["Init"] != null)
                    lua.Call(plugin.context["Init"]);
                log.Info("qlay", "Reloaded");
            }
            catch (InterpreterException ex)
            {
                log.Error("qlay", "Lua error " + ex.DecoratedMessage);
            }
            catch (Exception e)
            {
                log.Error("qlay", "Error " + e.Message);
                log.Error("qlay", e.StackTrace);
            }
        }
        private void LoadPluginFile(string dir, Table localEnv, string file=null)
        {
            if (file == null)
            {
                if (File.Exists(dir + "/init.lc"))
                {
                    lua.DoStream(File.OpenRead(dir + "/init.lc"), localEnv, dir + "/init.lc");
                }
                else
                {
                    lua.DoStream(File.OpenRead(dir + "/init.lua"), localEnv, dir + "/init.lua");
                }
            }
            else
            {
                lua.DoStream(File.OpenRead(dir + "/" + file), localEnv, dir + "/" + file);
            }
        }


        public override void OnDisable()
        {

        }

        public override void OnEnable()
        {
            //Register user data type;
            UserData.RegisterType<UnityEngine.GameObject>();
            UserData.RegisterType<Smod2.API.UserGroup>();
            UserData.RegisterType<Smod2.API.Item>();
            UserData.RegisterType<TeamRole>();
            UserData.RegisterType<Connection>();
            UserData.RegisterType<Server>();
            UserData.RegisterType<Map>();
            UserData.RegisterType<Door>();
            UserData.RegisterType<Elevator>();
            UserData.RegisterType<Round>();
            UserData.RegisterType<RoundStats>();
            UserData.RegisterType<Vector>();
            UserData.RegisterType<PocketDimensionExit>();
            UserData.RegisterType<TeslaGate>();
            UserData.RegisterType<Scp914.Recipe>();
            UserData.RegisterType<Modules.Timer>();
            UserData.RegisterType<Modules.Timer.TimerInstance>();
            UserData.RegisterType<Modules.File>();
            UserData.RegisterType<Functions.Player>();
            UserData.RegisterType<Functions.Warhead>();
            UserData.RegisterType<Functions.SCP914>();
            UserData.RegisterType<Modules.MySQL>();
            UserData.RegisterType<Modules.MySQLConnection>();
            UserData.RegisterType<Modules.MySQLQuery>();
            UserData.RegisterType<Modules.LuaSocket.LuaWrapper>();
            UserData.RegisterType<Modules.LuaSocket.qLuaPacket>();
            UserData.RegisterType<Modules.LuaSocket.qConnection>();
            UserData.RegisterType<Role>();
            UserData.RegisterType<Team>();
            UserData.RegisterType<UserRank>();
            UserData.RegisterType<ItemType>();
            UserData.RegisterType<AmmoType>();
            UserData.RegisterType<DamageType>();
            UserData.RegisterType<ElevatorStatus>();
            UserData.RegisterType<ElevatorType>();
            UserData.RegisterType<IntercomStatus>();
            UserData.RegisterType<KnobSetting>();
            UserData.RegisterType<PocketDimensionExitType>();
            UserData.RegisterType<RadioStatus>();
            UserData.RegisterType<ROUND_END_STATUS>();
            //Register classes;
            Hooks hooks = new Hooks(this);
            Functions.SCP914 scp914 = new Functions.SCP914(this);
            //Register events;

#if !DEBUG
            this.AddEventHandler(typeof(IEventHandlerRoundStart), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerCheckRoundEnd), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerRoundEnd), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerConnect), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerDisconnect), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerWaitingForPlayers), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerRoundRestart), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerUpdate), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerFixedUpdate), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerCheckEscape), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPlayerJoin), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerSpawn), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerTeamRespawn), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerSpawnRagdoll), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerCheckEscape), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerSetRole), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPlayerHurt), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPlayerDie), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPlayerPickupItem), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPlayerPickupItemLate), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPlayerDropItem), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerMedkitUse), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerLure), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerRadioSwitch), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerShoot), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerThrowGrenade), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandler106CreatePortal), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandler106Teleport), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerContain106), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPocketDimensionEnter), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPocketDimensionExit), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerPocketDimensionDie), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerIntercom), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerWarheadStartCountdown), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerWarheadStopCountdown), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerWarheadDetonate), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerLCZDecontaminate), hooks, Priority.Highest);
            this.AddEventHandler(typeof(IEventHandlerSCP914Activate), scp914, Priority.Highest);
#endif
            try
            {
                string luaPath = Directory.GetCurrentDirectory() + "/lua";
#if DEBUG 
                lua = new Script(CoreModules.Preset_Complete);
#else
                lua = new Script();
#endif

                UserData.RegisterType<Player>(new DynamicIndex(lua, typeof(Player), typeof( Functions.PlayerExtension)));


                wrappedMeta = lua.DoString(@"
				return { __index = function(t, name) 
                local obj = rawget(t, '_object'); if (obj) then return obj[name]; end 
                local meta = rawget(t, '_base');  if (meta) then return meta[name]; end
                end } ").Table;

                //Register lua globals;
                //ne //lua.Globals["file"] = new Modules.File(dir + "/");
                lua.Globals["timer"] =  Modules.Timer.singleton;
               // lua.Globals["hook"] = hooks;
                lua.Globals["mysql"] = new Modules.MySQL(this);
                lua.Globals["socket"] = new Modules.LuaSocket.LuaWrapper(this);
                Dictionary<string, object> StaticResource = new Dictionary<string, object>();
                StaticResource.Add("Role", UserData.CreateStatic<Role>());
                StaticResource.Add("Item", UserData.CreateStatic<ItemType>());
                StaticResource.Add("Ammo", UserData.CreateStatic<AmmoType>());
                StaticResource.Add("DamageType", UserData.CreateStatic<DamageType>());
                StaticResource.Add("ElevatorStatus", UserData.CreateStatic<ElevatorStatus>());
                StaticResource.Add("ElevatorType", UserData.CreateStatic<ElevatorType>());
                StaticResource.Add("IntercomStatus", UserData.CreateStatic<IntercomStatus>());
                StaticResource.Add("KnobSetting", UserData.CreateStatic<KnobSetting>());
                StaticResource.Add("PocketDimensionExitType", UserData.CreateStatic<PocketDimensionExitType>());
                StaticResource.Add("RadioStatus", UserData.CreateStatic<RadioStatus>());
                StaticResource.Add("RoundEndStatus", UserData.CreateStatic<ROUND_END_STATUS>());
                StaticResource.Add("Team", UserData.CreateStatic<Smod2.API.Team>());
                StaticResource.Add("UserRank", UserData.CreateStatic<UserRank>());
                lua.Globals["static"] = StaticResource;
                lua.Globals["player"] = new Functions.Player();
                lua.Globals["warhead"] = new Functions.Warhead();
                lua.Globals["scp914"] = scp914;


                var con = new Table(lua);
                con["SetString"] = new Action<string,string>((key,def) => {
                    if(def != null)
                        AddConfig(new Smod2.Config.ConfigSetting(key, def, Smod2.Config.SettingType.STRING, true, "qlay-variable"));
                    else
                        AddConfig(new Smod2.Config.ConfigSetting(key, "default", Smod2.Config.SettingType.STRING, true, "qlay-variable"));
                });
                con["GetString"] = CallbackFunction.FromDelegate(lua, new Func<string, string>(GetConfigString));
                con["GetNumber"] = CallbackFunction.FromDelegate(lua, new Func<string, float>(GetConfigFloat));
                con["GetBool"] = CallbackFunction.FromDelegate(lua, new Func<string, bool>(GetConfigBool));
                lua.Globals["con"] = con;
               


                luaHookCall = lua.DoString(Modules.StaticLua.LuaHook, null, "Hook system");
                lua.DoString(Modules.StaticLua.TableExtension, null, "Table extension");
                lua.DoString(Modules.StaticLua.DBModel, null, "DataBase Model");

                //file refresh
#if !DEBUG
                watcher = new FileSystemWatcher(luaPath + "/", "*.lua");
                //watcher.Filter = "*.*";
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += OnFileChanged;
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
#endif


                //load plugins

                if (Directory.Exists(luaPath))
                {
                    foreach (string dir in Directory.GetDirectories(luaPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        var g = lua.Globals;
                        Table localEnv = new Table(lua);
                        foreach (var item in g.Keys)
                        {
                            localEnv[item] = g[item];
                        }
                        localEnv["file"] = new Modules.File(dir + "/");
                        localEnv["include"] = new Action<string>((file) =>
                        {
                            LoadPluginFile(dir, localEnv, file);
                        });
                        localEnv["require"] = null;
                        //Info("env created ");


                        LoadPluginFile(dir, localEnv);
                        //Info("file loaded ");

                        Table pluginInfo = localEnv.Get("pluginInfo").ToObject<Table>();
                        string id = pluginInfo.Get("id").String;
                        string name = pluginInfo.Get("name").String;
                        string author = pluginInfo.Get("author").String;
                        string version = pluginInfo.Get("version").String;
                        //lua.Options.DebugPrint = s => { Info( "[" + id + "] " + s); };
                        lua.Options.DebugPrint = s => { Info("[LUA] " + s); };
                        log.Info("qlay->LUA", name + "(ver:" + version + ") by " + author + " has loaded");

                        localEnv["MsgN"] = new Action<string>((message) => {
                            log.Info(id, message);
                            luaHookCall.Function.Call("OnLog", "[" + id + "] " + message);
                        });

                        if (localEnv["Init"] != null)
                            lua.Call(localEnv["Init"]);
                        //Info("init called ");

                        LuaPlugin plugin = new LuaPlugin(dir);
                        plugin.id = id;
                        plugin.context = localEnv;
                        PluginList.Add(plugin);
                        //Info("plugin added");
                    }
                }
                else
                {
                    Directory.CreateDirectory(luaPath);
                }
            }
            catch (InterpreterException ex)
            {
                log.Error("qlay", "Lua error: " + ex.DecoratedMessage);
            }
        }

        public override void Register()
        {
            
        }
    }
}
