using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVSwitchCabinetBlock : MountedGVElectricElementBlock {
        public class StateInfo {
            public BlockMesh BaseBlockMesh;
            public BlockMesh SwitchBodyBlockMesh;
            public BlockMesh SwitchLeverBlockMesh;
            public BoundingBox[] TopCollisionBoxes;
            public BoundingBox[] BottomCollisionBoxes;
        }

        Texture2D m_baseTexture;
        public readonly BlockMesh m_standaloneBaseBlockMesh = new();
        public readonly BlockMesh m_standaloneSwitchBodyBlockMesh = new();
        public readonly BlockMesh m_standaloneSwitchLeverBlockMesh = new();
        readonly Dictionary<int, StateInfo> m_cachedStateInfos = new();

        public static readonly Point3[] m_upPoint3 = [Point3.UnitY, Point3.UnitY, Point3.UnitY, Point3.UnitY, -Point3.UnitZ, Point3.UnitZ];

        public static readonly int[] ColorIndex2Color = [
            0,
            8,
            15,
            11,
            12,
            13,
            14,
            1,
            2,
            3,
            4,
            5,
            6,
            10
        ];

        public static readonly int[] Color2ColorIndex = [
            0,
            7,
            8,
            9,
            10,
            11,
            12,
            -1,
            1,
            -1,
            13,
            3,
            4,
            5,
            6,
            2
        ];

        public bool m_modelsNotInitialized = true;
        public static ModelMesh m_baseModelMesh;
        public static ModelMeshPart m_baseModelMeshPart;
        public static Matrix m_baseBoneAbsoluteTransform;
        public static Model m_switchModel;
        public static ModelMesh m_switchBodyMesh;
        public static ModelMeshPart m_switchBodyMeshPart;
        public static Matrix m_switchBodyBoneAbsoluteTransform;
        public static ModelMesh m_switchLeverMesh;
        public static ModelMeshPart m_switchLeverMeshPart;
        public static Matrix m_switchLeverBoneAbsoluteTransform;

        public override void Initialize() {
            m_baseTexture = ContentManager.Get<Texture2D>("Textures/GVColoredCabinetBlock");
            if (m_modelsNotInitialized) {
                m_baseModelMesh = ContentManager.Get<Model>("Models/GVSignalGenerator").FindMesh("GVSignalGenerator");
                m_baseModelMeshPart = m_baseModelMesh.MeshParts[0];
                m_baseBoneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_baseModelMesh.ParentBone);
                m_switchModel = ContentManager.Get<Model>("Models/Switch");
                m_switchBodyMesh = m_switchModel.FindMesh("Body");
                m_switchBodyMeshPart = m_switchBodyMesh.MeshParts[0];
                m_switchBodyBoneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_switchBodyMesh.ParentBone);
                m_switchLeverMesh = m_switchModel.FindMesh("Lever");
                m_switchLeverMeshPart = m_switchLeverMesh.MeshParts[0];
                m_switchLeverBoneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_switchLeverMesh.ParentBone);
                m_modelsNotInitialized = true;
            }
            Matrix standaloneMatrix = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBaseBlockMesh.AppendModelMeshPart(
                m_baseModelMeshPart,
                m_baseBoneAbsoluteTransform * standaloneMatrix,
                false,
                false,
                false,
                false,
                Color.White
            );
            for (int colorIndex = 0; colorIndex < 14; colorIndex++) {
                Matrix switchMatrix = Matrix.CreateScale(0.3f)
                    * Matrix.CreateTranslation(colorIndex >= 7 ? -0.1f : 0.1f, 0.0625f, -1.1f + 0.2f * (colorIndex % 7));
                m_standaloneSwitchBodyBlockMesh.AppendModelMeshPart(
                    m_switchBodyMeshPart,
                    m_switchBodyBoneAbsoluteTransform * switchMatrix * standaloneMatrix,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                int color = ColorIndex2Color[colorIndex];
                m_standaloneSwitchLeverBlockMesh.AppendModelMeshPart(
                    m_switchLeverMeshPart,
                    m_switchLeverBoneAbsoluteTransform * Matrix.CreateRotationX(MathF.PI / 6) * switchMatrix * standaloneMatrix,
                    false,
                    false,
                    false,
                    false,
                    WorldPalette.DefaultColors[color]
                );
            }
        }

        public override GVElectricElement[] CreateGVElectricElements(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) {
            int face = GetFace(value);
            int data = Terrain.ExtractData(value);
            Point3 upDirection = m_upPoint3[face];
            Point3 another;
            bool isUp = GetIsTopPart(data);
            if (isUp) {
                another.X = x - upDirection.X;
                another.Y = y - upDirection.Y;
                another.Z = z - upDirection.Z;
            }
            else {
                return null;
            }
            if (!subsystemGVElectricity.m_GVElectricElementsToAdd[subterrainId].ContainsKey(another)) {
                int anotherValue = subsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(subterrainId)
                    .GetCellValue(another.X, another.Y, another.Z);
                if (Terrain.ExtractContents(anotherValue) == BlockIndex
                    && GetFace(anotherValue) == face) {
                    int anotherData = Terrain.ExtractData(anotherValue);
                    if (!GetIsTopPart(anotherData)) {
                        GVElectricElement[] result = new GVElectricElement[16];
                        for (int color = 0; color < 16; color++) {
                            result[color] = new SwitchCabinetGVElectricElement(
                                subsystemGVElectricity,
                                [new GVCellFace(x, y, z, face, 1 << color), new GVCellFace(another.X, another.Y, another.Z, face, 1 << color)],
                                subterrainId,
                                GetLeverState(data, color)
                            );
                        }
                        return result;
                    }
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
                m_standaloneBaseBlockMesh,
                m_baseTexture,
                color,
                2f * size,
                ref matrix,
                environmentData
            );
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneSwitchBodyBlockMesh, color, 2f * size, ref matrix, environmentData);
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneSwitchLeverBlockMesh,
                GVStaticStorage.WhiteTexture,
                color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (!GetIsTopPart(data)) {
                StateInfo stateInfo = GetStateInfo(GetState(data));
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    stateInfo.BaseBlockMesh,
                    Color.White,
                    null,
                    geometry.GetGeometry(m_baseTexture).SubsetOpaque
                );
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    stateInfo.SwitchBodyBlockMesh,
                    Color.White,
                    null,
                    geometry.SubsetOpaque
                );
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    stateInfo.SwitchLeverBlockMesh,
                    Color.White,
                    null,
                    geometry.GetGeometry(GVStaticStorage.WhiteTexture).SubsetOpaque
                );
            }
            GVBlockGeometryGenerator.GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                GetFaceFromDataStatic(data),
                0.375f,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int data = Terrain.ExtractData(value);
            StateInfo stateInfo = GetStateInfo(GetState(data));
            return GetIsTopPart(data) ? stateInfo.TopCollisionBoxes : stateInfo.BottomCollisionBoxes;
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, SetFace(0, raycastResult.CellFace.Face));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            dropValues.Add(new BlockDropValue { Value = BlockIndex, Count = 1 });
            showDebris = DestructionDebrisScale > 0f;
        }

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem,
            int value,
            int face,
            int connectorFace,
            int x,
            int y,
            int z,
            Terrain terrain) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public override int GetConnectionMask(int value) => 0xFFFF;
        public override int GetFace(int value) => Terrain.ExtractData(value) & 7;
        public static int GetFaceFromDataStatic(int data) => data & 7;
        public static int SetFace(int data, int face) => (data & -8) | (face & 7);
        public static bool GetIsTopPart(int data) => (data & 131072) != 0;
        public static int SetIsTopPart(int data, bool isUp) => (data & -131073) | (isUp ? 131072 : 0);

        public static bool GetLeverState(int data, int color) {
            int colorIndex = Color2ColorIndex[color];
            if (colorIndex < 0) {
                return false;
            }
            return (data & (1 << (colorIndex + 3))) != 0;
        }

        public static int SetLeverState(int data, int color, bool state) {
            int colorIndex = Color2ColorIndex[color];
            if (colorIndex < 0) {
                return data;
            }
            return (data & (-1 - (1 << (colorIndex + 3)))) | (state ? 1 << (colorIndex + 3) : 0);
        }

        public static int GetState(int data) => data & 131071;

        public StateInfo GetStateInfo(int state) {
            if (!m_cachedStateInfos.TryGetValue(state, out StateInfo result)) {
                result = GenerateStateInfo(state);
                m_cachedStateInfos.Add(state, result);
            }
            return result;
        }

        public StateInfo GenerateStateInfo(int state) {
            int face = GetFaceFromDataStatic(state);
            float radians;
            bool flag;
            if (face < 4) {
                radians = face * (float)Math.PI / 2f;
                flag = false;
            }
            else if (face == 4) {
                radians = -(float)Math.PI / 2f;
                flag = true;
            }
            else {
                radians = (float)Math.PI / 2f;
                flag = true;
            }
            Matrix m = Matrix.CreateRotationX((float)Math.PI / 2f)
                * Matrix.CreateTranslation(0f, 0f, -0.5f)
                * (flag ? Matrix.CreateRotationX(radians) : Matrix.CreateRotationY(radians))
                * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
            BlockMesh baseBlockMesh = new();
            baseBlockMesh.AppendModelMeshPart(
                m_baseModelMeshPart,
                m_baseBoneAbsoluteTransform * m,
                false,
                false,
                false,
                false,
                Color.White
            );
            BoundingBox[] bottomCollisionBoxes = new BoundingBox[15];
            BoundingBox[] topCollisionBoxes = new BoundingBox[15];
            BoundingBox baseBoundingBox = baseBlockMesh.CalculateBoundingBox();
            bottomCollisionBoxes[0] = baseBoundingBox;
            Vector3 downTranslation = new(-m_upPoint3[face]);
            topCollisionBoxes[0] = new BoundingBox(baseBoundingBox.Min + downTranslation, baseBoundingBox.Max + downTranslation);
            BoundingBox firstSwitchBoundingBox = default;
            BlockMesh switchBodyBlockMesh = new();
            BlockMesh switchLeverBlockMesh = new();
            for (int colorIndex = 0; colorIndex < 14; colorIndex++) {
                Matrix switchMatrix = Matrix.CreateScale(0.3f)
                    * Matrix.CreateTranslation(colorIndex >= 7 ? -0.1f : 0.1f, 0.0625f, -1.1f + 0.2f * (colorIndex % 7));
                switchBodyBlockMesh.AppendModelMeshPart(
                    m_switchBodyMeshPart,
                    m_switchBodyBoneAbsoluteTransform * switchMatrix * m,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                int color = ColorIndex2Color[colorIndex];
                switchLeverBlockMesh.AppendModelMeshPart(
                    m_switchLeverMeshPart,
                    m_switchLeverBoneAbsoluteTransform
                    * (GetLeverState(state, color) ? Matrix.CreateRotationX(-MathF.PI / 6) : Matrix.CreateRotationX(MathF.PI / 6))
                    * switchMatrix
                    * m,
                    false,
                    false,
                    false,
                    false,
                    WorldPalette.DefaultColors[color]
                );
                int collisionBoxIndex = colorIndex + 1;
                if (colorIndex == 0) {
                    firstSwitchBoundingBox = switchBodyBlockMesh.CalculateBoundingBox();
                    bottomCollisionBoxes[collisionBoxIndex] = firstSwitchBoundingBox;
                    topCollisionBoxes[collisionBoxIndex] = new BoundingBox(
                        firstSwitchBoundingBox.Min + downTranslation,
                        firstSwitchBoundingBox.Max + downTranslation
                    );
                }
                else {
                    Vector3 down = new(-m_upPoint3[face]);
                    Vector3 transition = down * 0.2f * (colorIndex % 7);
                    if (colorIndex >= 7) {
                        transition += Vector3.Cross(down, CellFace.FaceToVector3(face)) * 0.2f;
                    }
                    bottomCollisionBoxes[collisionBoxIndex] = new BoundingBox(
                        firstSwitchBoundingBox.Min + transition,
                        firstSwitchBoundingBox.Max + transition
                    );
                    topCollisionBoxes[collisionBoxIndex] = new BoundingBox(
                        firstSwitchBoundingBox.Min + downTranslation + transition,
                        firstSwitchBoundingBox.Max + downTranslation + transition
                    );
                }
            }
            return new StateInfo {
                BaseBlockMesh = baseBlockMesh,
                SwitchBodyBlockMesh = switchBodyBlockMesh,
                SwitchLeverBlockMesh = switchLeverBlockMesh,
                TopCollisionBoxes = topCollisionBoxes,
                BottomCollisionBoxes = bottomCollisionBoxes
            };
        }
    }
}