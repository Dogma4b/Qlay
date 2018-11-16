using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using ServerMod2.API;
using Smod2;
using Smod2.API;
using UnityEngine;

namespace Qlay.Functions
{
    [MoonSharpUserData]
    class Player
    {
        public List<Smod2.API.Player> GetAll()
        {
            try
            {
                return SmodPlayer.GetPlayers();
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog(ex.Message);
            }

            return null;
        }

        public List<Smod2.API.Player> GetByName(string name)
        {
            try
            {
                return SmodPlayer.GetPlayers(name);
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog(ex.Message);
            }

            return null;
        }

        public List<string> GetMuteList()
        {
            List<string> mutes = new List<string>();
            var ParsedMutes = FileManager.ReadAllLines(FileManager.GetAppFolder() + "mutes.txt");

            foreach (string mute in ParsedMutes)
            {
                mutes.Add(mute);
            }

            return mutes;
        }
    }

    public static class PlayerExtension
    {
        public static void SetPermission(this Smod2.API.Player self, double permission, bool isStaff = false)
        {
            GameObject player = (GameObject)self.GetGameObject();
            ServerRoles ComponentRole = player.GetComponent<ServerRoles>();
            ComponentRole.Permissions = (UInt64) permission;
            ComponentRole.RemoteAdmin = true;
            //ComponentRole.RemoteAdminMode = ServerRoles.AccessMode.GlobalAccess;
            ComponentRole.CallTargetOpenRemoteAdmin(ComponentRole.connectionToClient);
            if (isStaff)
            {
                ComponentRole.Staff = true;
            }
        }

        public static void SetMute(this Smod2.API.Player self, bool mute)
        {
            GameObject player = (GameObject)self.GetGameObject();
            player.GetComponent<CharacterClassManager>().SetMuted(mute); 
        }
        
        public static void SetOverwatch(this Smod2.API.Player self, bool overwatch)
        {
            GameObject player = (GameObject)self.GetGameObject();
            player.GetComponent<ServerRoles>().CmdSetOverwatchStatus(overwatch);
        }

        [MoonSharpUserDataMetamethod("__index")]
        public static void __index(this Smod2.API.Player self, DynValue k)
        {
            UserData.RegisterExtensionType(typeof(PlayerExtension));
        }

        public static double Test(this Smod2.API.Player self, double a, double b)
        {
            return a + b;
        }


    }

}
