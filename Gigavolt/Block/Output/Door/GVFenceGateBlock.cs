using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVFenceGateBlock : FenceBlock, IGVElectricElementBlock, IPaintableBlock {
        public const int Index = 866;
        public DynamicArray<GVElectricConnectionPath> m_GVtmpConnectionPaths = new DynamicArray<GVElectricConnectionPath>();
        public float[] m_pivotDistance = { 0.0625f, 0.0443f };

        public bool[] m_doubleSidedAndUseAlphaTest = { false, true };

        public int[] _m_coloredTextureSlot = { 23, 58 };

        public Color[] _m_postColor = { Color.White, new Color(192, 192, 192) };

        public Color[] _m_unpaintedColor = { Color.White, new Color(80, 80, 80) };

        public BlockMesh[] _m_standaloneBlockMesh = new BlockMesh[2];
        public BlockMesh[] _m_standaloneColoredBlockMesh = new BlockMesh[2];
        public Matrix[] m_boneAbsoluteTransform = new Matrix[2];
        public Matrix[] m_boneAbsoluteTransform2 = new Matrix[2];
        public ModelMeshPart[] m_modelMeshPart = new ModelMeshPart[2];
        public ModelMeshPart[] m_modelMeshPart2 = new ModelMeshPart[2];

        public Dictionary<int, BlockMesh> m_cachedBlockMeshes = new Dictionary<int, BlockMesh>();
        public Dictionary<int, BoundingBox[]> m_cachedCollisionBoxes = new Dictionary<int, BoundingBox[]>();
        public string[] m_displayNamesByModel = { "GV木栅栏门", "GV铁栅栏门" };

        public override void Initialize() {
            Model[] model = new Model[2];
            model[0] = ContentManager.Get<Model>("Models/WoodenFenceGate");
            model[1] = ContentManager.Get<Model>("Models/IronFenceGate");
            for (int i = 0; i < 2; i++) {
                m_boneAbsoluteTransform[i] = BlockMesh.GetBoneAbsoluteTransform(model[i].FindMesh("Post").ParentBone);
                m_boneAbsoluteTransform2[i] = BlockMesh.GetBoneAbsoluteTransform(model[i].FindMesh("Planks").ParentBone);
                m_modelMeshPart[i] = model[i].FindMesh("Post").MeshParts[0];
                m_modelMeshPart2[i] = model[i].FindMesh("Planks").MeshParts[0];
                BlockMesh blockMesh = new BlockMesh();
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
                BlockMesh blockMesh2 = new BlockMesh();
                blockMesh2.AppendBlockMesh(blockMesh);
                blockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(DefaultTextureSlot % 16 / 16f, DefaultTextureSlot / 16 / 16f, 0f));
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
            if (color.HasValue) {
                return new BlockDebrisParticleSystem(
                    subsystemTerrain,
                    position,
                    strength,
                    DestructionDebrisScale,
                    SubsystemPalette.GetColor(subsystemTerrain, color),
                    _m_coloredTextureSlot[GetModel(data)]
                );
            }
            return new BlockDebrisParticleSystem(
                subsystemTerrain,
                position,
                strength,
                DestructionDebrisScale,
                Color.White,
                DefaultTextureSlot
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
            GenerateGVWireVertices(
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

        public int? GetPaintColor(int value) => GetColor(Terrain.ExtractData(value));

        public int Paint(SubsystemTerrain terrain, int value, int? color) {
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

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemElectricity, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            return new FenceGateGVElectricElement(subsystemElectricity, new CellFace(x, y, z, GetHingeFace(data)));
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
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

        public static int? GetColor(int data) {
            if ((data & 0x10) != 0) {
                return (data >> 5) & 0xF;
            }
            return null;
        }

        public static int SetColor(int data, int? color) {
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
            blockMesh.TransformTextureCoordinates(color.HasValue ? Matrix.CreateTranslation(_m_coloredTextureSlot[model] % 16 / 16f, _m_coloredTextureSlot[model] / 16 / 16f, 0f) : Matrix.CreateTranslation(DefaultTextureSlot % 16 / 16f, DefaultTextureSlot / 16 / 16f, 0f));
            m_cachedBlockMeshes.Add(data, blockMesh);
            BoundingBox boundingBox = blockMesh.CalculateBoundingBox();
            boundingBox.Min.X = MathUtils.Saturate(boundingBox.Min.X);
            boundingBox.Min.Y = MathUtils.Saturate(boundingBox.Min.Y);
            boundingBox.Min.Z = MathUtils.Saturate(boundingBox.Min.Z);
            boundingBox.Max.X = MathUtils.Saturate(boundingBox.Max.X);
            boundingBox.Max.Y = MathUtils.Saturate(boundingBox.Max.Y);
            boundingBox.Max.Z = MathUtils.Saturate(boundingBox.Max.Z);
            box = new[] { boundingBox };
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

        public virtual void GenerateGVWireVertices(BlockGeometryGenerator generator, int value, int x, int y, int z, int mountingFace, float centerBoxSize, Vector2 centerOffset, TerrainGeometrySubset subset) {
            SubsystemGVElectricity SubsystemGVElectricity = generator.SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
            if (SubsystemGVElectricity == null) {
                return;
            }
            Color color = GVWireBlock.WireColor;
            int num = Terrain.ExtractContents(value);
            if (num == GVWireBlock.Index) {
                int? color2 = GVWireBlock.GetColor(Terrain.ExtractData(value));
                if (color2.HasValue) {
                    color = SubsystemPalette.GetColor(generator, color2);
                }
            }
            int num2 = Terrain.ExtractLight(value);
            float num3 = LightingManager.LightIntensityByLightValue[num2];
            Vector3 v = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) - 0.5f * CellFace.FaceToVector3(mountingFace);
            Vector3 vector = CellFace.FaceToVector3(mountingFace);
            Vector2 v2 = new Vector2(0.9376f, 0.0001f);
            Vector2 v3 = new Vector2(0.03125f, 0.00550781237f);
            Point3 point = CellFace.FaceToPoint3(mountingFace);
            int cellContents = generator.Terrain.GetCellContents(x - point.X, y - point.Y, z - point.Z);
            bool flag = cellContents == 2 || cellContents == 7 || cellContents == 8 || cellContents == 6 || cellContents == 62 || cellContents == 72;
            Vector3 v4 = CellFace.FaceToVector3(SubsystemGVElectricity.GetConnectorFace(mountingFace, GVElectricConnectorDirection.Top));
            Vector3 vector2 = CellFace.FaceToVector3(SubsystemGVElectricity.GetConnectorFace(mountingFace, GVElectricConnectorDirection.Left)) * centerOffset.X + v4 * centerOffset.Y;
            int num4 = 0;
            m_GVtmpConnectionPaths.Clear();
            SubsystemGVElectricity.GetAllConnectedNeighbors(
                x,
                y,
                z,
                mountingFace,
                m_GVtmpConnectionPaths
            );
            foreach (GVElectricConnectionPath tmpConnectionPath in m_GVtmpConnectionPaths) {
                if ((num4 & (1 << tmpConnectionPath.ConnectorFace)) == 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(mountingFace, 0, tmpConnectionPath.ConnectorFace);
                    if (!(centerOffset == Vector2.Zero)
                        || connectorDirection != GVElectricConnectorDirection.In) {
                        num4 |= 1 << tmpConnectionPath.ConnectorFace;
                        Color color3 = color;
                        if (num != GVWireBlock.Index) {
                            int cellValue = generator.Terrain.GetCellValue(x + tmpConnectionPath.NeighborOffsetX, y + tmpConnectionPath.NeighborOffsetY, z + tmpConnectionPath.NeighborOffsetZ);
                            if (Terrain.ExtractContents(cellValue) == GVWireBlock.Index) {
                                int? color4 = WireBlock.GetColor(Terrain.ExtractData(cellValue));
                                if (color4.HasValue) {
                                    color3 = SubsystemPalette.GetColor(generator, color4);
                                }
                            }
                        }
                        Vector3 vector3 = connectorDirection != GVElectricConnectorDirection.In ? CellFace.FaceToVector3(tmpConnectionPath.ConnectorFace) : -Vector3.Normalize(vector2);
                        Vector3 vector4 = Vector3.Cross(vector, vector3);
                        float s = centerBoxSize >= 0f ? MathUtils.Max(0.03125f, centerBoxSize / 2f) : centerBoxSize / 2f;
                        float num5 = connectorDirection == GVElectricConnectorDirection.In ? 0.03125f : 0.5f;
                        float num6 = connectorDirection == GVElectricConnectorDirection.In ? 0f :
                            tmpConnectionPath.ConnectorFace == tmpConnectionPath.NeighborFace ? num5 + 0.03125f :
                            tmpConnectionPath.ConnectorFace != CellFace.OppositeFace(tmpConnectionPath.NeighborFace) ? num5 : num5 - 0.03125f;
                        Vector3 v5 = v - vector4 * 0.03125f + vector3 * s + vector2;
                        Vector3 vector5 = v - vector4 * 0.03125f + vector3 * num5;
                        Vector3 vector6 = v + vector4 * 0.03125f + vector3 * num5;
                        Vector3 v6 = v + vector4 * 0.03125f + vector3 * s + vector2;
                        Vector3 vector7 = v + vector * 0.03125f + vector3 * (centerBoxSize / 2f) + vector2;
                        Vector3 vector8 = v + vector * 0.03125f + vector3 * num6;
                        if (flag && centerBoxSize == 0f) {
                            Vector3 vector9 = 0.25f * BlockGeometryGenerator.GetRandomWireOffset(0.5f * (v5 + v6), vector);
                            v5 += vector9;
                            v6 += vector9;
                            vector7 += vector9;
                        }
                        Vector2 vector10 = v2 + v3 * new Vector2(MathUtils.Max(0.0625f, centerBoxSize), 0f);
                        Vector2 vector11 = v2 + v3 * new Vector2(num5 * 2f, 0f);
                        Vector2 vector12 = v2 + v3 * new Vector2(num5 * 2f, 1f);
                        Vector2 vector13 = v2 + v3 * new Vector2(MathUtils.Max(0.0625f, centerBoxSize), 1f);
                        Vector2 vector14 = v2 + v3 * new Vector2(centerBoxSize, 0.5f);
                        Vector2 vector15 = v2 + v3 * new Vector2(num6 * 2f, 0.5f);
                        int num7 = Terrain.ExtractLight(generator.Terrain.GetCellValue(x + tmpConnectionPath.NeighborOffsetX, y + tmpConnectionPath.NeighborOffsetY, z + tmpConnectionPath.NeighborOffsetZ));
                        float num8 = LightingManager.LightIntensityByLightValue[num7];
                        float num9 = 0.5f * (num3 + num8);
                        float num10 = LightingManager.CalculateLighting(-vector4);
                        float num11 = LightingManager.CalculateLighting(vector4);
                        float num12 = LightingManager.CalculateLighting(vector);
                        float num13 = num10 * num3;
                        float num14 = num10 * num9;
                        float num15 = num11 * num9;
                        float num16 = num11 * num3;
                        float num17 = num12 * num3;
                        float num18 = num12 * num9;
                        Color color5 = new Color((byte)(color3.R * num13), (byte)(color3.G * num13), (byte)(color3.B * num13));
                        Color color6 = new Color((byte)(color3.R * num14), (byte)(color3.G * num14), (byte)(color3.B * num14));
                        Color color7 = new Color((byte)(color3.R * num15), (byte)(color3.G * num15), (byte)(color3.B * num15));
                        Color color8 = new Color((byte)(color3.R * num16), (byte)(color3.G * num16), (byte)(color3.B * num16));
                        Color color9 = new Color((byte)(color3.R * num17), (byte)(color3.G * num17), (byte)(color3.B * num17));
                        Color color10 = new Color((byte)(color3.R * num18), (byte)(color3.G * num18), (byte)(color3.B * num18));
                        int count = subset.Vertices.Count;
                        subset.Vertices.Count += 6;
                        TerrainVertex[] array = subset.Vertices.Array;
                        BlockGeometryGenerator.SetupVertex(
                            v5.X,
                            v5.Y,
                            v5.Z,
                            color5,
                            vector10.X,
                            vector10.Y,
                            ref array[count]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            vector5.X,
                            vector5.Y,
                            vector5.Z,
                            color6,
                            vector11.X,
                            vector11.Y,
                            ref array[count + 1]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            vector6.X,
                            vector6.Y,
                            vector6.Z,
                            color7,
                            vector12.X,
                            vector12.Y,
                            ref array[count + 2]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            v6.X,
                            v6.Y,
                            v6.Z,
                            color8,
                            vector13.X,
                            vector13.Y,
                            ref array[count + 3]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            vector7.X,
                            vector7.Y,
                            vector7.Z,
                            color9,
                            vector14.X,
                            vector14.Y,
                            ref array[count + 4]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            vector8.X,
                            vector8.Y,
                            vector8.Z,
                            color10,
                            vector15.X,
                            vector15.Y,
                            ref array[count + 5]
                        );
                        int count2 = subset.Indices.Count;
                        subset.Indices.Count += connectorDirection == GVElectricConnectorDirection.In ? 15 : 12;
                        int[] array2 = subset.Indices.Array;
                        array2[count2] = count;
                        array2[count2 + 1] = count + 5;
                        array2[count2 + 2] = count + 1;
                        array2[count2 + 3] = count + 5;
                        array2[count2 + 4] = count;
                        array2[count2 + 5] = count + 4;
                        array2[count2 + 6] = count + 4;
                        array2[count2 + 7] = count + 2;
                        array2[count2 + 8] = count + 5;
                        array2[count2 + 9] = count + 2;
                        array2[count2 + 10] = count + 4;
                        array2[count2 + 11] = count + 3;
                        if (connectorDirection == GVElectricConnectorDirection.In) {
                            array2[count2 + 12] = count + 2;
                            array2[count2 + 13] = count + 1;
                            array2[count2 + 14] = count + 5;
                        }
                    }
                }
            }
            if (centerBoxSize != 0f
                || (num4 == 0 && num != GVWireBlock.Index)) {
                return;
            }
            for (int i = 0; i < 6; i++) {
                if (i != mountingFace
                    && i != CellFace.OppositeFace(mountingFace)
                    && (num4 & (1 << i)) == 0) {
                    Vector3 vector16 = CellFace.FaceToVector3(i);
                    Vector3 v7 = Vector3.Cross(vector, vector16);
                    Vector3 v8 = v - v7 * 0.03125f + vector16 * 0.03125f;
                    Vector3 v9 = v + v7 * 0.03125f + vector16 * 0.03125f;
                    Vector3 vector17 = v + vector * 0.03125f;
                    if (flag) {
                        Vector3 vector18 = 0.25f * BlockGeometryGenerator.GetRandomWireOffset(0.5f * (v8 + v9), vector);
                        v8 += vector18;
                        v9 += vector18;
                        vector17 += vector18;
                    }
                    Vector2 vector19 = v2 + v3 * new Vector2(0.0625f, 0f);
                    Vector2 vector20 = v2 + v3 * new Vector2(0.0625f, 1f);
                    Vector2 vector21 = v2 + v3 * new Vector2(0f, 0.5f);
                    float num19 = LightingManager.CalculateLighting(vector16) * num3;
                    float num20 = LightingManager.CalculateLighting(vector) * num3;
                    Color color11 = new Color((byte)(color.R * num19), (byte)(color.G * num19), (byte)(color.B * num19));
                    Color color12 = new Color((byte)(color.R * num20), (byte)(color.G * num20), (byte)(color.B * num20));
                    int count3 = subset.Vertices.Count;
                    subset.Vertices.Count += 3;
                    TerrainVertex[] array3 = subset.Vertices.Array;
                    BlockGeometryGenerator.SetupVertex(
                        v8.X,
                        v8.Y,
                        v8.Z,
                        color11,
                        vector19.X,
                        vector19.Y,
                        ref array3[count3]
                    );
                    BlockGeometryGenerator.SetupVertex(
                        v9.X,
                        v9.Y,
                        v9.Z,
                        color11,
                        vector20.X,
                        vector20.Y,
                        ref array3[count3 + 1]
                    );
                    BlockGeometryGenerator.SetupVertex(
                        vector17.X,
                        vector17.Y,
                        vector17.Z,
                        color12,
                        vector21.X,
                        vector21.Y,
                        ref array3[count3 + 2]
                    );
                    int count4 = subset.Indices.Count;
                    subset.Indices.Count += 3;
                    int[] array4 = subset.Indices.Array;
                    array4[count4] = count3;
                    array4[count4 + 1] = count3 + 2;
                    array4[count4 + 2] = count3 + 1;
                }
            }
        }
    }
}