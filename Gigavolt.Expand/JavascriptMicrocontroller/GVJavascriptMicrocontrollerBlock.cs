using Engine;
using Engine.Graphics;

namespace Game {
    public class GVJavascriptMicrocontrollerBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 889;
        Texture2D texture;

        public override void Initialize() {
            base.Initialize();
            texture = ContentManager.Get<Texture2D>("Textures/GVJavascriptMicrocontrollerBlock");
        }

        public GVJavascriptMicrocontrollerBlock() : base("Models/GigavoltGates", "AnalogToDigitalConverter", 0.5f) { }

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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new JavascriptMicrocontrollerGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            GVJavascriptMicrocontrollerData blockData = subsystem.Project.FindSubsystem<SubsystemGVJavascriptMicrocontrollerBlockBehavior>(true).GetBlockData(new Point3(x, y, z));
            if (blockData == null) {
                return null;
            }
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(Terrain.ExtractData(value)), connectorFace);
                if (connectorDirection.HasValue) {
                    int type = blockData.m_portsDefinition[(int)connectorDirection.Value];
                    return type switch {
                        0 => GVElectricConnectorType.Input,
                        1 => GVElectricConnectorType.Output,
                        _ => null
                    };
                }
            }
            return null;
        }
    }
}