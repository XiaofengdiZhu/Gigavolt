using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVBitButtonCabinetBlock : MountedGVElectricElementBlock {
        public class FaceInfo {
            public BlockMesh BaseBlockMesh;
            public BlockMesh ButtonBlockMesh;
            public BoundingBox[] TopCollisionBoxes;
            public BoundingBox[] ButtonCollisionBoxes;
        }

        Texture2D m_baseTexture;
        public readonly BlockMesh m_standaloneBaseBlockMesh = new();
        public readonly BlockMesh m_standaloneButtonBlockMesh = new();
        readonly Dictionary<int, FaceInfo> m_cachedFaceInfos = new();

        public static readonly Point3[] m_upPoint3 = [Point3.UnitY, Point3.UnitY, Point3.UnitY, Point3.UnitY, -Point3.UnitZ, Point3.UnitZ];

        public bool m_modelsNotInitialized = true;
        public static ModelMesh m_baseModelMesh;
        public static ModelMeshPart m_baseModelMeshPart;
        public static Matrix m_baseBoneAbsoluteTransform;
        public static Model m_buttonModel;
        public static ModelMesh m_buttonMesh;
        public static ModelMeshPart m_buttonMeshPart;
        public static Matrix m_buttonBoneAbsoluteTransform;

        public override void Initialize() {
            m_baseTexture = ContentManager.Get<Texture2D>("Textures/GVBitButtonCabinetBlock");
            if (m_modelsNotInitialized) {
                m_baseModelMesh = ContentManager.Get<Model>("Models/GVSignalGenerator").FindMesh("GVSignalGenerator");
                m_baseModelMeshPart = m_baseModelMesh.MeshParts[0];
                m_baseBoneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_baseModelMesh.ParentBone);
                m_buttonModel = ContentManager.Get<Model>("Models/Button");
                m_buttonMesh = m_buttonModel.FindMesh("Button");
                m_buttonMeshPart = m_buttonMesh.MeshParts[0];
                m_buttonBoneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(m_buttonMesh.ParentBone);
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
            for (int bitIndex = 0; bitIndex < 32; bitIndex++) {
                Matrix buttonMatrix = Matrix.CreateScale(0.24f)
                    * Matrix.CreateTranslation(0.171f - bitIndex / 8 * 0.114f, 0.0625f, -1.075f + 0.175f * (bitIndex % 8));
                m_standaloneButtonBlockMesh.AppendModelMeshPart(
                    m_buttonMeshPart,
                    m_buttonBoneAbsoluteTransform * buttonMatrix * standaloneMatrix,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
            }
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
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
                        return new BitButtonCabinetGVElectricElement(
                            subsystemGVElectricity,
                            [new GVCellFace(x, y, z, face), new GVCellFace(another.X, another.Y, another.Z, face)],
                            subterrainId,
                            GetDuration(data)
                        );
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
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneButtonBlockMesh, color, 2f * size, ref matrix, environmentData);
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (!GetIsTopPart(data)) {
                FaceInfo stateInfo = GetFaceInfo(GetFaceFromDataStatic(data));
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
                    stateInfo.ButtonBlockMesh,
                    Color.White,
                    null,
                    geometry.SubsetOpaque
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
            FaceInfo stateInfo = GetFaceInfo(GetFaceFromDataStatic(data));
            return GetIsTopPart(data) ? stateInfo.TopCollisionBoxes : stateInfo.ButtonCollisionBoxes;
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, SetFace(Terrain.ExtractData(value), raycastResult.CellFace.Face));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            dropValues.Add(
                new BlockDropValue {
                    Value = Terrain.MakeBlockValue(BlockIndex, 0, SetDuration(0, GetDuration(Terrain.ExtractData(oldValue)))), Count = 1
                }
            );
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

        public override int GetFace(int value) => Terrain.ExtractData(value) & 7;
        public static int GetFaceFromDataStatic(int data) => data & 7;
        public static int SetFace(int data, int face) => (data & -8) | (face & 7);
        public static bool GetIsTopPart(int data) => (data & 131072) != 0;
        public static int SetIsTopPart(int data, bool isUp) => (data & -131073) | (isUp ? 131072 : 0);

        public static int GetDuration(int data) {
            int result = (data >> 3) & 16383;
            return result == 0 ? 10 : result;
        }

        public static int SetDuration(int data, int duration) => (data & -131065) | (((duration == 10 ? 0 : duration) & 16383) << 3);

        public FaceInfo GetFaceInfo(int face) {
            if (!m_cachedFaceInfos.TryGetValue(face, out FaceInfo result)) {
                result = GenerateStateInfo(face);
                m_cachedFaceInfos.Add(face, result);
            }
            return result;
        }

        public FaceInfo GenerateStateInfo(int face) {
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
            BoundingBox[] bottomCollisionBoxes = new BoundingBox[33];
            BoundingBox[] topCollisionBoxes = new BoundingBox[33];
            BoundingBox baseBoundingBox = baseBlockMesh.CalculateBoundingBox();
            bottomCollisionBoxes[0] = baseBoundingBox;
            Vector3 downTranslation = new(-m_upPoint3[face]);
            topCollisionBoxes[0] = new BoundingBox(baseBoundingBox.Min + downTranslation, baseBoundingBox.Max + downTranslation);
            BoundingBox firstButtonBoundingBox = default;
            BlockMesh buttonBlockMesh = new();
            for (int bitIndex = 0; bitIndex < 32; bitIndex++) {
                Matrix buttonMatrix = Matrix.CreateScale(0.24f)
                    * Matrix.CreateTranslation(0.171f - bitIndex / 8 * 0.114f, 0.0625f, -1.075f + 0.175f * (bitIndex % 8));
                buttonBlockMesh.AppendModelMeshPart(
                    m_buttonMeshPart,
                    m_buttonBoneAbsoluteTransform * buttonMatrix * m,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                int collisionBoxIndex = bitIndex + 1;
                if (bitIndex == 0) {
                    firstButtonBoundingBox = buttonBlockMesh.CalculateBoundingBox();
                    bottomCollisionBoxes[collisionBoxIndex] = firstButtonBoundingBox;
                    topCollisionBoxes[collisionBoxIndex] = new BoundingBox(
                        firstButtonBoundingBox.Min + downTranslation,
                        firstButtonBoundingBox.Max + downTranslation
                    );
                }
                else {
                    Vector3 down = new(-m_upPoint3[face]);
                    Vector3 transition = down * 0.175f * (bitIndex % 8);
                    if (bitIndex > 7) {
                        transition += Vector3.Cross(down, CellFace.FaceToVector3(face)) * (bitIndex / 8 * 0.114f);
                    }
                    bottomCollisionBoxes[collisionBoxIndex] = new BoundingBox(
                        firstButtonBoundingBox.Min + transition,
                        firstButtonBoundingBox.Max + transition
                    );
                    topCollisionBoxes[collisionBoxIndex] = new BoundingBox(
                        firstButtonBoundingBox.Min + downTranslation + transition,
                        firstButtonBoundingBox.Max + downTranslation + transition
                    );
                }
            }
            return new FaceInfo {
                BaseBlockMesh = baseBlockMesh,
                ButtonBlockMesh = buttonBlockMesh,
                TopCollisionBoxes = topCollisionBoxes,
                ButtonCollisionBoxes = bottomCollisionBoxes
            };
        }
    }
}