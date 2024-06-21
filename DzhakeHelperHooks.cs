using Celeste.Mod.DzhakeHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Collections;
using System.Reflection;
using YamlDotNet.Core;

namespace Celeste.Mod.DzhakeHelper
{
    public class DzhakeHelperHooks
    {

        public static void Load()
        {
            On.Celeste.LevelLoader.LoadingThread += CustomDashInitialize;
            On.Celeste.Player.DashBegin += CustomDashBegin;
            On.Celeste.Player.Die += PlayerDeath;
            On.Celeste.Player.Update += PlayerUpdate;
            Everest.Events.Player.OnSpawn += PlayerSpawn;

            IL.Celeste.Player.Render += PlayerRender;
        }


        public static void Unload()
        {
            On.Celeste.LevelLoader.LoadingThread -= CustomDashInitialize;
            On.Celeste.Player.DashBegin -= CustomDashBegin;
            On.Celeste.Player.Die -= PlayerDeath;
            On.Celeste.Player.Update -= PlayerUpdate;
            Everest.Events.Player.OnSpawn -= PlayerSpawn;

            IL.Celeste.Player.Render -= PlayerRender;
        }

        private static void CustomDashInitialize(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            ResetDashSession();
            orig(self);
        }   

        private static void CustomDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
            bool callOrig = true;

            SequenceBlockManager manager = self.Scene.Tracker.GetEntity<SequenceBlockManager>();
            if (manager != null)
            {
                if (DzhakeHelperModule.Session.HasSequenceDash)
                {
                    DzhakeHelperModule.Session.HasSequenceDash = false;
                    manager.CycleSequenceBlocks();
                }
                if (manager.everyDash)
                {
                    manager.CycleSequenceBlocks();
                }
            }

            if (callOrig)
            {
                orig(self);
            }
        }


        private static PlayerDeadBody PlayerDeath(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            ResetDashSession();
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }


        private static void ResetDashSession()
        {
            if (DzhakeHelperModule.Session != null)
            {
                DzhakeHelperModule.Session.HasSequenceDash = false;
            }
        }


        private static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            if (DzhakeHelperModule.Session.TimedKillTriggerTimeChanged == false)
            {
                DzhakeHelperModule.Session.TimedKillTriggerTime = 0f;
                DzhakeHelperModule.Session.TimedKillTriggerColor = Color.White;
            }
            DzhakeHelperModule.Session.TimedKillTriggerTimeChanged = false;
            DzhakeHelperModule.Session.TimedKillTriggerMaxTime = 0f;
            orig(self);
        }

        private static void PlayerRender(ILContext il)
        {
            bool happened = false;
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("Sprite"), instr => instr.MatchCall<Color>("get_White")))
            {
                cursor.EmitDelegate(PlayersColor);
                happened = true;
            }
            if (!happened)
            {
                Logger.Log(LogLevel.Error, "DzhakeHelper/Hooks/PlayerRender", "Hook was NOT applied! Report it to Dzhake, or someone else.");
            }
        }

        private static Color PlayersColor(Color oldColor)
        {
            if (DzhakeHelperModule.Session.TimedKillTriggerColor != Color.White)
            {
                oldColor = DzhakeHelperModule.Session.TimedKillTriggerColor;
            }
            return oldColor;
        }


        private static void PlayerSpawn(Player self)
        {
            foreach (CustomKey key in DzhakeHelperModule.Session.CurrentKeys)
            {
                self.Leader.GainFollower(key.follower);
            }
        }

    }

}
