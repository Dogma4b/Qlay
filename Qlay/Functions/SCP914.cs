using UnityEngine;
using ServerMod2;
using ServerMod2.API;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace Qlay.Functions
{
    class SCP914 : IEventHandlerSCP914Activate
    {

        private Qlay plugin;

        public SCP914(Plugin plugin)
        {
            this.plugin = plugin as Qlay;
        }

        public Scp914.Recipe[] GetRecepies()
        {
            return Object.FindObjectOfType<Scp914>().recipes;
        }

        public void OnSCP914Activate(SCP914ActivateEvent ev)
        {
            List<GameObject> objects = new List<GameObject>();
            List<SmodPlayer> players = new List<SmodPlayer>();

            foreach (Collider collider in ev.Inputs)
            {
                if(collider.GetType() == typeof(CapsuleCollider))
                {
                    if (collider.gameObject.name == "Player")
                    {
                        players.Add(new SmodPlayer(collider.gameObject));
                    }
                }
            }

            plugin.luaHookCall.Function.Call("OnSCP914Activate", players, ev.KnobSetting, ev.Intake, ev.Outtake);
        }
    }
}
