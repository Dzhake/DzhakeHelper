using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.DzhakeHelper.Entities;

[CustomEntity($"{nameof(DzhakeHelper)}/{nameof(SequenceSwitchBlock)}")]
[Tracked(false)]
public class SequenceSwitchBlock : Solid
{
    private uint seed;

    private readonly MTexture[,] edges = new MTexture[3, 3];

    private static readonly Color defaultBackgroundColor = Calc.HexToColor("191919");
    private static readonly Color defaultEdgeColor = Calc.HexToColor("646464");

    private Color BackgroundColor = defaultBackgroundColor;
    private Color EdgeColor = defaultEdgeColor;
    private Color currentEdgeColor, currentBackgroundColor;

    private Vector2 scale = Vector2.One;
    private Vector2 scaleStrength = Vector2.One;

    public bool singleColor, random;

    public List<int> indexes = new List<int>();
    public int nextColorIndex = 0;
    public List<Color> Colors = new List<Color>();

    public bool CustomColors;
    public bool AllowDashOnTop;

    public string sprite;

    public SequenceSwitchBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
    {

        SurfaceSoundIndex = SurfaceIndex.ZipMover;

        if (data.Bool("blue"))
        {
            indexes.Add(0);
        }
        if (data.Bool("rose"))
        {
            indexes.Add(1);
        }
        if (data.Bool("brightSun"))
        {
            indexes.Add(2);
        }
        if (data.Bool("malachite"))
        {
            indexes.Add(3);
        }

        if (indexes.Count == 0)
        {
            indexes = [0,1,2,3];
        }

        CustomColors = data.Bool("useCustomColors");

        if (CustomColors)
        {
            string[] customColors = data.Attr("customColors").Split(",");
            foreach (string c in customColors)
            {
                Colors.Add(Calc.HexToColorWithAlpha(c));
            }

            if (Colors.Count != indexes.Count)
            {
                Logger.Log(LogLevel.Error, "DzhakeHelper/SequenceSwitchBlock",$"Colors.Count is not same as indexes.Count! Colors is {Colors.Count}, and indexes.Count is {indexes.Count}");
            }
        }
        else
        {
            foreach (int i in indexes)
            {
                Colors.Add(Util.DefaultSequenceColors[i]);
            }
        }

        random = data.Bool("random");

        sprite = data.Attr("sprite");
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                edges[i, j] = GFX.Game[sprite].GetSubtexture(i * 8, j * 8, 8, 8);


        singleColor = indexes.Count == 1;

        NextColor(indexes[nextColorIndex], true);

        Add(new LightOcclude());
        Add(new SoundSource(SFX.game_01_console_static_loop)
        {
            Position = new Vector2(Width, Height) / 2f,
        });

        if (Width > 32)
            scaleStrength.X = Width / 32f;
        if (Height > 32)
            scaleStrength.Y = Height / 32f;

        Color col = Colors[nextColorIndex];

        SetEdgeColor(EdgeColor, EdgeColor);
        SetBackgroundColor(col, col);

        this.OnDashCollide = Dashed;

        AllowDashOnTop = data.Bool("allowsDashOnTop");
    }

    public override void Render()
    {
        Vector2 position = this.Position;
        this.Position += this.Shake;

        int x = (int)(this.Center.X + (this.X + 1 - this.Center.X) * this.scale.X);
        int y = (int)(this.Center.Y + (this.Y + 1 - this.Center.Y) * this.scale.Y);
        int rectW = (int)((this.Width - 2) * this.scale.X);
        int rectH = (int)((this.Height - 2) * this.scale.Y);
        var rect = new Rectangle(x, y, rectW, rectH);

        Color col = this.random
            ? Color.Lerp(defaultBackgroundColor, Color.White, (float)(0.05f * Math.Sin(this.Scene.TimeActive * 5f)) + 0.05f)
            : this.BackgroundColor != defaultBackgroundColor
                ? Color.Lerp(this.currentBackgroundColor, Color.Black, 0.2f)
                : this.currentBackgroundColor;

        Draw.Rect(rect, col);

        for (int i = rect.Y; i < rect.Bottom; i += 2)
        {
            float scale = 0.05f + (1f + (float)Math.Sin(i / 16f + this.Scene.TimeActive * 2f)) / 2f * 0.2f;
            Draw.Line(rect.X, i, rect.X + rect.Width, i, Color.White * 0.55f * scale);
        }

        uint tempseed = this.seed;
        PlaybackBillboard.DrawNoise(rect, ref tempseed, Color.White * 0.05f);

        int w = (int)(this.Width / 8f);
        int h = (int)(this.Height / 8f);

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                int tx = (i != 0) ? ((i != w - 1f) ? 1 : 2) : 0;
                int ty = (j != 0) ? ((j != h - 1f) ? 1 : 2) : 0;

                if (tx == 1 && ty == 1)
                    continue;

                Vector2 renderPos = new Vector2(i, j) * 8f + Vector2.One * 4f + this.Position;
                renderPos = this.Center + (renderPos - Center) * scale;
                this.edges[tx, ty].DrawCentered(renderPos, this.currentEdgeColor, this.scale);
            }
        }

        base.Render();
        this.Position = position;
    }

    public override void Update()
    {
        base.Update();

        if (this.Scene.OnInterval(0.1f))
            this.seed++;

        float t = Calc.Min(1f, 4f * Engine.DeltaTime);
        this.currentEdgeColor = Color.Lerp(this.currentEdgeColor, this.EdgeColor, t);
        this.currentBackgroundColor = Color.Lerp(this.currentBackgroundColor, this.BackgroundColor, t);

        this.scale = Calc.Approach(this.scale, Vector2.One, Engine.DeltaTime * 4f);
    }

    private void SetEdgeColor(Color targetColor, Color currentColor)
    {
        this.EdgeColor = targetColor;
        this.currentEdgeColor = currentColor;
    }

    private void SetBackgroundColor(Color targetColor, Color currentColor)
    {
        this.BackgroundColor = targetColor;
        this.currentBackgroundColor = currentColor;
    }

    private DashCollisionResults Dashed(Player player, Vector2 direction)
    {
        if (!SaveData.Instance.Assists.Invincible && player.CollideCheck<Spikes>())
            return DashCollisionResults.NormalCollision;

        if (indexes[nextColorIndex] == DzhakeHelperModule.Session.ActiveSequenceIndex)
            return DashCollisionResults.NormalCollision;

        if (player.StateMachine.State == Player.StRedDash)
            player.StateMachine.State = Player.StNormal;



        Switch(direction);
        return  (!AllowDashOnTop || !(direction.Y == 1)) ? DashCollisionResults.Rebound : DashCollisionResults.NormalCollision;
    }

    public void Switch(Vector2 direction)
    {
        this.scale = new Vector2(
            1f + (Math.Abs(direction.Y) * 0.5f - Math.Abs(direction.X) * 0.5f) / this.scaleStrength.X,
            1f + (Math.Abs(direction.X) * 0.5f - Math.Abs(direction.Y) * 0.5f) / this.scaleStrength.Y
        );

        if (this.random)
            nextColorIndex = Calc.Random.Next(0, indexes.Count);

        Util.SetSequenceColor(nextColorIndex);
        Color col = Colors[nextColorIndex];

        SetEdgeColor(defaultEdgeColor, col);
        currentBackgroundColor = Color.White;

        Audio.Play("event:/vortexHelperEvents/game/colorSwitch/hit", Center);

        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
        SceneAs<Level>().DirectionalShake(direction, 0.25f);
        StartShaking(0.25f);

        ParticleType p = LightningBreakerBox.P_Smash;
        p.Color = col; p.Color2 = Color.Lerp(col, Color.White, 0.5f);
        SmashParticles(direction.Perpendicular(), p);
        SmashParticles(-direction.Perpendicular(), p);
    }

    public void NextColor(int colorNext, bool start)
    {
        if (colorNext == indexes[nextColorIndex] && !singleColor)
        {
            if (!start)
                nextColorIndex++;

            if (nextColorIndex > indexes.Count - 1)
                nextColorIndex = 0;

            if (indexes[nextColorIndex] == DzhakeHelperModule.Session.ActiveSequenceIndex)
                nextColorIndex++;
        }

        BackgroundColor = Colors[nextColorIndex];
    }

    private void SmashParticles(Vector2 dir, ParticleType smashParticle)
    {
        float direction;
        Vector2 position;
        Vector2 positionRange;
        int num;

        if (dir == Vector2.UnitX)
        {
            direction = 0f;
            position = this.CenterRight - Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (this.Height - 6f) * 0.5f;
            num = (int)(this.Height / 8f) * 4;
        }
        else if (dir == -Vector2.UnitX)
        {
            direction = (float)Math.PI;
            position = this.CenterLeft + Vector2.UnitX * 12f;
            positionRange = Vector2.UnitY * (this.Height - 6f) * 0.5f;
            num = (int)(this.Height / 8f) * 4;
        }
        else if (dir == Vector2.UnitY)
        {
            direction = (float)Math.PI / 2f;
            position = this.BottomCenter - Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (this.Width - 6f) * 0.5f;
            num = (int)(this.Width / 8f) * 4;
        }
        else
        {
            direction = -(float)Math.PI / 2f;
            position = this.TopCenter + Vector2.UnitY * 12f;
            positionRange = Vector2.UnitX * (this.Width - 6f) * 0.5f;
            num = (int)(this.Width / 8f) * 4;
        }

        num += 2;
        SceneAs<Level>().Particles.Emit(smashParticle, num, position, positionRange, direction);
    }
}