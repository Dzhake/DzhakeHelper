using Celeste.Mod.DzhakeHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DzhakeHelper.Triggers
{
    [CustomEntity("DzhakeHelper/CycleSequenceBlocksTrigger")]

    internal class CycleSequenceBlocksTrigger : Trigger
    {

        public int cyclesCount;

        private bool triggered = false;

        public CycleSequenceBlocksTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            cyclesCount = data.Int("cyclesCount");
        }

        public override void OnEnter(Player player)
        {
            if (!triggered) 
            {
                for (int i = 0; i < cyclesCount; i++)
                {
                    base.Scene.Tracker.GetEntity<SequenceBlockManager>()?.CycleSequenceBlocks();
                }
                triggered = true;
            }
        }
    }
}
