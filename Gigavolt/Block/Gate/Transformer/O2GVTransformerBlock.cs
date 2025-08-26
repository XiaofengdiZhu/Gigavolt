using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class O2GVTransformerBlock : RotateableMountedGVElectricElementBlock, IElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 844;
        public readonly Texture2D texture;

        public O2GVTransformerBlock() : base("Models/GigavoltGates", "AndGate", 0.5f) =>
            texture = ContentManager.Get<Texture2D>("Textures/O2GVTransformer");

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) => new O2GVTransformerGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem,
            int value,
            int face,
            int connectorFace,
            int x,
            int y,
            int z,
            Terrain terrain) {
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(
                    GetFace(value),
                    GetRotation(Terrain.ExtractData(value)),
                    connectorFace
                );
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
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
            generator.GenerateWireVertices(
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

        public ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z) =>
            new O2GVTransformerElectricElement(subsystemElectricity, new CellFace(x, y, z, GetFace(value)));

        public ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain,
            int value,
            int face,
            int connectorFace,
            int x,
            int y,
            int z) {
            if (GetFace(value) == face) {
                ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(
                    GetFace(value),
                    GetRotation(Terrain.ExtractData(value)),
                    connectorFace
                );
                if (connectorDirection == ElectricConnectorDirection.Bottom) {
                    return ElectricConnectorType.Input;
                }
            }
            return null;
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
                texture,
                color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public List<int> GetCustomWheelPanelValues(int centerValue) => IGVCustomWheelPanelBlock.TransformerValues;
    }
}