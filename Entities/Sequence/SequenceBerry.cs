using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace Celeste.Mod.DzhakeHelper.Entities;

[CustomEntity(nameof(DzhakeHelper) + "/" + nameof(SequenceBerry))]
[RegisterStrawberry(true, true)]
public class SequenceBerry : Entity, IStrawberry
{

    public EntityID ID;
     
    public Sprite sprite;

    public Follower Follower;

    public Wiggler wiggler;

    public Wiggler rotateWiggler;

    public BloomPoint bloom;

    public VertexLight light;

    public Tween lightTween;

    public float wobble;
     
    public Vector2 start;

    public float collectTimer;

    public bool collected;

    public bool isGhostBerry;

    public bool flyingAway;

    public float flapSpeed;

    public bool ReturnHomeWhenLost = true;

    public bool Winged;

    public ManualSequenceComponent sequenceComponent;

    public bool OnlyGrabIfActive;
    public bool OnlyCollectIfActive;
    public bool OnlyFlyAwayIfActive;



    public SequenceBerry(EntityData data, Vector2 offset, EntityID gid)
    {
        Add(sequenceComponent = new ManualSequenceComponent(data, this));
        ID = gid;
        Position = (start = data.Position + offset);
        Winged = data.Bool("winged");
        isGhostBerry = SaveData.Instance.CheckStrawberry(ID);
        base.Depth = -100;
        base.Collider = new Hitbox(14f, 14f, -7f, -7f);
        Add(new PlayerCollider(OnPlayer));
        Add(new MirrorReflection());
        Add(Follower = new Follower(ID, null, OnLoseLeader));
        Follower.FollowDelay = 0.3f;
        Add(new DashListener
        {
            OnDash = OnDash
        });

        OnlyGrabIfActive = data.Bool("onlyGrabIfActive");
        OnlyCollectIfActive = data.Bool("onlyCollectIfActive");
        OnlyFlyAwayIfActive = data.Bool("onlyFlyAwayIfActive");
    }

     
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (SaveData.Instance.CheckStrawberry(ID))
        {
            sprite = GFX.SpriteBank.Create("ghostberry");

            sprite.Color.A *= (byte)0.8f;
        }
        else
        {
            sprite = GFX.SpriteBank.Create("strawberry");
        }

        Add(sprite);
        if (Winged)
        {
            sprite.Play("flap");
        }

        sprite.OnFrameChange = OnAnimate;
        Add(wiggler = Wiggler.Create(0.4f, 4f,   (float v) =>
        {
            sprite.Scale = Vector2.One * (1f + v * 0.35f);
        }));
        Add(rotateWiggler = Wiggler.Create(0.5f, 4f,   (float v) =>
        {
            sprite.Rotation = v * 30f * (MathF.PI / 180f);
        }));
        Add(bloom = new BloomPoint(isGhostBerry ? 0.5f : 1f, 12f));
        Add(light = new VertexLight(Color.White, 1f, 16, 24));
        Add(lightTween = light.CreatePulseTween());

        if ((scene as Level).Session.BloomBaseAdd > 0.1f)
        {
            bloom.Alpha *= 0.5f;
        }
    }

     
     
    public void OnDash(Vector2 dir)
    {
        if (!flyingAway && Winged && (!OnlyFlyAwayIfActive || sequenceComponent.Activated))
        {
            base.Depth = -1000000;
            Add(new Coroutine(FlyAwayRoutine()));
            flyingAway = true;
        }
    }

     
     
    public void OnAnimate(string id)
    {
        if (!flyingAway && id == "flap" && sprite.CurrentAnimationFrame % 9 == 4)
        {
            Audio.Play("event:/game/general/strawberry_wingflap", Position);
            flapSpeed = -50f;
        }

        int num = (id == "flap") ? 25 : 30;
        if (sprite.CurrentAnimationFrame == num)
        {
            lightTween.Start();
            if (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>()))
            {
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
            }
            else
            {
                Audio.Play("event:/game/general/strawberry_pulse", Position);
                SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
            }
        }
    }

     
    public void OnPlayer(Player player)
    {
        if (OnlyGrabIfActive && !sequenceComponent.Activated)
        {
            return;
        }
        if (Follower.Leader != null || collected)
        {
            return;
        }

        ReturnHomeWhenLost = true;
        if (Winged)
        {
            Level level = SceneAs<Level>();
            Winged = false;
            sprite.Rate = 0f;
            Alarm.Set(this, Follower.FollowDelay,   () =>
            {
                sprite.Rate = 1f;
                sprite.Play("idle");
                level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position + new Vector2(8f, 0f), new Vector2(4f, 2f));
                level.Particles.Emit(Strawberry.P_WingsBurst, 8, Position - new Vector2(8f, 0f), new Vector2(4f, 2f));
            });
        }

        Audio.Play(isGhostBerry ? "event:/game/general/strawberry_blue_touch" : "event:/game/general/strawberry_touch", Position);
        player.Leader.GainFollower(Follower);
        wiggler.Start();
        base.Depth = -1000000;
    }

     

     
    public IEnumerator FlyAwayRoutine()
    {
        rotateWiggler.Start();
        flapSpeed = -200f;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.25f, start: true);
        tween.OnUpdate =   (Tween t) =>
        {
            flapSpeed = MathHelper.Lerp(-200f, 0f, t.Eased);
        };
        Add(tween);
        yield return 0.1f;
        Audio.Play("event:/game/general/strawberry_laugh", Position);
        yield return 0.2f;
        if (!Follower.HasLeader)
        {
            Audio.Play("event:/game/general/strawberry_flyaway", Position);
        }

        tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
        tween.OnUpdate =   (Tween t) =>
        {
            flapSpeed = MathHelper.Lerp(0f, -200f, t.Eased);
        };
        Add(tween);
    }

     
     
    public IEnumerator CollectRoutine(int collectIndex)
    {
        _ = Scene;
        Tag = Tags.TransitionUpdate;
        Depth = -2000010;
        int num = 0;
        if (isGhostBerry)
        {
            num = 1;
        }

        Audio.Play("event:/game/general/strawberry_get", Position, "colour", num, "count", collectIndex);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        sprite.Play("collect");
        while (sprite.Animating)
        {
            yield return null;
        }

        Scene.Add(new StrawberryPoints(Position, isGhostBerry, collectIndex, false));
        RemoveSelf();
    }

     
     
    public void OnLoseLeader()
    {
        if (collected || !ReturnHomeWhenLost)
        {
            return;
        }

        Alarm.Set(this, 0.15f,   () =>
        {
            Vector2 vector = (start - Position).SafeNormalize();
            float num = Vector2.Distance(Position, start);
            float num2 = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
            Vector2 control = start + vector * 16f + vector.Perpendicular() * num2 * Calc.Random.Choose(1, -1);
            SimpleCurve curve = new SimpleCurve(Position, start, control);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), start: true);
            tween.OnUpdate =   (Tween f) =>
            {
                Position = curve.GetPoint(f.Eased);
            };
            tween.OnComplete = delegate
            {
                base.Depth = 0;
            };
            Add(tween);
        });
    }

     
    public void OnCollect()
    {
        if (!collected)
        {
            int collectIndex = 0;
            collected = true;
            if (Follower.Leader != null)
            {
                Player obj = Follower.Leader.Entity as Player;
                collectIndex = obj.StrawberryCollectIndex;
                obj.StrawberryCollectIndex++;
                obj.StrawberryCollectResetTimer = 2.5f;
                Follower.Leader.LoseFollower(Follower);
            }

            SaveData.Instance.AddStrawberry(ID, false);
            Session session = (base.Scene as Level).Session;
            session.DoNotLoad.Add(ID);
            session.Strawberries.Add(ID);
            session.UpdateLevelStartDashes();
            Add(new Coroutine(CollectRoutine(collectIndex)));
        }
    }

     
    public override void Update()
    {
        if (sequenceComponent.Activated)
        {
            sprite.Color = sequenceComponent.color;
        }
        else
        {
            sprite.Color = sequenceComponent.pressedColor;
        }

        if (!collected)
        {
            if (!Winged)
            {
                wobble += Engine.DeltaTime * 4f;
                Sprite obj = sprite;
                BloomPoint bloomPoint = bloom;
                float num2 = (light.Y = (float)Math.Sin(wobble) * 2f);
                float y = (bloomPoint.Y = num2);
                obj.Y = y;
            }

            int followIndex = Follower.FollowIndex;
            if (Follower.Leader != null && Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this))
            {
                Player player = Follower.Leader.Entity as Player;
                bool flag = false;
                if (player != null && player.Scene != null && !player.StrawberriesBlocked && player.OnSafeGround)
                {
                    flag = true;
                }

                if (flag)
                {
                    collectTimer += Engine.DeltaTime;
                    if (collectTimer > 0.15f && (!OnlyCollectIfActive || sequenceComponent.Activated))
                    {
                        OnCollect();
                    }
                }
                else
                {
                    collectTimer = Math.Min(collectTimer, 0f);
                }
            }
            else
            {
                if (followIndex > 0)
                {
                    collectTimer = -0.15f;
                }

                if (Winged)
                {
                    base.Y += flapSpeed * Engine.DeltaTime;
                    if (flyingAway)
                    {
                        if (base.Y < (float)(SceneAs<Level>().Bounds.Top - 16))
                        {
                            RemoveSelf();
                        }
                    }
                    else
                    {
                        flapSpeed = Calc.Approach(flapSpeed, 20f, 170f * Engine.DeltaTime);
                        if (base.Y < start.Y - 5f)
                        {
                            base.Y = start.Y - 5f;
                        }
                        else if (base.Y > start.Y + 5f)
                        {
                            base.Y = start.Y + 5f;
                        }
                    }
                }
            }
        }

        base.Update();
        if (Follower.Leader != null && base.Scene.OnInterval(0.08f))
        {
            ParticleType type = (isGhostBerry ? Strawberry.P_GhostGlow : Strawberry.P_Glow );
            SceneAs<Level>().ParticlesFG.Emit(type, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
        }
    }
}