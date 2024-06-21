using System;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity($"{nameof(DzhakeHelper)}/{nameof(CustomKey)}")]
    public class CustomKey : Entity
    {

        public ParticleType P_Shimmer;
        public ParticleType P_Insert;

        public EntityID ID;

        public bool IsUsed;

        public bool StartedUsing;

        public Follower follower;

        public Sprite sprite;

        public Wiggler wiggler;

        public VertexLight light;

        public ParticleEmitter shimmerParticles;

        public float wobble;

        public bool wobbleActive;

        public Tween tween;

        public Alarm alarm;

        public Vector2[] nodes;
        public bool BubbleReturn;
        public float BubbleReturnDelay;

        public bool Turning;

        public Color color;

        public bool Temporary;

        public int Group;
        public bool OpenAny;
        public string GetSfx;

        public CustomKey(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            Group = data.Int("group");
            OpenAny = data.Bool("openAny");
            BubbleReturn = data.Bool("bubbleReturn");
            BubbleReturnDelay = data.Float("bubbleReturnDelay", 0.3f);
            GetSfx = data.Attr("getSfx", "event:/game/general/key_get");
            Temporary = data.Bool("temporary");
            nodes = data.NodesOffset(offset);
            color = data.HexColorWithAlpha("color");
            ID = id;
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Add(follower = new Follower(id));
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            string spriteName = data.Attr("sprite","objects/DzhakeHelper/customKey/");
            Add(sprite = new Sprite(GFX.Game,spriteName));
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.Add("enter", "enter", 0.1f);
            sprite.CenterOrigin();
            sprite.Play("idle");
            sprite.Color = color;
            Add(new TransitionListener
            {
                OnOut =  (float f) =>
                {
                    StartedUsing = false;
                    if (!IsUsed)
                    {
                        if (tween != null)
                        {
                            tween.RemoveSelf();
                            tween = null;
                        }

                        if (alarm != null)
                        {
                            alarm.RemoveSelf();
                            alarm = null;
                        }

                        Turning = false;
                        Visible = true;
                        sprite.Visible = true;
                        sprite.Rate = 1f;
                        sprite.Scale = Vector2.One;
                        sprite.Play("idle");
                        sprite.Rotation = 0f;
                        wiggler.Stop();
                        follower.MoveTowardsLeader = true;
                    }
                }
            });
            Add(wiggler = Wiggler.Create(0.4f, 4f,  (float v) =>
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(light = new VertexLight(Color.White, 1f, 32, 48));

            P_Shimmer = new ParticleType
            {
                Color = data.HexColorWithAlpha("particleColor1",Calc.HexToColorWithAlpha("e2d926")),
                Color2 = data.HexColorWithAlpha("particleColor2",Calc.HexToColorWithAlpha("fffeef")),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.5f,
                LifeMax = 0.8f,
                Size = 1f,
                SpeedMin = 1f,
                SpeedMax = 2f,
                DirectionRange = MathF.PI * 2f
            };

            P_Insert = new ParticleType(Key.P_Shimmer)
            {
                SpeedMin = 40f,
                SpeedMax = 60f,
                SpeedMultiplier = 0.05f
            };
        }

        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            ParticleSystem particlesFG = (scene as Level).ParticlesFG;
            Add(shimmerParticles = new ParticleEmitter(particlesFG, P_Shimmer, Vector2.Zero, new Vector2(6f, 6f), 1, 0.1f));
            shimmerParticles.SimulateCycle();
        }

        
        public override void Update()
        {
            if (wobbleActive)
            {
                wobble += Engine.DeltaTime * 4f;
                sprite.Y = (float)Math.Sin(wobble);
            }

            if (!IsUsed && !Collidable && Util.TryGetPlayer(out Player player) && !player.Leader.Followers.Contains(follower) && !player.Dead)
            {
                player.Leader.GainFollower(follower);
            }

            base.Update();
        }

        
        
        public void OnPlayer(Player player)
        {
            SceneAs<Level>().Particles.Emit(P_Insert, 10, Position, Vector2.One * 3f);
            if (!string.IsNullOrEmpty(GetSfx))
                Audio.Play(GetSfx, Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            player.Leader.GainFollower(follower);
            Collidable = false;
            Session session = SceneAs<Level>().Session;
            session.UpdateLevelStartDashes();
            wiggler.Start();
            base.Depth = -1000000;
            if (nodes != null && nodes.Length >= 2)
            {
                Add(new Coroutine(NodeRoutine(player)));
            }

            if (!Temporary)
            {
                DzhakeHelperModule.Session.CurrentKeys.Add(this);
                session.DoNotLoad.Add(ID);
            }
        }

        public IEnumerator NodeRoutine(Player player)
        {
            yield return BubbleReturnDelay;
            if (!player.Dead && BubbleReturn)
            {
                Audio.Play("event:/game/general/cassette_bubblereturn", SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
                player.StartCassetteFly(nodes[1], nodes[0]);
            }
        }

        
        public void RegisterUsed()
        {
            IsUsed = true;
            if (follower.Leader != null)
            {
                follower.Leader.LoseFollower(follower);
            }
        }

        
        public IEnumerator UseRoutine(Vector2 target)
        {
            Turning = true;
            follower.MoveTowardsLeader = false;
            wiggler.Start();
            wobbleActive = false;
            sprite.Y = 0f;
            Vector2 position = Position;
            SimpleCurve curve = new SimpleCurve(position, target, (target + position) / 2f + new Vector2(0f, -48f));
            tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 1f, start: true);
            tween.OnUpdate =  (Tween t) =>
            {
                Position = curve.GetPoint(t.Eased);
                sprite.Rate = 1f + t.Eased * 2f;
            };
            Add(tween);
            yield return tween.Wait();
            tween = null;
            while (sprite.CurrentAnimationFrame != 4)
            {
                yield return null;
            }

            shimmerParticles.Active = false;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            for (int i = 0; i < 16; i++)
            {
                SceneAs<Level>().ParticlesFG.Emit(P_Insert, Center, MathF.PI / 8f * (float)i);
            }

            sprite.Play("enter");
            yield return 0.3f;
            tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.3f, start: true);
            tween.OnUpdate =  (Tween t) =>
            {
                sprite.Rotation = t.Eased * (MathF.PI / 2f);
            };
            Add(tween);
            yield return tween.Wait();
            tween = null;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            alarm = Alarm.Set(this, 1f,  () =>
            {
                alarm = null;
                tween = Tween.Create(Tween.TweenMode.Oneshot, null, 1f, start: true);
                tween.OnUpdate =  (Tween t) =>
                {
                    light.Alpha = 1f - t.Eased;
                };
                tween.OnComplete = delegate
                {
                    DzhakeHelperModule.Session.CurrentKeys.Remove(this);
                    RemoveSelf();
                };
                Add(tween);
            });
            yield return 0.2f;
            for (int j = 0; j < 8; j++)
            {
                SceneAs<Level>().ParticlesFG.Emit(P_Insert, Center, MathF.PI / 4f * (float)j);
            }

            sprite.Visible = false;
            Turning = false;
        }
    }
}
