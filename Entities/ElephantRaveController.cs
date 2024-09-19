using System.Collections.Generic;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/ElephantRaveController")]

    public class ElephantRaveController : Entity
    {
        public float t = 23;
        public int phase;
        public Backdrop bg;
        public Backdrop bgSky;

        public float timeUntilTick;
        public float tickTime = 0.5f;
        public float tickTimeDecrease = 0.02f;

        public Color[] bgColors = [Color.Blue, Color.Aqua, Color.Beige, Color.Yellow, Color.Wheat, Color.LightGreen, Color.Pink, Color.Orange,
        Color.Gold, Color.Crimson, Color.Moccasin, Color.LightSalmon, Color.PeachPuff, Color.CornflowerBlue, Color.Coral];
        // These colors are beautiful, but they're too light
        public Color prevColor = Color.White;
        public Color currColor = Color.White;
        public Color skyColor = new(10, 155, 255);

        private Vector2 levelOffset;
        private int maxLazerX;
        private float horizontalSpawnDelay;

        public ElephantRaveController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            levelOffset = offset;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            maxLazerX = (scene as Level).Bounds.Width - 16;
            foreach (Backdrop backdrop in SceneAs<Level>().Background.Backdrops)
            {
                if (backdrop.Tags.Contains("elephantRaveBg"))
                {
                    bg = backdrop;
                    bg.Color = skyColor;
                }
                else if (backdrop.Tags.Contains("elephantRaveSky"))
                {
                    bgSky = backdrop;
                    SceneAs<Level>().Session.SetFlag("elephantRavePhase2", false);
                }

                if (bg != null && bgSky != null) // Once found all the needed bgs...
                    break;
            }
        }

        public override void Update()
        {
            base.Update();
            t += Engine.DeltaTime;


            if (t >= 4 && phase == 0) phase = 1; // Small delay before first phase
            if (phase == 0) return;

            if (t >= 24)
            {
                phase = 2;
                bg.Color = skyColor;
                tickTime = 0.15f;
                SceneAs<Level>().Session.SetFlag("elephantRavePhase2");
            }

            timeUntilTick -= Engine.DeltaTime;
            if (phase == 1)
            {
                tickTime -= tickTimeDecrease * Engine.DeltaTime;
                if (Beautiful) bg.Color = Color.Lerp(currColor, prevColor, timeUntilTick / tickTime);
            }

            if (timeUntilTick < 0)
            {
                if (phase == 1)
                {
                    if (Beautiful)
                    {
                        prevColor = currColor;
                        currColor = GetColor();
                    }
                    else
                        bg.Color = GetColor();
                }

                GenerateLazer();
                timeUntilTick = tickTime;

                if (phase == 2 && t > horizontalSpawnDelay)
                {
                    ElephantLazer lazer = new(levelOffset, new(0,(Scene as Level).Bounds.Bottom - 60),ElephantLazer.LazerType.Horizontal);
                    Scene.Add(lazer);
                    horizontalSpawnDelay = t + 3;
                }
            }
        }

        private void GenerateLazer()
        {
            Vector2 lazerPos = new(Calc.Random.Next(maxLazerX), 0);
            ElephantLazer lazer = new(levelOffset, lazerPos, ElephantLazer.LazerType.Descending);
            Scene.Add(lazer);
        }

        private Color GetColor()
        {
            Color result = currColor;

            
            if (!Beautiful)
            {
                int min = 0;
                int max = 50;
                result.R = (byte)Calc.Random.Next(min, max);
                result.G = (byte)Calc.Random.Next(min, max);
                result.B = (byte)Calc.Random.Next(min, max);
            }
            

            while (prevColor == result && Beautiful)
            {
                result = Calc.Random.Choose(bgColors);
            }

            return result;
        }

        private bool Beautiful = false;  //More beautiful, yet less elephant-like version
    }
}
