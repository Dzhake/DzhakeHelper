using Celeste.Mod.DzhakeHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Triggers
{
    [CustomEntity("DzhakeHelper/CycleSequenceBlocksTrigger")]

    public class CycleSequenceBlocksTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {

        public int cyclesCount = data.Int("cyclesCount");

        private bool triggered;

        public override void OnEnter(Player player)
        {
            if (triggered) return;

            for (int i = 0; i < cyclesCount; i++)
            {
                Scene.Tracker.GetEntity<SequenceBlockManager>()?.CycleSequenceBlocks();
            }
            triggered = true;
        }
    }
}
