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

        public ParticleEmitter? shimmerParticles;

        public float wobble;

        public bool wobbleActive;

        public Tween? tween;

        public Alarm? alarm;

        public Vector2[] nodes;
        public bool BubbleReturn;
        public float BubbleReturnDelay;

        public bool Turning;

        public Color color;

        public bool Temporary;
        public bool Persistent;

        public int Group;
        public bool OpenAny;
        public string GetSfx;

        public string SpriteName;
        public Color ParticleColor1;
        public Color ParticleColor2;

        public CustomKey(EntityData data, Vector2 offset, EntityID id) : this(data.Position, offset, id,
            data.Int("group"), data.Bool("openAny"),
            data.Bool("bubbleReturn"), data.Float("bubbleReturnDelay", 0.3f), 
            data.Attr("getSfx", "event:/game/general/key_get"),  data.Bool("temporary"),
            data.Attr("sprite","objects/DzhakeHelper/customKey/"), data.HexColorWithAlpha("color"),
            data.HexColorWithAlpha("particleColor1", Calc.HexToColorWithAlpha("e2d926")),
            data.HexColorWithAlpha("particleColor2", Calc.HexToColorWithAlpha("fffeef")), data.NodesOffset(offset),
            false, data.Bool("persistent"))
        {}

        public CustomKey(CustomKeyInfo info) : this(info.position, new(), info.ID, info.Group,
            info.OpenAny, false, 0.3f, "", false,
            info.SpriteName, info.color, info.ParticleColor1, info.ParticleColor2, null, true, info.Persistent)
        {}

        public CustomKey(Vector2 position, Vector2 offset, EntityID id, int group, bool openAny, bool bubbleReturn,
            float bubbleReturnDelay, string getSfx, bool temporary, string spriteName, Color color, Color particleColor1,
            Color particleColor2, Vector2[] nodes, bool collected, bool persistent)
            : base(position + offset)
        {
            Depth = -1000000;
            Collidable = !collected;
            Group = group;
            OpenAny = openAny;
            BubbleReturn = bubbleReturn;
            BubbleReturnDelay = bubbleReturnDelay;
            GetSfx = getSfx;
            Temporary = temporary;
            this.nodes = nodes;
            this.color = color;
            ID = id;
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Add(follower = new Follower(id));
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            SpriteName = spriteName;
            Add(sprite = new Sprite(GFX.Game, SpriteName));
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

            ParticleColor1 = particleColor1;
            ParticleColor2 = particleColor2;

            P_Shimmer = new ParticleType
            {
                Color = ParticleColor1,
                Color2 = ParticleColor2,
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
                SpeedMultiplier = 0.05f,
                Color = ParticleColor1,
                Color2 = ParticleColor2,
            };

            if (!IsUsed && !Collidable && Util.TryGetPlayer(out Player player) && !player.Leader.Followers.Contains(follower) && !player.Dead)
            {
                player.Leader.GainFollower(follower);
            }
        }

        
        public override void Added(Scene scene)
        {
            if (Persistent && DHSaveData.UsedKeysIDs.Contains(ID.ID))
                RemoveSelf();
            base.Added(scene);
            ParticleSystem particlesFG = (scene as Level).ParticlesFG;
            Add(shimmerParticles = new ParticleEmitter(particlesFG, P_Shimmer, Vector2.Zero, new Vector2(6f, 6f),1, 0.1f));
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
            if (nodes != null && nodes.Length >= 2)
            {
                Add(new Coroutine(NodeRoutine(player)));
            }

            if (!Temporary)
            {
                session.DoNotLoad.Add(ID);
                DzhakeHelperModule.Session.CurrentKeys.Add(new(this));
            }
        }

        public IEnumerator NodeRoutine(Player player)
        {
            yield return BubbleReturnDelay;
            if (player.Dead || !BubbleReturn) yield break;
            Audio.Play("event:/game/general/cassette_bubblereturn", SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
            player.StartCassetteFly(nodes[1], nodes[0]);
        }

        
        public void RegisterUsed()
        {
            IsUsed = true;
            follower.Leader?.LoseFollower(follower);
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
                    if (Temporary) (Scene as Level).Session.DoNotLoad.Add(ID);
                    if (Persistent) DHSaveData.UsedKeysIDs.Add(ID.ID);
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

        public struct CustomKeyInfo(CustomKey key)
        {
            public EntityID ID = key.ID;
            public Color color = key.color;
            public int Group = key.Group;
            public bool Persistent = key.Persistent;
            public bool OpenAny = key.OpenAny;
            public string SpriteName = key.SpriteName;
            public Color ParticleColor1 = key.ParticleColor1;
            public Color ParticleColor2 = key.ParticleColor2;
            public Vector2 position = key.Position;
        }
    }
}
