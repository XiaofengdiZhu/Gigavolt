using System;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDebugBlock : GenerateGVWireVerticesBlock, IGVElectricElementBlock {
        public const int Index = 842;

        public BlockMesh[] m_blockMeshesByData = new BlockMesh[4];
        public BlockMesh m_standaloneBlockMesh = new();
        public Texture2D texture;
        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new DebugGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, 4));

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            if (face == 4
                && SubsystemGVElectricity.GetConnectorDirection(4, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/GVDebugTable");
            texture = ContentManager.Get<Texture2D>("Textures/GVDebugBlock");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Lever").ParentBone);
            for (int i = 0; i < 4; i++) {
                m_blockMeshesByData[i] = new BlockMesh();
                Matrix identity = Matrix.Identity;
                identity *= Matrix.CreateRotationY(i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
                m_blockMeshesByData[i]
                .AppendModelMeshPart(
                    model.FindMesh("Lever").MeshParts[0],
                    boneAbsoluteTransform * identity,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
            }
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Lever").MeshParts[0],
                boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f),
                false,
                false,
                false,
                false,
                Color.White
            );
            base.Initialize();
        }

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) => true;

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value);
            if (num < m_blockMeshesByData.Length) {
                generator.GenerateShadedMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByData[num],
                    Color.White,
                    null,
                    null,
                    geometry.GetGeometry(texture).SubsetAlphaTest
                );
            }
            GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                4,
                0.18f,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                texture,
                color,
                size,
                ref matrix,
                environmentData
            );
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float num = Vector3.Dot(forward, Vector3.UnitZ);
            float num2 = Vector3.Dot(forward, Vector3.UnitX);
            float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
            float num4 = Vector3.Dot(forward, -Vector3.UnitX);
            int data = 0;
            if (num == MathUtils.Max(num, num2, num3, num4)) {
                data = 2;
            }
            else if (num2 == MathUtils.Max(num, num2, num3, num4)) {
                data = 3;
            }
            else if (num3 == MathUtils.Max(num, num2, num3, num4)) {
                data = 0;
            }
            else if (num4 == MathUtils.Max(num, num2, num3, num4)) {
                data = 1;
            }
            BlockPlacementData result = default;
            result.Value = Terrain.ReplaceData(Terrain.ReplaceContents(0, 842), data);
            result.CellFace = raycastResult.CellFace;
            return result;
        }
    }
}