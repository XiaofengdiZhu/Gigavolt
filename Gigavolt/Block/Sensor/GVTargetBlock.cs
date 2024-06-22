using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVTargetBlock : MountedGVElectricElementBlock {
        public const int Index = 860;

        public readonly BoundingBox[][] m_boundingBoxes = { new[] { new BoundingBox(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 0.0625f)) }, new[] { new BoundingBox(new Vector3(0f, 0f, 0f), new Vector3(0.0625f, 1f, 1f)) }, new[] { new BoundingBox(new Vector3(0f, 0f, 0.9375f), new Vector3(1f, 1f, 1f)) }, new[] { new BoundingBox(new Vector3(0.9375f, 0f, 0f), new Vector3(1f, 1f, 1f)) } };

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int mountingFace = GetMountingFace(Terrain.ExtractData(value));
            if (mountingFace >= 0
                && mountingFace < 4) {
                return m_boundingBoxes[mountingFace];
            }
            return base.GetCustomCollisionBoxes(terrain, value);
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            if (raycastResult.CellFace.Face < 4) {
                result.CellFace = raycastResult.CellFace;
                int data = Terrain.ExtractData(value);
                result.Value = Terrain.MakeBlockValue(Index, 0, SetMountingFace(SetClassic(data, GetClassic(data)), raycastResult.CellFace.Face));
            }
            return result;
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            TerrainGeometrySubset subsetAlphaTest = geometry.SubsetAlphaTest;
            DynamicArray<TerrainVertex> vertices = subsetAlphaTest.Vertices;
            DynamicArray<int> indices = subsetAlphaTest.Indices;
            int count = vertices.Count;
            int data = Terrain.ExtractData(value);
            int num = Terrain.ExtractLight(value);
            int mountingFace = GetMountingFace(data);
            float s = LightingManager.LightIntensityByLightValueAndFace[num + 16 * mountingFace];
            Color color = Color.MultiplyColorOnly(Color.White, s);
            switch (mountingFace) {
                case 2:
                    vertices.Count += 4;
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x,
                        y,
                        z + 1,
                        color,
                        DefaultTextureSlot,
                        0,
                        ref vertices.Array[count]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x + 1,
                        y,
                        z + 1,
                        color,
                        DefaultTextureSlot,
                        1,
                        ref vertices.Array[count + 1]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x + 1,
                        y + 1,
                        z + 1,
                        color,
                        DefaultTextureSlot,
                        2,
                        ref vertices.Array[count + 2]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x,
                        y + 1,
                        z + 1,
                        color,
                        DefaultTextureSlot,
                        3,
                        ref vertices.Array[count + 3]
                    );
                    indices.Add(count);
                    indices.Add(count + 1);
                    indices.Add(count + 2);
                    indices.Add(count + 2);
                    indices.Add(count + 1);
                    indices.Add(count);
                    indices.Add(count + 2);
                    indices.Add(count + 3);
                    indices.Add(count);
                    indices.Add(count);
                    indices.Add(count + 3);
                    indices.Add(count + 2);
                    break;
                case 3:
                    vertices.Count += 4;
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x + 1,
                        y,
                        z,
                        color,
                        DefaultTextureSlot,
                        0,
                        ref vertices.Array[count]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x + 1,
                        y + 1,
                        z,
                        color,
                        DefaultTextureSlot,
                        3,
                        ref vertices.Array[count + 1]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x + 1,
                        y + 1,
                        z + 1,
                        color,
                        DefaultTextureSlot,
                        2,
                        ref vertices.Array[count + 2]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x + 1,
                        y,
                        z + 1,
                        color,
                        DefaultTextureSlot,
                        1,
                        ref vertices.Array[count + 3]
                    );
                    indices.Add(count);
                    indices.Add(count + 1);
                    indices.Add(count + 2);
                    indices.Add(count + 2);
                    indices.Add(count + 1);
                    indices.Add(count);
                    indices.Add(count + 2);
                    indices.Add(count + 3);
                    indices.Add(count);
                    indices.Add(count);
                    indices.Add(count + 3);
                    indices.Add(count + 2);
                    break;
                case 0:
                    vertices.Count += 4;
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x,
                        y,
                        z,
                        color,
                        DefaultTextureSlot,
                        0,
                        ref vertices.Array[count]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x + 1,
                        y,
                        z,
                        color,
                        DefaultTextureSlot,
                        1,
                        ref vertices.Array[count + 1]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x + 1,
                        y + 1,
                        z,
                        color,
                        DefaultTextureSlot,
                        2,
                        ref vertices.Array[count + 2]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x,
                        y + 1,
                        z,
                        color,
                        DefaultTextureSlot,
                        3,
                        ref vertices.Array[count + 3]
                    );
                    indices.Add(count);
                    indices.Add(count + 2);
                    indices.Add(count + 1);
                    indices.Add(count + 1);
                    indices.Add(count + 2);
                    indices.Add(count);
                    indices.Add(count + 2);
                    indices.Add(count);
                    indices.Add(count + 3);
                    indices.Add(count + 3);
                    indices.Add(count);
                    indices.Add(count + 2);
                    break;
                case 1:
                    vertices.Count += 4;
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x,
                        y,
                        z,
                        color,
                        DefaultTextureSlot,
                        0,
                        ref vertices.Array[count]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x,
                        y + 1,
                        z,
                        color,
                        DefaultTextureSlot,
                        3,
                        ref vertices.Array[count + 1]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x,
                        y + 1,
                        z + 1,
                        color,
                        DefaultTextureSlot,
                        2,
                        ref vertices.Array[count + 2]
                    );
                    BlockGeometryGenerator.SetupLitCornerVertex(
                        x,
                        y,
                        z + 1,
                        color,
                        DefaultTextureSlot,
                        1,
                        ref vertices.Array[count + 3]
                    );
                    indices.Add(count);
                    indices.Add(count + 2);
                    indices.Add(count + 1);
                    indices.Add(count + 1);
                    indices.Add(count + 2);
                    indices.Add(count);
                    indices.Add(count + 2);
                    indices.Add(count);
                    indices.Add(count + 3);
                    indices.Add(count + 3);
                    indices.Add(count);
                    indices.Add(count + 2);
                    break;
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawFlatOrImageExtrusionBlock(
                primitivesRenderer,
                value,
                size,
                ref matrix,
                null,
                color,
                false,
                environmentData
            );
        }

        public static int GetMountingFace(int data) => data & 3;

        public static int SetMountingFace(int data, int face) => (data & -4) | (face & 3);

        public override int GetFace(int value) => GetMountingFace(Terrain.ExtractData(value));

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new TargetGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId, GetClassic(Terrain.ExtractData(value)));

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => LanguageControl.Get(GetType().Name, GetClassic(Terrain.ExtractData(value)) ? "ClassicDisplayName" : "DisplayName");
        public override string GetDescription(int value) => LanguageControl.Get(GetType().Name, GetClassic(Terrain.ExtractData(value)) ? "ClassicDescription" : "Description");
        public override string GetCategory(int value) => GetClassic(Terrain.ExtractData(value)) ? "GV Electrics Regular" : "GV Electrics Shift";
        public override int GetDisplayOrder(int value) => GetClassic(Terrain.ExtractData(value)) ? 14 : 8;
        public override IEnumerable<int> GetCreativeValues() => [Index, Terrain.MakeBlockValue(Index, 0, SetClassic(0, true))];
        public static bool GetClassic(int data) => (data & 4) != 0;
        public static int SetClassic(int data, bool classic) => (data & -5) | (classic ? 4 : 0);
    }
}