using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.DzhakeHelper;

// to the entirety of #code_modding: thank you so much lmao i could not have done this without you guys
// oh and to snip for dealing with all my stupid bullshit .w.

/*Code from https://github.com/cyrus0729/CyrusSandbox/blob/master/Source/Entities/HitboxController.cs

Dzhake: do you have plans to ever release your mod?
Aletris: not sure ngl
Dzhake: then maybe i should try porting working entities to my helper so people can use them?\n i think custom hitbox is pretty popular request
... *Aletris sends screenshots how they try to upload stuff to github*
Aletris: https://github.com/cyrus0729/CyrusSandbox i hope i did it right
 */

namespace Celeste.Mod.CyrusSandbox.Entities
{

    [CustomEntity("CyrusHelper/HitboxController")]
    public class HitboxController : Entity
    {

        private Vector2 newHitbox;
        private Vector2 newHurtbox;

        private Vector2 newduckHitbox;
        private Vector2 newduckHurtbox;

        private Vector2 newfeatherHitbox;
        private Vector2 newfeatherHurtbox;

        private Vector2 newHitboxOffset;
        private Vector2 newHurtboxOffset;

        private Vector2 newduckHitboxOffset;
        private Vector2 newduckHurtboxOffset;

        private Vector2 newfeatherHitboxOffset;
        private Vector2 newfeatherHurtboxOffset;

        public bool AdvancedMode;
        public bool ModifyHitbox;

        public HitboxController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

            newHitbox = data.Vector("Hitbox");
            newHurtbox = data.Vector("Hurtbox");

            newduckHitbox = data.Vector("duckHitbox");
            newduckHurtbox = data.Vector("duckHurtbox");

            newfeatherHitbox = data.Vector("featherHitbox");
            newfeatherHurtbox = data.Vector("featherHurtbox");

            AdvancedMode = data.Bool("advancedMode");

            if (!AdvancedMode) return;

            newHitboxOffset = data.Vector("HitboxOffset");
            newHurtboxOffset = data.Vector("HurtboxOffset");

            newduckHitboxOffset = data.Vector("duckHitboxOffset");
            newduckHurtboxOffset = data.Vector("duckHurtboxOffset");

            newfeatherHitboxOffset = data.Vector("featherHitboxOffset");
            newfeatherHurtboxOffset = data.Vector("featherHurtboxOffset");
        }


        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = Scene.Tracker.GetEntity<Player>();

            player.normalHitbox.Width = newHitbox.X;
            player.normalHitbox.Height = newHitbox.Y;

            player.duckHitbox.Width = newduckHitbox.X;
            player.duckHitbox.Height = newduckHitbox.Y;

            player.starFlyHitbox.Width = newfeatherHitbox.X;
            player.starFlyHitbox.Height = newfeatherHitbox.Y;

            player.normalHurtbox.Width = newHurtbox.X;
            player.normalHurtbox.Height = newHurtbox.Y;

            player.duckHurtbox.Width = newduckHurtbox.X;
            player.duckHurtbox.Height = newduckHurtbox.Y;

            player.starFlyHurtbox.Width = newfeatherHurtbox.X;
            player.starFlyHurtbox.Height = newfeatherHurtbox.Y;

            if (!AdvancedMode) return;

            player.normalHitbox.Position.X = newHitboxOffset.X;
            player.normalHitbox.Position.Y = newHitboxOffset.Y;

            player.duckHitbox.Position.X = newduckHitboxOffset.X;
            player.duckHitbox.Position.Y = newduckHitboxOffset.Y;

            player.starFlyHitbox.Position.X = newfeatherHitboxOffset.X;
            player.starFlyHitbox.Position.Y = newfeatherHitboxOffset.Y;

            player.normalHurtbox.Position.X = newHurtboxOffset.X;
            player.normalHurtbox.Position.Y = newHurtboxOffset.Y;

            player.duckHurtbox.Position.X = newduckHurtboxOffset.X;
            player.duckHurtbox.Position.Y = newduckHurtboxOffset.Y;

            player.starFlyHurtbox.Position.X = newfeatherHurtboxOffset.X;
            player.starFlyHurtbox.Position.Y = newfeatherHurtboxOffset.Y;
        }

        public override void Removed(Scene scene)
        {
            Player player = Scene.Tracker.GetEntity<Player>();

            player.normalHitbox.Width = 8f;
            player.normalHitbox.Height = 11f;

            player.duckHitbox.Width = 8f;
            player.duckHitbox.Height = 6f;

            player.starFlyHitbox.Width = 8f;
            player.starFlyHitbox.Height = 8f;

            player.normalHurtbox.Width = 8f;
            player.normalHurtbox.Height = 9f;

            player.duckHurtbox.Width = 8f;
            player.duckHurtbox.Height = 4f;

            player.starFlyHurtbox.Width = 6f;
            player.starFlyHurtbox.Height = 6f;

            if (AdvancedMode)
            {
                player.normalHitbox.Position.X = -4;
                player.normalHitbox.Position.Y = -11;

                player.duckHitbox.Position.X = -4;
                player.duckHitbox.Position.Y = -6;

                player.starFlyHitbox.Position.X = -4;
                player.starFlyHitbox.Position.Y = -10;

                player.normalHurtbox.Position.X = -4;
                player.normalHurtbox.Position.Y = -11;

                player.duckHurtbox.Position.X = -4;
                player.duckHurtbox.Position.Y = -6;

                player.starFlyHurtbox.Position.X = -3;
                player.starFlyHurtbox.Position.Y = -9;
            }
            base.Removed(scene);
        }
    }
}