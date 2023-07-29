using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVTrapdoorBlock : GenerateGVWireVerticesBlock, IGVElectricElementBlock {
        public const int Index = 865;

        public BlockMesh[] m_standaloneBlockMesh = new BlockMesh[2];
        public Matrix[] m_boneAbsoluteTransform = new Matrix[2];
        public ModelMeshPart[] m_modelMeshPart = new ModelMeshPart[2];
        public Dictionary<int, BlockMesh> m_cachedBlockMeshes = new Dictionary<int, BlockMesh>();
        public Dictionary<int, BoundingBox[]> m_cachedCollisionBoxes = new Dictionary<int, BoundingBox[]>();
        public string[] m_displayNamesByModel = { "GV木活板门", "GV铁活板门" };
        public int[] m_creativeValuesByModel = { Terrain.MakeBlockValue(Index, 0, 0), Terrain.MakeBlockValue(Index, 0, SetModel(0, 1)) };

        public override void Initialize() {
            base.Initialize();
            Model[] model = new Model[2];
            model[0] = ContentManager.Get<Model>("Models/WoodenTrapdoor");
            model[1] = ContentManager.Get<Model>("Models/CellTrapdoor");
            for (int i = 0; i < 2; i++) {
                m_boneAbsoluteTransform[i] = BlockMesh.GetBoneAbsoluteTransform(model[i].FindMesh("Trapdoor").ParentBone);
                m_modelMeshPart[i] = model[i].FindMesh("Trapdoor").MeshParts[0];
                BlockMesh blockMesh = new BlockMesh();
                blockMesh.AppendModelMeshPart(
                    m_modelMeshPart[i],
                    m_boneAbsoluteTransform[i] * Matrix.CreateTranslation(0f, 0f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_standaloneBlockMesh[i] = blockMesh;
            }
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (!m_cachedBlockMeshes.TryGetValue(data, out BlockMesh blockMesh)) {
                GenerateMeshAndBox(data, out blockMesh, out _);
            }
            generator.GenerateShadedMeshVertices(
                this,
                x,
                y,
                z,
                blockMesh,
                Color.White,
                null,
                null,
                geometry.SubsetAlphaTest
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh[GetModel(Terrain.ExtractData(value))],
                color,
                size,
                ref matrix,
                environmentData
            );
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            int rotation;
            bool upsideDown;
            if (raycastResult.CellFace.Face < 4) {
                rotation = raycastResult.CellFace.Face;
                upsideDown = raycastResult.HitPoint().Y - raycastResult.CellFace.Y > 0.5f;
            }
            else {
                Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
                float num = Vector3.Dot(forward, Vector3.UnitZ);
                float num2 = Vector3.Dot(forward, Vector3.UnitX);
                float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
                float num4 = Vector3.Dot(forward, -Vector3.UnitX);
                rotation = num == MathUtils.Max(num, num2, num3, num4) ? 2 :
                    num2 == MathUtils.Max(num, num2, num3, num4) ? 3 :
                    num3 != MathUtils.Max(num, num2, num3, num4) ? num4 == MathUtils.Max(num, num2, num3, num4) ? 1 : 0 : 0;
                upsideDown = raycastResult.CellFace.Face == 5;
            }
            int data = SetModel(SetOpen(SetRotation(SetUpsideDown(0, upsideDown), rotation), 0), GetModel(Terrain.ExtractData(value)));
            BlockPlacementData result = default;
            result.Value = Terrain.ReplaceData(Terrain.ReplaceContents(0, BlockIndex), data);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int data = Terrain.ExtractData(value);
            if (!m_cachedCollisionBoxes.TryGetValue(data, out BoundingBox[] box)) {
                GenerateMeshAndBox(data, out _, out box);
            }
            return box;
        }

        public override bool ShouldAvoid(int value) => GetOpen(Terrain.ExtractData(value)) > 11;

        public override bool IsHeatBlocker(int value) => GetOpen(Terrain.ExtractData(value)) >= 90;

        public override IEnumerable<int> GetCreativeValues() => m_creativeValuesByModel;
        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => m_displayNamesByModel[GetModel(Terrain.ExtractData(value))];

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int model = GetModel(Terrain.ExtractData(oldValue));
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetModel(0, model)), Count = 1 });
            showDebris = true;
        }

        public override float GetDensity(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 0.5f,
                _ => 3f
            };
        }

        public override float GetFuelFireDuration(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 5f,
                _ => 0f
            };
        }

        public override float GetFireDuration(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 12f,
                _ => 0f
            };
        }

        public override float GetExplosionResilience(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 2f,
                _ => 10f
            };
        }

        public override BlockDigMethod GetBlockDigMethod(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => BlockDigMethod.Hack,
                _ => BlockDigMethod.Quarry
            };
        }

        public override float GetDigResilience(int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => 5f,
                _ => 10f
            };
        }

        public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value) {
            int model = GetModel(Terrain.ExtractData(value));
            return model switch {
                0 => "Wood",
                _ => "Metal"
            };
        }

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemElectricity, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            return new TrapDoorGVElectricElement(subsystemElectricity, new CellFace(x, y, z, GetMountingFace(data)));
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (face == GetMountingFace(data)) {
                int rotation = GetRotation(data);
                if (SubsystemGVElectricity.GetConnectorDirection(4, (4 - rotation) % 4, connectorFace) == GVElectricConnectorDirection.Top) {
                    return GVElectricConnectorType.Input;
                }
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public static int GetRotation(int data) => data & 3;

        public static int GetOpen(int data) {
            int num = (data >> 4) & 127;
            return num > 90 ? 90 : num;
        }

        public static bool GetUpsideDown(int data) => (data & 4) != 0;

        public static int SetRotation(int data, int rotation) => (data & -4) | (rotation & 3);

        public static int SetOpen(int data, int open) {
            open = open > 90 ? 90 : open;
            return (data & -2033) | ((open & 127) << 4);
        }

        public static int SetUpsideDown(int data, bool upsideDown) {
            if (!upsideDown) {
                return data & -5;
            }
            return data | 4;
        }

        public static int GetMountingFace(int data) {
            if (!GetUpsideDown(data)) {
                return 4;
            }
            return 5;
        }

        public static int GetModel(int data) => (data >> 3) & 1;

        public static int SetModel(int data, int model) => (data & -9) | ((model & 1) << 3);

        public void GenerateMeshAndBox(int data, out BlockMesh blockMesh, out BoundingBox[] box) {
            blockMesh = new BlockMesh();
            int rotation = GetRotation(data);
            int open = GetOpen(data);
            bool upsideDown = GetUpsideDown(data);
            int model = GetModel(data);
            Matrix identity = Matrix.Identity;
            identity *= Matrix.CreateTranslation(0f, -0.0625f, 0.4375f) * Matrix.CreateRotationX(open > 0 ? -MathUtils.DegToRad(open) : 0f) * Matrix.CreateTranslation(0f, 0.0625f, -0.4375f);
            identity *= Matrix.CreateRotationZ(upsideDown ? (float)Math.PI : 0f);
            identity *= Matrix.CreateRotationY(rotation * (float)Math.PI / 2f);
            identity *= Matrix.CreateTranslation(new Vector3(0.5f, upsideDown ? 1 : 0, 0.5f));
            blockMesh.AppendModelMeshPart(
                m_modelMeshPart[model],
                m_boneAbsoluteTransform[model] * identity,
                false,
                false,
                false,
                false,
                Color.White
            );
            blockMesh.GenerateSidesData();
            m_cachedBlockMeshes.Add(data, blockMesh);
            BoundingBox boundingBox = blockMesh.CalculateBoundingBox();
            box = new[] { boundingBox };
            m_cachedCollisionBoxes.Add(data, box);
        }
    }
}