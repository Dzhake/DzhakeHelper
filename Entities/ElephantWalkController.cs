using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/ElephantWalkController")]

    public class ElephantWalkController : Entity
    {
        public static float Angle;
        public static int Direction = 1; // 1 -> right, -1 -> left
        public static float Range;
        public static float Speed;
        
        public ElephantWalkController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Range = data.Float("range", 0.5f);
            Speed = data.Float("speed", 0.1f);
        }

        public override void Update()
        {
            if (DzhakeHelperHooks.PlayerMoved)
            {
                Angle += Speed * Direction; // Add a bit of rotation in that direction
                if (Math.Abs(Angle) > Math.Abs(Direction * Range))
                    Direction *= -1; // Reverse direction when hit end of "range"
            }
            else
            {
                Angle = 0f;
            }
            base.Update();
        }

        public override void Removed(Scene scene)
        {
            Angle = 0f;
            base.Removed(scene);
        }
    }
}
