using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

// to the entirety of #code_modding: thank you so much lmao i could not have done this without you guys
// oh and to snip for dealing with all my stupid bullshit .w.

/*Code from https://github.com/cyrus0729/CyrusSandbox/blob/master/Source/Entities/HitboxController.cs

Dzhake: do you have plans to ever release your mod?
Aletris: not sure ngl
Dzhake: then maybe i should try porting working entities to my helper so people can use them?\n i think custom hitbox is pretty popular request
... *Aletris sends screenshots how they try to upload stuff to github*
Aletris: https://github.com/cyrus0729/CyrusSandbox i hope i did it right
 */

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/PlayerHitboxController")]
    public class PlayerHitboxController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
    {
        private Vector2 normalHitbox = data.Vector("normalHitbox");
        private Vector2 normalHurtbox = data.Vector("normalHurtbox");

        private Vector2 duckHitbox = data.Vector("duckHitbox");
        private Vector2 duckHurtbox = data.Vector("duckHurtbox");

        private Vector2 featherHitbox = data.Vector("featherHitbox");
        private Vector2 featherHurtbox = data.Vector("featherHurtbox");

        private Vector2 normalHitboxOffset = data.Vector("normalHitboxOffset");
        private Vector2 normalHurtboxOffset = data.Vector("normalHurtboxOffset");

        private Vector2 duckHitboxOffset = data.Vector("duckHitboxOffset");
        private Vector2 duckHurtboxOffset = data.Vector("duckHurtboxOffset");

        private Vector2 featherHitboxOffset = data.Vector("featherHitboxOffset");
        private Vector2 featherHurtboxOffset = data.Vector("featherHurtboxOffset");


        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = Scene.Tracker.GetEntity<Player>();

            player.normalHitbox.Width = normalHitbox.X;
            player.normalHitbox.Height = normalHitbox.Y;

            player.duckHitbox.Width = duckHitbox.X;
            player.duckHitbox.Height = duckHitbox.Y;

            player.starFlyHitbox.Width = featherHitbox.X;
            player.starFlyHitbox.Height = featherHitbox.Y;

            player.normalHurtbox.Width = normalHurtbox.X;
            player.normalHurtbox.Height = normalHurtbox.Y;

            player.duckHurtbox.Width = duckHurtbox.X;
            player.duckHurtbox.Height = duckHurtbox.Y;

            player.starFlyHurtbox.Width = featherHurtbox.X;
            player.starFlyHurtbox.Height = featherHurtbox.Y;

            player.normalHitbox.Position.X = normalHitboxOffset.X;
            player.normalHitbox.Position.Y = normalHitboxOffset.Y;

            player.duckHitbox.Position.X = duckHitboxOffset.X;
            player.duckHitbox.Position.Y = duckHitboxOffset.Y;

            player.starFlyHitbox.Position.X = featherHitboxOffset.X;
            player.starFlyHitbox.Position.Y = featherHitboxOffset.Y;

            player.normalHurtbox.Position.X = normalHurtboxOffset.X;
            player.normalHurtbox.Position.Y = normalHurtboxOffset.Y;

            player.duckHurtbox.Position.X = duckHurtboxOffset.X;
            player.duckHurtbox.Position.Y = duckHurtboxOffset.Y;

            player.starFlyHurtbox.Position.X = featherHurtboxOffset.X;
            player.starFlyHurtbox.Position.Y = featherHurtboxOffset.Y;
        }

        public override void Removed(Scene scene)
        {
            //reset values to default
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player is null) return;

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

            base.Removed(scene);
        }
    }
}