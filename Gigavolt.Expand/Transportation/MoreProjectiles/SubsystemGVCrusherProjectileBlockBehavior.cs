using System;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVCrusherProjectileBlockBehavior : SubsystemBlockBehavior {
        public SubsystemBlockBehaviors m_subsystemBlockBehaviors;
        public override int[] HandledBlocks => [GVCrusherProjectileBlock.Index];

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemBlockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(true);
        }

        public override bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem) {
            if (cellFace.HasValue) {
                if (Terrain.ExtractData(worldItem.Value) == 0) {
                    SubsystemTerrain.DestroyCell(
                        int.MaxValue,
                        cellFace.Value.X,
                        cellFace.Value.Y,
                        cellFace.Value.Z,
                        0,
                        false,
                        false
                    );
                }
                else {
                    try {
                        ComponentMiner componentMiner = new();
                        SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(SubsystemTerrain.Terrain.GetCellValue(cellFace.Value.X, cellFace.Value.Y, cellFace.Value.Z)));
                        for (int i = 0; i < blockBehaviors.Length; i++) {
                            blockBehaviors[i].OnInteract(new TerrainRaycastResult { CellFace = cellFace.Value }, componentMiner);
                        }
                    }
                    catch (Exception e) {
                        Log.Error(e);
                    }
                }
                return true;
            }
            return false;
        }
    }
}