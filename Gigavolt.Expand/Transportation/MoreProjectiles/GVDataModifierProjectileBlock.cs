using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDataModifierProjectileBlock : Block, IGVCustomWheelPanelBlock {
        public const int Index = 884;

        public readonly BlockMesh m_standaloneBlockMesh = new();

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/MoreProjectiles");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("GVDataModifier").ParentBone);
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("GVDataModifier").MeshParts[0],
                boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f),
                false,
                false,
                false,
                false,
                Color.White
            );
            SubsystemGVProjectiles.m_dataModifierBlockContent = Index;
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

        public List<int> GetCustomWheelPanelValues(int centerValue) => [GVCrusherProjectileBlock.Index, Terrain.MakeBlockValue(GVCrusherProjectileBlock.Index, 0, 1), Index];
    }
}