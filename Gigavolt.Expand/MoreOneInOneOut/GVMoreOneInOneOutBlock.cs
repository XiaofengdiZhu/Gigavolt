using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVMoreOneInOneOutBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 881;
        public readonly Texture2D[] textures = new Texture2D[16];

        public readonly string[] textureNames = [
            "Sin",
            "Cos",
            "Tan",
            "Cot",
            "Sec",
            "Csc",
            "Asin",
            "Acos",
            "Atan",
            "Sinh",
            "Cosh",
            "Tanh",
            "Deg2Rad",
            "Rad2Deg",
            "SMR",
            "TCR"
        ];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 16; i++) {
                textures[i] = ContentManager.Get<Texture2D>($"Textures/GVMoreOneInOneOutBlock/{textureNames[i]}");
            }
        }

        public GVMoreOneInOneOutBlock() : base("Models/GigavoltGates", "AndGate", 0.5f) { }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer,
            int value,
            Color color,
            float size,
            ref Matrix matrix,
            DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                textures[GetType(Terrain.ExtractData(value))],
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
                geometry.GetGeometry(textures[GetType(Terrain.ExtractData(value))]).SubsetOpaque
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
            uint subterrainId) => new MoreOneInOneOutGVElectricElement(
            subsystemGVElectricity,
            new GVCellFace(x, y, z, GetFace(value)),
            value,
            subterrainId
        );

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem,
            int value,
            int face,
            int connectorFace,
            int x,
            int y,
            int z,
            Terrain terrain) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection =
                    SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => LanguageControl.Get(
            GetType().Name,
            "DisplayName",
            GetType(Terrain.ExtractData(value)).ToString()
        );

        public override string GetDescription(int value) {
            string typeName = GetType().Name;
            string start = LanguageControl.Get(typeName, "DescriptionStart");
            string name = GetDisplayName(null, value);
            string end = LanguageControl.Get(typeName, "DescriptionEnd", GetType(Terrain.ExtractData(value)).ToString());
            return string.Format(LanguageControl.Get(typeName, "DescriptionFormat"), start, name, end);
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < 16; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, SetType(0, i));
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, SetType(data, GetType(data))), Count = 1 });
            showDebris = true;
        }

        public static int GetType(int data) => (data >> 5) & 15;

        public static int SetType(int data, int type) => (data & -481) | ((type & 15) << 5);
    }
}