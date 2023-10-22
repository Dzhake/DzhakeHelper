﻿using Celeste.Mod.DzhakeHelper.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DzhakeHelper.Triggers
{
    [CustomEntity("DzhakeHelper/SetCossetteBlocksTrigger")]

    internal class SetCossetteBlocksTrigger : Trigger
    {

        public int newIndex;

        private bool triggered = false;

        private bool OneUse;

        public SetCossetteBlocksTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            newIndex = data.Int("newIndex");
            OneUse = data.Bool("oneUse");
        }

        public override void OnEnter(Player player)
        {
            if (!(triggered && OneUse)) 
            {

                base.Scene.Tracker.GetEntity<CossetteBlockManager>()?.SetCossetteBlocks(newIndex);
                triggered = true;

            }
        }
    }
}
