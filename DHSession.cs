using Celeste.Mod.DzhakeHelper.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.DzhakeHelper
{
    public class DHSession : EverestModuleSession
    {
        //Sequence
        public bool HasSequenceDash { get; set; } = false;

        public int ActiveSequenceIndex
        {
            get
            {
                return ActiveSequenceIndexes.TryGetValue(0, out int index) ? index : 0;
            }
            set
            {
                ActiveSequenceIndexes[0] = value;
            }
        }

        public Dictionary<int, int> ActiveSequenceIndexes = new();

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
