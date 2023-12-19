using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVWireHarnessBlock : GenerateGVWireVerticesBlock, IGVElectricWireElementBlock {
        public const int Index = 890;
        public readonly BlockMesh m_standaloneBlockMesh = new();
        public readonly BoundingBox[] m_collisionBoxesByFace = new BoundingBox[6];
        public static readonly Color WireColor1 = Color.Orange;
        public static readonly Color WireColor2 = Color.LightRed;

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Wire");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Wire").ParentBone);
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Wire").MeshParts[0],
                boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f),
                false,
                false,
                false,
                false,
                Color.White
            );
            m_standaloneBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(0.9375f, 0f, 0f));
            for (int i = 0; i < 6; i++) {
                Vector3 v = CellFace.FaceToVector3(i);
                Vector3 v2 = new Vector3(0.5f, 0.5f, 0.5f) - 0.5f * v;
                Vector3 v3;
                Vector3 v4;
                if (v.X != 0f) {
                    v3 = new Vector3(0f, 1f, 0f);
                    v4 = new Vector3(0f, 0f, 1f);
                }
                else if (v.Y != 0f) {
                    v3 = new Vector3(1f, 0f, 0f);
                    v4 = new Vector3(0f, 0f, 1f);
                }
                else {
                    v3 = new Vector3(1f, 0f, 0f);
                    v4 = new Vector3(0f, 1f, 0f);
                }
                Vector3 v5 = v2 - 0.5f * v3 - 0.5f * v4;
                Vector3 v6 = v2 + 0.5f * v3 + 0.5f * v4 + 0.05f * v;
                m_collisionBoxesByFace[i] = new BoundingBox(Vector3.Min(v5, v6), Vector3.Max(v5, v6));
            }
        }

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => null;

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            if (!WireExistsOnFace(value, face)) {
                return null;
            }
            return GVElectricConnectorType.InputOutput;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public int GetConnectedWireFacesMask(int value, int face) {
            int num = 0;
            if (WireExistsOnFace(value, face)) {
                int num2 = CellFace.OppositeFace(face);
                bool flag = false;
                for (int i = 0; i < 6; i++) {
                    if (i == face) {
                        num |= 1 << i;
                    }
                    else if (i != num2
                        && WireExistsOnFace(value, i)) {
                        num |= 1 << i;
                        flag = true;
                    }
                }
                if (flag && WireExistsOnFace(value, num2)) {
                    num |= 1 << num2;
                }
            }
            return num;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            BoundingBox[] array = new BoundingBox[6];
            for (int i = 0; i < 6; i++) {
                array[i] = WireExistsOnFace(value, i) ? m_collisionBoxesByFace[i] : default;
            }
            return array;
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            for (int i = 0; i < 6; i++) {
                if (WireExistsOnFace(value, i)) {
                    GenerateGVWireVertices(
                        generator,
                        value,
                        x,
                        y,
                        z,
                        i,
                        0f,
                        Vector2.Zero,
                        geometry.SubsetOpaque
                    );
                }
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                color * WireColor1,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            Point3 point = CellFace.FaceToPoint3(raycastResult.CellFace.Face);
            int cellValue = subsystemTerrain.Terrain.GetCellValue(raycastResult.CellFace.X + point.X, raycastResult.CellFace.Y + point.Y, raycastResult.CellFace.Z + point.Z);
            int num = Terrain.ExtractContents(cellValue);
            Block block = BlocksManager.Blocks[num];
            int wireFacesBitmask = GetWireFacesBitmask(cellValue);
            int num2 = wireFacesBitmask | (1 << raycastResult.CellFace.Face);
            BlockPlacementData result;
            if (num2 != wireFacesBitmask
                || !(block is GVWireHarnessBlock)) {
                result = default;
                result.Value = SetWireFacesBitmask(value, num2);
                result.CellFace = raycastResult.CellFace;
                return result;
            }
            result = default;
            return result;
        }

        public override BlockPlacementData GetDigValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, int toolValue, TerrainRaycastResult raycastResult) {
            int wireFacesBitmask = GetWireFacesBitmask(value);
            wireFacesBitmask &= ~(1 << raycastResult.CollisionBoxIndex);
            BlockPlacementData result = default;
            result.Value = SetWireFacesBitmask(value, wireFacesBitmask);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            for (int i = 0; i < 6; i++) {
                if (WireExistsOnFace(oldValue, i)
                    && !WireExistsOnFace(newValue, i)) {
                    dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, 0), Count = 1 });
                }
            }
            showDebris = dropValues.Count > 0;
        }

        public static bool WireExistsOnFace(int value, int face) => (GetWireFacesBitmask(value) & (1 << face)) != 0;

        public static int GetWireFacesBitmask(int value) {
            if (Terrain.ExtractContents(value) == Index) {
                return Terrain.ExtractData(value) & 0x3F;
            }
            return 0;
        }

        public static int SetWireFacesBitmask(int value, int bitmask) {
            int num = Terrain.ExtractData(value);
            num &= -64;
            num |= bitmask & 0x3F;
            return Terrain.ReplaceData(Terrain.ReplaceContents(value, Index), num);
        }

        public bool IsWireHarness(int value) => true;
    }
}