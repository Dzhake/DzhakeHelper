using System;

namespace Celeste.Mod.DzhakeHelper {
    public class DzhakeHelperModule : EverestModule {
        public static DzhakeHelperModule Instance;

        public override Type SettingsType => typeof(DzhakeHelperModuleSettings);
        public static DzhakeHelperModuleSettings Settings => (DzhakeHelperModuleSettings)Instance._Settings;

        public override Type SessionType => typeof(DHSession);
        public static DHSession Session => (DHSession)Instance._Session;

        public override Type SaveDataType => typeof(DHSaveData);
        public static DHSaveData SaveData => (DHSaveData)Instance._SaveData;

        public DzhakeHelperModule() {
            Instance = this;
        }

        public override void Load() {
            DzhakeHelperHooks.Load();
        }

        public override void Unload() {
            DzhakeHelperHooks.Unload();
        }
    }
}