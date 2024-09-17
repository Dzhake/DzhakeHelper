using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.DzhakeHelper.Entities;

namespace Celeste.Mod.DzhakeHelper {
    public class DzhakeHelperModuleSession : EverestModuleSession {
        //Sequence
        public bool HasSequenceDash { get; set; } = false;
        public int ActiveSequenceIndex;

        //Lua
        public Dictionary<string, object> StoredVariables = new();

        //Timed Kill Trigger
        public float TimedKillTriggerTime { get; set; } = 0f;
        public bool TimedKillTriggerTimeChanged { get; set; } = false;
        public float TimedKillTriggerMaxTime { get; set; } = 0f;
        public Color TimedKillTriggerColor { get; set; } = Color.White;

        //Custom Keys
        public List<CustomKey.CustomKeyInfo> CurrentKeys { get; set; } = new();
    }
}
