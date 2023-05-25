namespace Game {
    class GigavoltModLoader : ModLoader {
        public override void __ModInitialize() {
            ModsManager.RegisterHook("OnProjectDisposed", this);
        }

        public override void OnProjectDisposed() {
            foreach (SoundGeneratorGVElectricElement element in GVStaticStorage.GVSGCFEEList) {
                if (element.m_sound != null) {
                    if (element.m_playing) {
                        element.m_sound.Stop();
                    }
                    element.m_sound.Dispose();
                }
            }
            GVStaticStorage.GVSGCFEEList.Clear();
        }
    }
}