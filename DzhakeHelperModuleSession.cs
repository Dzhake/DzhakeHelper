using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using Celeste.Mod.DzhakeHelper.Entities;

namespace Celeste.Mod.DzhakeHelper {
    public class DzhakeHelperModuleSession : EverestModuleSession {
        public bool HasSequenceDash { get; set; } = false;

        public Dictionary<string, object> StoredVariables = new();

        public float TimedKillTriggerTime { get; set; } = 0f;

        public bool TimedKillTriggerTimeChanged { get; set; } = false;

        public float TimedKillTriggerMaxTime { get; set; } = 0f;

        public Color TimedKillTriggerColor { get; set; } = Color.White;

        public int ActiveSequenceIndex;

        public List<CustomKey> CurrentKeys { get; set; } = new();
    }
}
