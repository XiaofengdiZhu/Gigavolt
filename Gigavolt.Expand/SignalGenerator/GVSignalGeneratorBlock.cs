using System;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVSignalGeneratorBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 897;
        Texture2D texture;
        public readonly BoundingBox[][] m_bottomCollisionBoxes = new BoundingBox[24][];

        public static readonly Point3[] m_upPoint3 = [
            Point3.UnitY,
            Point3.UnitX,
            -Point3.UnitY,
            -Point3.UnitX,
            Point3.UnitY,
            -Point3.UnitZ,
            -Point3.UnitY,
            Point3.UnitZ,
            Point3.UnitY,
            -Point3.UnitX,
            -Point3.UnitY,
            Point3.UnitX,
            Point3.UnitY,
            Point3.UnitZ,
            -Point3.UnitY,
            -Point3.UnitZ,
            -Point3.UnitZ,
            Point3.UnitX,
            Point3.UnitZ,
            -Point3.UnitX,
            Point3.UnitZ,
            Point3.UnitX,
            -Point3.UnitZ,
            -Point3.UnitX
        ];

        public override void Initialize() {
            texture = ContentManager.Get<Texture2D>("Textures/GVSignalGeneratorBlock");
            ModelMesh modelMesh = ContentManager.Get<Model>(m_modelName).FindMesh(m_meshName);
            ModelMeshPart modelMeshPart = modelMesh.MeshParts[0];
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(modelMesh.ParentBone);
            for (int i = 0; i < 6; i++) {
                float radians;
                bool flag;
                if (i < 4) {
                    radians = i * (float)Math.PI / 2f;
                    flag = false;
                }
                else if (i == 4) {
                    radians = -(float)Math.PI / 2f;
                    flag = true;
                }
                else {
                    radians = (float)Math.PI / 2f;
                    flag = true;
                }
                for (int j = 0; j < 4; j++) {
                    int num = (i << 2) + j;
                    Matrix m = Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateRotationZ(-j * (float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * (flag ? Matrix.CreateRotationX(radians) : Matrix.CreateRotationY(radians)) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                    BlockMesh blockMesh = new();
                    blockMesh.AppendModelMeshPart(
                        modelMeshPart,
                        boneAbsoluteTransform * m,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    m_blockMeshes[num] = blockMesh;
                    m_collisionBoxes[num] = [blockMesh.CalculateBoundingBox()];
                    BlockMesh blockMesh2 = new();
                    blockMesh2.AppendModelMeshPart(
                        modelMeshPart,
                        boneAbsoluteTransform * m * Matrix.CreateTranslation(new Vector3(-m_upPoint3[num])),
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    m_bottomCollisionBoxes[num] = [blockMesh2.CalculateBoundingBox()];
                }
            }
            Matrix m2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBlockMesh.AppendModelMeshPart(
                modelMeshPart,
                boneAbsoluteTransform * m2,
                false,
                false,
                false,
                false,
                Color.White
            );
        }

        public GVSignalGeneratorBlock() : base("Models/GVSignalGenerator", "GVSignalGenerator", 0.375f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) {
            int face = GetFace(value);
            int data = Terrain.ExtractData(value);
            int rotation = GetRotation(data);
            Point3 upDirection = m_upPoint3[face * 4 + rotation];
            Point3 another;
            GVCellFace up;
            GVCellFace bottom;
            bool isUp = GetIsTopPart(data);
            if (isUp) {
                up = new GVCellFace(x, y, z, face);
                another.X = up.X - upDirection.X;
                another.Y = up.Y - upDirection.Y;
                another.Z = up.Z - upDirection.Z;
                bottom = new GVCellFace(another.X, another.Y, another.Z, face);
            }
            else {
                bottom = new GVCellFace(x, y, z, face);
                another.X = bottom.X + upDirection.X;
                another.Y = bottom.Y + upDirection.Y;
                another.Z = bottom.Z + upDirection.Z;
                up = new GVCellFace(another.X, another.Y, another.Z, face);
            }
            if (!subsystemGVElectricity.m_GVElectricElementsToAdd[subterrainId].ContainsKey(another)) {
                int anotherValue = subsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(subterrainId).GetCellValue(another.X, another.Y, another.Z);
                if (Terrain.ExtractContents(anotherValue) == BlockIndex
                    && GetFace(anotherValue) == face) {
                    int anotherData = Terrain.ExtractData(anotherValue);
                    if (GetRotation(anotherData) == rotation
                        && GetIsTopPart(anotherData) != isUp) {
                        return new SignalGeneratorGVElectricElement(subsystemGVElectricity, [bottom, up], subterrainId);
                    }
                }
            }
            return null;
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
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

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int num = data & 0x1F;
            if (!GetIsTopPart(data)) {
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
            }
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

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int data = Terrain.ExtractData(value);
            return GetIsTopPart(data) ? m_bottomCollisionBoxes[data & 0x1F] : m_collisionBoxes[data & 0x1F];
        }

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            bool isUp = GetIsTopPart(data);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                switch (connectorDirection) {
                    case GVElectricConnectorDirection.Right:
                    case GVElectricConnectorDirection.Left: return GVElectricConnectorType.Input;
                    case GVElectricConnectorDirection.Top: return isUp ? GVElectricConnectorType.Output : null;
                    case GVElectricConnectorDirection.Bottom: return isUp ? null : GVElectricConnectorType.Input;
                    case GVElectricConnectorDirection.In: return isUp ? GVElectricConnectorType.Output : GVElectricConnectorType.Input;
                }
            }
            return null;
        }

        public static bool GetIsTopPart(int data) => (data & 32) != 0;
        public static int SetIsTopPart(int data, bool isUp) => (data & -33) | (isUp ? 32 : 0);
    }
}