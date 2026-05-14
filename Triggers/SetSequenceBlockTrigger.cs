using Celeste.Mod.DzhakeHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DzhakeHelper.Triggers
{
    [CustomEntity("DzhakeHelper/SetSequenceBlocksTrigger")]

    public class SetSequenceBlocksTrigger : Trigger
    {

        public int newIndex;

        private bool triggered = false;

        private bool OneUse;
        public int SequenceGroup;

        public SetSequenceBlocksTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            newIndex = data.Int("newIndex");
            OneUse = data.Bool("oneUse");
            SequenceGroup = data.Int("sequenceGroup");
        }

        public override void OnEnter(Player player)
        {
            if (!(triggered && OneUse))
            {
                foreach (Entity? managerEnt in Scene.Tracker.GetEntities<SequenceBlockManager>())
                    if (managerEnt is SequenceBlockManager manager && manager.SequenceGroup == SequenceGroup) manager.SetSequenceBlocks(newIndex);
                triggered = true;
            }
        }
    }
}
