using System;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/SequenceBlockManager")]
    
    [Tracked]

    public class SequenceBlockManager : Entity
    {
        private readonly int startWith;

        public int typesCount = -1;

        public bool everyDash;

        public EntityID ID;

        public static int CurrentIndex
        {
            get { return DzhakeHelperModule.Session.ActiveSequenceIndex; }
            set { DzhakeHelperModule.Session.ActiveSequenceIndex = value; }
        }

        public SequenceBlockManager(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            startWith = data.Int("startWith");
            everyDash = data.Bool("everyDash");
            ID = id;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Util.Log("awake");

            foreach (SequenceBlock entity in scene.Tracker.GetEntities<SequenceBlock>())
                if (entity.ID.Level == ID.Level && entity.Index > typesCount)
                    typesCount = entity.Index;

            foreach (ManualSequenceComponent component in scene.Tracker.GetComponents<ManualSequenceComponent>())
                typesCount = Math.Max(component.Index, typesCount);

            foreach (SequenceComponent component1 in scene.Tracker.GetComponents<SequenceComponent>())
                typesCount = Math.Max(component1.Index, typesCount);

            typesCount++; //index is 0-3 and typesCount is 1-4
            if (typesCount == 1) typesCount++; // 2 is minimum cuz why not

            CurrentIndex = startWith;
            Util.Log($"typesCount: {typesCount}");

            UpdateBlocks();
        }


        public override void Update()
        {
            if (Scene is Level level && level.Session != null)
                for (int i = 0; i < 4; i++)
                {
                    if (i == CurrentIndex || !level.Session.GetFlag($"DzhakeHelper_Sequence_{i}")) continue;
                    //if this is not active color and there is flag
                    SetSequenceBlocks(i);
                    break;
                }
            

            base.Update();
        }


        public void UpdateBlocks()
        {
            foreach (SequenceBlock entity in Scene.Tracker.GetEntities<SequenceBlock>())
                entity.Activated = entity.Index == CurrentIndex;

            foreach (ManualSequenceComponent component in Scene.Tracker.GetComponents<ManualSequenceComponent>())
                component.Activated = component.Index == CurrentIndex;

            foreach (SequenceSwitchBlock switchBlock in Scene.Tracker.GetEntities<SequenceSwitchBlock>())
                switchBlock.NextColor(CurrentIndex, false);

            foreach (SequenceComponent component1 in Scene.Tracker.GetComponents<SequenceComponent>())
                component1.Activated = component1.Index == CurrentIndex;

            for (int i = 0; i < 4; i++)
            {
                (Scene as Level)?.Session?.SetFlag($"DzhakeHelper_Sequence_{i}", false);
            }
            (Scene as Level)?.Session?.SetFlag($"DzhakeHelper_Sequence_{CurrentIndex}");
            Util.Log($"typesCount: {typesCount}, CurrentIndex: {CurrentIndex}");
        }

        public void CycleSequenceBlocks(int times = 1)
        {
            CurrentIndex = (CurrentIndex + times) % typesCount;
            UpdateBlocks();
        }

        public void SetSequenceBlocks(int newIndex)
        {
            CurrentIndex = newIndex;
            UpdateBlocks();
        }
    }
}
