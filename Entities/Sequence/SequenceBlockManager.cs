using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/SequenceBlockManager")]

    [Tracked]

    public class SequenceBlockManager(EntityData data, Vector2 offset, EntityID id) : Entity(data.Position + offset)
    {
        private readonly int startWith = data.Int("startWith");

        public int typesCount = -1;

        public bool everyDash = data.Bool("everyDash");
        public int SequenceGroup = data.Int("sequenceGroup");

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

        public int LocalCurrentIndex
        {
            get
            {
                return DzhakeHelperModule.Session.ActiveSequenceIndexes.TryGetValue(SequenceGroup, out int index) ? index : 0;
            }
            set
            {
                DzhakeHelperModule.Session.ActiveSequenceIndexes[SequenceGroup] = value;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            foreach (SequenceBlock entity in scene.Tracker.GetEntities<SequenceBlock>())
                if (entity.ID.Level == ID.Level && entity.SequenceGroup == SequenceGroup && entity.Index > typesCount)
                    typesCount = entity.Index;

            foreach (ManualSequenceComponent component in scene.Tracker.GetComponents<ManualSequenceComponent>())
                if (component.SequenceGroup == SequenceGroup)
                    typesCount = Math.Max(component.Index, typesCount);

            typesCount++; //index is 0-3 and typesCount is 1-4
            if (typesCount == 1) typesCount++; // 2 is minimum cuz why not

            LocalCurrentIndex = startWith;

            UpdateBlocks();
        }


        public override void Update()
        {
            Level? level = Scene as Level;
            if (level?.Session is null) return;

            for (int i = 0; i < 4; i++)
            {
                //if this is not active color and there is flag
                if (i != LocalCurrentIndex && (level.Session.GetFlag($"DzhakeHelper_Sequence_G{SequenceGroup}_{i}") || (SequenceGroup == 0 && level.Session.GetFlag($"DzhakeHelper_Sequence_{i}"))))
                {
                    SetSequenceBlocks(i);
                    break;
                }

                continue;
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
                if (entity.SequenceGroup == SequenceGroup)
                    entity.Activated = entity.Index == LocalCurrentIndex;

            foreach (ManualSequenceComponent component in Scene.Tracker.GetComponents<ManualSequenceComponent>())
                if (component.SequenceGroup == SequenceGroup)
                    component.Activated = component.Index == LocalCurrentIndex;

            foreach (SequenceSwitchBlock switchBlock in Scene.Tracker.GetEntities<SequenceSwitchBlock>())
                if (switchBlock.SequenceGroup == SequenceGroup)
                    switchBlock.NextColor(LocalCurrentIndex, false);

            Session? session = (Scene as Level)?.Session;
            if (session is null) return;

            if (SequenceGroup == 0)
                for (int i = 0; i < 4; i++)
                    session.SetFlag($"DzhakeHelper_Sequence_{i}", i == LocalCurrentIndex);

            for (int i = 0; i < 4; i++)
                session.SetFlag($"DzhakeHelper_Sequence_G{SequenceGroup}_{i}", i == LocalCurrentIndex);
        }

        public void CycleSequenceBlocks(int times = 1)
        {
            LocalCurrentIndex = (LocalCurrentIndex + times) % typesCount;
            UpdateBlocks();
        }

        public void SetSequenceBlocks(int newIndex)
        {
            LocalCurrentIndex = newIndex;
            UpdateBlocks();
        }
    }
}
