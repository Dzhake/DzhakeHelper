using System;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Entities;

[Tracked(false)]
public class ManualSequenceComponent : Component
{
    public Color color;
    public Color pressedColor;

    public int Index;
    public bool Activated = false;

    public bool UseCustomColor = false;

    public Entity entity;
    public EntityID ID;

    public ManualSequenceComponent(EntityData data, Entity entity)
        : base(active: true, visible: false)
    {
        this.entity = entity;

        Index = data.Int("index");

        UseCustomColor = data.Bool("useCustomColor");
        if (UseCustomColor)
        {
            color = data.HexColorWithAlpha("color");
        }
        else
        {
            color = Util.DefaultSequenceColors[Index];
        }

        Color c = Calc.HexToColor("667da5");
        pressedColor = new Color((float)(int)c.R / 255f * ((float)(int)color.R / 255f), (float)(int)c.G / 255f * ((float)(int)color.G / 255f), (float)(int)c.B / 255f * ((float)(int)color.B / 255f), 1f);
    }




}
