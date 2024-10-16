using Celeste.Mod.Entities;
using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DzhakeHelper.Entities;

[CustomEntity("DzhakeHelper/SequenceFallingBlock")]
public class SequenceFallingBlock : SequenceBlock
{
    public bool Triggered = false;
    public float FallDelay;

    public bool HasStartedFalling { get; private set; }
    public float MaxSpeed;
    public float SpeedMultiplier;


    public SequenceFallingBlock(EntityData data, Vector2 offset, EntityID id)
        : base(data, offset, id)
    {
        FallDelay = data.Float("fallDelay",1f);
        MaxSpeed = data.Float("maxSpeed", 180f);
        SpeedMultiplier = data.Float("speedMultiplier", 0.4f);
        Add(new Coroutine(Sequence()));
    }

    public override void OnStaticMoverTrigger(StaticMover sm)
    {
        Triggered = true;
    }

    private bool PlayerWaitCheck()
    {
        return Triggered
            ? true
            : HasPlayerRider() ? true : CollideCheck<Player>(Position - Vector2.UnitX) || CollideCheck<Player>(Position + Vector2.UnitX);
    }

    private IEnumerator Sequence()
    {
        while ((!Triggered && !(HasPlayerClimbing() || HasPlayerOnTop() || HasPlayerRider())) || !Activated)
        {
            yield return null;
        }

        foreach (SequenceBlock block in group)
        {
            if (block is SequenceFallingBlock b)
            {
                b.Triggered = true;
            }
        }

        HasStartedFalling = true;
        while (true)
        {
            ShakeSfx();
            StartShaking();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return 0.2f;

            float timer = FallDelay;
            while (timer > 0f && PlayerWaitCheck())
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }

            StopShaking();
            for (int i = 2; i < Width; i += 4)
            {
                if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                {
                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float)Math.PI / 2f);
                }
                SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
            }
            float speed = 0f;
            while (true)
            {
                Level level = SceneAs<Level>();
                speed = Calc.Approach(speed, MaxSpeed, 500f * Engine.DeltaTime * SpeedMultiplier);
                if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                {
                    break;
                }

                if (Top > level.Bounds.Bottom + 16 || (Top > level.Bounds.Bottom - 1 && CollideCheck<Solid>(Position + new Vector2(0f, 1f))))
                {
                    Collidable = Visible = false;
                    yield return 0.2f;

                    if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f)))
                    {
                        yield return 0.2f;
                        SceneAs<Level>().Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    }

                    RemoveSelf();
                    DestroyStaticMovers();
                    yield break;
                }

                yield return null;
            }

            ImpactSfx();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
            StartShaking();
            LandParticles();
            yield return 0.2f;

            StopShaking();
            if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f)))
            {
                break;
            }

            while (CollideCheck<Platform>(Position + new Vector2(0f, 1f)))
            {
                yield return 0.1f;
            }
        }
        Safe = true;
    }

    public override void Render()
    {
        Position += Shake;
        base.Render();
        Position -= Shake;
    }

    private void LandParticles()
    {
        for (int i = 2; i <= Width; i += 4)
        {
            if (Scene.CollideCheck<Solid>(BottomLeft + new Vector2(i, 3f)))
            {
                SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + i, Bottom), Vector2.One * 4f, -(float)Math.PI / 2f);
                float direction = (!(i < Width / 2f)) ? 0f : ((float)Math.PI);
                SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + i, Bottom), Vector2.One * 4f, direction);
            }
        }
    }

    private void ShakeSfx()
    {
        Audio.Play(SFX.game_gen_fallblock_shake, Center);
    }

    private void ImpactSfx()
    {
        Audio.Play(SFX.game_gen_fallblock_impact, BottomCenter);
    }
}