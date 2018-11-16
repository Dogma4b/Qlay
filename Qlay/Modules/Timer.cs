using MoonSharp.Interpreter;
using Smod2;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Qlay.Modules
{
    [MoonSharpUserData]
    public class Timer
    {
        [MoonSharpHidden]
        static List<TimerInstance> Timers = new List<TimerInstance>();
        [MoonSharpHidden]
        static List<TimerInstance> timers = new List<TimerInstance>();
        [MoonSharpHidden]
        public static Timer singleton = new Timer();
        [MoonSharpHidden]
        public static void MainThreadCheck(Plugin q)
        {
            try
            {
                if (timers.Count > 0)
                {
                    var now = Time.realtimeSinceStartup;// DateTime.UtcNow.TimeOfDay;
                    //q.Info("debug timers tick count: " + timers.Count + "  at " + now);
                    var orderedInvoke = timers.OrderBy(t => t.nextInvokeTime).ToList();
                    for (int i = 0; i < orderedInvoke.Count; i++)
                    {
                        var t = orderedInvoke[i];
                        if (now >= t.nextInvokeTime)
                        {
                            //q.Info("debug timer invoke: " + t.Identifier + "  at " + now);
                            t.Invoke(q);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                q.Info("error in timer call " + e.Message);
                q.Info(e.StackTrace);
            }
        }
        public static void MainThreadCheckDebug()
        {
            try
            {
                if (timers.Count > 0)
                {
                    var now = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
                    //  Console.WriteLine("debug timers tick count: " + timers.Count + "  at " + now);
                    var orderedInvoke = timers.OrderBy(t => t.nextInvokeTime).ToList();
                    for (int i = 0; i < orderedInvoke.Count; i++)
                    {
                        var t = orderedInvoke[i];
                        if (now >= t.nextInvokeTime)
                        {
                            Console.WriteLine("debug timer invoke: " + t.Identifier + "  at " + now);
                            t.InvokeDebug();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error in timer call " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        private class LuaTimerInstance : TimerInstance
        {
            new internal DynValue Function { get; set; }

            public LuaTimerInstance(string identifier, int delay, int repeations, DynValue function) : base(identifier, delay, repeations)
            { 
                Function = function; 
            }

            public override void Invoke(Plugin q)
            {
                try
                {
                    Function.Function.Call();
                }
                catch (Exception e)
                {
                    q.Info("error in timer " + Identifier + " call " + e.Message);
                    q.Info(e.StackTrace);
                }

                loopsRemaining--;

                if (loopsRemaining > 0)
                {
                    var now = Time.realtimeSinceStartup; //
                    nextInvokeTime = now + ((float)Delay) / 1000; // now + TimeSpan.FromMilliseconds(Delay); 
                }
                else
                {
                    Stop();
                }
            }
            public override void InvokeDebug()
            {
                try
                {
                    Function.Function.Call();
                }
                catch (Exception e)
                {
                    Console.WriteLine("error in timer " + Identifier + " call " + e.Message);
                    Console.WriteLine(e.StackTrace);
                }

                loopsRemaining--;

                if (loopsRemaining > 0)
                {
                    var now = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
                    nextInvokeTime = now + ((float)Delay) / 1000; // now + TimeSpan.FromMilliseconds(Delay); 
                }
                else
                {
                    Stop();
                }
            }
        }
        public class TimerInstance
        {
            public string Identifier { get; set; }
            protected int Delay { get; set; }
            protected int Loops { get; set; }
            protected bool Enabled { get; set; }
            protected int loopsRemaining;

            protected Action Function;

            internal float nextInvokeTime;

            protected TimerInstance(string identifier, int delay, int repeations)
            {
                Identifier = identifier;
                Loops = repeations;
                Delay = delay; 
                loopsRemaining = Loops;
            }
            public TimerInstance(string identifier, int delay, int repeations, Action function) : this(identifier, delay, repeations)
            { 
                Function = function; 
            }


            public void Start()
            {
                if (Enabled) Stop();
                var now = Time.realtimeSinceStartup; // DateTime.UtcNow.TimeOfDay;
                Enabled = true;
                nextInvokeTime = now + ((float)Delay) / 1000; // TimeSpan.FromMilliseconds(Delay);
                timers.Add(this);
            }
            public void Stop()
            {
                Enabled = false;
                timers.Remove(this);
                Timers.Remove(this);
            }
            public virtual void Invoke(Plugin q)
            {
                if (Function == null) return;
                try
                {
                    Function();
                }
                catch (Exception e)
                {
                    q.Info("error in timer " + Identifier + " call " + e.Message);
                    q.Info(e.StackTrace);
                }

                loopsRemaining--;

                if (loopsRemaining > 0)
                {
                    var now = Time.realtimeSinceStartup; //
                    nextInvokeTime = now + ((float)Delay) / 1000; // now + TimeSpan.FromMilliseconds(Delay); 
                }
                else
                {
                    Stop();
                }
            }
            public virtual void InvokeDebug()
            {
                if (Function == null) return;
                try
                {
                    Function();
                }
                catch (Exception e)
                {
                    Console.WriteLine("error in timer " + Identifier + " call " + e.Message);
                    Console.WriteLine(e.StackTrace);
                }

                loopsRemaining--;

                if (loopsRemaining > 0)
                {
                    var now = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
                    nextInvokeTime = now + ((float)Delay) / 1000; // now + TimeSpan.FromMilliseconds(Delay); 
                }
                else
                {
                    Stop();
                }
            }
        }
    

        [MoonSharpHidden]
        private Timer()
        {  
        }

        public void Simple(int miliseconds, DynValue function)
        {
            var t = new LuaTimerInstance("simple ..." + function.ReferenceID, miliseconds, 1, function);
            Timers.Add(t);
            t.Start();
        }
        [MoonSharpHidden]
        public void Simple(int miliseconds, Action function)
        {
            var t = new TimerInstance("simple ..." + function.Method, miliseconds, 1, function);
            Timers.Add(t);
            t.Start();
        }

        public object Create(string identifier, int delay, int repeations, DynValue function)
        {
            var t = new LuaTimerInstance(identifier, delay, repeations, function); 
            Timers.Add(t);
            t.Start();
            return t;
        }

        [MoonSharpHidden]
        public void Create(string identifier, int delay, int repeations, Action function)
        {
            var t = new TimerInstance(identifier, delay, repeations, function);
            Timers.Add(t);
            t.Start();
        }

        public void Destroy(string identifier)
        {
            TimerInstance luatimer = Timers.Find(timer => timer.Identifier == identifier);
            luatimer.Stop();
            Timers.Remove(luatimer);
        }

        public bool Exists(string identifier)
        {
            return Timers.Exists(timer => timer.Identifier == identifier);
        }
    }
}
