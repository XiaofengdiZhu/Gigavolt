using Engine;
using Engine.Graphics;

namespace Game {
    public class GVPlayerMonitorBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 893;
        Texture2D texture;

        public override void Initialize() {
            base.Initialize();
            texture = ContentManager.Get<Texture2D>("Textures/GVPlayerMonitorBlock");
        }

        public GVPlayerMonitorBlock() : base("Models/GigavoltGates", "AnalogToDigitalConverter", 0.375f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new PlayerMonitorGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                texture,
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
                geometry.GetGeometry(texture).SubsetOpaque
            );
            GenerateGVWireVertices(
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

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Bottom
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Left
                    || connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Top) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }
    }
}