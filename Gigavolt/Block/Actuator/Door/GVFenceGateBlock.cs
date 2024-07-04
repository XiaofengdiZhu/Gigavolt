using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVFenceGateBlock : FenceBlock, IGVElectricElementBlock, IPaintableBlock {
        public const int Index = 866;
        public DynamicArray<GVElectricConnectionPath> m_GVtmpConnectionPaths = [];
        public float[] m_pivotDistance = [0.0625f, 0.0443f];

        public bool[] m_doubleSidedAndUseAlphaTest = [false, true];

        public int[] _m_defaultTextureSlot = [4, 58];
        public int[] _m_coloredTextureSlot = [23, 58];

        public Color[] _m_postColor = [Color.White, new Color(192, 192, 192)];

        public Color[] _m_unpaintedColor = [Color.White, new Color(80, 80, 80)];

        public BlockMesh[] _m_standaloneBlockMesh = new BlockMesh[2];
        public BlockMesh[] _m_standaloneColoredBlockMesh = new BlockMesh[2];
        public Matrix[] m_boneAbsoluteTransform = new Matrix[2];
        public Matrix[] m_boneAbsoluteTransform2 = new Matrix[2];
        public ModelMeshPart[] m_modelMeshPart = new ModelMeshPart[2];
        public ModelMeshPart[] m_modelMeshPart2 = new ModelMeshPart[2];

        public Dictionary<int, BlockMesh> m_cachedBlockMeshes = new();
        public Dictionary<int, BoundingBox[]> m_cachedCollisionBoxes = new();
        public string[] m_displayNamesByModel = ["GV木栅栏门", "GV铁栅栏门"];

        public override void Initialize() {
            Model[] model = new Model[2];
            model[0] = ContentManager.Get<Model>("Models/WoodenFenceGate");
            model[1] = ContentManager.Get<Model>("Models/IronFenceGate");
            for (int i = 0; i < 2; i++) {
                m_boneAbsoluteTransform[i] = BlockMesh.GetBoneAbsoluteTransform(model[i].FindMesh("Post").ParentBone);
                m_boneAbsoluteTransform2[i] = BlockMesh.GetBoneAbsoluteTransform(model[i].FindMesh("Planks").ParentBone);
                m_modelMeshPart[i] = model[i].FindMesh("Post").MeshParts[0];
                m_modelMeshPart2[i] = model[i].FindMesh("Planks").MeshParts[0];
                BlockMesh blockMesh = new();
                blockMesh.AppendModelMeshPart(
                    m_modelMeshPart[i],
                    m_boneAbsoluteTransform[i] * Matrix.CreateTranslation(0f, -0.5f, 0f),
                    false,
                    false,
                    false,
                    false,
                    _m_postColor[i]
                );
                blockMesh.AppendModelMeshPart(
                    m_modelMeshPart2[i],
                    m_boneAbsoluteTransform2[i] * Matrix.CreateTranslation(0f, -0.5f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                if (m_doubleSidedAndUseAlphaTest[i]) {
                    blockMesh.AppendModelMeshPart(
                        m_modelMeshPart2[i],
                        m_boneAbsoluteTransform2[i] * Matrix.CreateTranslation(0f, -0.5f, 0f),
                        false,
                        true,
                        false,
                        true,
                        Color.White
                    );
                }
                BlockMesh blockMesh2 = new();
                blockMesh2.AppendBlockMesh(blockMesh);
                blockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(_m_defaultTextureSlot[i] % 16 / 16f, _m_defaultTextureSlot[i] / 16 / 16f, 0f));
                blockMesh2.TransformTextureCoordinates(Matrix.CreateTranslation(_m_coloredTextureSlot[i] % 16 / 16f, _m_coloredTextureSlot[i] / 16 / 16f, 0f));
                _m_standaloneBlockMesh[i] = blockMesh;
                _m_standaloneColoredBlockMesh[i] = blockMesh2;
            }
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = Terrain.ExtractData(value);
            int? color = GetColor(data);
            return SubsystemPalette.GetName(subsystemTerrain, color, m_displayNamesByModel[GetModel(data)]);
        }

        public override string GetCategory(int value) {
            if (!GetColor(Terrain.ExtractData(value)).HasValue) {
                return base.GetCategory(value);
            }
            return "Painted";
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int model = 0; model < 2; model++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, SetModel(SetColor(0, null), model));
                int i = 0;
                while (i < 16) {
                    yield return Terrain.MakeBlockValue(BlockIndex, 0, SetModel(SetColor(0, i), model));
                    int num = i + 1;
                    i = num;
                }
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            showDebris = true;
            int data = Terrain.ExtractData(oldValue);
            int model = GetModel(data);
            int? color = GetColor(data);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, SetModel(SetColor(0, color), model)), Count = 1 });
        }

        public override float GetDensity(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 0.5f,
                _ => 3f
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

        public override float GetFireDuration(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 15f,
                _ => 0f
            };
        }

        public override float GetExplosionResilience(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 3f,
                _ => 20f
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
                _ => 10f
            };
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 4,
                _ => 58
            };
        }

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
            int num8 = 0;
            int num9 = 0;
            switch (num5) {
                case 0:
                    num8 = -1;
                    break;
                case 1:
                    num9 = 1;
                    break;
                case 2:
                    num8 = 1;
                    break;
                default:
                    num9 = -1;
                    break;
            }
            int cellValue = subsystemTerrain.Terrain.GetCellValue(num6 + num8, y, num7 + num9);
            int cellValue2 = subsystemTerrain.Terrain.GetCellValue(num6 - num8, y, num7 - num9);
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
            Block block2 = BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)];
            int data = Terrain.ExtractData(cellValue);
            int data2 = Terrain.ExtractData(cellValue2);
            int data3 = SetRightHanded(rightHanded: (block is FenceGateBlock && GetRotation(data) == num5) || ((!(block2 is FenceGateBlock) || GetRotation(data2) != num5) && !block.IsCollidable_(cellValue)), data: SetOpen(SetRotation(Terrain.ExtractData(value), num5), 0));
            BlockPlacementData result = default;
            result.Value = Terrain.ReplaceData(Terrain.ReplaceContents(0, BlockIndex), data3);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength) {
            int data = Terrain.ExtractData(value);
            int? color = GetColor(data);
            int model = GetModel(data);
            if (color.HasValue) {
                return new BlockDebrisParticleSystem(
                    subsystemTerrain,
                    position,
                    strength,
                    DestructionDebrisScale,
                    SubsystemPalette.GetColor(subsystemTerrain, color),
                    _m_coloredTextureSlot[model]
                );
            }
            return new BlockDebrisParticleSystem(
                subsystemTerrain,
                position,
                strength,
                DestructionDebrisScale,
                Color.White,
                _m_defaultTextureSlot[model]
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int model = GetModel(data);
            if (!m_cachedBlockMeshes.TryGetValue(data, out BlockMesh blockMesh)) {
                GenerateMeshAndBox(data, out blockMesh, out _);
            }
            int? color = GetColor(data);
            generator.GenerateMeshVertices(
                this,
                x,
                y,
                z,
                blockMesh,
                color.HasValue ? SubsystemPalette.GetColor(generator, color) : _m_unpaintedColor[model],
                null,
                m_doubleSidedAndUseAlphaTest[model] ? geometry.SubsetAlphaTest : geometry.SubsetOpaque
            );
            GVBlockGeometryGenerator.GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                GetHingeFace(data),
                m_pivotDistance[model] * 2f,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int data = Terrain.ExtractData(value);
            int? color2 = GetColor(data);
            int model = GetModel(data);
            if (color2.HasValue) {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    _m_standaloneColoredBlockMesh[model],
                    color * SubsystemPalette.GetColor(environmentData, color2),
                    size,
                    ref matrix,
                    environmentData
                );
            }
            else {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    _m_standaloneBlockMesh[model],
                    color * _m_unpaintedColor[model],
                    size,
                    ref matrix,
                    environmentData
                );
            }
        }

        public new int? GetPaintColor(int value) => GetColor(Terrain.ExtractData(value));

        public new int Paint(SubsystemTerrain terrain, int value, int? color) {
            int data = Terrain.ExtractData(value);
            return Terrain.ReplaceData(value, SetColor(data, color));
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int data = Terrain.ExtractData(value);
            if (!m_cachedCollisionBoxes.TryGetValue(data, out BoundingBox[] box)) {
                GenerateMeshAndBox(data, out _, out box);
            }
            return box;
        }

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new FenceGateGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetHingeFace(Terrain.ExtractData(value))), subterrainId);

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int hingeFace = GetHingeFace(Terrain.ExtractData(value));
            if (face == hingeFace) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public static int GetRotation(int data) => data & 3;

        public static int GetModel(int data) => (data >> 2) & 1;

        public static int GetOpen(int data) {
            int num = (data >> 9) & 127;
            return num > 90 ? 90 : num;
        }

        public static bool GetRightHanded(int data) => (data & 8) == 0;

        public static int SetRotation(int data, int rotation) => (data & -4) | (rotation & 3);

        public static int SetOpen(int data, int open) {
            open = open > 90 ? 90 : open;
            return (data & -65025) | ((open & 127) << 9);
        }

        public static int SetModel(int data, int model) => (data & -5) | ((model & 1) << 2);

        public static int SetRightHanded(int data, bool rightHanded) {
            if (rightHanded) {
                return data & -9;
            }
            return data | 8;
        }

        public static int GetHingeFace(int data) {
            int rotation = GetRotation(data);
            int num = rotation - 1 < 0 ? 3 : rotation - 1;
            if (!GetRightHanded(data)) {
                num = CellFace.OppositeFace(num);
            }
            return num;
        }

        public new static int? GetColor(int data) {
            if ((data & 0x10) != 0) {
                return (data >> 5) & 0xF;
            }
            return null;
        }

        public new static int SetColor(int data, int? color) {
            if (color.HasValue) {
                return (data & -497) | 0x10 | ((color.Value & 0xF) << 5);
            }
            return data & -497;
        }

        public void GenerateMeshAndBox(int data, out BlockMesh blockMesh, out BoundingBox[] box) {
            blockMesh = new BlockMesh();
            int model = GetModel(data);
            int rotation = GetRotation(data);
            int open = GetOpen(data);
            bool rightHanded = GetRightHanded(data);
            float num = !rightHanded ? 1 : -1;
            int? color = GetColor(data);
            Matrix identity = Matrix.Identity;
            identity *= Matrix.CreateScale(0f - num, 1f, 1f);
            identity *= Matrix.CreateTranslation((0.5f - m_pivotDistance[model]) * num, 0f, 0f) * Matrix.CreateRotationY(open > 0 ? num * MathUtils.DegToRad(open) : 0f) * Matrix.CreateTranslation((0f - (0.5f - m_pivotDistance[model])) * num, 0f, 0f);
            identity *= Matrix.CreateTranslation(0f, 0f, 0f) * Matrix.CreateRotationY(rotation * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
            blockMesh.AppendModelMeshPart(
                m_modelMeshPart[model],
                m_boneAbsoluteTransform[model] * identity,
                false,
                !rightHanded,
                false,
                false,
                _m_postColor[model]
            );
            blockMesh.AppendModelMeshPart(
                m_modelMeshPart2[model],
                m_boneAbsoluteTransform2[model] * identity,
                false,
                !rightHanded,
                false,
                false,
                Color.White
            );
            if (m_doubleSidedAndUseAlphaTest[model]) {
                blockMesh.AppendModelMeshPart(
                    m_modelMeshPart2[model],
                    m_boneAbsoluteTransform2[model] * identity,
                    false,
                    rightHanded,
                    false,
                    true,
                    Color.White
                );
            }
            blockMesh.TransformTextureCoordinates(color.HasValue ? Matrix.CreateTranslation(_m_coloredTextureSlot[model] % 16 / 16f, _m_coloredTextureSlot[model] / 16 / 16f, 0f) : Matrix.CreateTranslation(_m_defaultTextureSlot[model] % 16 / 16f, _m_defaultTextureSlot[model] / 16 / 16f, 0f));
            m_cachedBlockMeshes.Add(data, blockMesh);
            BoundingBox boundingBox = blockMesh.CalculateBoundingBox();
            boundingBox.Min.X = MathUtils.Saturate(boundingBox.Min.X);
            boundingBox.Min.Y = MathUtils.Saturate(boundingBox.Min.Y);
            boundingBox.Min.Z = MathUtils.Saturate(boundingBox.Min.Z);
            boundingBox.Max.X = MathUtils.Saturate(boundingBox.Max.X);
            boundingBox.Max.Y = MathUtils.Saturate(boundingBox.Max.Y);
            boundingBox.Max.Z = MathUtils.Saturate(boundingBox.Max.Z);
            box = [boundingBox];
            m_cachedCollisionBoxes.Add(data, box);
        }

        public GVFenceGateBlock() : base(
            null,
            false,
            false,
            0,
            Color.Transparent,
            Color.Transparent
        ) { }
    }
}