using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;

namespace Game {
    public class GVBatteryBlock : GVBaseBlock, IGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 818;

        public readonly BlockMesh m_standaloneBlockMesh = new();

        public readonly BlockMesh m_blockMesh = new();

        public readonly BoundingBox[] m_collisionBoxes = new BoundingBox[1];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Battery");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Battery").ParentBone);
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Battery").MeshParts[0],
                boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f),
                false,
                false,
                false,
                false,
                Color.White
            );
            m_blockMesh.AppendModelMeshPart(
                model.FindMesh("Battery").MeshParts[0],
                boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f),
                false,
                false,
                false,
                false,
                Color.White
            );
            m_collisionBoxes[0] = m_blockMesh.CalculateBoundingBox();
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            showDebris = true;
            if (toolLevel >= RequiredToolLevel) {
                int data = Terrain.ExtractData(oldValue);
                dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, data), Count = 1 });
            }
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) => m_collisionBoxes;

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            generator.GenerateMeshVertices(
                this,
                x,
                y,
                z,
                m_blockMesh,
                Color.White,
                null,
                geometry.SubsetOpaque
            );
            GVBlockGeometryGenerator.GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                4,
                0.72f,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                color,
                1f * size,
                ref matrix,
                environmentData
            );
        }

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new BatteryGVElectricElement(subsystemGVElectricity, value, new GVCellFace(x, y, z, 4), subterrainId);

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, Terrain terrain) {
            if (face == 4
                && SubsystemGVElectricity.GetConnectorDirection(4, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;
        public override bool IsNonDuplicable_(int value) => ((Terrain.ExtractData(value) >> 1) & 4095) > 0;

        public int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVBatteryBlockBehavior subsystem = project.FindSubsystem<SubsystemGVBatteryBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0 ? centerValue : subsystem.SetIdToValue(centerValue, subsystem.StoreItemDataAtUniqueId((GigaVoltageLevelData)subsystem.GetItemData(id).Copy()));
        }
    }
}