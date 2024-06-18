using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using Monocle;
using System.Collections;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity($"{nameof(DzhakeHelper)}/{nameof(CustomDoor)}")]
    public class CustomDoor : Solid
    {
        public static ParticleType P_Appear;

        public EntityID ID;

        public bool UnlockingRegistered;

        public Sprite sprite;

        public bool opening;

        public bool stepMusicProgress;

        public string unlockSfx;

        public int Group;
        public bool OpenedByAny;
        public bool OpenedByVanillaKeys;

        public CustomDoor(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, 32f, 32f, safe: false)
        {
            stepMusicProgress = data.Bool("stepMusicProgress");
            OpenedByAny = data.Bool("openedByAny");
            OpenedByVanillaKeys = data.Bool("openedByVanillaKeys", true);
            Group = data.Int("group");
            string spriteName = data.Attr("sprite", "objects/DzhakeHelper/customDoor/");
            unlockSfx = data.Attr("unlockSfx", "event:/game/03_resort/key_unlock");
            ID = id;
            DisableLightsInside = false;
            Add(new PlayerCollider(OnPlayer, new Circle(60f, 16f, 16f)));
            Add(sprite = new Sprite(GFX.Game,spriteName));
            sprite.AddLoop("idle","lockdoor",0.1f,0);
            sprite.Add("open", "lockdoor",0.06f, 0,1,2,3,4,5,6,7,8,9);
            sprite.Add("burst","lockdoor",0.06f,10,11,12,13,14,15,16,17,18);
            sprite.Play("idle");
            sprite.Position -= new Vector2(Width, Height / 2);
            sprite.Color = data.HexColorWithAlpha("color");;
            P_Appear = new ParticleType
            {
                ColorMode = ParticleType.ColorModes.Blink,
                Color = data.HexColorWithAlpha("particleColor1",Calc.HexToColorWithAlpha("FF3D63")),
                Color2 = data.HexColorWithAlpha("particleColor2",Calc.HexToColorWithAlpha("FF75DE")),
                LifeMin = 0.4f,
                LifeMax = 1.2f,
                SpeedMin = 30f,
                SpeedMax = 70f,
                Size = 1f,
                SizeRange = 0f,
                SpeedMultiplier = 0.6f,
                DirectionRange = 0.436332315f
            };
        }

        public void Appear()
        {
            Visible = true;
            sprite.Play("appear");
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
            {
                Level level = base.Scene as Level;
                if (!CollideCheck<Solid>(Position - Vector2.UnitX))
                {
                    level.Particles.Emit(P_Appear, 16, Position + new Vector2(3f, 16f), new Vector2(2f, 10f), MathF.PI);
                    level.Particles.Emit(P_Appear, 16, Position + new Vector2(29f, 16f), new Vector2(2f, 10f), 0f);
                }

                level.Shake();
            }, 0.25f, start: true));
        }

        public void OnPlayer(Player player)
        {
            if (opening)
            {
                return;
            }

            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is Key key && !key.StartedUsing && OpenedByVanillaKeys)
                {
                    TryOpen(player, follower);
                    break;
                }

                if (follower.Entity is CustomKey key1 && (OpenedByAny || key1.OpenAny || key1.Group == Group))
                {
                    AddTag(Tags.Global);
                    TryOpen(player,follower);
                    break;
                }
            }
        }

        public void TryOpen(Player player, Follower fol)
        {
            Collidable = false;
            if (!base.Scene.CollideCheck<Solid>(player.Center, base.Center))
            {
                opening = true;
                if (fol.Entity is Key key)
                    key.StartedUsing = true;
                else if (fol.Entity is CustomKey key1)
                    key1.StartedUsing = true;
                Add(new Coroutine(UnlockRoutine(fol)));
            }

            Collidable = true;
        }

        public IEnumerator UnlockRoutine(Follower fol)
        {
            SoundEmitter emitter = SoundEmitter.Play(unlockSfx, this);
            emitter.Source.DisposeOnTransition = true;
            Level level = SceneAs<Level>();
            Key key = fol.Entity as Key;
            CustomKey key1 = fol.Entity as CustomKey;
            if (key != null)
                Add(new Coroutine(key.UseRoutine(Center + new Vector2(0f, 2f))));
            else
                Add(new Coroutine(key1.UseRoutine(Center + new Vector2(0f, 2f))));
            yield return 1.2f;
            UnlockingRegistered = true;
            if (stepMusicProgress)
            {
                level.Session.Audio.Music.Progress++;
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            }

            level.Session.DoNotLoad.Add(ID);
            if (key != null)
            {
                key.RegisterUsed();
                while (key.Turning)
                {
                    yield return null;
                }
            }
            else
            {
                key1.RegisterUsed();
                while (key1.Turning)
                {
                    yield return null;
                }
            }

            Tag |= Tags.TransitionUpdate;
            Collidable = false;
            emitter.Source.DisposeOnTransition = false;
            yield return sprite.PlayRoutine("open");
            level.Shake();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            yield return sprite.PlayRoutine("burst");
            RemoveSelf();
        }
    }
}
