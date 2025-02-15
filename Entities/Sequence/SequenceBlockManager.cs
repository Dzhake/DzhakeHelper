using System;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/SequenceBlockManager")]
    
    [Tracked]

    public class SequenceBlockManager(EntityData data, Vector2 offset, EntityID id) : Entity(data.Position + offset)
    {
        private readonly int startWith = data.Int("startWith");

        public int typesCount = -1;

        public bool everyDash = data.Bool("everyDash");

        public EntityID ID = id;

        public string Flag = data.Attr("flag");
        private bool flagEnabled;
        public bool CycleWhenFlagOn = data.Bool("cycleWhenFlagOn");
        public bool CycleWhenFlagOff = data.Bool("cycleWhenFlagOff");

        public static int CurrentIndex
        {
            get { return DzhakeHelperModule.Session.ActiveSequenceIndex; }
            set { DzhakeHelperModule.Session.ActiveSequenceIndex = value; }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

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

            UpdateBlocks();
        }


        public override void Update()
        {
            Level? level = Scene as Level;
            if (level?.Session is null) return;

            for (int i = 0; i < 4; i++)
            {
                //if this is not active color and there is flag
                if (i == CurrentIndex || !level.Session.GetFlag($"DzhakeHelper_Sequence_{i}")) continue;
                SetSequenceBlocks(i);
                break;
            }

            bool flag = Util.ParseFlags(level, Flag);
            if (flag != flagEnabled)
            {
                flagEnabled = flag;
                if ((flagEnabled && CycleWhenFlagOn) || (!flagEnabled && CycleWhenFlagOff)) CycleSequenceBlocks();
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
            
            Session? session = (Scene as Level)?.Session;
            if (session is null) return;

            for (int i = 0; i < 4; i++)
                session.SetFlag($"DzhakeHelper_Sequence_{i}", i == CurrentIndex);
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
