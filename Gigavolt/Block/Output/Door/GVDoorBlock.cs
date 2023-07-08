using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDoorBlock : GenerateGVWireVerticesBlock, IGVElectricElementBlock {
        public float m_pivotDistance = 0.0625f;
        public const int Index = 864;

        public BlockMesh[] m_standaloneBlockMesh = new BlockMesh[3];
        public Matrix[] m_boneAbsoluteTransform = new Matrix[3];
        public ModelMeshPart[] m_modelMeshPart = new ModelMeshPart[3];
        public Dictionary<int, BlockMesh> m_cachedBlockMeshes = new Dictionary<int, BlockMesh>();
        public Dictionary<int, BoundingBox[]> m_cachedCollisionBoxes = new Dictionary<int, BoundingBox[]>();
        public string[] m_displayNamesByModel = { "GV木门", "GV铁门", "GV铁栅门" };
        public int[] m_creativeValuesByModel = { Terrain.MakeBlockValue(Index, 0, 0), Terrain.MakeBlockValue(Index, 0, SetModel(0, 1)), Terrain.MakeBlockValue(Index, 0, SetModel(0, 2)) };

        public override void Initialize() {
            base.Initialize();
            Model[] model = new Model[3];
            model[0] = ContentManager.Get<Model>("Models/WoodenDoor");
            model[1] = ContentManager.Get<Model>("Models/IronDoor");
            model[2] = ContentManager.Get<Model>("Models/CellDoor");
            for (int i = 0; i < 3; i++) {
                m_boneAbsoluteTransform[i] = BlockMesh.GetBoneAbsoluteTransform(model[i].FindMesh("Door").ParentBone);
                m_modelMeshPart[i] = model[i].FindMesh("Door").MeshParts[0];
                BlockMesh blockMesh = new BlockMesh();
                blockMesh.AppendModelMeshPart(
                    m_modelMeshPart[i],
                    m_boneAbsoluteTransform[i] * Matrix.CreateTranslation(0f, -1f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_standaloneBlockMesh[i] = blockMesh;
            }
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (IsBottomPart(generator.Terrain, x, y, z)) {
                if (!m_cachedBlockMeshes.TryGetValue(data, out BlockMesh blockMesh)) {
                    GenerateMeshAndBox(data, out blockMesh, out _);
                }
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    blockMesh,
                    Color.White,
                    null,
                    geometry.SubsetAlphaTest
                );
            }
            Vector2 centerOffset = GetRightHanded(data) ? new Vector2(-0.45f, 0f) : new Vector2(0.45f, 0f);
            GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                GetHingeFace(data),
                0.01f,
                centerOffset,
                geometry.SubsetOpaque
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh[GetModel(Terrain.ExtractData(value))],
                color,
                0.75f * size,
                ref matrix,
                environmentData
            );
        }

        public override int GetShadowStrength(int value) => 4 + (int)Math.Round((float)GetOpen(Terrain.ExtractData(value)) / 90 * (DefaultShadowStrength - 4));

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float num = Vector3.Dot(forward, Vector3.UnitZ);
            float num2 = Vector3.Dot(forward, Vector3.UnitX);
            float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
            float num4 = Vector3.Dot(forward, -Vector3.UnitX);
            int num5 = 0;
            if (num == MathUtils.Max(num, num2, num3, num4)) {
                num5 = 2;
            }
            else if (num2 == MathUtils.Max(num, num2, num3, num4)) {
                num5 = 3;
            }
            else if (num3 == MathUtils.Max(num, num2, num3, num4)) {
                num5 = 0;
            }
            else if (num4 == MathUtils.Max(num, num2, num3, num4)) {
                num5 = 1;
            }
            Point3 point = CellFace.FaceToPoint3(raycastResult.CellFace.Face);
            int num6 = raycastResult.CellFace.X + point.X;
            int y = raycastResult.CellFace.Y + point.Y;
            int num7 = raycastResult.CellFace.Z + point.Z;
            bool rightHanded = true;
            switch (num5) {
                case 0:
                    int cellValue = subsystemTerrain.Terrain.GetCellValue(num6 - 1, y, num7);
                    rightHanded = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsTransparent_(cellValue);
                    break;
                case 1:
                    cellValue = subsystemTerrain.Terrain.GetCellValue(num6, y, num7 + 1);
                    rightHanded = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsTransparent_(cellValue);
                    break;
                case 2:
                    cellValue = subsystemTerrain.Terrain.GetCellValue(num6 + 1, y, num7);
                    rightHanded = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsTransparent_(cellValue);
                    break;
                case 3:
                    cellValue = subsystemTerrain.Terrain.GetCellValue(num6, y, num7 - 1);
                    rightHanded = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsTransparent_(cellValue);
                    break;
            }
            int data = SetModel(SetRightHanded(SetOpen(SetRotation(0, num5), 0), rightHanded), GetModel(Terrain.ExtractData(value)));
            BlockPlacementData result = default;
            result.Value = Terrain.ReplaceData(Terrain.ReplaceContents(0, BlockIndex), data);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int data = Terrain.ExtractData(value);
            if (!m_cachedCollisionBoxes.TryGetValue(data, out BoundingBox[] box)) {
                GenerateMeshAndBox(data, out _, out box);
            }
            return box;
        }

        public override bool ShouldAvoid(int value) => GetOpen(Terrain.ExtractData(value)) > 0;

        public override bool IsHeatBlocker(int value) => GetOpen(Terrain.ExtractData(value)) >= 90;

        public override IEnumerable<int> GetCreativeValues() => m_creativeValuesByModel;
        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => m_displayNamesByModel[GetModel(Terrain.ExtractData(value))];

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int model = GetModel(Terrain.ExtractData(oldValue));
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetModel(0, model)), Count = 1 });
            showDebris = true;
        }

        #region base information of block

        public override float GetDensity(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 0.5f,
                _ => 3f
            };
        }

        public override float GetFuelHeatLevel(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 1f,
                _ => 0f
            };
        }

        public override float GetFuelFireDuration(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 5f,
                _ => 0f
            };
        }

        public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => "Wood",
                _ => "Metal"
            };
        }

        public override float GetProjectileStickProbability(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 0.95f,
                _ => 0f
            };
        }

        public override float GetFireDuration(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 12f,
                _ => 0f
            };
        }

        public override float GetExplosionResilience(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 3f,
                _ => 60f
            };
        }

        public override BlockDigMethod GetBlockDigMethod(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => BlockDigMethod.Hack,
                _ => BlockDigMethod.Quarry
            };
        }

        public override float GetDigResilience(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 5f,
                1 => 14f,
                _ => 12f
            };
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 4,
                1 => 82,
                _ => 80
            };
        }

        #endregion

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemElectricity, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            return new DoorGVElectricElement(subsystemElectricity, new CellFace(x, y, z, GetHingeFace(data)));
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int hingeFace = GetHingeFace(Terrain.ExtractData(value));
            if (face == hingeFace) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(hingeFace, 0, connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Input;
                }
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public static int GetRotation(int data) => data & 3;

        public static int GetOpen(int data) {
            int num = (data >> 5) & 127;
            return num > 90 ? 90 : num;
        }

        public static bool GetRightHanded(int data) => (data & 4) == 0;

        public static int SetRotation(int data, int rotation) => (data & -4) | (rotation & 3);

        public static int SetOpen(int data, int open) {
            open = open > 90 ? 90 : open;
            return (data & -4065) | ((open & 127) << 5);
        }

        public static int SetRightHanded(int data, bool rightHanded) {
            if (rightHanded) {
                return data & -5;
            }
            return data | 4;
        }

        public static bool IsTopPart(Terrain terrain, int x, int y, int z) => BlocksManager.Blocks[terrain.GetCellContents(x, y - 1, z)] is GVDoorBlock;

        public static bool IsBottomPart(Terrain terrain, int x, int y, int z) => BlocksManager.Blocks[terrain.GetCellContents(x, y + 1, z)] is GVDoorBlock;

        public static int GetHingeFace(int data) {
            int rotation = GetRotation(data);
            int num = rotation - 1 < 0 ? 3 : rotation - 1;
            if (!GetRightHanded(data)) {
                num = CellFace.OppositeFace(num);
            }
            return num;
        }

        public static int GetModel(int data) => (data >> 3) & 3;

        public static int SetModel(int data, int model) => (data & -25) | ((model & 3) << 3);

        public void GenerateMeshAndBox(int data, out BlockMesh blockMesh, out BoundingBox[] box) {
            blockMesh = new BlockMesh();
            int rotation = GetRotation(data);
            int open = GetOpen(data);
            bool rightHanded = GetRightHanded(data);
            int model = GetModel(data);
            float num = !rightHanded ? 1 : -1;
            Matrix identity = Matrix.Identity;
            identity *= Matrix.CreateScale(0f - num, 1f, 1f);
            identity *= Matrix.CreateTranslation((0.5f - m_pivotDistance) * num, 0f, 0f) * Matrix.CreateRotationY(open > 0 ? num * MathUtils.DegToRad(open) : 0f) * Matrix.CreateTranslation((0f - (0.5f - m_pivotDistance)) * num, 0f, 0f);
            identity *= Matrix.CreateTranslation(0f, 0f, 0.5f - m_pivotDistance) * Matrix.CreateRotationY(rotation * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
            blockMesh.AppendModelMeshPart(
                m_modelMeshPart[model],
                m_boneAbsoluteTransform[model] * identity,
                false,
                !rightHanded,
                false,
                false,
                Color.White
            );
            m_cachedBlockMeshes.Add(data, blockMesh);
            BoundingBox boundingBox = blockMesh.CalculateBoundingBox();
            boundingBox.Max.Y = 1f;
            box = new[] { boundingBox };
            m_cachedCollisionBoxes.Add(data, box);
        }
    }
}