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
            return SmodPlayer.GetPlayers();
        }

        public List<Smod2.API.Player> GetByName(string name)
        {
            return SmodPlayer.GetPlayers(name);
        }

        public Smod2.API.Player GetById(int id)
        {
            foreach(Smod2.API.Player player in SmodPlayer.GetPlayers())
            {
                if (((GameObject)player.GetGameObject()).GetComponent<RemoteAdmin.QueryProcessor>().PlayerId == id)
                {
                    return player;
                }
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
        public static void SetPermission(this Smod2.API.Player self, string color, string badge, double permission, bool cover, bool hidden, bool isStaff = false)
        {
            GameObject player = (GameObject)self.GetGameObject();
            ServerRoles ComponentRole = player.GetComponent<ServerRoles>();
            UserGroup group = new UserGroup();
            group.BadgeColor = color;
            group.BadgeText = badge;
            group.Permissions = (ulong)permission;
            group.Cover = cover;
            group.HiddenByDefault = hidden;
            ComponentRole.SetGroup(group, false, false, false);
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

        public static int GetMaxHP(this Smod2.API.Player self, int maxhp)
        {
            GameObject player = (GameObject)self.GetGameObject();
            return player.GetComponent<PlayerStats>().maxHP;
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
