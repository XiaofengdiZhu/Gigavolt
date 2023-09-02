using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVCrusherProjectileBlock : Block {
        public const int Index = 883;

        public BlockMesh m_standaloneBlockMesh = new BlockMesh();

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Snowball");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Snowball").ParentBone);
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Snowball").MeshParts[0],
                boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f),
                false,
                false,
                false,
                false,
                Color.White
            );
            SubsystemGVProjectiles.m_crusherBlockValue = Index;
            SubsystemGVProjectiles.m_interactorBlockValue = Terrain.MakeBlockValue(Index, 0, 1);
            base.Initialize();
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) { }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                color,
                2.5f * size,
                ref matrix,
                environmentData
            );
        }

        public override IEnumerable<int> GetCreativeValues() => new[] { Index, Terrain.MakeBlockValue(Index, 0, 1) };

        public override int GetFaceTextureSlot(int face, int value) {
            switch (Terrain.ExtractData(value)) {
                case 1: return 165;
                default: return 45;
            }
        }
    }
}