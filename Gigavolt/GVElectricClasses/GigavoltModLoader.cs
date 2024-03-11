namespace Game {
    public class GigavoltModLoader : ModLoader {
        public override void __ModInitialize() {
            ModsManager.RegisterHook("OnProjectDisposed", this);
            ModsManager.RegisterHook("ToFreeChunks", this);
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
            GVStaticStorage.PreventChunkFromBeingFree = false;
            GVStaticStorage.GVUsingChunks.Clear();
        }

        public override void ToFreeChunks(TerrainUpdater terrainUpdater, TerrainChunk chunk, out bool KeepWorking) {
            KeepWorking = GVStaticStorage.PreventChunkFromBeingFree && GVStaticStorage.GVUsingChunks.Contains(chunk.Coords);
        }
    }
}