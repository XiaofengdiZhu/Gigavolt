using System;
using System.Collections.Generic;
using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVSubterrain : Subsystem, IUpdateable, IDrawable {
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemBlockBehaviors m_subsystemBlockBehaviors;

        public UpdateOrder UpdateOrder => UpdateOrder.Terrain;
        public int[] DrawOrders => [0, 100];

        public override void Load(ValuesDictionary valuesDictionary) {
            GVStaticStorage.GVSubterrainSystemDictionary.Clear();
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemBlockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(true);
        }

        public void Update(float dt) {
            foreach (GVSubterrainSystem subterrainSystem in GVStaticStorage.GVSubterrainSystemDictionary.Values) {
                subterrainSystem.Update();
            }
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach (GVSubterrainSystem subterrainSystem in GVStaticStorage.GVSubterrainSystemDictionary.Values) {
                subterrainSystem.Draw(camera, drawOrder);
            }
        }

        const float floatTolerance = 0.01f;

        public bool DropSubterrain(GVSubterrainSystem rootSubterrainSystem) {
            Dictionary<Point3, int> blocks = new();
            Dictionary<Point3, bool> isPointLoaded = new();
            Stack<GVSubterrainSystem> subterrainSystems = new();
            subterrainSystems.Push(rootSubterrainSystem);
            while (subterrainSystems.Count > 0) {
                GVSubterrainSystem subterrainSystem = subterrainSystems.Pop();
                subterrainSystem.GlobalTransform.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 _);
                if (Math.Abs(scale.X - 1f) < floatTolerance
                    && Math.Abs(scale.Y - 1f) < floatTolerance
                    && Math.Abs(scale.Z - 1f) < floatTolerance
                    && IsParallelToAxis(rotation.GetForwardVector())
                    && IsParallelToAxis(rotation.GetRightVector())
                    && IsParallelToAxis(rotation.GetUpVector())) {
                    foreach (TerrainChunk chunk in subterrainSystem.Terrain.AllocatedChunks) {
                        if (chunk.State >= TerrainChunkState.InvalidVertices1) {
                            for (int x = 0; x < 16; x++) {
                                for (int y = 0; y < 256; y++) {
                                    for (int z = 0; z < 16; z++) {
                                        Point3 pointInSubterrain = new((chunk.Coords.X << 4) + x, y, (chunk.Coords.Y << 4) + z);
                                        int value = chunk.GetCellValueFast(x, y, z);
                                        if (Terrain.ExtractContents(value) == AirBlock.Index) {
                                            continue;
                                        }
                                        Point3 pointInWorld = Terrain.ToCell(Vector3.Transform(new Vector3(pointInSubterrain.X + 0.5f, pointInSubterrain.Y + 0.5f, pointInSubterrain.Z + 0.5f), subterrainSystem.GlobalTransform));
                                        if (pointInWorld.Y < 0
                                            || pointInWorld.Y > 255
                                            || blocks.ContainsKey(pointInWorld)
                                            || m_subsystemTerrain.Terrain.GetCellContentsFast(pointInWorld.X, pointInWorld.Y, pointInWorld.Z) != AirBlock.Index) {
                                            return false;
                                        }
                                        blocks.Add(pointInWorld, Terrain.ReplaceLight(value, 0));
                                    }
                                }
                            }
                        }
                    }
                    foreach (GVSubterrainSystem child in subterrainSystem.Children.Values) {
                        subterrainSystems.Push(child);
                    }
                }
                else {
                    return false;
                }
            }
            foreach (KeyValuePair<Point3, int> block in blocks) {
                Point3 point = block.Key;
                int chunkX = point.X >> 4;
                int chunkZ = point.Z >> 4;
                TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCoords(chunkX, chunkZ);
                if (chunkAtCell == null) {
                    chunkAtCell = m_subsystemTerrain.Terrain.AllocateChunk(chunkX, chunkZ);
                    chunkAtCell.State = TerrainChunkState.InvalidLight;
                    chunkAtCell.ThreadState = TerrainChunkState.InvalidLight;
                    isPointLoaded.Add(point, false);
                }
                else {
                    isPointLoaded.Add(point, true);
                }
                m_subsystemTerrain.Terrain.SetCellValueFast(point.X, point.Y, point.Z, block.Value);
                m_subsystemTerrain.TerrainUpdater.DowngradeChunkNeighborhoodState(chunkAtCell.Coords, 1, TerrainChunkState.InvalidLight, false);
                chunkAtCell.ModificationCounter++;
            }
            foreach (KeyValuePair<Point3, int> block in blocks) {
                Point3 point = block.Key;
                SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(block.Value));
                foreach (SubsystemBlockBehavior behaviors in blockBehaviors) {
                    behaviors.OnBlockGenerated(
                        block.Value,
                        point.X,
                        point.Y,
                        point.Z,
                        isPointLoaded[point]
                    );
                }
            }
            return true;
        }

        public static bool IsParallelToAxis(Vector3 vector) {
            vector = Vector3.Normalize(vector);
            return Math.Abs(vector.X) - 1f < floatTolerance || Math.Abs(vector.Y) - 1f < floatTolerance || Math.Abs(vector.Z) - 1f < floatTolerance;
        }

        public override void Dispose() {
            foreach (GVSubterrainSystem subterrainSystem in GVStaticStorage.GVSubterrainSystemDictionary.Values) {
                subterrainSystem.Dispose();
            }
            GVStaticStorage.GVSubterrainSystemDictionary.Clear();
        }
    }
}