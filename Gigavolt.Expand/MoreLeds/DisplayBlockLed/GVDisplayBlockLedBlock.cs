using Engine.Graphics;
using Engine;
using System.Collections.Generic;

namespace Game
{
    public class GVDisplayBlockLedBlock : RotateableMountedGVElectricElementBlock
    {
        public const int Index = 1014;
        Texture2D texture;
        Texture2D texture1;

        public override void Initialize()
        {
            base.Initialize();
            texture = ContentManager.Get<Texture2D>("Textures/GVDisplayBlockLedBlock");
            texture1 = ContentManager.Get<Texture2D>("Textures/GVDisplayBlockLedBlock1");
        }

        public GVDisplayBlockLedBlock()
            : base("Models/GVOneSurfaceBlock", "OneLed", 0.5f)
        {
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, GetType(Terrain.ExtractData(value)) == 1 ? texture1 : texture, color, 2f * size, ref matrix, environmentData);
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            int num = Terrain.ExtractData(value) & 0x1F;
            generator.GenerateMeshVertices(this, x, y, z, m_blockMeshes[num], Color.White, null, geometry.GetGeometry(GetType(Terrain.ExtractData(value)) == 1 ? texture1 : texture).SubsetOpaque);
            GenerateGVWireVertices(generator, value, x, y, z, GetFace(value), m_centerBoxSize, Vector2.Zero, geometry.SubsetOpaque);
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            return new DisplayBlockLedGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));
        }

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            int face2 = GetFace(value);
            if (face == face2 && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue)
            {
                return GVElectricConnectorType.Input;
            }

            return null;
        }

        public override IEnumerable<int> GetCreativeValues()
        {
            return new[]
            {
                Terrain.MakeBlockValue(Index, 0, 0),
                Terrain.MakeBlockValue(Index, 0, SetType(0, 1))
            };
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
        {
            return GetType(Terrain.ExtractData(value)) == 1 ? "GV复杂方块展示板" : "GV简单方块展示板";
        }

        public override string GetDescription(int value)
        {
            return GetType(Terrain.ExtractData(value)) == 1 ? "和简单方块展示板类似，但它可以控制绘制的方块的大小、位置、旋转、亮度、颜色，详见本Mod Github页面的介绍" : "输入等于要显示方块的值的电压，就会在其面前绘制该方块";
        }

        public static int StaticGetFace(int data)
        {
            return (data >> 2) & 7;
        }

        public static int GetType(int data)
        {
            return (data >> 5) & 1;
        }

        public static int SetType(int data, int type)
        {
            return (data & -33) | (type << 5);
        }
    }
}