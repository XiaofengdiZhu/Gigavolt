using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVCrusherProjectileBlock : Block, IGVCustomWheelPanelBlock {
        public const int Index = 883;

        public readonly BlockMesh[] m_standaloneBlockMeshes = new BlockMesh[2];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/MoreProjectiles");
            for (int i = 0; i < 2; i++) {
                m_standaloneBlockMeshes[i] = new BlockMesh();
                ModelMesh mesh = model.FindMesh(i == 0 ? "GVCrusher" : "GVInteractor");
                Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(mesh.ParentBone);
                m_standaloneBlockMeshes[i]
                .AppendModelMeshPart(
                    mesh.MeshParts[0],
                    boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
            }
            SubsystemGVProjectiles.m_crusherBlockValue = Index;
            SubsystemGVProjectiles.m_interactorBlockValue = Terrain.MakeBlockValue(Index, 0, 1);
            base.Initialize();
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) { }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int data = Terrain.ExtractData(value);
            if (data < m_standaloneBlockMeshes.Length) {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    m_standaloneBlockMeshes[data],
                    color,
                    2.5f * size,
                    ref matrix,
                    environmentData
                );
            }
        }

        public override IEnumerable<int> GetCreativeValues() => new[] { Index, Terrain.MakeBlockValue(Index, 0, 1) };

        public override int GetFaceTextureSlot(int face, int value) {
            switch (Terrain.ExtractData(value)) {
                case 1: return 136;
                default: return 45;
            }
        }

        public List<int> GetCustomWheelPanelValues(int centerValue) => [Index, Terrain.MakeBlockValue(Index, 0, 1), GVDataModifierProjectileBlock.Index];
    }
}