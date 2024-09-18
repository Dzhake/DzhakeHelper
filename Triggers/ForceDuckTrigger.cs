using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Triggers
{
    [CustomEntity("DzhakeHelper/ForceDuckTrigger")]

    public class ForceDuckTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            player.Ducking = true;
        }
    }
}