using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/SuperHotItem")]
    public class SuperHotItem : Entity
    {
        public float airTime;
        public float inAir;


        public Entity holder;

        public float cooldown;
        public float cooldownTimer;

        public SuperHotItem(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(new Sprite(GFX.Game, data.Attr("sprite")));
        }

        public override void Update()
        {
            base.Update();
        }
    }
}