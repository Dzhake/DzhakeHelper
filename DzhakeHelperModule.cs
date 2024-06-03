using System;
//test commit
namespace Celeste.Mod.DzhakeHelper {
    public class DzhakeHelperModule : EverestModule {
        public static DzhakeHelperModule Instance;

        public override Type SettingsType => typeof(DzhakeHelperModuleSettings);
        public static DzhakeHelperModuleSettings Settings => (DzhakeHelperModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(DzhakeHelperModuleSession);
        public static DzhakeHelperModuleSession Session => (DzhakeHelperModuleSession) Instance._Session;

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