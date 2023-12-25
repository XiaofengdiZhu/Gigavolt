using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    public class GVCopperHammerBlock() : HammerBlock(47, 77) {
        public const int Index = 867;
        public readonly Texture2D WhiteTexture = Texture2D.Load(new Image(1, 1) { Pixels = { [0] = Color.White } });

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            environmentData = environmentData ?? BlocksManager.m_defaultEnvironmentData;
            int? blockColor = GetColor(Terrain.ExtractData(value));
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                blockColor.HasValue ? WhiteTexture :
                environmentData.SubsystemTerrain == null ? BlocksTexturesManager.DefaultBlocksTexture : environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture,
                blockColor.HasValue ? color * SubsystemPalette.GetColor(environmentData, blockColor) : color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int? paintColor = GetColor(Terrain.ExtractData(value));
            return paintColor.HasValue ? SubsystemPalette.GetName(subsystemTerrain, paintColor, "遥控器") : base.GetDisplayName(subsystemTerrain, value);
        }

        public override string GetCategory(int value) => GetColor(Terrain.ExtractData(value)).HasValue ? "GV Electrics Multiple" : "GV Electrics Expand";

        public override string GetDescription(int value) => GetColor(Terrain.ExtractData(value)).HasValue ? "远程操作和遥控器同色的开关、按钮" : base.GetDescription(value);

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(Index);
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 0));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 8));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 15));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 11));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 12));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 13));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 14));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 1));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 2));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 3));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 4));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 5));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 6));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 10));
        }

        public override int GetDisplayOrder(int value) => GetColor(Terrain.ExtractData(value)).HasValue ? 3 : base.GetDisplayOrder(value);

        public override bool IsEditable_(int value) => !GetColor(Terrain.ExtractData(value)).HasValue;

        public static int? GetColor(int data) {
            if ((data & 16) != 0) {
                return data & 0xF;
            }
            return null;
        }

        public static int SetColor(int data, int? color) {
            if (color.HasValue) {
                return (data & -32) | 16 | (color.Value & 0xF);
            }
            return data & -32;
        }
    }
}