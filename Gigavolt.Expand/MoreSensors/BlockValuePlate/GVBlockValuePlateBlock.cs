using System;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVBlockValuePlateBlock : MountedGVElectricElementBlock {
        public const int Index = 876;

        public BlockMesh m_standaloneBlockMeshesByMaterial;

        public BlockMesh[] m_blockMeshesByData = new BlockMesh[16];

        public BoundingBox[][] m_collisionBoxesByData = new BoundingBox[16][];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/PressurePlate");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("PressurePlate").ParentBone);
            int num = 78;
            for (int j = 0; j < 6; j++) {
                int num2 = SetMountingFace(0, j);
                Matrix matrix = j >= 4 ? j != 4 ? Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f) : Matrix.CreateTranslation(0.5f, 0f, 0.5f) : Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY(j * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                m_blockMeshesByData[num2] = new BlockMesh();
                m_blockMeshesByData[num2]
                .AppendModelMeshPart(
                    model.FindMesh("PressurePlate").MeshParts[0],
                    boneAbsoluteTransform * matrix,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_blockMeshesByData[num2].TransformTextureCoordinates(Matrix.CreateTranslation(num % 16 / 16f, num / 16 / 16f, 0f));
                m_blockMeshesByData[num2].GenerateSidesData();
                Vector3 vector = Vector3.Transform(new Vector3(-0.5f, 0f, -0.5f), matrix);
                Vector3 vector2 = Vector3.Transform(new Vector3(0.5f, 0.0625f, 0.5f), matrix);
                vector.X = MathF.Round(vector.X * 100f) / 100f;
                vector.Y = MathF.Round(vector.Y * 100f) / 100f;
                vector.Z = MathF.Round(vector.Z * 100f) / 100f;
                vector2.X = MathF.Round(vector2.X * 100f) / 100f;
                vector2.Y = MathF.Round(vector2.Y * 100f) / 100f;
                vector2.Z = MathF.Round(vector2.Z * 100f) / 100f;
                m_collisionBoxesByData[num2] = [new BoundingBox(new Vector3(MathF.Min(vector.X, vector2.X), MathF.Min(vector.Y, vector2.Y), MathF.Min(vector.Z, vector2.Z)), new Vector3(MathF.Max(vector.X, vector2.X), MathF.Max(vector.Y, vector2.Y), MathF.Max(vector.Z, vector2.Z)))];
            }
            Matrix identity = Matrix.Identity;
            m_standaloneBlockMeshesByMaterial = new BlockMesh();
            m_standaloneBlockMeshesByMaterial.AppendModelMeshPart(
                model.FindMesh("PressurePlate").MeshParts[0],
                boneAbsoluteTransform * identity,
                false,
                false,
                false,
                false,
                Color.White
            );
            m_standaloneBlockMeshesByMaterial.TransformTextureCoordinates(Matrix.CreateTranslation(num % 16 / 16f, num / 16 / 16f, 0f));
        }

        public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength) => new(
            subsystemTerrain,
            position,
            strength,
            DestructionDebrisScale,
            Color.White,
            78
        );

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            int data = SetMountingFace(Terrain.ExtractData(value), raycastResult.CellFace.Face);
            int value2 = Terrain.ReplaceData(value, data);
            BlockPlacementData result = default;
            result.Value = value2;
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = Terrain.ExtractData(value);
            if (num >= m_collisionBoxesByData.Length) {
                return null;
            }
            return m_collisionBoxesByData[num];
        }

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) => face != CellFace.OppositeFace(GetFace(value));

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value);
            if (num < m_blockMeshesByData.Length
                && m_blockMeshesByData[num] != null) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByData[num],
                    Color.White,
                    null,
                    geometry.SubsetOpaque
                );
                GVBlockGeometryGenerator.GenerateGVWireVertices(
                    generator,
                    value,
                    x,
                    y,
                    z,
                    GetFace(value),
                    0.8125f,
                    Vector2.Zero,
                    geometry.SubsetOpaque
                );
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMeshesByMaterial,
                color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new BlockValuePlateGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public static int GetMountingFace(int data) => (data >> 1) & 7;

        public static int SetMountingFace(int data, int face) => (data & -15) | ((face & 7) << 1);

        public override int GetFace(int value) => GetMountingFace(Terrain.ExtractData(value));
    }
}