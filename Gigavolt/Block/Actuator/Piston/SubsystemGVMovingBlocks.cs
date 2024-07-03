using System;
using Engine;

namespace Game {
    public class SubsystemGVMovingBlocks : SubsystemMovingBlocks {
        public new void GenerateGeometry(MovingBlockSet movingBlockSet) {
            Point3 point = default;
            point.X = movingBlockSet.CurrentVelocity.X > 0f ? (int)MathF.Floor(movingBlockSet.Position.X) : point.X = (int)MathF.Ceiling(movingBlockSet.Position.X);
            point.Y = movingBlockSet.CurrentVelocity.Y > 0f ? (int)MathF.Floor(movingBlockSet.Position.Y) : point.Y = (int)MathF.Ceiling(movingBlockSet.Position.Y);
            point.Z = movingBlockSet.CurrentVelocity.Z > 0f ? (int)MathF.Floor(movingBlockSet.Position.Z) : point.Z = (int)MathF.Ceiling(movingBlockSet.Position.Z);
            if (!(point != movingBlockSet.GeometryGenerationPosition)) {
                return;
            }
            Point3 p = new(movingBlockSet.Box.Left, movingBlockSet.Box.Top, movingBlockSet.Box.Near);
            Point3 point2 = new(movingBlockSet.Box.Width, movingBlockSet.Box.Height, movingBlockSet.Box.Depth);
            int num = point.Y + p.Y;
            point2.Y = MathUtils.Min(point2.Y, 254);
            if (m_blockGeometryGenerator == null) {
                int x = 2;
                x = (int)MathUtils.NextPowerOf2((uint)x);
                m_blockGeometryGenerator = new BlockGeometryGenerator(
                    new Terrain(),
                    m_subsystemTerrain,
                    null,
                    Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true),
                    null,
                    Project.FindSubsystem<SubsystemPalette>(true)
                );
                for (int i = 0; i < x; i++) {
                    for (int j = 0; j < x; j++) {
                        m_blockGeometryGenerator.Terrain.AllocateChunk(i, j);
                    }
                }
            }
            Terrain terrain = m_subsystemTerrain.Terrain;
            for (int k = 0; k < point2.X + 2; k++) {
                for (int l = 0; l < point2.Z + 2; l++) {
                    int x2 = k + p.X + point.X - 1;
                    int z = l + p.Z + point.Z - 1;
                    int shaftValue = terrain.GetShaftValue(x2, z);
                    m_blockGeometryGenerator.Terrain.SetTemperature(k, l, Terrain.ExtractTemperature(shaftValue));
                    m_blockGeometryGenerator.Terrain.SetHumidity(k, l, Terrain.ExtractHumidity(shaftValue));
                    for (int m = 0; m < point2.Y + 2; m++) {
                        if (m_blockGeometryGenerator.Terrain.IsCellValid(k, m + num, l)) {
                            int y = m + p.Y + point.Y - 1;
                            int cellValue = terrain.GetCellValue(x2, y, z);
                            int num2 = Terrain.ExtractContents(cellValue);
                            int light = Terrain.ExtractLight(cellValue);
                            int shadowStrength = BlocksManager.Blocks[num2].GetShadowStrength(cellValue);
                            int value = Terrain.MakeBlockValue(ShadowBlock.Index, light, ShadowBlock.SetShadowStrength(0, shadowStrength));
                            m_blockGeometryGenerator.Terrain.SetCellValueFast(k, m + num, l, value);
                        }
                    }
                }
            }
            m_blockGeometryGenerator.Terrain.SeasonTemperature = terrain.SeasonTemperature;
            m_blockGeometryGenerator.Terrain.SeasonHumidity = terrain.SeasonHumidity;
            foreach (MovingBlock block in movingBlockSet.Blocks) {
                int x3 = block.Offset.X - p.X + 1;
                int num3 = block.Offset.Y - p.Y + 1;
                int z2 = block.Offset.Z - p.Z + 1;
                if (m_blockGeometryGenerator.Terrain.IsCellValid(x3, num3 + num, z2)) {
                    int cellLightFast = m_blockGeometryGenerator.Terrain.GetCellLightFast(x3, num3 + num, z2);
                    int value2 = Terrain.ReplaceLight(block.Value, cellLightFast);
                    m_blockGeometryGenerator.Terrain.SetCellValueFast(x3, num3 + num, z2, value2);
                }
            }
            m_blockGeometryGenerator.ResetCache();
            movingBlockSet.Vertices.Count = 0;
            movingBlockSet.Indices.Count = 0;
            for (int n = 1; n < point2.X + 1; n++) {
                for (int num4 = 1; num4 < point2.Y + 1; num4++) {
                    for (int num5 = 1; num5 < point2.Z + 1; num5++) {
                        if (num4 + num > 0
                            && num4 + num < 255) {
                            int cellValueFast = m_blockGeometryGenerator.Terrain.GetCellValueFast(n, num4 + num, num5);
                            int num6 = Terrain.ExtractContents(cellValueFast);
                            if (num6 != 0) {
                                if (m_blockGeometryGenerator.Terrain.GetChunkAtCell(n + 1, num5) == null) {
                                    m_blockGeometryGenerator.Terrain.AllocateChunk((n + 1) >> 4, num5 >> 4);
                                }
                                if (m_blockGeometryGenerator.Terrain.GetChunkAtCell(n, num5 + 1) == null) {
                                    m_blockGeometryGenerator.Terrain.AllocateChunk(n >> 4, (num5 + 1) >> 4);
                                }
                                BlocksManager.Blocks[num6]
                                .GenerateTerrainVertices(
                                    m_blockGeometryGenerator,
                                    movingBlockSet.Geometry,
                                    cellValueFast,
                                    n,
                                    num4 + num,
                                    num5
                                );
                            }
                        }
                    }
                }
            }
            movingBlockSet.GeometryOffset = new Vector3(p) + new Vector3(0f, -num, 0f) - new Vector3(1f);
            movingBlockSet.GeometryGenerationPosition = point;
        }

        public new void DrawMovingBlockSet(Camera camera, MovingBlockSet movingBlockSet) {
            if (m_vertices.Count <= 20000
                && camera.ViewFrustum.Intersection(movingBlockSet.BoundingBox(false))) {
                GenerateGeometry(movingBlockSet);
                int count = m_vertices.Count;
                int[] array = movingBlockSet.Indices.Array;
                int count2 = movingBlockSet.Indices.Count;
                Vector3 vector = movingBlockSet.Position + movingBlockSet.GeometryOffset;
                TerrainVertex[] array2 = movingBlockSet.Vertices.Array;
                int count3 = movingBlockSet.Vertices.Count;
                for (int i = 0; i < count3; i++) {
                    TerrainVertex item = array2[i];
                    item.X += vector.X;
                    item.Y += vector.Y;
                    item.Z += vector.Z;
                    m_vertices.Add(item);
                }
                for (int j = 0; j < count2; j++) {
                    m_indices.Add(array[j] + count);
                }
            }
        }
    }
}