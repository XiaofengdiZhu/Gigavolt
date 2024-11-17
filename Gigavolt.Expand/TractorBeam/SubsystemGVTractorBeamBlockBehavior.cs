using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVTractorBeamBlockBehavior : SubsystemBlockBehavior, IDrawable, IGVBlockBehavior {
        public SubsystemGVSubterrain m_subsystemGVSubterrain;
        public readonly Dictionary<uint, Dictionary<GVCellFace, Vector3>> m_indicatorLine = new();
        public readonly Dictionary<uint, Dictionary<Point3, HashSet<Point3>>> m_scanPreviews = new();
        public readonly Dictionary<uint, Dictionary<Point3, GVSubterrainSystem>> m_subterrainSystems = new();
        public readonly FlatBatch3D m_flatBatch = new() { Layer = 0, DepthStencilState = DepthStencilState.None, RasterizerState = RasterizerState.CullNoneScissor, BlendState = BlendState.AlphaBlend };

        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVTractorBeamBlock>()];
        public UpdateOrder UpdateOrder => UpdateOrder.Terrain;
        public int[] DrawOrders => [2000];

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemGVSubterrain = Project.FindSubsystem<SubsystemGVSubterrain>(true);
        }

        public void Draw(Camera camera, int drawOrder) {
            if (drawOrder == DrawOrders[0]) {
                foreach ((uint _, Dictionary<Point3, HashSet<Point3>> previews) in m_scanPreviews) {
                    foreach (HashSet<Point3> points in previews.Values) {
                        foreach (Point3 point in points) {
                            m_flatBatch.QueueBoundingBox(
                                new BoundingBox(
                                    point.X,
                                    point.Y,
                                    point.Z,
                                    point.X + 1f,
                                    point.Y + 1f,
                                    point.Z + 1f
                                ),
                                Color.Yellow
                            );
                        }
                    }
                }
                foreach ((uint subterrainId, Dictionary<GVCellFace, Vector3> systems) in m_indicatorLine) {
                    if (systems.Count <= 0) {
                        continue;
                    }
                    bool inSubterrain = subterrainId != 0;
                    Matrix transform = inSubterrain ? GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform : default;
                    foreach ((GVCellFace key, Vector3 value) in systems) {
                        Vector3 origin = new(key.X + 0.5f, key.Y + 0.5f, key.Z + 0.5f);
                        Vector3 p1 = origin - 0.43f * GVCellFace.FaceToVector3(key.Face);
                        Vector3 p2 = origin + value;
                        if (inSubterrain) {
                            p1 = Vector3.Transform(p1, transform);
                            p2 = Vector3.Transform(p2, transform);
                        }
                        m_flatBatch.QueueLine(p1, p2, Color.Yellow);
                    }
                }
                m_flatBatch.Flush(camera.ViewProjectionMatrix);
            }
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) => OnBlockRemoved(
            value,
            newValue,
            x,
            y,
            z,
            null
        );

        public void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) {
            Point3 tractorBeamBlockPoint = new(x, y, z);
            uint subterrainId = system?.ID ?? 0;
            RemoveSubterrain(tractorBeamBlockPoint, subterrainId, true);
            RemovePreview(tractorBeamBlockPoint, subterrainId);
            RemoveIndicatorLine(new GVCellFace(x, y, z, GVBlocksManager.GetBlock<GVTractorBeamBlock>()?.GetFace(value) ?? 0), subterrainId);
        }

        public void SetIndicatorLine(GVCellFace cellFace, uint subterrainId, Vector3 targetOffset) {
            if (m_indicatorLine.TryGetValue(subterrainId, out Dictionary<GVCellFace, Vector3> lines)) {
                lines[cellFace] = targetOffset;
            }
            else {
                m_indicatorLine[subterrainId] = new Dictionary<GVCellFace, Vector3> { { cellFace, targetOffset } };
            }
        }

        public void RemoveIndicatorLine(GVCellFace cellFace, uint subterrainId) {
            if (m_indicatorLine.TryGetValue(subterrainId, out Dictionary<GVCellFace, Vector3> lines)) {
                lines.Remove(cellFace);
                if (lines.Count == 0) {
                    m_indicatorLine.Remove(subterrainId);
                }
            }
        }

        public void AddPreview(Point3 tractorBeamBlockPoint, uint subterrainId, Vector3 targetOffset) {
            Vector3 start = new(tractorBeamBlockPoint.X + 0.5f + targetOffset.X, tractorBeamBlockPoint.Y + 0.5f + targetOffset.Y, tractorBeamBlockPoint.Z + 0.5f + targetOffset.Z);
            Point3 startPoint = Terrain.ToCell(subterrainId == 0u ? start : Vector3.Transform(start, GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform));
            Point3 non = Point3.Zero;
            Dictionary<Point3, int> scanResult = ScanTerrain(tractorBeamBlockPoint, startPoint, ref non, ref non);
            if (scanResult != null) {
                if (m_scanPreviews.TryGetValue(subterrainId, out Dictionary<Point3, HashSet<Point3>> previews)) {
                    previews[tractorBeamBlockPoint] = new HashSet<Point3>(scanResult.Keys);
                }
                else {
                    m_scanPreviews[subterrainId] = new Dictionary<Point3, HashSet<Point3>> { { tractorBeamBlockPoint, new HashSet<Point3>(scanResult.Keys) } };
                }
            }
        }

        public void RemovePreview(Point3 tractorBeamBlockPoint, uint subterrainId) {
            if (m_scanPreviews.TryGetValue(subterrainId, out Dictionary<Point3, HashSet<Point3>> previews)) {
                previews.Remove(tractorBeamBlockPoint);
                if (previews.Count == 0) {
                    m_scanPreviews.Remove(subterrainId);
                }
            }
        }

        public GVSubterrainSystem AddSubterrain(Point3 tractorBeamBlockPoint, uint subterrainId, Vector3 targetOffset, Matrix transform) {
            Vector3 scanStart = new(tractorBeamBlockPoint.X + 0.5f + targetOffset.X, tractorBeamBlockPoint.Y + 0.5f + targetOffset.Y, tractorBeamBlockPoint.Z + 0.5f + targetOffset.Z);
            if (subterrainId != 0u) {
                scanStart = Vector3.Transform(scanStart, GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform);
            }
            Point3 min = new(int.MaxValue, int.MaxValue, int.MaxValue);
            Point3 max = new(int.MinValue, int.MinValue, int.MinValue);
            Dictionary<Point3, int> scanResult = ScanTerrain(tractorBeamBlockPoint, Terrain.ToCell(scanStart), ref min, ref max);
            if (scanResult != null) {
                Dictionary<Point3, int> blocks = scanResult.Select(pair => new KeyValuePair<Point3, int>(pair.Key - min, pair.Value)).ToDictionary();
                if (m_subterrainSystems.TryGetValue(subterrainId, out Dictionary<Point3, GVSubterrainSystem> systems)
                    && systems.TryGetValue(tractorBeamBlockPoint, out GVSubterrainSystem subterrainSystem)) {
                    subterrainSystem.Dispose(true);
                }
                subterrainSystem = new GVSubterrainSystem(
                    Project,
                    transform,
                    tractorBeamBlockPoint,
                    new Vector3(min.X - scanStart.X, min.Y - scanStart.Y, min.Z - scanStart.Z),
                    subterrainId == 0 ? null : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId],
                    Point2.Zero,
                    Point2XZ(max - min),
                    blocks
                );
                foreach (Point3 point in scanResult.Keys) {
                    SubsystemTerrain.ChangeCell(point.X, point.Y, point.Z, AirBlock.Index);
                }
                if (systems == null
                    || systems.Count == 0) {
                    m_subterrainSystems[subterrainId] = new Dictionary<Point3, GVSubterrainSystem> { { tractorBeamBlockPoint, subterrainSystem } };
                }
                else {
                    systems[tractorBeamBlockPoint] = subterrainSystem;
                }
                return subterrainSystem;
            }
            return null;
        }

        public bool RemoveSubterrain(Point3 tractorBeamBlockPoint, uint subterrainId, bool force = false) {
            if (m_subterrainSystems.TryGetValue(subterrainId, out Dictionary<Point3, GVSubterrainSystem> systems)) {
                if (systems.TryGetValue(tractorBeamBlockPoint, out GVSubterrainSystem subterrainSystem)) {
                    bool dropSuccess = m_subsystemGVSubterrain.DropSubterrain(subterrainSystem);
                    if (dropSuccess || force) {
                        if (!dropSuccess) {
                            subterrainSystem.Dispose(true);
                        }
                        systems.Remove(tractorBeamBlockPoint);
                        if (systems.Count == 0) {
                            m_subterrainSystems.Remove(subterrainId);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public Dictionary<Point3, int> ScanTerrain(Point3 tractorBeamBlockPoint, Point3 startPoint, ref Point3 min, ref Point3 max) {
            bool recordMinMax = min != max;
            Dictionary<Point3, int> result = [];
            Stack<Point3> toTest = new();
            toTest.Push(startPoint);
            while (toTest.Count > 0) {
                Point3 point = toTest.Pop();
                if (point == tractorBeamBlockPoint) {
                    return null;
                }
                if (point.Y < 0
                    || point.Y > 255
                    || result.ContainsKey(point)) {
                    continue;
                }
                TerrainChunk chunk = SubsystemTerrain.Terrain.GetChunkAtCell(point.X, point.Z);
                if (chunk == null) {
                    return null;
                }
                int value = chunk.GetCellValueFast(point.X & 15, point.Y, point.Z & 15);
                int contents = Terrain.ExtractContents(value);
                if (GVSubterrainSystem.IsBlockAllowedForSubterrain(contents)) {
                    if (chunk.State <= TerrainChunkState.InvalidContents4
                        || (!chunk.IsLoaded && chunk.ModificationCounter == 0)) {
                        return null;
                    }
                    result.Add(point, value);
                    if (recordMinMax) {
                        if (point.X < min.X) {
                            min.X = point.X;
                        }
                        if (point.X > max.X) {
                            max.X = point.X;
                        }
                        if (point.Y < min.Y) {
                            min.Y = point.Y;
                        }
                        if (point.Y > max.Y) {
                            max.Y = point.Y;
                        }
                        if (point.Z < min.Z) {
                            min.Z = point.Z;
                        }
                        if (point.Z > max.Z) {
                            max.Z = point.Z;
                        }
                    }
                }
                else {
                    continue;
                }
                toTest.Push(new Point3(point.X - 1, point.Y, point.Z));
                toTest.Push(new Point3(point.X + 1, point.Y, point.Z));
                toTest.Push(new Point3(point.X, point.Y - 1, point.Z));
                toTest.Push(new Point3(point.X, point.Y + 1, point.Z));
                toTest.Push(new Point3(point.X, point.Y, point.Z - 1));
                toTest.Push(new Point3(point.X, point.Y, point.Z + 1));
            }
            return result.Count > 0 ? result : null;
        }

        public static Point2 Point2XZ(Point3 point) => new(point.X, point.Z);
    }
}