using System;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.DzhakeHelper.Entities;

[Tracked(false)]
public class SequenceComponent : Component
{
    public Color color;
    public Color pressedColor;

    public int Index;
    public bool Activated = false;

    public bool UseCustomColor = false;

    public Entity entity;

    public bool entityCollidable;

    public List<Color> entityColors;


    public SequenceComponent(Entity entity, int index, bool useCustomColor, Color customColor)
        : base(active: true, visible: true)
    {
        this.entity = entity;

        Index = index;

        UseCustomColor = useCustomColor;
        if (UseCustomColor)
        {
            color = customColor;
        }
        else
        {
            color = Util.DefaultSequenceColors[Index];
        }

        Color c = Calc.HexToColor("667da5");
        pressedColor = new Color((float)(int)c.R / 255f * ((float)(int)color.R / 255f), (float)(int)c.G / 255f * ((float)(int)color.G / 255f), (float)(int)c.B / 255f * ((float)(int)color.B / 255f), 1f);

        entityCollidable = entity.Collidable;
    }

    public override void Update()
    {
        if (entityCollidable != entity.Collidable) entityCollidable = entity.Collidable;

        entity.Collidable = Activated && entityCollidable;

        base.Update();
    }

    public override void Render()
    {
        foreach (Component component in entity.Components)
        {
            if (component is Sprite sprite)
            {
                entityColors.Add(sprite.Color);
                sprite.Color = sprite.Color.Mult(entity.Collidable ? color : pressedColor);
            }
        }

        base.Render();

        int i = 0;
        foreach (Component component in entity.Components)
        {
            if (component is Sprite sprite)
            {
                sprite.SetColor(entityColors[i]);
                i++;
            }
        }
    }




}
