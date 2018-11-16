using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qlay.Modules
{
    class StaticLua
    {
        public static string LuaHook = @"
            hook = hook or {}

            hook.lua_hooks = hook.lua_hooks or {}
            local lua_hooks = hook.lua_hooks

            function hook.Add(type, id, func)
	            local case = lua_hooks[type] or {}
	            case.functions = case.functions or {}
	            if not case.functions[id] then
		            case.count = (case.count or 0) + 1
	            end
	            case.functions[id] = func 
	            lua_hooks[type] = case
            end
            function hook.Remove(type, id)
	            local case = lua_hooks[type]
	            if case then
		            if case.functions[id] then
			            case.functions[id] = nil
			            case.count = case.count - 1
		            end
		            if(case._count == 0) then
			            lua_hooks[type] = nil
		            end
	            end
            end
            function hook.GetTable()
	            return table.Copy(lua_hooks)
            end

            function hook.Call(eventid,...)
	            local case = lua_hooks[eventid]
	            if case then
		            for k,v in pairs(case.functions) do
			            local success, result = pcall(v,...)
			            if success then
				            if result then
					            return result
				            end
			            else
				            print('Error in hook: ' .. eventid .. '.' .. k .. ' -> ' .. result)
			            end
                    end

                end
            end

            return hook.Call";

        public static string TableExtension = @"function TableToString( tbl, indent, maxlevel )
            local srep = string.rep
            if not indent then indent = 0 end
            if not maxlevel then maxlevel = 2 end
            if indent > maxlevel then return end
            local st = """"
                for k, v in pairs(tbl,stdcomp) do
                    formatting = srep("" "", indent) .. ""["" .. tostring(k) .. ""]: ""
                    if type(v) == ""table"" then
                        if v ~= tbl then
                            st = st .. formatting .. ""\r\n""
                            st = st .. (TableToString(v, indent+1,maxlevel) or """")
                        else
                            st = st .. formatting .. tostring(v) .. ""\r\n""
                        end
                    else
                        st = st .. formatting .. tostring(v) .. ""\r\n""
                    end
                end
            return st
        end

        function PrintTable( tbl, maxlevel )
            print(""Printing table...\r\n"" .. TableToString(tbl, 0, maxlevel))
        end

        function SaveTable( fname, tbl, maxlevel )
            file:write(fname, TableToString(tbl, 0, maxlevel))
        end

        function table.Random(tbl)
            local keys = {}
            for k,v in pairs(tbl) do
                keys[#keys+1] = k
            end
            idx = keys[math.random(1,#keys)]
            return tbl[idx], idx
        end";

        public static string DBModel = @"";
    }
}
