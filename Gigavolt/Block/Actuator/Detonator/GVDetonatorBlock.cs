using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDetonatorBlock : MountedGVElectricElementBlock {
        public const int Index = 861;

        public readonly BlockMesh m_standaloneBlockMesh = new();
        public readonly BlockMesh[] m_blockMeshesByData = new BlockMesh[6];
        public readonly BoundingBox[][] m_collisionBoxesByData = new BoundingBox[6][];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Detonator");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Detonator").ParentBone);
            for (int i = 0; i < 6; i++) {
                int num = i;
                Matrix m = i >= 4 ? i != 4 ? Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f) : Matrix.CreateTranslation(0.5f, 0f, 0.5f) : Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY(i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                m_blockMeshesByData[num] = new BlockMesh();
                m_blockMeshesByData[num]
                .AppendModelMeshPart(
                    model.FindMesh("Detonator").MeshParts[0],
                    boneAbsoluteTransform * m,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_collisionBoxesByData[num] = [m_blockMeshesByData[num].CalculateBoundingBox()];
            }
            Matrix m2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Detonator").MeshParts[0],
                boneAbsoluteTransform * m2,
                false,
                false,
                false,
                false,
                Color.White
            );
        }

        public override int GetFace(int value) => Terrain.ExtractData(value) & 7;

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            result.Value = Terrain.ReplaceData(value, SetClassic(raycastResult.CellFace.Face, GetClassic(Terrain.ExtractData(value))));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = GetFace(value);
            if (num >= m_collisionBoxesByData.Length) {
                return null;
            }
            return m_collisionBoxesByData[num];
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = GetFace(value);
            if (num < m_blockMeshesByData.Length) {
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
                    0.125f,
                    Vector2.Zero,
                    geometry.SubsetOpaque
                );
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                color,
                4f * size,
                ref matrix,
                environmentData
            );
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new DetonatorGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId, GetClassic(Terrain.ExtractData(value)));

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => LanguageControl.Get(GetType().Name, GetClassic(Terrain.ExtractData(value)) ? "ClassicDisplayName" : "DisplayName");
        public override string GetDescription(int value) => LanguageControl.Get(GetType().Name, GetClassic(Terrain.ExtractData(value)) ? "ClassicDescription" : "Description");
        public override string GetCategory(int value) => GetClassic(Terrain.ExtractData(value)) ? "GV Electrics Regular" : "GV Electrics Shift";
        public override int GetDisplayOrder(int value) => GetClassic(Terrain.ExtractData(value)) ? 38 : 22;
        public override IEnumerable<int> GetCreativeValues() => [BlockIndex, Terrain.MakeBlockValue(BlockIndex, 0, SetClassic(0, true))];
        public static bool GetClassic(int data) => (data & 8) != 0;
        public static int SetClassic(int data, bool classic) => (data & -9) | (classic ? 8 : 0);
    }
}