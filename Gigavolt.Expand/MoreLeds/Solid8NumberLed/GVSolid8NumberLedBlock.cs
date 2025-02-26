using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVSolid8NumberLedBlock : CubeBlock, IGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 887;

        public Texture2D m_texture;
        public Texture2D m_textureFull;

        public override void Initialize() {
            m_texture = ContentManager.Get<Texture2D>("Textures/GVSolid8NumberLedBlock");
            m_textureFull = ContentManager.Get<Texture2D>("Textures/GVSolid8NumberLedBlockFull");
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            generator.GenerateCubeVertices(
                this,
                value,
                x,
                y,
                z,
                Color.White,
                geometry.GetGeometry(m_texture).OpaqueSubsetsByFace
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            if (environmentData.DrawBlockMode == DrawBlockMode.UI) {
                color = new Color(158, 158, 158);
            }
            BlocksManager.DrawCubeBlock(
                primitivesRenderer,
                value,
                new Vector3(size),
                1f,
                ref matrix,
                color,
                color,
                environmentData,
                m_textureFull
            );
        }

        public override int GetTextureSlotCount(int value) => 1;

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new Solid8NumberLedGVElectricElement(subsystemGVElectricity, new Point3(x, y, z), subterrainId);

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, Terrain terrain) => GVElectricConnectorType.Input;

        public int GetConnectionMask(int value) => int.MaxValue;

        public List<int> GetCustomWheelPanelValues(int centerValue) => IGVCustomWheelPanelBlock.LedValues;
    }
}