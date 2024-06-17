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

        const float FloatTolerance = 0.01f;

        public bool DropSubterrain(GVSubterrainSystem rootSubterrainSystem) {
            if (rootSubterrainSystem.m_hasDropped) {
                return true;
            }
            Dictionary<Point3, int> blocks = new();
            Dictionary<Point3, bool> isPointLoaded = new();
            Stack<GVSubterrainSystem> subterrainSystems = new();
            HashSet<GVSubterrainSystem> dropped = new();
            subterrainSystems.Push(rootSubterrainSystem);
            while (subterrainSystems.Count > 0) {
                GVSubterrainSystem subterrainSystem = subterrainSystems.Pop();
                if (subterrainSystem.m_hasDropped) {
                    continue;
                }
                Matrix transform = subterrainSystem.GlobalTransform;
                if (Math.Abs(transform.M11 * transform.M11 + transform.M12 * transform.M12 + transform.M13 * transform.M13 - 1) < FloatTolerance
                    && IsParallelToAxis(transform.Forward)
                    && IsParallelToAxis(transform.Right)) {
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
                    dropped.Add(subterrainSystem);
                    foreach (GVSubterrainSystem child in subterrainSystem.Children.Values) {
                        subterrainSystems.Push(child);
                    }
                }
                else {
                    return false;
                }
            }
            foreach (GVSubterrainSystem system in dropped) {
                if (!system.m_hasDropped) {
                    system.m_hasDropped = true;
                    system.Dispose(true);
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
            foreach ((Point3 point, int value) in blocks) {
                SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(value));
                foreach (SubsystemBlockBehavior behaviors in blockBehaviors) {
                    behaviors.OnBlockGenerated(
                        value,
                        point.X,
                        point.Y,
                        point.Z,
                        isPointLoaded[point]
                    );
                }
            }
            return true;
        }

        public Terrain GetTerrain(uint id) => id == 0 ? m_subsystemTerrain.Terrain : GVStaticStorage.GVSubterrainSystemDictionary[id]?.Terrain;

        public void ChangeCell(int x, int y, int z, uint subterrainId, int value, bool updateModificationCounter = true) {
            if (subterrainId == 0) {
                m_subsystemTerrain.ChangeCell(
                    x,
                    y,
                    z,
                    value,
                    updateModificationCounter
                );
            }
            else {
                GVStaticStorage.GVSubterrainSystemDictionary[subterrainId]
                ?.ChangeCell(
                    x,
                    y,
                    z,
                    value,
                    updateModificationCounter
                );
            }
        }

        public void DestroyCell(int toolLevel, int x, int y, int z, uint subterrainId, int newValue, bool noDrop, bool noParticleSystem) {
            if (subterrainId == 0) {
                m_subsystemTerrain.DestroyCell(
                    toolLevel,
                    x,
                    y,
                    z,
                    newValue,
                    noDrop,
                    noParticleSystem
                );
            }
            else {
                GVStaticStorage.GVSubterrainSystemDictionary[subterrainId]
                ?.DestroyCell(
                    toolLevel,
                    x,
                    y,
                    z,
                    newValue,
                    noDrop,
                    noParticleSystem
                );
            }
        }

        public static bool IsParallelToAxis(Vector3 vector) {
            vector = Vector3.Normalize(vector);
            return IsNumberNearOne(vector.X) || IsNumberNearOne(vector.Y) || IsNumberNearOne(vector.Z);
        }

        public static bool IsNumberNearOne(float number) => Math.Abs(number - 1f) < FloatTolerance || Math.Abs(number + 1f) < FloatTolerance;


        public override void Dispose() {
            foreach (GVSubterrainSystem subterrainSystem in GVStaticStorage.GVSubterrainSystemDictionary.Values) {
                subterrainSystem.Dispose(true);
            }
            GVStaticStorage.GVSubterrainSystemDictionary.Clear();
        }
    }
}