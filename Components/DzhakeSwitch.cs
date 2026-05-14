using Monocle;
using System;

namespace Celeste.Mod.DzhakeHelper.Entities;

[Tracked(false)]
public class DzhakeSwitch : Component
{
    public bool GroundReset;

    public Action OnActivate;

    public Action OnDeactivate;

    public Action OnFinish;

    public Action OnStartFinished;

    public bool Activated { get; private set; }

    public bool Finished { get; private set; }

    public int Group;
    public int SequenceGroup;

    public DzhakeSwitch(EntityData data)
        : base(active: true, visible: false)
    {
        GroundReset = data.Bool("groundReset");
        Group = data.Int("group");
        SequenceGroup = data.Int("sequenceGroup");
    }

    public override void EntityAdded(Scene scene)
    {
        base.EntityAdded(scene);
    }



    public override void Update()
    {
        base.Update();
        if (GroundReset && Activated && !Finished)
        {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.OnGround())
            {
                Deactivate();
            }
        }
    }


    public bool Activate()
    {
        if (!Finished && !Activated)
        {
            Activated = true;
            if (OnActivate != null)
            {
                OnActivate();
            }
            return FinishedGroupCheck(SceneAs<Level>());
        }
        return false;
    }

    public void Deactivate()
    {
        if (!Finished && Activated)
        {
            Activated = false;
            if (OnDeactivate != null)
            {
                OnDeactivate();
            }
        }
    }

    public void Finish()
    {
        Finished = true;
        if (OnFinish != null)
        {
            OnFinish();
        }
    }

    public void StartFinished()
    {
        if (!Finished)
        {
            bool finished = (Activated = true);
            Finished = finished;
            if (OnStartFinished != null)
            {
                OnStartFinished();
            }
        }
    }


    private bool FinishedGroupCheck(Level level)
    {
        foreach (SequenceTouchSwitch entity in level.Tracker.GetEntities<SequenceTouchSwitch>())
        {
            if (entity.Group == Group && entity.SequenceGroup == SequenceGroup && !entity.Switch.Activated)
            {
                return false;
            }
        }
        foreach (DzhakeSwitch component2 in level.Tracker.GetComponents<DzhakeSwitch>())
        {
            if (component2.Group == Group && component2.SequenceGroup == SequenceGroup)
            {
                component2.Finish();
            }
        }

        foreach (Entity? managerEnt in Scene.Tracker.GetEntities<SequenceBlockManager>())
            if (managerEnt is SequenceBlockManager manager && manager.SequenceGroup == SequenceGroup) manager.CycleSequenceBlocks();

        return true;
    }
}
