using System;
using System.Linq;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using MoonSharp.Interpreter.Interop;

namespace Qlay
{
    public class DynamicIndex : MoonSharp.Interpreter.Interop.IUserDataDescriptor
    {
        public string Name => "Player";

        public Type Type { get; private set; }

        Dictionary<DynValue, DynValue> vars = new Dictionary<DynValue, DynValue>();
        Dictionary<string, Func<object, object>> prop_get = new Dictionary<string, Func<object, object>>();
        Dictionary<string, Action<object, object>> prop_set = new Dictionary<string, Action<object, object>>();

        public string AsString(object obj)
        {
            return obj.ToString();
        }

        public DynamicIndex(Script s, Type t, params Type[] extension_types)
        {
            Type = t;
            var met = t.GetMethods().Where(ff => ff.IsPublic && ff.MemberType == System.Reflection.MemberTypes.Method);
            var overloaded = met.GroupBy(x => x.Name)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();
            var met2 = met.ToList();
            met2.RemoveAll(ff => overloaded.Contains(ff.Name));
            //met = met.Except(overloaded).ToList();

            foreach (var m in met2)
            {
                if (m.GetCustomAttributes(typeof(MoonSharpHiddenAttribute), false).Count() > 0) continue;

                // vars.Add(DynValue.NewString(m.Name), DynValue.NewCallback(CallbackFunction.FromMethodInfo(s, m)));

                if (m.IsStatic)
                {
                    var pars = m.GetParameters();
                    vars.Add(DynValue.NewString(m.Name), DynValue.NewCallback(new CallbackFunction(  new Func<ScriptExecutionContext,CallbackArguments,DynValue>((sc,a) => {
                        try
                        {
                            var args = new object[pars.Length]; //a.GetArray().Select(ff => ff.ToObject()).ToArray();

                            for (int i = 0; i < a.Count; i++)
                            {
                                args[i] = a[i].ToObject(pars[i].ParameterType);
                            }

                            return DynValue.FromObject(sc.OwnerScript, m.Invoke(null, args));// a.GetArray().Select(ff => ff.ToObject()).ToArray())); }
                        }
                        catch (Exception e)
                        {
                            sc.OwnerScript.DoString("print('error '.." + e.Message + ") print('at '..debug.traceback())");
                            return DynValue.Nil;
                        }
                    }))));

                }
                else
                {
                    var pars = m.GetParameters();
                    vars.Add(DynValue.NewString(m.Name), DynValue.NewCallback(new CallbackFunction(new Func<ScriptExecutionContext, CallbackArguments, DynValue>((sc, a) => {
                        try
                        {
                            var args = new object[pars.Length + 1]; //a.GetArray().Select(ff => ff.ToObject()).ToArray();
                            args[0] = a[0].ToObject();
                            for (int i = 1; i < a.Count; i++)
                            {
                                args[i] = a[i].ToObject(pars[i - 1].ParameterType);
                            }

                            return DynValue.FromObject(sc.OwnerScript, m.Invoke(args[0], args.Skip(1).ToArray()));
                        }
                        catch (Exception e)
                        {
                            sc.OwnerScript.DoString("print('error '.." + e.Message + ") print('at '..debug.traceback())");
                            return DynValue.Nil;
                        }
                    }))));

                }
            }
            foreach (var ovm in overloaded)
            {
                AddOverloadedMethod2(s, ovm);
            }

            foreach (var prop in t.GetProperties())
            { 
                vars.Add(DynValue.NewString("Get" + prop.Name), DynValue.NewCallback(new CallbackFunction(new Func<ScriptExecutionContext, CallbackArguments, DynValue>((sc, a) => {
                    return DynValue.FromObject(sc.OwnerScript, prop.GetValue(a[0].ToObject(), null));
                }))));
                vars.Add(DynValue.NewString("Set" + prop.Name), DynValue.NewCallback(new CallbackFunction(new Func<ScriptExecutionContext, CallbackArguments, DynValue>((sc, a) => {
                    prop.SetValue(a[0].ToObject(), a[1].ToObject(), null);
                    return DynValue.Nil;
                }))));

                prop_get.Add(prop.Name, new Func<object, object>((j) => { return prop.GetValue(j, null); }));
                prop_set.Add(prop.Name, new Action<object, object>((j, v) => { prop.SetValue(j, v, null); }));
            }


            foreach (var et in extension_types)
            {
                var met1 = et.GetMethods().Where(ff => ff.IsPublic && ff.IsStatic && ff.MemberType == System.Reflection.MemberTypes.Method);
                foreach (var m1 in met1)
                {
                    var pars = m1.GetParameters();
                    vars.Add(DynValue.NewString(m1.Name), DynValue.NewCallback(new CallbackFunction(new Func<ScriptExecutionContext, CallbackArguments, DynValue>((sc, a) => {
                        //var args = a.GetArray().Select(ff => ff.ToObject()).ToArray();
                        try
                        {
                            var args = new object[pars.Length]; //a.GetArray().Select(ff => ff.ToObject()).ToArray();
                            args[0] = a[0].ToObject();
                            for (int i = 1; i < a.Count; i++)
                            {
                                args[i] = a[i].ToObject(pars[i].ParameterType);
                            }

                            return DynValue.FromObject(sc.OwnerScript, m1.Invoke(null, args));
                        }
                        catch (Exception e)
                        {
                            sc.OwnerScript.DoString("print('error '.." + e.Message + ") print('at '..debug.traceback())");
                            return DynValue.Nil;
                        }
                    }))));
                    //.FromMethodInfo(s, m1)));
                }
            }

        }
        void AddOverloadedMethod(Script s, string name)
        {
            var ov = new OverloadedMethodMemberDescriptor(name, Type);
            foreach (var method in Type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(mi => mi.Name == name))
            {
                ov.AddOverload(new MethodMemberDescriptor(method));
            }
            vars.Add(DynValue.NewString(name), DynValue.NewCallback(ov.GetCallbackFunction(s)));
        }
        void AddOverloadedMethod2(Script s, string name)
        {
            var ov = new OverloadedMethodMemberDescriptor(name, Type);
            var mr = Type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(mi => mi.Name == name).ToArray();


            vars.Add(DynValue.NewString(name), DynValue.NewCallback(new CallbackFunction(new Func<ScriptExecutionContext, CallbackArguments, DynValue>((sc, a) => {
                try
                {
                    var args = a.GetArray().Select(ff => ff.ToObject()).ToArray();
                    var atypes = args.Skip(1).Select(ff => ff.GetType()).ToArray();
                    var m = Type.GetMethod(name, atypes); //Soo slow
                    if (m != null)
                    {
                        return DynValue.FromObject(sc.OwnerScript, m.Invoke(args[0], args.Skip(1).ToArray()));
                    }
                    return DynValue.Nil;
                }
                catch(Exception e)
                {
                    sc.OwnerScript.DoString("print('error '.." + e.Message + ") print('at '..debug.traceback())");
                    return DynValue.Nil;
                }
            }))));

        }
        public DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
        {
            DynValue r;
            if(vars.TryGetValue(index, out r))
            {
                return r;
            }
            Func<object, object> v;
            if (prop_get.TryGetValue(  index.String, out v))
            {
                return DynValue.FromObject(script, v(obj));
            }
            return DynValue.Nil;
        }
        public bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
        {
            Action<object, object> v;
            if (prop_set.TryGetValue(index.String, out v))
            {
                v(obj, value.ToObject());
                return true;
            }
            return false;
        }

        public bool IsTypeCompatible(Type type, object obj)
        {
            return Type.IsAssignableFrom(type);
        }

        public DynValue MetaIndex(Script script, object obj, string metaname)
        {
            throw new NotImplementedException();
        }

    } 

}
