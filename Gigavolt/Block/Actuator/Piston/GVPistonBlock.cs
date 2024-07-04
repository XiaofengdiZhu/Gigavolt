using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVPistonBlock : Block, IGVElectricElementBlock {
        public const int Index = 854;

        public readonly BlockMesh[] m_standaloneBlockMeshes = new BlockMesh[4];

        public readonly BlockMesh[] m_blockMeshesByData = new BlockMesh[48];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Pistons");
            for (GVPistonMode GVPistonMode = GVPistonMode.Pushing; GVPistonMode <= GVPistonMode.Complex; GVPistonMode++) {
                for (int i = 0; i < 2; i++) {
                    string name = i == 0 ? "PistonRetracted" : "PistonExtended";
                    Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(name).ParentBone);
                    for (int j = 0; j < 6; j++) {
                        int num = SetFace(SetIsExtended(SetMode(0, GVPistonMode), i != 0), j);
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
                        if (i == 0) {
                            switch (GVPistonMode) {
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
                Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("PistonRetracted").ParentBone);
                m_standaloneBlockMeshes[(int)GVPistonMode] = new BlockMesh();
                m_standaloneBlockMeshes[(int)GVPistonMode]
                .AppendModelMeshPart(
                    model.FindMesh("PistonRetracted").MeshParts[0],
                    boneAbsoluteTransform2 * Matrix.CreateTranslation(0f, -0.5f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                switch (GVPistonMode) {
                    case GVPistonMode.Pulling:
                        m_standaloneBlockMeshes[(int)GVPistonMode].TransformTextureCoordinates(Matrix.CreateTranslation(0f, 0.0625f, 0f), 4);
                        break;
                    case GVPistonMode.StrictPulling:
                        m_standaloneBlockMeshes[(int)GVPistonMode].TransformTextureCoordinates(Matrix.CreateTranslation(0f, 0.125f, 0f), 4);
                        break;
                    case GVPistonMode.Complex:
                        m_standaloneBlockMeshes[(int)GVPistonMode].TransformTextureCoordinates(Matrix.CreateTranslation(0f, -0.5625f, 0f), 4);
                        break;
                }
            }
        }

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) {
            int data = Terrain.ExtractData(value);
            int face2 = GetFace(data);
            if (GetIsExtended(data)) {
                if (face != face2) {
                    return face != CellFace.OppositeFace(face2);
                }
                return false;
            }
            return false;
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value) & 0x3F;
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

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int mode = (int)GetMode(Terrain.ExtractData(value));
            if (mode < m_standaloneBlockMeshes.Length
                && m_standaloneBlockMeshes[mode] != null) {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    m_standaloneBlockMeshes[mode],
                    color,
                    1f * size,
                    ref matrix,
                    environmentData
                );
            }
        }

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new PistonGVElectricElement(subsystemGVElectricity, new GVPoint3(x, y, z, subterrainId), GetMode(Terrain.ExtractData(value)) == GVPistonMode.Complex);

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) => GVElectricConnectorType.Input;

        public int GetConnectionMask(int value) => int.MaxValue;

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(Index, 0, SetMode(0, GVPistonMode.Pushing));
            yield return Terrain.MakeBlockValue(Index, 0, SetMode(0, GVPistonMode.Pulling));
            yield return Terrain.MakeBlockValue(Index, 0, SetMode(0, GVPistonMode.StrictPulling));
            yield return Terrain.MakeBlockValue(Index, 0, SetMode(0, GVPistonMode.Complex));
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => Mode2Name(GetMode(Terrain.ExtractData(value)));

        public static string Mode2Name(GVPistonMode mode) {
            switch (mode) {
                case GVPistonMode.Pulling: return "GV粘性活塞";
                case GVPistonMode.StrictPulling: return "GV严格粘性活塞";
                case GVPistonMode.Complex: return "GV复杂活塞";
                default: return "GV活塞";
            }
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float num = float.PositiveInfinity;
            int face = 0;
            for (int i = 0; i < 6; i++) {
                float num2 = Vector3.Dot(CellFace.FaceToVector3(i), forward);
                if (num2 < num) {
                    num = num2;
                    face = i;
                }
            }
            int data = Terrain.ExtractData(value);
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, SetFace(data, face));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetMode(SetFace(SetIsExtended(data, false), 0), GetMode(data))), Count = 1 });
            showDebris = true;
        }

        public override bool IsNonDuplicable_(int value) {
            int data = Terrain.ExtractData(value);
            return GetMode(data) != GVPistonMode.Complex && ((data >> 6) & 4095) > 0;
        }

        public override bool IsEditable_(int value) => GetMode(Terrain.ExtractData(value)) != GVPistonMode.Complex;

        public static bool GetIsExtended(int data) => (data & 1) != 0;

        public static int SetIsExtended(int data, bool isExtended) => (data & -2) | (isExtended ? 1 : 0);

        public static GVPistonMode GetMode(int data) => (GVPistonMode)((data >> 1) & 3);

        public static int SetMode(int data, GVPistonMode mode) => (data & -7) | (((int)mode & 3) << 1);

        public static int GetFace(int data) => (data >> 3) & 7;

        public static int SetFace(int data, int face) => (data & -57) | ((face & 7) << 3);
    }
}