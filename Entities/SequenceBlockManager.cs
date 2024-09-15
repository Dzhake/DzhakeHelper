using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/SequenceBlockManager")]
    
    [Tracked]

    public class SequenceBlockManager : Entity
    {
        private readonly int startWith;

        public int typesCount = -1;

        public bool everyDash;

        public static int CurrentIndex { get { return DzhakeHelperModule.Session.ActiveSequenceIndex; } set { DzhakeHelperModule.Session.ActiveSequenceIndex = value; } }

        public SequenceBlockManager(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            startWith = data.Int("startWith");
            everyDash = data.Bool("everyDash");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            foreach (SequenceBlock entity in scene.Tracker.GetEntities<SequenceBlock>())
            {
                if (entity.Index > typesCount)
                {
                    typesCount = entity.Index;
                }
            }

            foreach (ManualSequenceComponent component in base.Scene.Tracker.GetComponents<ManualSequenceComponent>())
            {
                if (component.Index > typesCount)
                {
                    typesCount = component.Index;
                }
            }

            foreach (SequenceComponent component1 in base.Scene.Tracker.GetComponents<SequenceComponent>())
            {
                if (component1.Index > typesCount)
                {
                    typesCount = component1.Index;
                }
            }

            typesCount++;
            if (typesCount == 1) typesCount++; // 2 is minimum cuz why not

            CurrentIndex = startWith;

            UpdateBlocks();
        }


        public override void Update()
        {
            for (int i = 0; i < 4; i++)
            {
                if (((Scene as Level)?.Session?.GetFlag($"DzhakeHelper_Sequence_{i}") ?? false) == (i != CurrentIndex))  //if this is active color and there is no flag, or it's not active
                {
                    SetSequenceBlocks(i);
                    break;
                }
            }

            base.Update();
        }


        public void UpdateBlocks()
        {
            foreach (SequenceBlock entity in Scene.Tracker.GetEntities<SequenceBlock>())
            {
                entity.Activated = entity.Index == CurrentIndex;
            }

            foreach (ManualSequenceComponent component in Scene.Tracker.GetComponents<ManualSequenceComponent>())
            {
                component.Activated = component.Index == CurrentIndex;
            }

            foreach (SequenceSwitchBlock switchBlock in Scene.Tracker.GetEntities<SequenceSwitchBlock>())
            {
                switchBlock.NextColor(CurrentIndex, false);
            } 

            foreach (SequenceComponent component1 in Scene.Tracker.GetComponents<SequenceComponent>())
            {
                component1.Activated = component1.Index == CurrentIndex;
            }

            for (int i = 0; i < 4; i++)
            {
                (Scene as Level)?.Session?.SetFlag($"DzhakeHelper_Sequence_{i}", false);
            }
            (Scene as Level)?.Session?.SetFlag($"DzhakeHelper_Sequence_{CurrentIndex}", true);
        }

        public void CycleSequenceBlocks(int times = 1)
        {
            for (int i = 0;  i < times; i++)
            {
                CurrentIndex++;
                CurrentIndex %= typesCount;
            }
            // outside loop, cuz why do i need to update those each time?
            UpdateBlocks();
        }

        public void SetSequenceBlocks(int newIndex)
        {
            CurrentIndex = newIndex;
            UpdateBlocks();
        }
    }
}
