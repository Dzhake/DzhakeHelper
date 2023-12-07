﻿using System;
using Monocle;

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

    public DzhakeSwitch(EntityData data)
        : base(active: true, visible: false)
    {
        GroundReset = data.Bool("groundReset");
        Group = data.Int("group");
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

    public static bool Check(Scene scene)
    {
        return scene.Tracker.GetComponent<DzhakeSwitch>()?.Finished ?? false;
    }


    private bool FinishedGroupCheck(Level level)
    {
        foreach (SequenceTouchSwitch entity in level.Tracker.GetEntities<SequenceTouchSwitch>())
        {
            if (entity.Group == Group && !entity.Switch.Activated)
            {
                return false;
            }
        }
        foreach (DzhakeSwitch component2 in level.Tracker.GetComponents<DzhakeSwitch>())
        {
            if (component2.Group == Group)
            {
                component2.Finish();
            }
        }
        base.Scene.Tracker.GetEntity<SequenceBlockManager>()?.CycleSequenceBlocks();
        return true;
    }
}
