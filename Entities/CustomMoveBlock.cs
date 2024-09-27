using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using Celeste.Mod.DzhakeHelper.Components;

namespace Celeste.Mod.DzhakeHelper.Entities;

[CustomEntity("DzhakeHelper/CustomMoveBlock")]
public class CustomMoveBlock : Solid
{
    public enum Directions
    {
        Left,
        Right,
        Up,
        Down
    }

     
    public enum MovementState
    {
        Idling,
        Moving,
        Breaking
    }

     
    public class Border : Entity
    {
        public CustomMoveBlock Parent;

         
        public Border(CustomMoveBlock parent)
        {
            Parent = parent;
            base.Depth = 1;
        }
         
        public override void Update()
        {
            if (Parent.Scene != base.Scene)
            {
                RemoveSelf();
            }

            base.Update();
        }
         
        public override void Render()
        {
            Draw.Rect(Parent.X + Parent.Shake.X - 1f, Parent.Y + Parent.Shake.Y - 1f, Parent.Width + 2f, Parent.Height + 2f, Color.Black);
        }
    }

    [Pooled]
     
    public class Debris : Actor
    {
        public Image sprite;

        public Vector2 home;
        public Vector2 speed;

        public bool shaking;
        public bool returning;

        public float returnEase;
        public float returnDuration;

        public SimpleCurve returnCurve;

        public bool firstHit;

        public float alpha;

        public Collision onCollideH;
        public Collision onCollideV;

        public float spin;
         
        public Debris()
            : base(Vector2.Zero)
        {
            base.Tag = Tags.TransitionUpdate;
            base.Collider = new Hitbox(4f, 4f, -2f, -2f);
            onCollideH =   (CollisionData c) =>
            {
                speed.X = (0f - speed.X) * 0.5f;
            };
            onCollideV =   (CollisionData c) =>
            {
                if (firstHit || speed.Y > 50f)
                {
                    Audio.Play("event:/game/general/debris_stone", Position, "debris_velocity", Calc.ClampedMap(speed.Y, 0f, 600f));
                }

                if (speed.Y > 0f && speed.Y < 40f)
                {
                    speed.Y = 0f;
                }
                else
                {
                    speed.Y = (0f - speed.Y) * 0.25f;
                }

                firstHit = false;
            };
        }

         
        public override void OnSquish(CollisionData data)
        {
        }

         
        public Debris Init(Vector2 position, Vector2 center, Vector2 returnTo, string texture)
        {
            Remove(sprite);
            Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures($"{texture}debris"))));
            sprite.CenterOrigin();
            sprite.FlipX = Calc.Random.Chance(0.5f);

            Collidable = true;
            Position = position;
            speed = (position - center).SafeNormalize(60f + Calc.Random.NextFloat(60f));
            home = returnTo;
            sprite.Position = Vector2.Zero;
            sprite.Rotation = Calc.Random.NextAngle();
            returning = false;
            shaking = false;
            sprite.Scale.X = 1f;
            sprite.Scale.Y = 1f;
            sprite.Color = Color.White;
            alpha = 1f;
            firstHit = false;
            spin = Calc.Random.Range(3.49065852f, 10.4719753f) * (float)Calc.Random.Choose(1, -1);
            return this;
        }

         
        public override void Update()
        {
            base.Update();

            if (!returning)
            {
                if (Collidable)
                {
                    speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 100f);
                    if (!OnGround())
                    {
                        speed.Y += 400f * Engine.DeltaTime;
                    }

                    MoveH(speed.X * Engine.DeltaTime, onCollideH);
                    MoveV(speed.Y * Engine.DeltaTime, onCollideV);
                }

                if (shaking && base.Scene.OnInterval(0.05f))
                {
                    sprite.X = -1 + Calc.Random.Next(3);
                    sprite.Y = -1 + Calc.Random.Next(3);
                }
            }
            else
            {
                Position = returnCurve.GetPoint(Ease.CubeOut(returnEase));
                returnEase = Calc.Approach(returnEase, 1f, Engine.DeltaTime / returnDuration);
                sprite.Scale = Vector2.One * (1f + returnEase * 0.5f);
            }

            if ((base.Scene as Level).Transitioning)
            {
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 4f);
                sprite.Color = Color.White * alpha;
            }

            sprite.Rotation += spin * Calc.ClampedMap(Math.Abs(speed.Y), 50f, 150f) * Engine.DeltaTime;
        }

        public void StopMoving()
        {
            Collidable = false;
        }

        public void StartShaking()
        {
            shaking = true;
        }

         
        public void ReturnHome(float duration)
        {
            if (base.Scene != null)
            {
                Camera camera = (base.Scene as Level).Camera;
                if (base.X < camera.X)
                {
                    base.X = camera.X - 8f;
                }

                if (base.Y < camera.Y)
                {
                    base.Y = camera.Y - 8f;
                }

                if (base.X > camera.X + 320f)
                {
                    base.X = camera.X + 320f + 8f;
                }

                if (base.Y > camera.Y + 180f)
                {
                    base.Y = camera.Y + 180f + 8f;
                }
            }

            returning = true;
            returnEase = 0f;
            returnDuration = duration;
            Vector2 vector = (home - Position).SafeNormalize();
            Vector2 control = (Position + home) / 2f + new Vector2(vector.Y, 0f - vector.X) * (Calc.Random.NextFloat(16f) + 16f) * Calc.Random.Facing();
            returnCurve = new SimpleCurve(Position, home, control);
        }
    }

    public static ParticleType P_Activate = MoveBlock.P_Activate;
    public static ParticleType P_Break = MoveBlock.P_Break;
    public static ParticleType P_Move = MoveBlock.P_Move;
     
    public float Accel;
    public float MoveSpeed;
    public const float SteerSpeed = MathF.PI * 16f;
    public const float MaxAngle = MathF.PI / 4f;
    public const float NoSteerTime = 0.2f;
    public float CrashTime;
    public float CrashResetTime;
    public float RegenTime;
    public bool canSteer;
     
    public Directions direction;

    public float homeAngle;

    public int angleSteerSign;

    public Vector2 startPosition;

    public MovementState state;
     
    public bool leftPressed;
    public bool rightPressed;
    public bool topPressed;
     
    public float speed;
    public float targetSpeed;
    public float angle;
    public float targetAngle;
     
    public Player noSquish;
     
    public List<Image> body = new();
    public List<Image> topButton = new();
    public List<Image> leftButton = new();
    public List<Image> rightButton = new();
    public List<MTexture> arrows;

    public Border border;

    public bool dontBreak;

    public Color fillColor;
    public float flash;
    public SoundSource moveSfx;
     
    public bool triggered;
     
    public Color idleBgFill = Calc.HexToColor("474070");
    public Color pressedBgFill = Calc.HexToColor("30b335");
    public Color breakingBgFill = Calc.HexToColor("cc2541");
     
    public float particleRemainder;

    public string Texture;

     
    public CustomMoveBlock(Vector2 position, int width, int height, Directions direction, bool canSteer, string texture, float acceleration, float moveSpeed, float crashTime, float crashResetTime, float regenTime,
        Color idleBgFill, Color pressedBgFill, Color breakingBgFill, bool bottomButton, bool dontBreak)
        : base(position, width, height, safe: false)
    {
        this.dontBreak = dontBreak;
        this.idleBgFill = idleBgFill;
        this.pressedBgFill = pressedBgFill;
        this.breakingBgFill = breakingBgFill;

        fillColor = this.idleBgFill;

        Texture = texture;

        Accel = acceleration;
        MoveSpeed = moveSpeed;
        CrashTime = crashTime;
        CrashResetTime = crashResetTime;
        RegenTime = regenTime;

        base.Depth = -1;
        startPosition = position;
        this.canSteer = canSteer;
        this.direction = direction;
        switch (direction)
        {
            default:
                homeAngle = (targetAngle = (angle = 0f));
                angleSteerSign = 1;
                break;
            case Directions.Left:
                homeAngle = (targetAngle = (angle = MathF.PI));
                angleSteerSign = -1;
                break;
            case Directions.Up:
                homeAngle = (targetAngle = (angle = -MathF.PI / 2f));
                angleSteerSign = 1;
                break;
            case Directions.Down:
                homeAngle = (targetAngle = (angle = MathF.PI / 2f));
                angleSteerSign = -1;
                break;
        }

        int num = width / 8;
        int num2 = height / 8;
        MTexture baseTexture = GetTexture("base");
        MTexture buttonTexture = GetTexture("button");
        switch (canSteer)
        {
            case true when direction is Directions.Left or Directions.Right:
            {
                for (int i = 0; i < num; i++)
                {
                    int num3 = ((i != 0) ? ((i < num - 1) ? 1 : 2) : 0);
                    AddImage(buttonTexture.GetSubtexture(num3 * 8, 0, 8, 8),
                        new Vector2(i * 8, -4f), 0f, new Vector2(1f, 1f), topButton);
                }

                baseTexture = GetTexture("base_h");
                break;
            }
            case true when (direction == Directions.Up || direction == Directions.Down):
            {
                for (int j = 0; j < num2; j++)
                {
                    int num4 = ((j != 0) ? ((j < num2 - 1) ? 1 : 2) : 0);
                    AddImage(buttonTexture.GetSubtexture(num4 * 8, 0, 8, 8),
                        new Vector2(-4f, j * 8), MathF.PI / 2f, new Vector2(1f, -1f), leftButton);
                    AddImage(buttonTexture.GetSubtexture(num4 * 8, 0, 8, 8),
                        new Vector2((num - 1) * 8 + 4, j * 8), MathF.PI / 2f, new Vector2(1f, 1f), rightButton);
                }

                baseTexture = GetTexture("base_v");
                break;
            }
        }

        for (int k = 0; k < num; k++)
        {
            for (int l = 0; l < num2; l++)
            {
                int num5 = ((k != 0) ? ((k < num - 1) ? 1 : 2) : 0);
                int num6 = ((l != 0) ? ((l < num2 - 1) ? 1 : 2) : 0);
                AddImage(baseTexture.GetSubtexture(num5 * 8, num6 * 8, 8, 8),
                    new Vector2(k, l) * 8f, 0f, new Vector2(1f, 1f), body);
            }
        }

        arrows = GFX.Game.GetAtlasSubtextures($"{texture}arrow");
        Add(moveSfx = new SoundSource());
        Add(new Coroutine(Controller()));
        UpdateColors();
        Add(new LightOcclude(0.5f));
        if (this.canSteer && bottomButton)
        {
            Add(new MoveBlockBottomComponent());
        }
    }

     
    public CustomMoveBlock(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Enum("direction", Directions.Left),
            data.Bool("canSteer", true), data.Attr("texture", "objects/moveBlock/"), data.Float("acceleration", 300f),
            data.Float("moveSpeed", 60f), data.Float("crashTime", 0.15f), data.Float("crashResetTime", 0.1f),
            data.Float("regenTime",3f), data.HexColorWithAlpha("idleFillColor", Calc.HexToColor("474070")),
            data.HexColorWithAlpha("pressedFillColor", Calc.HexToColor("30b335")),
            data.HexColorWithAlpha("breakingFillColor", Calc.HexToColor("cc2541")),
              data.Bool("bottomButton"), data.Bool("dontBreak"))
    {
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        scene.Add(border = new Border(this));
    }

     
    public IEnumerator Controller()
    {
        while (true)
        {
            triggered = false;
            state = MovementState.Idling;
            while (!triggered && !HasPlayerRider())
            {
                yield return null;
            }

            Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
            state = MovementState.Moving;
            StartShaking(0.2f);
            ActivateParticles();
            yield return 0.2f;
            targetSpeed = MoveSpeed;
            moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
            moveSfx.Param("arrow_stop", 0f);
            StopPlayerRunIntoAnimation = false;
            float crashTimer = CrashTime;
            float crashResetTimer = CrashResetTime;
            float noSteerTimer = NoSteerTime;
            while (true)
            {
                if (canSteer)
                {
                    targetAngle = homeAngle;
                    bool triggeredByPlayer = direction is not (Directions.Right or Directions.Left) ? HasPlayerClimbing() : HasPlayerOnTop();
                    if (triggeredByPlayer && noSteerTimer > 0f)
                    {
                        noSteerTimer -= Engine.DeltaTime;
                    }

                    if (triggeredByPlayer)
                    {
                        if (noSteerTimer <= 0f)
                        {
                            if (direction == Directions.Right || direction == Directions.Left)
                            {
                                targetAngle = homeAngle + MathF.PI / 4f * (float)angleSteerSign * (float)Input.MoveY.Value;
                            }
                            else
                            {
                                targetAngle = homeAngle + MathF.PI / 4f * (float)angleSteerSign * (float)Input.MoveX.Value;
                            }
                        }
                    }
                    else
                    {
                        noSteerTimer = 0.2f;
                    }
                }

                if (Scene.OnInterval(0.02f))
                {
                    MoveParticles();
                }

                speed = Calc.Approach(speed, targetSpeed, 300f * Engine.DeltaTime);
                angle = Calc.Approach(angle, targetAngle, MathF.PI * 16f * Engine.DeltaTime);
                Vector2 vector = Calc.AngleToVector(angle, speed);
                Vector2 vec = vector * Engine.DeltaTime;
                bool shouldBreak;
                if (direction is Directions.Right or Directions.Left)
                {
                    shouldBreak = MoveCheck(vec.XComp());
                    noSquish = Scene.Tracker.GetEntity<Player>();
                    MoveVCollideSolids(vec.Y, thruDashBlocks: false);
                    noSquish = null;
                    LiftSpeed = vector;
                    if (Scene.OnInterval(0.03f))
                    {
                        if (vec.Y > 0f)
                        {
                            ScrapeParticles(Vector2.UnitY);
                        }
                        else if (vec.Y < 0f)
                        {
                            ScrapeParticles(-Vector2.UnitY);
                        }
                    }
                }
                else
                {
                    shouldBreak = MoveCheck(vec.YComp());
                    noSquish = Scene.Tracker.GetEntity<Player>();
                    MoveHCollideSolids(vec.X, thruDashBlocks: false);
                    noSquish = null;
                    LiftSpeed = vector;
                    if (Scene.OnInterval(0.03f))
                    {
                        if (vec.X > 0f)
                        {
                            ScrapeParticles(Vector2.UnitX);
                        }
                        else if (vec.X < 0f)
                        {
                            ScrapeParticles(-Vector2.UnitX);
                        }
                    }

                    if (direction == Directions.Down && Top > SceneAs<Level>().Bounds.Bottom + 32)
                    {
                        shouldBreak = true;
                    }
                }

                if (shouldBreak)
                {
                    moveSfx.Param("arrow_stop", 1f);
                    crashResetTimer = 0.1f;
                    if (!(crashTimer > 0f))
                    {
                        if (!dontBreak) break;
                    }

                    crashTimer -= Engine.DeltaTime;
                }
                else
                {
                    moveSfx.Param("arrow_stop", 0f);
                    if (crashResetTimer > 0f)
                    {
                        crashResetTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        crashTimer = 0.15f;
                    }
                }

                Level level = Scene as Level;
                if (Left < level.Bounds.Left || Top < level.Bounds.Top || Right > level.Bounds.Right)
                {
                    if (!dontBreak) break;
                }

                yield return null;
            }

            Audio.Play("event:/game/04_cliffside/arrowblock_break", Position);
            moveSfx.Stop();
            state = MovementState.Breaking;
            speed = targetSpeed = 0f;
            angle = targetAngle = homeAngle;
            StartShaking(0.2f);
            StopPlayerRunIntoAnimation = true;
            yield return 0.2f;
            BreakParticles();
            List<Debris> debris = new List<Debris>();
            for (int i = 0; i < Width; i += 8)
            {
                for (int j = 0; j < Height; j += 8)
                {
                    Vector2 vector2 = new Vector2(i + 4f, j + 4f);
                    Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + vector2, Center, startPosition + vector2, Texture);
                    debris.Add(debris2);
                    Scene.Add(debris2);
                }
            }

            Vector2 amount = startPosition - Position;
            DisableStaticMovers();
            MoveStaticMovers(amount);
            Position = startPosition;
            bool visible = false;
            Collidable = false;
            Visible = visible;
            yield return 2.2f;
            foreach (Debris item in debris)
            {
                item.StopMoving();
            }

            while (CollideCheck<Actor>() || CollideCheck<Solid>())
            {
                yield return null;
            }

            Collidable = true;
            EventInstance instance = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
            Coroutine component;
            Coroutine routine = (component = new Coroutine(SoundFollowsDebrisCenter(instance, debris)));
            Add(component);
            foreach (Debris item2 in debris)
            {
                item2.StartShaking();
            }

            yield return 0.2f;
            foreach (Debris item3 in debris)
            {
                item3.ReturnHome(0.65f);
            }

            yield return 0.6f;
            routine.RemoveSelf();
            foreach (Debris item4 in debris)
            {
                item4.RemoveSelf();
            }

            Audio.Play("event:/game/04_cliffside/arrowblock_reappear", Position);
            Visible = true;
            EnableStaticMovers();
            speed = (targetSpeed = 0f);
            angle = (targetAngle = homeAngle);
            noSquish = null;
            fillColor = idleBgFill;
            UpdateColors();
            flash = 1f;
        }
    }


    public IEnumerator SoundFollowsDebrisCenter(EventInstance instance, List<Debris> debris)
    {
        while (true)
        {
            instance.getPlaybackState(out var pLAYBACK_STATE);
            if (pLAYBACK_STATE == PLAYBACK_STATE.STOPPED)
            {
                break;
            }

            Vector2 zero = Vector2.Zero;
            foreach (Debris debri in debris)
            {
                zero += debri.Position;
            }

            zero /= (float)debris.Count;
            Audio.Position(instance, zero);
            yield return null;
        }
    }

    public override void Update()
    {
        base.Update();
        Level level = Scene as Level;
        if (canSteer)
        {
            bool flag = (direction == Directions.Up || direction == Directions.Down) && CollideCheck<Player>(Position + new Vector2(-1f, 0f));
            bool flag2 = (direction == Directions.Up || direction == Directions.Down) && CollideCheck<Player>(Position + new Vector2(1f, 0f));
            bool flag3 = (direction == Directions.Left || direction == Directions.Right) && CollideCheck<Player>(Position + new Vector2(0f, -1f));
            foreach (Image item in topButton)
            {
                item.Y = (flag3 ? 2 : 0);
            }

            foreach (Image item2 in leftButton)
            {
                item2.X = (flag ? 2 : 0);
            }

            foreach (Image item3 in rightButton)
            {
                item3.X = base.Width + (float)(flag2 ? (-2) : 0);
            }

            if ((flag && !leftPressed) || (flag3 && !topPressed) || (flag2 && !rightPressed))
            {
                Audio.Play("event:/game/04_cliffside/arrowblock_side_depress", Position);
            }

            if ((!flag && leftPressed) || (!flag3 && topPressed) || (!flag2 && rightPressed))
            {
                Audio.Play("event:/game/04_cliffside/arrowblock_side_release", Position);
            }

            leftPressed = flag;
            rightPressed = flag2;
            topPressed = flag3;
        }

        if (moveSfx != null && moveSfx.Playing)
        {
            int num = (int)Math.Floor((0f - (Calc.AngleToVector(angle, 1f) * new Vector2(-1f, 1f)).Angle() + MathF.PI * 2f) % (MathF.PI * 2f) / (MathF.PI * 2f) * 8f + 0.5f);
            moveSfx.Param("arrow_influence", num + 1);
        }

        //"Optmimization"
        if (dontBreak && (Top > level.Bounds.Bottom || Bottom < level.Bounds.Top || Left > level.Bounds.Right || Right < level.Bounds.Left))
            RemoveSelf();
        

        border.Visible = Visible;
        flash = Calc.Approach(flash, 0f, Engine.DeltaTime * 5f);
        UpdateColors();
    }

    public override void OnStaticMoverTrigger(StaticMover sm)
    {
        triggered = true;
    }


    public override void MoveHExact(int move)
    {
        if (noSquish != null && ((move < 0 && noSquish.X < base.X) || (move > 0 && noSquish.X > base.X)))
        {
            while (move != 0 && noSquish.CollideCheck<Solid>(noSquish.Position + Vector2.UnitX * move))
            {
                move -= Math.Sign(move);
            }
        }

        base.MoveHExact(move);
    }

     
    public override void MoveVExact(int move)
    {
        if (noSquish != null && move < 0 && noSquish.Y <= base.Y)
        {
            while (move != 0 && noSquish.CollideCheck<Solid>(noSquish.Position + Vector2.UnitY * move))
            {
                move -= Math.Sign(move);
            }
        }

        base.MoveVExact(move);
    }

     
    public bool MoveCheck(Vector2 speed)
    {
        if (speed.X != 0f)
        {
            if (!MoveHCollideSolids(speed.X, thruDashBlocks: false)) return false;

            for (int i = 1; i <= 3; i++)
            {
                for (int num = 1; num >= -1; num -= 2)
                {
                    Vector2 vector = new Vector2(Math.Sign(speed.X), i * num);
                    if (!CollideCheck<Solid>(Position + vector))
                    {
                        MoveVExact(i * num);
                        MoveHExact(Math.Sign(speed.X));
                        return false;
                    }
                }
            }

            return true;

        }

        if (speed.Y != 0f)
        {
            if (!MoveVCollideSolids(speed.Y, thruDashBlocks: false)) return false;

            for (int j = 1; j <= 3; j++)
            {
                for (int num2 = 1; num2 >= -1; num2 -= 2)
                {
                    Vector2 vector2 = new Vector2(j * num2, Math.Sign(speed.Y));
                    if (!CollideCheck<Solid>(Position + vector2))
                    {
                        MoveHExact(j * num2);
                        MoveVExact(Math.Sign(speed.Y));
                        return false;
                    }
                }
            }

            return true;

        }

        return false;
    }

     
    public void UpdateColors()
    {
        Color value = idleBgFill;
        if (state == MovementState.Moving)
        {
            value = pressedBgFill;
        }
        else if (state == MovementState.Breaking)
        {
            value = breakingBgFill;
        }

        fillColor = Color.Lerp(fillColor, value, 10f * Engine.DeltaTime);
        foreach (Image item in topButton)
        {
            item.Color = fillColor;
        }

        foreach (Image item2 in leftButton)
        {
            item2.Color = fillColor;
        }

        foreach (Image item3 in rightButton)
        {
            item3.Color = fillColor;
        }
    }

     
    public void AddImage(MTexture tex, Vector2 position, float rotation, Vector2 scale, List<Image> addTo)
    {
        Image image = new Image(tex);
        image.Position = position + new Vector2(4f, 4f);
        image.CenterOrigin();
        image.Rotation = rotation;
        image.Scale = scale;
        Add(image);
        addTo?.Add(image);
    }

     
    public void SetVisible(List<Image> images, bool visible)
    {
        foreach (Image image in images)
        {
            image.Visible = visible;
        }
    }

     
    public override void Render()
    {
        Vector2 position = Position;
        Position += base.Shake;
        if (Get<MoveBlockBottomComponent>() is { } component)
        {
            component.Render();
        }
        foreach (Image item in leftButton)
        {
            item.Render();
        }

        foreach (Image item2 in rightButton)
        {
            item2.Render();
        }

        foreach (Image item3 in topButton)
        {
            item3.Render();
        }

        Draw.Rect(base.X + 3f, base.Y + 3f, base.Width - 6f, base.Height - 6f, fillColor);
        foreach (Image item4 in body)
        {
            item4.Render();
        }

        Draw.Rect(base.Center.X - 4f, base.Center.Y - 4f, 8f, 8f, fillColor);
        if (state != MovementState.Breaking)
        {
            int value = (int)Math.Floor((0f - angle + MathF.PI * 2f) % (MathF.PI * 2f) / (MathF.PI * 2f) * 8f + 0.5f);
            arrows[Calc.Clamp(value, 0, 7)].DrawCentered(base.Center);
        }
        else
        {
            GetTexture("x").DrawCentered(base.Center);
        }

        float num = flash * 4f;
        Draw.Rect(base.X - num, base.Y - num, base.Width + num * 2f, base.Height + num * 2f, Color.White * flash);
        Position = position;
    }

     
    public void ActivateParticles()
    {
        bool flag = direction == Directions.Down || direction == Directions.Up;
        bool num = (!canSteer || !flag) && !CollideCheck<Player>(Position - Vector2.UnitX);
        bool flag2 = (!canSteer || !flag) && !CollideCheck<Player>(Position + Vector2.UnitX);
        bool flag3 = (!canSteer || flag) && !CollideCheck<Player>(Position - Vector2.UnitY);
        if (num)
        {
            SceneAs<Level>().ParticlesBG.Emit(P_Activate, (int)(base.Height / 2f), base.CenterLeft, Vector2.UnitY * (base.Height - 4f) * 0.5f, MathF.PI);
        }

        if (flag2)
        {
            SceneAs<Level>().ParticlesBG.Emit(P_Activate, (int)(base.Height / 2f), base.CenterRight, Vector2.UnitY * (base.Height - 4f) * 0.5f, 0f);
        }

        if (flag3)
        {
            SceneAs<Level>().ParticlesBG.Emit(P_Activate, (int)(base.Width / 2f), base.TopCenter, Vector2.UnitX * (base.Width - 4f) * 0.5f, -MathF.PI / 2f);
        }

        SceneAs<Level>().ParticlesBG.Emit(P_Activate, (int)(base.Width / 2f), base.BottomCenter, Vector2.UnitX * (base.Width - 4f) * 0.5f, MathF.PI / 2f);
    }

     
    public void BreakParticles()
    {
        Vector2 center = base.Center;
        for (int i = 0; (float)i < base.Width; i += 4)
        {
            for (int j = 0; (float)j < base.Height; j += 4)
            {
                Vector2 vector = Position + new Vector2(2 + i, 2 + j);
                SceneAs<Level>().Particles.Emit(P_Break, 1, vector, Vector2.One * 2f, (vector - center).Angle());
            }
        }
    }

     
    public void MoveParticles()
    {
        Vector2 position;
        Vector2 positionRange;
        float num;
        float num2;
        if (direction == Directions.Right)
        {
            position = base.CenterLeft + Vector2.UnitX;
            positionRange = Vector2.UnitY * (base.Height - 4f);
            num = MathF.PI;
            num2 = base.Height / 32f;
        }
        else if (direction == Directions.Left)
        {
            position = base.CenterRight;
            positionRange = Vector2.UnitY * (base.Height - 4f);
            num = 0f;
            num2 = base.Height / 32f;
        }
        else if (direction == Directions.Down)
        {
            position = base.TopCenter + Vector2.UnitY;
            positionRange = Vector2.UnitX * (base.Width - 4f);
            num = -MathF.PI / 2f;
            num2 = base.Width / 32f;
        }
        else
        {
            position = base.BottomCenter;
            positionRange = Vector2.UnitX * (base.Width - 4f);
            num = MathF.PI / 2f;
            num2 = base.Width / 32f;
        }

        particleRemainder += num2;
        int num3 = (int)particleRemainder;
        particleRemainder -= num3;
        positionRange *= 0.5f;
        if (num3 > 0)
        {
            SceneAs<Level>().ParticlesBG.Emit(P_Move, num3, position, positionRange, num);
        }
    }

     
    public void ScrapeParticles(Vector2 dir)
    {
        _ = Collidable;
        Collidable = false;
        if (dir.X != 0f)
        {
            float x = ((!(dir.X > 0f)) ? (base.Left - 1f) : base.Right);
            for (int i = 0; (float)i < base.Height; i += 8)
            {
                Vector2 vector = new Vector2(x, base.Top + 4f + (float)i);
                if (base.Scene.CollideCheck<Solid>(vector))
                {
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, vector);
                }
            }
        }
        else
        {
            float y = ((!(dir.Y > 0f)) ? (base.Top - 1f) : base.Bottom);
            for (int j = 0; (float)j < base.Width; j += 8)
            {
                Vector2 vector2 = new Vector2(base.Left + 4f + (float)j, y);
                if (base.Scene.CollideCheck<Solid>(vector2))
                {
                    SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, vector2);
                }
            }
        }

        Collidable = true;
    }


    public MTexture GetTexture(string image, Atlas? atlas = null)
    {
        atlas ??= GFX.Game;
        return !atlas.textures.TryGetValue(Texture + image, out var value) ? atlas["objects/moveBlock/" + image] : value;
    }
}