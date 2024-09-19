using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [Tracked]
    public class ElephantLazer : Entity
    {
        public enum LazerType {Descending, Horizontal}

        public LazerType type;
        public Color[] colors = [new(228, 5, 6), new(254, 104, 0)];
        public Color green = new(152, 235, 0);
        public Color currColor;
        public Color prevColor;

        public Color[] allColors = [new(228, 5, 6), new(254, 104, 0), new(152, 235, 0)];

        public float t;
        public int direction; // obv -1 is left and 1 is right. Used by Horizontal lazers.
        private int levelWidth;
        private int levelHeight;
        private Vector2 levelOffset;

        public MTexture texture = GFX.Game["objects/DzhakeHelper/elephantLazer/idle"];

        public ElephantLazer(Vector2 offset, Vector2 position, LazerType type)
            : base(position + offset)
        {
            levelOffset = offset;
            direction = Calc.Random.Next(2) == 0 ? -1 : 1; //generate random "bool"
            this.type = type;
            Depth = Depths.TheoCrystal;

            if (type == LazerType.Descending) base.Collider = new Hitbox(16f, 16f);
            else base.Collider = new Hitbox(16f, 32f);
            Collidable = true;
            Add(new PlayerCollider(OnPlayer));

            currColor = Calc.Random.Choose(colors);
        }

        public override void Awake(Scene scene)
        {
            Rectangle bounds = (Scene as Level).Bounds;
            levelWidth = bounds.Width + 16; // haha +16 is so funny
            levelHeight = bounds.Height;
        }

        public override void Render()
        {
            if (type == LazerType.Descending)
            {
                Color color = Color.Lerp(currColor, green, t * 2);
                switch (t)
                {
                    case < 0.5f:
                        texture.DrawCentered(Center, color);
                        break;
                    case > 0.5f and < 1f:
                        color *= (1f - t) * 2f;
                        texture.DrawCentered(new(Center.X, Center.Y + (t - 1f) * 32), color);
                        texture.DrawCentered(new(Center.X, Center.Y + (t - 0.5f) * 32), color);
                        break;
                    case > 1:
                    {
                        int y = 0;
                        while ((Scene as Level).IsInCamera(new(Center.X, y), 8))
                        {
                            texture.DrawCentered(new(Center.X, y), color);
                            y += 16;
                        }
                        break;
                    }
                }
            }
            else
            {
                Color color = Color.Lerp(prevColor, currColor, t%1);
                for (int i = 8; i >= (levelHeight - 32) * -1; i -= 16)
                {
                    bool colored = i >= -8 || i <= (levelHeight - 64) * -1;
                    texture.DrawCentered(new(Center.X, Center.Y + i),
                         colored ? color : Color.Black * 0.33f);
                }
            }

            base.Render();
        }

        public void OnPlayer(Player player) => player.Die(Vector2.Zero);

        public override void Update()
        {
            t += Engine.DeltaTime;

            if (type == LazerType.Descending)
            {
                if (t > 1) Collider.Height = (Scene as Level).Bounds.Height;
                if (t > 1.2f) RemoveSelf();
            }
            else
            {
                X = levelOffset.X + (direction == -1 ? levelWidth : -16) + t * direction * 100;

                if (X < -16 || X > levelWidth + 16)
                    RemoveSelf();

                Color newColor = allColors[(int)t % 3];
                if (newColor != currColor)
                {
                    prevColor = currColor;
                    currColor = newColor;
                }
            }


            base.Update();
        }
    }
}
