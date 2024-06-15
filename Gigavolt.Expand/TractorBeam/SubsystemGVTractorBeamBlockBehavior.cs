using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVTractorBeamBlockBehavior : SubsystemBlockBehavior, IDrawable {
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemGVSubterrain m_subsystemGVSubterrain;
        public readonly Dictionary<GVCellFace, Vector3> m_indicatorLine = new();
        public readonly Dictionary<Point3, HashSet<Point3>> m_scanPreviews = new();
        public readonly Dictionary<Point3, GVSubterrainSystem> m_subterrainSystems = new();
        public readonly FlatBatch3D m_flatBatch = new() { Layer = 0, DepthStencilState = DepthStencilState.None, RasterizerState = RasterizerState.CullNoneScissor, BlendState = BlendState.AlphaBlend };

        public override int[] HandledBlocks => [GVTractorBeamBlock.Index];
        public UpdateOrder UpdateOrder => UpdateOrder.Terrain;
        public int[] DrawOrders => [2000];

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemGVSubterrain = Project.FindSubsystem<SubsystemGVSubterrain>(true);
        }

        public void Draw(Camera camera, int drawOrder) {
            if (drawOrder == DrawOrders[0]) {
                foreach (HashSet<Point3> points in m_scanPreviews.Values) {
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
                foreach (KeyValuePair<GVCellFace, Vector3> pair in m_indicatorLine) {
                    GVCellFace key = pair.Key;
                    Vector3 origin = new(key.X + 0.5f, key.Y + 0.5f, key.Z + 0.5f);
                    m_flatBatch.QueueLine(origin - 0.43f * GVCellFace.FaceToVector3(key.Face), origin + pair.Value, Color.Yellow);
                }
                m_flatBatch.Flush(camera.ViewProjectionMatrix);
            }
        }

        public void AddPreview(Point3 tractorBeamBlockPoint, Point3 startPoint) {
            Point3 non = Point3.Zero;
            Dictionary<Point3, int> scanResult = ScanTerrain(tractorBeamBlockPoint, startPoint, ref non, ref non);
            if (scanResult != null) {
                m_scanPreviews[tractorBeamBlockPoint] = new HashSet<Point3>(scanResult.Keys);
            }
        }

        public void RemovePreview(Point3 tractorBeamBlockPoint) {
            m_scanPreviews.Remove(tractorBeamBlockPoint);
        }

        public GVSubterrainSystem AddSubterrain(Point3 tractorBeamBlockPoint, Vector3 scanStart, Matrix transform) {
            Point3 min = new(int.MaxValue, int.MaxValue, int.MaxValue);
            Point3 max = new(int.MinValue, int.MinValue, int.MinValue);
            Dictionary<Point3, int> scanResult = ScanTerrain(tractorBeamBlockPoint, Terrain.ToCell(scanStart), ref min, ref max);
            if (scanResult != null) {
                Dictionary<Point3, int> blocks = scanResult.Select(pair => new KeyValuePair<Point3, int>(pair.Key - min, pair.Value)).ToDictionary();
                if (m_subterrainSystems.TryGetValue(tractorBeamBlockPoint, out GVSubterrainSystem subterrainSystem)) {
                    subterrainSystem.Dispose();
                }
                subterrainSystem = new GVSubterrainSystem(
                    Project,
                    transform,
                    tractorBeamBlockPoint,
                    new Vector3(min.X - scanStart.X, min.Y - scanStart.Y, min.Z - scanStart.Z),
                    null,
                    Point2.Zero,
                    Point2XZ(max - min),
                    blocks
                );
                foreach (Point3 point in scanResult.Keys) {
                    m_subsystemTerrain.ChangeCell(point.X, point.Y, point.Z, AirBlock.Index);
                }
                m_subterrainSystems[tractorBeamBlockPoint] = subterrainSystem;
                return subterrainSystem;
            }
            return null;
        }

        public bool RemoveSubterrain(Point3 tractorBeamBlockPoint) {
            if (m_subterrainSystems.TryGetValue(tractorBeamBlockPoint, out GVSubterrainSystem subterrainSystem)
                && m_subsystemGVSubterrain.DropSubterrain(subterrainSystem)) {
                subterrainSystem.Dispose();
                m_subterrainSystems.Remove(tractorBeamBlockPoint);
                return true;
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
                TerrainChunk chunk = m_subsystemTerrain.Terrain.GetChunkAtCell(point.X, point.Z);
                if (chunk == null) {
                    return null;
                }
                int value = chunk.GetCellValueFast(point.X & 15, point.Y, point.Z & 15);
                int contents = Terrain.ExtractContents(value);
                if (GVSubterrainSystem.IsBlockAllowedForSubterrain(contents)) {
                    if (chunk.State <= TerrainChunkState.InvalidContents4
                        || !chunk.IsLoaded) {
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