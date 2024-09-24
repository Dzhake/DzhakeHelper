using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/ElephantRaveController")]
    [Tracked]

    public class ElephantRaveController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
    {
        public float t = 0f;
        public int phase;

        public Backdrop bg;
        public CustomDecal picture_decal;
        public CustomDecal ws_decal;
        public InvisibleBarrier barrier;
        public Entity heart; //we only need Y, so it doesn't worth adding collab utils 2 reference

        public float timeUntilTick;
        public float tickTime = 0.5f;
        public float tickTimeDecrease = 0.02f;

        public Color prevColor = Color.White;
        public Color currColor = Color.White;
        public Color skyColor = new(10, 155, 255);

        private int maxLazerX;
        private float horizontalSpawnDelay;
        private float blinkDelay;
        private bool firstRun = true;

        private static List<string> flags = ["elephantRaveSky", "Rave", "Life", "Love", "Misunderstanding", "FunnyNoses",
            "Elephant", "elephantRaveSpace", "vortex", "windSnow"];

        public const string music = "event:/DzhakeCrossoverCrablab/ElephantRave";
        public const string picture_decal_path = "decals/Dzhake/CrossoverCrablab/FunnyNoses";
        public const string ws_decal_path = "decals/Dzhake/CrossoverCrablab/WhiteScreen";
        public const WindController.Patterns pattern = WindController.Patterns.Left;


        public override void Awake(Scene scene)
        {
            Audio.SetMusic(null);
            Level level = SceneAs<Level>();

            base.Awake(scene);
            maxLazerX = level.Bounds.Width - 16;
            
            //Reset all flags
            foreach (string s in flags)
                level.Session.SetFlag(s, false);

            //Find bg
            foreach (Backdrop backdrop in level.Background.Backdrops)
            {
                if (backdrop.Tags.Contains("elephantRaveBg"))
                {
                    bg = backdrop;
                    bg.Color = Color.Black;
                }

                if (bg != null) // Once found all the needed bgs...
                    break;
            }

            if (bg == null)
                throw new ArgumentException($"One(Some) styleground(s) required for elephant rave was(were) not found!");

            //Find needed entities
            foreach (Entity e in Scene.Entities)
            {
                if (e is CustomDecal d)
                {
                    if (d.Name == picture_decal_path) picture_decal = d;
                    else if (d.Name == ws_decal_path) ws_decal = d;
                }
                else if (e is InvisibleBarrier b) barrier = b;
                else if (e.GetType().FullName == "Celeste.Mod.CollabUtils2.Entities.MiniHeart") heart = e;

                if (picture_decal != null && ws_decal != null && barrier != null) break;
            }
        }

        public override void Render()
        {
            base.Render();
            //Loading bars
            int c = Calc.Random.Next(150);
            Color col = new(c, c, c);
            if (phase == 0 && t >= 2f)
            {
                Draw.Rect(40, 88, 240 * (t - 2) / 2, 8, col);
                Draw.HollowRect(40, 88, 240, 8, Color.Black);
            }
            else if (phase == 4 && t >= 80f)
            {
                int xShake = Calc.Random.Next(-5, 6);
                int yShake = Calc.Random.Next(-2, 3);
                Draw.Rect(40 + xShake, 88 + yShake, 240 * (t - 80) / 5, 8, col);
                //Draw.HollowRect(40 + xShake, 88 + yShake, 240, 8, Color.Black);
            }
           
        }

        public override void Update()
        {
            base.Update();
            //Don't start until player is woke up
            Level level = SceneAs<Level>();
            if (!Util.TryGetPlayer(out Player player)) return;

            if (firstRun && player.StateMachine.State != Player.StNormal) return;
            Audio.SetMusic(music);
            firstRun = false;

            t += Engine.DeltaTime;
            blinkDelay -= Engine.DeltaTime;

            if (phase == 0)
            {
                //Texts "Elephant", "Rave", and loading bar
                switch (t)
                {
                    case <= 1f:
                        level.Session.SetFlag("Elephant");
                        bg.Color = Color.White;
                        break;
                    case > 1f and <= 2f:
                        level.Session.SetFlag("Rave");
                        level.Session.SetFlag("Elephant", false);
                        bg.Color = Color.Black;
                        break;
                    case > 2f:
                        level.Session.SetFlag("Rave", false);
                        int c = Calc.Random.Next(150);
                        bg.Color = new(c, c, c, byte.MaxValue);
                        if (blinkDelay > 0)
                            bg.Color = currColor;
                        else
                        {
                            currColor = bg.Color;
                            blinkDelay = 0.05f;
                        }
                        break;
                }
            }

            if (t >= 4 && phase == 0) phase = 1; //Next phase!

            if (phase == 0) return;

            if (t >= 24 && phase == 1) // Phase 2. Increase tickTime a bit, because of horizontal lazers.
            {
                phase = 2;
                bg.Color = skyColor;
                tickTime = 0.15f;
                horizontalSpawnDelay = t + 4f;
                level.Session.SetFlag("elephantRaveSky");
            }
            else if (t >= 44 && phase == 2) // No tickTime cuz no lazers
            {
                phase = 3;
                tickTime = 0f;
            }
            else if (t >= 60f && phase == 3) //reset all flags (text) and make styleground appear
            {
                phase = 4;
                tickTime = 0.6f;
                foreach (string s in flags)
                    level.Session.SetFlag(s, false);
                level.Session.SetFlag("elephantRaveSpace");
                picture_decal.Visible = false;
                bg.Color = Color.White * 0f;
            }
            else if (t >= 80f && t < 85f && phase == 4)
            {
                level.Session.SetFlag("vortex");
            }
            else if (t >= 85f && phase == 4)
            {
                level.Session.SetFlag("vortex", false);
                level.Session.SetFlag("windSnow");
                phase = 5;
                tickTime = 0.15f;
                
                //Add wind
                WindController windController = Scene.Entities.FindFirst<WindController>();
                if (windController == null)
                {
                    windController = new WindController(pattern);
                    Scene.Add(windController);
                }
                else
                    windController.SetPattern(pattern);
            }
            else if (t >= 100f && phase == 5)
            {
                phase = 6;
                //Remove all lazers
                List<ElephantLazer> lazers = new();
                foreach (Entity e in level.Entities)
                    if (e is ElephantLazer l) lazers.Add(l);

                foreach (ElephantLazer lazer in lazers)
                    lazer.RemoveSelf();

                //Make mini heart appear
                heart.Y = 80;
            }

            if (phase is not (3 or 6)) timeUntilTick -= Engine.DeltaTime; //in phase 3 tickTime is 0

            if (phase is 1 or 4)
            { //Slowly decrease tick time
                tickTime -= tickTimeDecrease * Engine.DeltaTime;
            }
            
            if (phase == 3)
            { //Show texts and white fg
                bg.Color = Color.Lerp(skyColor, Color.White, t - 44);
                if (t >= 49) picture_decal.Color = Color.White * (t - 49);
                if (t >= 52f)
                    level.Session.SetFlag("Life");
                if (t >= 54f)
                    level.Session.SetFlag("Love");
                if (t >= 56f)
                    level.Session.SetFlag("Misunderstanding");
                if (t >= 58f)
                    level.Session.SetFlag("FunnyNoses");

                if (t >= 57f)
                    ws_decal.Color = Color.White * ((t - 57f) / 1.5f);
            }
            else if (phase == 4)
                ws_decal.Color = Color.White * (61f - t);
            else if (phase == 5)
                ws_decal.Color = Color.White * (t - 99.5f) * 2;
            else if (phase == 6)
                ws_decal.Color = Color.White * (100.5f - t) * 2f;
                

            if (timeUntilTick < 0)
            {
                if (phase == 1)
                    bg.Color = GetColor();

                GenerateLazer(phase is 4 or 5);
                timeUntilTick = tickTime;

                if (phase == 2 && t > horizontalSpawnDelay)
                { //Spawn horizontal lazers each 3 seconds
                    ElephantLazer lazer = new(offset, new(0,(Scene as Level).Bounds.Bottom - 60),
                        ElephantLazer.LazerType.Horizontal);
                    Scene.Add(lazer);
                    horizontalSpawnDelay = t + 3;
                }
            }
        }

        private void GenerateLazer(bool ascending)
        {
            ElephantLazer.LazerType type = ascending ? Calc.Random.Next(0, 2) == 0 ?
                    ElephantLazer.LazerType.Descending : ElephantLazer.LazerType.Ascending
                : ElephantLazer.LazerType.Descending; //if "ascending" then generate both at random, otherwise descending only.
            Vector2 lazerPos = new(Calc.Random.Next(maxLazerX), 0);
            ElephantLazer lazer = new(offset, lazerPos, type);
            Scene.Add(lazer);
        }

        private Color GetColor() //Returns random bg color
        {
            Color result = currColor;
            int min = 0;
            int max = 50;
            result.R = (byte)Calc.Random.Next(min, max);
            result.G = (byte)Calc.Random.Next(min, max);
            result.B = (byte)Calc.Random.Next(min, max);

            return result;
        }
    }
}
