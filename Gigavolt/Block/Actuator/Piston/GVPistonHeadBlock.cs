using System;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVPistonHeadBlock : Block {
        public const int Index = 855;

        public readonly BlockMesh[] m_blockMeshesByData = new BlockMesh[48];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Pistons");
            for (int i = 0; i < 2; i++) {
                string name = i == 0 ? "PistonHead" : "PistonShaft";
                Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(name).ParentBone);
                for (GVPistonMode pistonMode = GVPistonMode.Pushing; pistonMode <= GVPistonMode.Complex; pistonMode++) {
                    for (int j = 0; j < 6; j++) {
                        int num = SetFace(SetMode(SetIsShaft(0, i != 0), pistonMode), j);
                        Matrix m = j < 4 ? Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationY(j * (float)Math.PI / 2f + (float)Math.PI) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f) :
                            j != 4 ? Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationX(-(float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f) : Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                        m_blockMeshesByData[num] = new BlockMesh();
                        m_blockMeshesByData[num]
                        .AppendModelMeshPart(
                            model.FindMesh(name).MeshParts[0],
                            boneAbsoluteTransform * m,
                            false,
                            false,
                            false,
                            false,
                            Color.White
                        );
                        switch (pistonMode) {
                            case GVPistonMode.Pulling:
                                m_blockMeshesByData[num].TransformTextureCoordinates(Matrix.CreateTranslation(0f, 0.0625f, 0f), 1 << j);
                                break;
                            case GVPistonMode.StrictPulling:
                                m_blockMeshesByData[num].TransformTextureCoordinates(Matrix.CreateTranslation(0f, 0.125f, 0f), 1 << j);
                                break;
                            case GVPistonMode.Complex:
                                m_blockMeshesByData[num].TransformTextureCoordinates(Matrix.CreateTranslation(0f, -0.5625f, 0f), 1 << j);
                                break;
                        }
                    }
                }
            }
        }

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) {
            int data = Terrain.ExtractData(value);
            if (GetTransparent(data)) {
                return true;
            }
            return face != GetFace(data);
        }

        public override int GetShadowStrength(int value) {
            int data = Terrain.ExtractData(value);
            if (GetTransparent(data)) {
                return 0;
            }
            if (!GetIsShaft(data)) {
                return base.GetShadowStrength(value);
            }
            return 0;
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value);
            if (GetTransparent(num)) {
                return;
            }
            if (num < m_blockMeshesByData.Length
                && m_blockMeshesByData[num] != null) {
                generator.GenerateShadedMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByData[num],
                    Color.White,
                    null,
                    null,
                    geometry.SubsetOpaque
                );
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) { }

        public static GVPistonMode GetMode(int data) => (GVPistonMode)(data & 3);

        public static int SetMode(int data, GVPistonMode mode) => (data & -4) | ((int)mode & 3);

        public static bool GetIsShaft(int data) => (data & 4) != 0;

        public static int SetIsShaft(int data, bool isShaft) => (data & -5) | (isShaft ? 4 : 0);

        public static int GetFace(int data) => (data >> 3) & 7;

        public static int SetFace(int data, int face) => (data & -57) | ((face & 7) << 3);
        public static bool GetTransparent(int data) => ((data >> 6) & 1) == 1;
        public static int SetTransparent(int data, bool transparent) => (data & -65) | (transparent ? 64 : 0);
    }
}