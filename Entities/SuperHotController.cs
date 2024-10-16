using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/SuperHotController")]
    public class SuperHotController : Entity
    {
        private Vector2 prevPlayerPos;
        private MTexture cursor;

        private SuperHotItem? heldItem;

        public SuperHotController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            cursor = GFX.Game["objects/DzhakeHelper/superhotController/cursor"];
            Depth = Depths.Top;
        }

        public override void Update()
        {
            base.Update();
            Player? player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (!(player?.InControl ?? false))
            {
                Engine.DeltaTime = 1f;
                return;
            }

            //Slowdown
            float posDiff = (player.Position - prevPlayerPos).Length();
            bool moving = !player.InControl || Input.Aim.Value.Length() > 0.3f || (!player.onGround && !Input.Jump) || Input.Dash
                        || Input.CrouchDash || Input.Grab;
            Engine.TimeRate = Calc.Approach(Engine.TimeRate, moving ? 1f : 0.1f, posDiff + 0.01f);

            //Cursor
            Vector2 cursorPos = player.Position - Util.GameMousePos();
            float angle = cursorPos.Angle();

            prevPlayerPos = player.Position;
        }

        public override void Render()
        {
            base.Render();

            //Cursor
            Vector2 mousePos = Util.GameMousePos();
            float angle = heldItem?.cooldownTimer / heldItem?.cooldown * 90f * Calc.DegToRad ?? 0f;

            cursor.DrawCentered(mousePos, Color.White, Vector2.One, angle);
        }
    }
}