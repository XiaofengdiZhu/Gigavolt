using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVCopperHammerBlockBehavior : SubsystemBlockBehavior {
        SubsystemAudio m_subsystemAudio;
        public override int[] HandledBlocks => new int[0];

        public override bool OnUse(Ray3 ray, ComponentMiner componentMiner) {
            TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
            if (terrainRaycastResult.HasValue) {
                bool flag = false;
                CellFace cellFace = terrainRaycastResult.Value.CellFace;
                int value = terrainRaycastResult.Value.Value;
                int contents = Terrain.ExtractContents(value);
                if (contents == GVWireBlock.Index) {
                    flag = true;
                    SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, Terrain.MakeBlockValue(GVEWireThroughBlock.Index, Terrain.ExtractLight(value), Terrain.ExtractData(value)));
                }
                else if (contents == GVEWireThroughBlock.Index) {
                    flag = true;
                    int data = Terrain.ExtractData(value);
                    int type = GVEWireThroughBlock.GetType(data);
                    SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, type < 3 ? Terrain.ReplaceData(value, GVEWireThroughBlock.SetType(data, type + 1)) : Terrain.MakeBlockValue(GVWireBlock.Index, Terrain.ExtractLight(value), GVEWireThroughBlock.SetType(data, 0)));
                }
                if (flag) {
                    m_subsystemAudio.PlaySound(
                        "Audio/Click",
                        1f,
                        0f,
                        new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                        2f,
                        true
                    );
                }
                return true;
            }
            return false;
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
            base.Load(valuesDictionary);
        }
    }
}