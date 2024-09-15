using MonoMod.Utils;
using NLua;
using System.Linq;
using Monocle;
using System.Collections.Generic;
using System.Collections;
using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Utils
{
    /// <summary>
    /// Class, which you can easily use from lua, but technically can be used from c# too
    /// </summary>
    public static class Lua
    {

        // Privating
        public static object GetPrivateMember(object obj, string name)
        {
            DynamicData objData = DynamicData.For(obj);
            return objData.Get<object>(name);
        }
        public static void SetPrivateMember(object obj, string name, object newValue)
        {
            DynamicData objData = DynamicData.For(obj);
            objData.Set(name, newValue);
        }
        public static object CallMethod(object obj, string name, LuaTable args)
        {
            object[] argArray = args.Values.Cast<object>().ToArray();
            DynamicData objData = DynamicData.For(obj);
            return objData.Invoke(name, argArray);
        }
        public static object CallMethod(object obj, string name)
        {
            DynamicData objData = DynamicData.For(obj);
            return objData.Invoke(name);
        }


        // Storing
        public static void StoreVariable(object obj, string name) 
        {
            DzhakeHelperModule.Session.StoredVariables.Add(name,obj);
        }
        public static object GetStoredVariable(string name)
        {
            DzhakeHelperModule.Session.StoredVariables.TryGetValue(name, out object value);
            return value;
        }
        public static void RemoveStoredVariable(string name)
        {
            DzhakeHelperModule.Session.StoredVariables.Remove(name);   
        }

        

        // https://github.com/Cruor/LuaCutscenes/blob/master/Helpers/LuaHelper.cs

        #region Conversions
        public static LuaTable DictionaryToLuaTable(IDictionary<object, object> dict)
        {
            NLua.Lua lua = Everest.LuaLoader.Context;
            LuaTable table = lua.DoString("return {}").FirstOrDefault() as LuaTable;

            foreach (KeyValuePair<object, object> pair in dict)
            {
                table[pair.Key] = pair.Value;
            }

            return table;
        }

        public static LuaTable ListToLuaTable(IList list)
        {
            NLua.Lua lua = Everest.LuaLoader.Context;
            LuaTable table = lua.DoString("return {}").FirstOrDefault() as LuaTable;

            int ptr = 1;

            foreach (var value in list)
            {
                table[ptr++] = value;
            }


            return table;
        }
        #endregion


        // just some random stuff lol

        public static IEnumerator CustomWalkTo(Player player, float x, bool walkBackwards = false, float speedMultiplier = 1f, bool keepWalkingIntoWalls = false, bool changeSpriteAfter = true)
        {
            player.StateMachine.State = 11;
            if (Math.Abs(player.X - x) > 4f && !player.Dead)
            {
                player.DummyMoving = true;
                if (walkBackwards)
                {
                    player.Sprite.Rate = -1f;
                    player.Facing = (Facings)Math.Sign(player.X - x);
                }
                else
                {
                    player.Facing = (Facings)Math.Sign(x - player.X);
                }

                while (Math.Abs(x - player.X) > 4f && Engine.Scene != null && (keepWalkingIntoWalls || !player.CollideCheck<Solid>(player.Position + Vector2.UnitX * Math.Sign(x - player.X))))
                {
                    player.Speed.X = Calc.Approach(player.Speed.X, (float)Math.Sign(x - player.X) * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
                    yield return null;
                }

                if (changeSpriteAfter)
                {
                    player.Sprite.Rate = 1f;
                    player.Sprite.Play("idle");
                }
                player.DummyMoving = false;
            }
        }
    }
}
