using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Qlay.Modules;
using Smod2.EventSystem.Events;

namespace Qlay
{
    [MoonSharpUserData]
    class Hooks: IEventHandlerRoundStart, IEventHandlerCheckRoundEnd, IEventHandlerRoundEnd, IEventHandlerConnect, IEventHandlerDisconnect, IEventHandlerWaitingForPlayers, IEventHandlerRoundRestart, IEventHandlerUpdate, IEventHandlerFixedUpdate,
        IEventHandlerPlayerJoin, IEventHandlerSpawn, IEventHandlerTeamRespawn, IEventHandlerSpawnRagdoll, IEventHandlerCheckEscape, IEventHandlerSetRole, IEventHandlerPlayerHurt, IEventHandlerPlayerDie, IEventHandlerPlayerPickupItem, IEventHandlerPlayerPickupItemLate, IEventHandlerPlayerDropItem, IEventHandlerMedkitUse, IEventHandlerLure, IEventHandlerRadioSwitch, IEventHandlerShoot, IEventHandlerThrowGrenade,
        IEventHandler106CreatePortal, IEventHandler106Teleport, IEventHandlerContain106, IEventHandlerPocketDimensionEnter, IEventHandlerPocketDimensionExit, IEventHandlerPocketDimensionDie,
        IEventHandlerIntercom, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown, IEventHandlerWarheadDetonate, IEventHandlerLCZDecontaminate
    {
        private Qlay plugin;

        public Hooks(Plugin plugin)
        {
            this.plugin = plugin as Qlay;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnRoundStart", ev.Server);
        }

        public void OnCheckRoundEnd(CheckRoundEndEvent ev)
        {
            if (ev.Server.NumPlayers <= 2)
            {
                ev.Status = ROUND_END_STATUS.ON_GOING;
            }

            plugin.luaHookCall.Function.Call("OnCheckRoundEnd", ev.Server, ev.Round, ev.Status);
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnRoundEnd", ev.Server, ev.Round, ev.Status);
        }

        public void OnConnect(ConnectEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnConnect", ev.Connection);
        }

        public void OnDisconnect(DisconnectEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnDisconnect", ev.Connection);
        }

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnWaitingForPlayers", ev.Server);
        }

        public void OnRoundRestart(RoundRestartEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnRoundRestart", ev.Server);
        }

        public void OnUpdate(UpdateEvent ev)
        {
            Modules.LuaSocket.SocketConnections.UpdateConnectionsReceive();
            Timer.MainThreadCheck(plugin);
            plugin.luaHookCall.Function.Call("OnUpdate");
        }

        public void OnFixedUpdate(FixedUpdateEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnFixedUpdate");
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnPlayerJoin", ev.Player);
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnSpawn", ev.Player, ev.SpawnPos);
        }

        public void OnTeamRespawn(TeamRespawnEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnTeamRespawn", ev.PlayerList, ev.SpawnChaos);
        }

        public void OnSpawnRagdoll(PlayerSpawnRagdollEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnSpawnRagdoll", ev.Player, ev.Role, ev.Position, ev.Rotation);
        }

        public void OnCheckEscape(PlayerCheckEscapeEvent ev)
        {
            var args = plugin.luaHookCall.Function.Call("OnCheckEscape", ev.Player, ev.ChangeRole, ev.AllowEscape);
            if (args.Type == DataType.Table)
            {
                var table = args.Table;

                var ChangeRole = table.Get("ChangeRole");
                if (ChangeRole.IsNotNil()) ev.ChangeRole = ChangeRole.ToObject<Role>();

                var AllowEscape = table.Get("AllowEscape");
                if (AllowEscape.IsNotNil()) ev.AllowEscape = AllowEscape.Boolean;
            }
        }
         
        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            var args = plugin.luaHookCall.Function.Call("OnSetInventory", ev.Player, ev.Role, ev.Items);
            if (args.Type == DataType.Table)
            {
                var table = args.Table;

                var Role = table.Get("Role");
                if (Role.IsNotNil()) ev.Role = Role.ToObject<Role>();

                var Items = table.Get("Items");
                if (Items.IsNotNil())
                {
                    ev.Items = Items.ToObject<List<ItemType>>();
                }
            }

            args = plugin.luaHookCall.Function.Call("OnSetRole", ev.Player, ev.Role, ev.Items);
            if (args.Type == DataType.Table)
            {
                var table = args.Table;

                var Role = table.Get("Role");
                if (Role.IsNotNil()) ev.Role = Role.ToObject<Role>();

                var Items = table.Get("Items");
                if (Items.IsNotNil())
                {
                    ev.Items = Items.ToObject<List<ItemType>>();
                }
            }
        }

        public void OnPlayerHurt(PlayerHurtEvent ev)
        {
            var args = plugin.luaHookCall.Function.Call("OnPlayerHurt", ev.Player, ev.Attacker, ev.Damage, ev.DamageType);
            if (args.Type == DataType.Table)
            {
                var table = args.Table;

                var Damage = table.Get("Items");
                if (Damage.IsNotNil()) ev.Damage = (float)Damage.Number;
                var DamageType = table.Get("Items");
                if (DamageType.IsNotNil()) ev.DamageType = DamageType.ToObject<DamageType>();
            }
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            var args = plugin.luaHookCall.Function.Call("OnPlayerDie", ev.Player, ev.Killer, ev.SpawnRagdoll, ev.DamageTypeVar);
            if (args.Type == DataType.Table)
            {
                var table = args.Table;

                var SpawnRagdoll = table.Get("SpawnRagdoll");
                if (SpawnRagdoll.IsNotNil()) ev.SpawnRagdoll = SpawnRagdoll.Boolean;
                var DamageType = table.Get("DamageType");
                if (DamageType.IsNotNil()) ev.DamageTypeVar = DamageType.ToObject<DamageType>();
            }
        }

        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnPlayerPickupItem", ev.Player, ev.Item, ev.Allow, ev.ChangeTo);
        }

        public void OnPlayerPickupItemLate(PlayerPickupItemLateEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnPlayerPickupItemLate", ev.Player, ev.Item);
        }

        public void OnPlayerDropItem(PlayerDropItemEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnPlayerDropItem", ev.Player, ev.Item, ev.Allow, ev.ChangeTo);
        }

        public void OnMedkitUse(PlayerMedkitUseEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnMedkitUse", ev.Player, ev.RecoverHealth);
        }

        public void OnLure(PlayerLureEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnLure", ev.Player, ev.AllowContain);
        }

        public void OnPlayerRadioSwitch(PlayerRadioSwitchEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnPlayerRadioSwitch", ev.Player, ev.ChangeTo);
        }

        public void OnShoot(PlayerShootEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnShoot", ev.Player, ev.Target, ev.Weapon);
        }

        public void OnThrowGrenade(PlayerThrowGrenadeEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnThrowGrenade", ev.Player, ev.GrenadeType, ev.SlowThrow, ev.Direction);
        }

        public void On106CreatePortal(Player106CreatePortalEvent ev)
        {
            plugin.luaHookCall.Function.Call("On106CreatePortal", ev.Player, ev.Position);
        }

        public void On106Teleport(Player106TeleportEvent ev)
        {
            plugin.luaHookCall.Function.Call("On106Teleport", ev.Player, ev.Position);
        }

        public void OnContain106(PlayerContain106Event ev)
        {
            plugin.luaHookCall.Function.Call("OnContain106", ev.Player, ev.SCP106s, ev.ActivateContainment);
        }

        public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnPocketDimensionEnter", ev.Player, ev.TargetPosition, ev.LastPosition, ev.Damage);
        }

        public void OnPocketDimensionExit(PlayerPocketDimensionExitEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnPocketDimensionExit", ev.Player, ev.ExitPosition);
        }

        public void OnPocketDimensionDie(PlayerPocketDimensionDieEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnPocketDimensionDie", ev.Player, ev.Die);
        }

        public void OnIntercom(PlayerIntercomEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnIntercom", ev.Player, ev.SpeechTime, ev.CooldownTime);
        }

        public void OnStartCountdown(WarheadStartEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnStartCountdown", ev.Activator, ev.Cancel, ev.IsResumed, ev.OpenDoorsAfter, ev.TimeLeft);
        }

        public void OnStopCountdown(WarheadStopEvent ev)
        {
            plugin.luaHookCall.Function.Call("OnStopCountdown", ev.Activator, ev.Cancel, ev.TimeLeft);
        }

        public void OnDetonate()
        {
            plugin.luaHookCall.Function.Call("OnDetonate");
        }

        public void OnDecontaminate()
        {
            plugin.luaHookCall.Function.Call("OnDecontaminate");
        }
    }
}
