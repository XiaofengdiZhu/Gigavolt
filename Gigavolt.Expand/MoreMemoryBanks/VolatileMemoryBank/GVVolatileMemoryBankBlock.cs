using Engine;
using Engine.Graphics;
using GameEntitySystem;

namespace Game {
    public class GVVolatileMemoryBankBlock : GVMemoryBankBlock {
        public new const int Index = 871;
        public Texture2D m_texture;

        public GVVolatileMemoryBankBlock() => m_modelName = "Models/GVMemoryBank";

        public override void Initialize() {
            RotateableMountedGVElectricElementBlockInitialize();
            m_texture = ContentManager.Get<Texture2D>("Textures/GVVolatileMemoryBankBlock");
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer,
            int value,
            Color color,
            float size,
            ref Matrix matrix,
            DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                m_texture,
                color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value) & 0x1F;
            generator.GenerateMeshVertices(
                this,
                x,
                y,
                z,
                m_blockMeshes[num],
                Color.White,
                null,
                geometry.GetGeometry(m_texture).SubsetOpaque
            );
            GVBlockGeometryGenerator.GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                GetFace(value),
                m_centerBoxSize,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) => new VolatileMemoryBankGVElectricElement(
            subsystemGVElectricity,
            new GVCellFace(x, y, z, GetFace(value)),
            value,
            subterrainId
        );

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            SubsystemGVVolatileMemoryBankBlockBehavior subsystem =
                subsystemTerrain.Project.FindSubsystem<SubsystemGVVolatileMemoryBankBlockBehavior>(true);
            if (subsystem.GetIdFromValue(value) == 0) {
                value = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(new GVVolatileMemoryBankData()));
            }
            return base.GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
        }

        public override int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVVolatileMemoryBankBlockBehavior subsystem = project.FindSubsystem<SubsystemGVVolatileMemoryBankBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0
                ? centerValue
                : subsystem.SetIdToValue(centerValue, subsystem.StoreItemDataAtUniqueId((GVVolatileMemoryBankData)subsystem.GetItemData(id).Copy()));
        }
    }
}