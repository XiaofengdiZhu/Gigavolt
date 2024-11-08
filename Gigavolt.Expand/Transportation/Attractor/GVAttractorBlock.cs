using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVAttractorBlock : GVBaseBlock, IGVElectricElementBlock {
        public const int Index = 996;

        public readonly BlockMesh[] m_meshesByData = new BlockMesh[2];

        public readonly BlockMesh m_standaloneMesh = new();

        public readonly BoundingBox[][] m_collisionBoxesByData = new BoundingBox[2][];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/GVAttractor");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Magnet").ParentBone);
            for (int i = 0; i < 2; i++) {
                m_meshesByData[i] = new BlockMesh();
                m_meshesByData[i]
                .AppendModelMeshPart(
                    model.FindMesh("Magnet").MeshParts[0],
                    boneAbsoluteTransform * Matrix.CreateRotationY((float)Math.PI / 2f * i) * Matrix.CreateTranslation(0.5f, 0f, 0.5f),
                    false,
                    false,
                    true,
                    false,
                    Color.White
                );
                m_collisionBoxesByData[i] = [m_meshesByData[i].CalculateBoundingBox()];
            }
            m_standaloneMesh.AppendModelMeshPart(
                model.FindMesh("Magnet").MeshParts[0],
                boneAbsoluteTransform * Matrix.CreateScale(1.5f) * Matrix.CreateTranslation(0f, -0.25f, 0f),
                false,
                false,
                true,
                false,
                Color.White
            );
            base.Initialize();
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            showDebris = true;
            if (toolLevel >= RequiredToolLevel) {
                int data = Terrain.ExtractData(oldValue);
                dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, data), Count = 1 });
            }
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = Terrain.ExtractData(value);
            if (num < m_collisionBoxesByData.Length) {
                return m_collisionBoxesByData[num];
            }
            return null;
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value);
            if (num < m_collisionBoxesByData.Length) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_meshesByData[num],
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
                    4,
                    0.2f,
                    Vector2.Zero,
                    geometry.SubsetOpaque
                );
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneMesh,
                color,
                size,
                ref matrix,
                environmentData
            );
        }

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new AttractorGVElectricElement(subsystemGVElectricity, new Point3(x, y, z), subterrainId);

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            int data = !(MathF.Abs(forward.X) > MathF.Abs(forward.Z)) ? 1 : 0;
            BlockPlacementData result = default;
            result.CellFace = raycastResult.CellFace;
            result.Value = Terrain.ReplaceData(value, data);
            return result;
        }


        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, Terrain terrain) {
            if (face == 4
                && SubsystemGVElectricity.GetConnectorDirection(4, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;
    }
}