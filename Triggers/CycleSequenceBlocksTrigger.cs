using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Triggers
{
    [CustomEntity("DzhakeHelper/CycleSequenceBlocksTrigger")]

    public class CycleSequenceBlocksTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        public int SequenceGroup = data.Int("sequnceGroup");
        public int cyclesCount = data.Int("cyclesCount");

        private bool triggered;

        public override void OnEnter(Player player)
        {
            if (triggered) return;
            Util.CycleSequenceColor(cyclesCount, SequenceGroup);
            triggered = true;
        }
    }
}
