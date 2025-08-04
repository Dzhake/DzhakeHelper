using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/VoidLift")]
    public class VoidLift : Entity
    {

        private Sprite sprite;

        public float NewSpeed;
        public bool RefillDash;
        public bool SetSpeedXToZero;

        public float RespawnTimer;

        public float RespawnTime = 0.1f;

        public bool DashToLeft;
        public bool DashToRight;
        public bool DashToTop;
        public bool DashToBottom;
        public bool DashToTopLeft;
        public bool DashToTopRight;
        public bool DashToBottomLeft;
        public bool DashToBottomRight;

        public string sfx;

        // Deco :D
        private readonly BloomPoint bloom;
        private readonly VertexLight light;


        public VoidLift(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(OnPlayer));

            NewSpeed = data.Float("newSpeed", -400);
            RefillDash = data.Bool("refillDash", false);
            SetSpeedXToZero = data.Bool("setSpeedxToZero", true);
            RespawnTime = data.Float("respawnTime", 0.1f);

            string str = data.Attr("sprite");
            Add(sprite = new Sprite(GFX.Game, str + "idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Depth = 100;

            DashToLeft = data.Bool("dashToLeft");
            DashToRight = data.Bool("dashToRight");
            DashToTop = data.Bool("dashToTop");
            DashToBottom = data.Bool("dashToBottom");
            DashToTopLeft = data.Bool("dashToTopLeft");
            DashToTopRight = data.Bool("dashToTopRight");
            DashToBottomLeft = data.Bool("dashToBottomLeft");
            DashToBottomRight = data.Bool("dashToBottomRight");

            sfx = data.Attr("sfx");


            Add(bloom = new BloomPoint(0.8f, 16f));
            Add(light = new VertexLight(Color.White, 1f, 16, 48));
        }

        public override void Update()
        {
            base.Update();
            if (RespawnTimer > 0)
            {
                RespawnTimer -= Engine.DeltaTime;
                if (RespawnTimer < 0) RespawnTimer = 0;
            }

            light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
            bloom.Alpha = light.Alpha * 0.8f;
        }


        private void OnPlayer(Player player)
        {
            if (!player.DashAttacking || !(RespawnTimer <= 0) || (((player.DashDir != Util.Directions[Util.DirectionEnum.Right]) || !DashToRight) && ((player.DashDir != Util.Directions[Util.DirectionEnum.Left]) || !DashToLeft) && ((player.DashDir != Util.Directions[Util.DirectionEnum.Bottom]) || !DashToBottom) && ((player.DashDir != Util.Directions[Util.DirectionEnum.Top]) || !DashToTop) && (!(player.DashDir.X > 0f) || !(player.DashDir.Y > 0f) || !DashToBottomRight) && (!(player.DashDir.X < 0f) || !(player.DashDir.Y > 0f) || !DashToBottomLeft) && (!(player.DashDir.X > 0f) || !(player.DashDir.Y < 0f) || !DashToTopRight) && (!(player.DashDir.X < 0f) || !(player.DashDir.Y < 0f) || !DashToTopLeft))) return;
            
            player.DashDir = new Vector2(0, -1);

            //uhh wtf is this
            Collider collider = player.Collider;
            player.Collider = player.normalHitbox;
            //player.MoveVExact((int)(Y - base.Bottom));
            if (!player.Inventory.NoRefills && RefillDash)
                player.RefillDash();

            RespawnTimer = RespawnTime;

            player.RefillStamina();
            player.StateMachine.State = 0;
            player.jumpGraceTimer = 0f;
            player.varJumpTimer = 0.2f;
            player.AutoJump = true;
            player.AutoJumpTimer = 0f;
            player.wallSlideTimer = 1.2f;
            player.wallBoostTimer = 0f;
            player.varJumpSpeed = (player.Speed.Y = NewSpeed);
            player.launched = false;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            player.Sprite.Scale = new Vector2(0.6f, 1.4f);
            player.Collider = collider;

            if (SetSpeedXToZero) player.Speed.X = 0f;

            (Scene as Level)?.ParticlesFG.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
            if (!string.IsNullOrEmpty(sfx))
                SoundEmitter.Play(sfx);
        }
    }
}