using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public static class GVAStar {
        class Node(Point3 position, Node previous = null) {
            public readonly Point3 Position = position;
            public Node Previous = previous;
            public float G;
            public float H;

            public float F => G + H;

            public Stack<Point3> GeneratePath() {
                Stack<Point3> path = new();
                Node node = this;
                while (node.Previous != null) {
                    path.Push(node.Position);
                    node = node.Previous;
                }
                path.Push(node.Position);
                return path;
            }

            public override bool Equals(object obj) {
                if (obj is Node node) {
                    return Position == node.Position;
                }
                return false;
            }

            public override int GetHashCode() => Position.GetHashCode();
        }

        public static Stack<Point3> FindPath(Point3 start, Point3 end, Terrain terrain) {
            List<Node> open = [];
            HashSet<Point3> closed = [];
            int tried = 0;
            open.Add(new Node(start));
            while (open.Count > 0) {
                if (tried++ > 20000) {
                    break;
                }
                Node current = open[0];
                if (current.Position == end) {
                    return current.GeneratePath();
                }
                foreach (Node node in open) {
                    if (node.F < current.F
                        || (node.F.Equals(current.F) && node.H < current.H)) {
                        current = node;
                    }
                }
                if (current.Position == end) {
                    return current.GeneratePath();
                }
                open.Remove(current);
                closed.Add(current.Position);
                foreach (Point3 face in CellFace.m_faceToPoint3) {
                    Point3 neighborPosition = current.Position + face;
                    if (closed.Contains(neighborPosition)) {
                        continue;
                    }
                    Node neighbor = null;
                    foreach (Node node in open) {
                        if (node.Position == neighborPosition) {
                            neighbor = node;
                            break;
                        }
                    }
                    if (neighbor == null) {
                        if (!terrain.IsCellValid(neighborPosition.X, neighborPosition.Y, neighborPosition.Z)
                            || terrain.GetCellContentsFast(neighborPosition.X, neighborPosition.Y, neighborPosition.Z) != 0) {
                            closed.Add(neighborPosition);
                            continue;
                        }
                        neighbor = new Node(neighborPosition, current) {
                            G = current.G + 1,
                            H = Math.Abs(neighborPosition.X - end.X) + Math.Abs(neighborPosition.Y - end.Y) + Math.Abs(neighborPosition.Z - end.Z)
                        };
                        open.Add(neighbor);
                    }
                    else {
                        float g = current.G + 1;
                        if (neighbor.G == 0
                            || g < neighbor.G) {
                            neighbor.G = g;
                            neighbor.Previous = current;
                        }
                    }
                }
            }
            return null;
        }
    }
}