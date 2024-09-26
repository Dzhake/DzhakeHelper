using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    public class Rect(Vector2 position, Vector2 size, Color color, bool cutscene = true, bool hollow = false)
        : Entity(position)
    {
        public Color color = color;
        public float width = size.X;
        public float height = size.Y;
        public bool hollow = hollow;
        public bool cutscene = cutscene;

        public override void Render()
        {
            base.Render();
            if (hollow)
                Draw.HollowRect(Position, width, height, color);
            else
                Draw.Rect(Position, width, height, color);
        }

        public override void Update()
        {
            base.Update();
            if (cutscene && !(Scene as Level).InCutscene)
                RemoveSelf();
        }

        /// <summary>
        /// Returns screen-sized Rect
        /// </summary>
        public static Rect Screen(Color color, bool cutscene = true) =>
            new(new(0,0), new(320f, 180f), color, cutscene);
    }
}
