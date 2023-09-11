using Engine;
using Engine.Graphics;

namespace Game {
    public class GVSolidifiedAirBlock : Block {
        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            Color newColor = new Color(color.RGB, 100);
            DrawCubeBlock(
                primitivesRenderer,
                value,
                new Vector3(size),
                ref matrix,
                newColor,
                newColor,
                environmentData,
                environmentData.SubsystemTerrain != null ? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture : BlocksTexturesManager.DefaultBlocksTexture
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) { }

        public const int Index = 888;

        public static void DrawCubeBlock(PrimitivesRenderer3D primitivesRenderer, int value, Vector3 size, ref Matrix matrix, Color color, Color topColor, DrawBlockEnvironmentData environmentData, Texture2D texture) {
            environmentData = environmentData ?? BlocksManager.m_defaultEnvironmentData;
            TexturedBatch3D texturedBatch3D = primitivesRenderer.TexturedBatch(
                texture,
                true,
                0,
                DepthStencilState.Default,
                RasterizerState.CullCounterClockwiseScissor,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp
            );
            float s = LightingManager.LightIntensityByLightValue[environmentData.Light];
            color = Color.MultiplyColorOnly(color, s);
            topColor = Color.MultiplyColorOnly(topColor, s);
            Vector3 translation = matrix.Translation;
            Vector3 vector = matrix.Right * size.X;
            Vector3 v = matrix.Up * size.Y;
            Vector3 v2 = matrix.Forward * size.Z;
            Vector3 v3 = translation + 0.5f * (-vector - v - v2);
            Vector3 v4 = translation + 0.5f * (vector - v - v2);
            Vector3 v5 = translation + 0.5f * (-vector + v - v2);
            Vector3 v6 = translation + 0.5f * (vector + v - v2);
            Vector3 v7 = translation + 0.5f * (-vector - v + v2);
            Vector3 v8 = translation + 0.5f * (vector - v + v2);
            Vector3 v9 = translation + 0.5f * (-vector + v + v2);
            Vector3 v10 = translation + 0.5f * (vector + v + v2);
            if (environmentData.ViewProjectionMatrix.HasValue) {
                Matrix m = environmentData.ViewProjectionMatrix.Value;
                Vector3.Transform(ref v3, ref m, out v3);
                Vector3.Transform(ref v4, ref m, out v4);
                Vector3.Transform(ref v5, ref m, out v5);
                Vector3.Transform(ref v6, ref m, out v6);
                Vector3.Transform(ref v7, ref m, out v7);
                Vector3.Transform(ref v8, ref m, out v8);
                Vector3.Transform(ref v9, ref m, out v9);
                Vector3.Transform(ref v10, ref m, out v10);
            }
            int num = Terrain.ExtractContents(value);
            Block block = BlocksManager.Blocks[num];
            Vector4 vector2 = Vector4.Zero;
            int textureSlotCount = block.GetTextureSlotCount(value);
            int textureSlot = block.GetFaceTextureSlot(0, value);
            vector2.X = (float)(textureSlot % textureSlotCount) / textureSlotCount;
            vector2.Y = (float)(textureSlot / textureSlotCount) / textureSlotCount;
            vector2.W = vector2.Y + 1f / textureSlotCount;
            vector2.Z = vector2.X + 1f / textureSlotCount;
            texturedBatch3D.QueueQuad(
                color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Forward)),
                p1: v3,
                p2: v5,
                p3: v6,
                p4: v4,
                texCoord1: new Vector2(vector2.X, vector2.W),
                texCoord2: new Vector2(vector2.X, vector2.Y),
                texCoord3: new Vector2(vector2.Z, vector2.Y),
                texCoord4: new Vector2(vector2.Z, vector2.W)
            );
            textureSlot = block.GetFaceTextureSlot(2, value);
            vector2.X = (float)(textureSlot % textureSlotCount) / textureSlotCount;
            vector2.Y = (float)(textureSlot / textureSlotCount) / textureSlotCount;
            vector2.W = vector2.Y + 1f / textureSlotCount;
            vector2.Z = vector2.X + 1f / textureSlotCount;
            texturedBatch3D.QueueQuad(
                color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(matrix.Forward)),
                p1: v7,
                p2: v8,
                p3: v10,
                p4: v9,
                texCoord1: new Vector2(vector2.Z, vector2.W),
                texCoord2: new Vector2(vector2.X, vector2.W),
                texCoord3: new Vector2(vector2.X, vector2.Y),
                texCoord4: new Vector2(vector2.Z, vector2.Y)
            );
            textureSlot = block.GetFaceTextureSlot(5, value);
            vector2.X = (float)(textureSlot % textureSlotCount) / textureSlotCount;
            vector2.Y = (float)(textureSlot / textureSlotCount) / textureSlotCount;
            vector2.W = vector2.Y + 1f / textureSlotCount;
            vector2.Z = vector2.X + 1f / textureSlotCount;
            texturedBatch3D.QueueQuad(
                color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Up)),
                p1: v3,
                p2: v4,
                p3: v8,
                p4: v7,
                texCoord1: new Vector2(vector2.X, vector2.Y),
                texCoord2: new Vector2(vector2.Z, vector2.Y),
                texCoord3: new Vector2(vector2.Z, vector2.W),
                texCoord4: new Vector2(vector2.X, vector2.W)
            );
            textureSlot = block.GetFaceTextureSlot(4, value);
            vector2.X = (float)(textureSlot % textureSlotCount) / textureSlotCount;
            vector2.Y = (float)(textureSlot / textureSlotCount) / textureSlotCount;
            vector2.W = vector2.Y + 1f / textureSlotCount;
            vector2.Z = vector2.X + 1f / textureSlotCount;
            texturedBatch3D.QueueQuad(
                color: Color.MultiplyColorOnly(topColor, LightingManager.CalculateLighting(matrix.Up)),
                p1: v5,
                p2: v9,
                p3: v10,
                p4: v6,
                texCoord1: new Vector2(vector2.X, vector2.W),
                texCoord2: new Vector2(vector2.X, vector2.Y),
                texCoord3: new Vector2(vector2.Z, vector2.Y),
                texCoord4: new Vector2(vector2.Z, vector2.W)
            );
            textureSlot = block.GetFaceTextureSlot(1, value);
            vector2.X = (float)(textureSlot % textureSlotCount) / textureSlotCount;
            vector2.Y = (float)(textureSlot / textureSlotCount) / textureSlotCount;
            vector2.W = vector2.Y + 1f / textureSlotCount;
            vector2.Z = vector2.X + 1f / textureSlotCount;
            texturedBatch3D.QueueQuad(
                color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Right)),
                p1: v3,
                p2: v7,
                p3: v9,
                p4: v5,
                texCoord1: new Vector2(vector2.Z, vector2.W),
                texCoord2: new Vector2(vector2.X, vector2.W),
                texCoord3: new Vector2(vector2.X, vector2.Y),
                texCoord4: new Vector2(vector2.Z, vector2.Y)
            );
            textureSlot = block.GetFaceTextureSlot(3, value);
            vector2.X = (float)(textureSlot % textureSlotCount) / textureSlotCount;
            vector2.Y = (float)(textureSlot / textureSlotCount) / textureSlotCount;
            vector2.W = vector2.Y + 1f / textureSlotCount;
            vector2.Z = vector2.X + 1f / textureSlotCount;
            texturedBatch3D.QueueQuad(
                color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(matrix.Right)),
                p1: v4,
                p2: v6,
                p3: v10,
                p4: v8,
                texCoord1: new Vector2(vector2.X, vector2.W),
                texCoord2: new Vector2(vector2.X, vector2.Y),
                texCoord3: new Vector2(vector2.Z, vector2.Y),
                texCoord4: new Vector2(vector2.Z, vector2.W)
            );
        }
    }
}