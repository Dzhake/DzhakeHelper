using System;
using Celeste.Mod.DzhakeHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Monocle;

namespace Celeste.Mod.DzhakeHelper
{
    public static class DzhakeHelperHooks
    {
        public static bool PlayerMoved;
        private static Vector2 previousPlayerPos;

        public static void Load()
        {
            On.Celeste.LevelLoader.LoadingThread += CustomDashInitialize;
            On.Celeste.Player.DashBegin += CustomDashBegin;
            On.Celeste.Player.Die += PlayerDeath;
            On.Celeste.Player.Update += PlayerUpdate;
            On.Celeste.Player.Render += PlayerRender;
            Everest.Events.Player.OnSpawn += PlayerSpawn;
            
            IL.Celeste.Player.Render += PlayerRender_IL;
        }


        public static void Unload()
        {
            On.Celeste.LevelLoader.LoadingThread -= CustomDashInitialize;
            On.Celeste.Player.DashBegin -= CustomDashBegin;
            On.Celeste.Player.Die -= PlayerDeath;
            On.Celeste.Player.Update -= PlayerUpdate;
            On.Celeste.Player.Render -= PlayerRender;
            Everest.Events.Player.OnSpawn -= PlayerSpawn;

            IL.Celeste.Player.Render -= PlayerRender_IL;
        }

        //Sequence
        private static void CustomDashInitialize(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            ResetDashSession();
            orig(self);
        }   

        //Sequence
        private static void CustomDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
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

            orig(self);
        }

        //Sequence
        private static PlayerDeadBody PlayerDeath(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            ResetDashSession();
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        //Sequence
        private static void ResetDashSession()
        {
            if (DzhakeHelperModule.Session != null)
            {
                DzhakeHelperModule.Session.HasSequenceDash = false;
            }
        }

        private static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            PlayerMoved = previousPlayerPos != self.Position;
            previousPlayerPos = self.Position;

            //Timed Kill Trigger
            if (DzhakeHelperModule.Session.TimedKillTriggerTimeChanged == false)
            {
                DzhakeHelperModule.Session.TimedKillTriggerTime = 0f;
                DzhakeHelperModule.Session.TimedKillTriggerColor = Color.White;
            }
            DzhakeHelperModule.Session.TimedKillTriggerTimeChanged = false;
            DzhakeHelperModule.Session.TimedKillTriggerMaxTime = 0f;

            orig(self);
        }

        

        private static void PlayerRender(On.Celeste.Player.orig_Render orig, Player self)
        {
            if (ElephantWalkController.Angle != 0f)
            {
                Camera camera = (self.Scene as Level).Camera;
                Vector2 offset = camera.Position - self.Center; // Offset for matrix, to rotate around player
                Draw.SpriteBatch.End(); //ends the previous batch
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None,
                    RasterizerState.CullNone, null, camera.Matrix * Matrix.CreateTranslation(offset.X, offset.Y, 0)
                    * Matrix.CreateRotationZ(ElephantWalkController.Angle) * Matrix.CreateTranslation(-offset.X, -offset.Y, 0));
                orig(self);
                Draw.SpriteBatch.End(); // Ends the rotated batch
                GameplayRenderer.Begin(); // creates a new batch where the previous batch "was"
            }
            else
                orig(self);
        }

        //Change color while in Timed Kill Trigger
        private static void PlayerRender_IL(ILContext il)
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
                Logger.Log(LogLevel.Error, "DzhakeHelper/Hooks/PlayerRender", "Hook was NOT applied! Please report this to Dzhake.");
            }
        }

        //Timed Kill Trigger
        private static Color PlayersColor(Color oldColor)
        {
            if (DzhakeHelperModule.Session.TimedKillTriggerColor != Color.White)
            {
                oldColor = DzhakeHelperModule.Session.TimedKillTriggerColor;
            }
            return oldColor;
        }


        // Custom Keys
        private static void PlayerSpawn(Player self)
        {
            if (DzhakeHelperModule.Session?.CurrentKeys == null || self == null) return;

            Scene level = self.Scene;
            foreach (CustomKey.CustomKeyInfo info in DzhakeHelperModule.Session.CurrentKeys)
            {
                CustomKey key = new(info);
                self.Leader.GainFollower(key.follower);
                key.Position = self.Position + new Vector2(-12 * (int)self.Facing, -8f);
                level.Add(key);
            }
        }
    }

}
