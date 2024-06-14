using Engine;
using Engine.Graphics;

namespace Game {
    public class GVInventoryControllerBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 886;
        Texture2D texture;

        public override void Initialize() {
            base.Initialize();
            texture = ContentManager.Get<Texture2D>("Textures/GVInventoryControllerBlock");
        }

        public GVInventoryControllerBlock() : base("Models/GigavoltGates", "AnalogToDigitalConverter", 0.375f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new InventoryControllerGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

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

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                switch (connectorDirection) {
                    case GVElectricConnectorDirection.Top: return GVElectricConnectorType.Output;
                    case GVElectricConnectorDirection.Right:
                    case GVElectricConnectorDirection.Left:
                    case GVElectricConnectorDirection.In:
                    case GVElectricConnectorDirection.Bottom: return GVElectricConnectorType.Input;
                }
            }
            return null;
        }
    }
}