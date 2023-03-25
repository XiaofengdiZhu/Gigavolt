using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
    public class GVBatteryBlock : GenerateGVWireVerticesBlock, IGVElectricElementBlock
    {
        public const int Index = 838;

        public BlockMesh m_standaloneBlockMesh = new BlockMesh();

        public BlockMesh m_blockMesh = new BlockMesh();

        public BoundingBox[] m_collisionBoxes = new BoundingBox[1];

        public override void Initialize()
        {
            Model model = ContentManager.Get<Model>("Models/Battery");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Battery").ParentBone);
            m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Battery").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
            m_blockMesh.AppendModelMeshPart(model.FindMesh("Battery").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
            m_collisionBoxes[0] = m_blockMesh.CalculateBoundingBox();
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
        {
            showDebris = true;
            if (toolLevel >= RequiredToolLevel)
            {
                int data = Terrain.ExtractData(oldValue);
                dropValues.Add(new BlockDropValue
                {
                    Value = Terrain.MakeBlockValue(Index, 0, data),
                    Count = 1
                });
            }
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
        {
            return m_collisionBoxes;
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            generator.GenerateMeshVertices(this, x, y, z, m_blockMesh, Color.White, null, geometry.SubsetOpaque);
            GenerateGVWireVertices(generator, value, x, y, z, 4, 0.72f, Vector2.Zero, geometry.SubsetOpaque);
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 1f * size, ref matrix, environmentData);
        }

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            try
            {
                return new BatteryGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, 4));
            }catch(System.Exception ex) { Log.Error(ex);return null; }
        }

        public GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            if (face == 4 && SubsystemGVElectricity.GetConnectorDirection(4, 0, connectorFace).HasValue)
            {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public int GetConnectionMask(int value)
        {
            return int.MaxValue;
        }
    }
}
