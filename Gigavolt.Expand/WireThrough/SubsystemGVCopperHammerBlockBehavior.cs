using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVCopperHammerBlockBehavior : SubsystemBlockBehavior, IDrawable {
        SubsystemAudio m_subsystemAudio;
        public PrimitivesRenderer3D m_primitivesRenderer = new();
        public FlatBatch3D m_flatBatch;
        public int m_type;
        public Point3? m_startPoint;
        public Point3? m_endPoint;
        public Point3[] m_glowPoints = Array.Empty<Point3>();
        public override int[] HandledBlocks => Array.Empty<int>();

        public override bool OnUse(Ray3 ray, ComponentMiner componentMiner) {
            TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
            if (terrainRaycastResult.HasValue) {
                bool flag = false;
                CellFace cellFace = terrainRaycastResult.Value.CellFace;
                int value = terrainRaycastResult.Value.Value;
                int contents = Terrain.ExtractContents(value);
                if (contents == GVWireBlock.Index) {
                    flag = true;
                    SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, Terrain.MakeBlockValue(GVEWireThroughBlock.Index, Terrain.ExtractLight(value), Terrain.ExtractData(value)));
                }
                else if (contents == GVEWireThroughBlock.Index) {
                    flag = true;
                    int data = Terrain.ExtractData(value);
                    int type = GVEWireThroughBlock.GetType(data);
                    SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, type < 3 ? Terrain.ReplaceData(value, GVEWireThroughBlock.SetType(data, type + 1)) : Terrain.MakeBlockValue(GVWireBlock.Index, Terrain.ExtractLight(value), GVEWireThroughBlock.SetType(data, 0)));
                }
                else {
                    Point3 point3 = cellFace.Point + CellFace.FaceToPoint3(cellFace.Face);
                    int content = SubsystemTerrain.Terrain.GetCellContents(point3.X, point3.Y, point3.Z);
                    if (content == 0) {
                        flag = true;
                        if (m_startPoint == null) {
                            if (m_endPoint == point3) {
                                m_endPoint = null;
                                bool isEmpty = true;
                                foreach (Point3 glowPoint in m_glowPoints) {
                                    if (SubsystemTerrain.Terrain.GetCellContentsFast(glowPoint.X, glowPoint.Y, glowPoint.Z) != 0) {
                                        isEmpty = false;
                                    }
                                }
                                if (isEmpty) {
                                    for (int i = 0; i < m_glowPoints.Length; i++) {
                                        Point3 glowPoint = m_glowPoints[i];
                                        int face1 = CellFace.Point3ToFace(i == 0 ? m_glowPoints[1] - glowPoint : glowPoint - m_glowPoints[i - 1], 6);
                                        int face2 = CellFace.Point3ToFace(i == m_glowPoints.Length - 1 ? m_glowPoints[i - 1] - glowPoint : glowPoint - m_glowPoints[i + 1], 6);
                                        SubsystemTerrain.ChangeCell(m_glowPoints[i].X, m_glowPoints[i].Y, m_glowPoints[i].Z, Terrain.MakeBlockValue(GVEWireThroughBlock.Index, 0, GVEWireThroughBlock.SetWireFacesBitmask(GVEWireThroughBlock.SetType(0, m_type), (1 << face1) | (1 << face2))));
                                    }
                                    m_glowPoints = Array.Empty<Point3>();
                                }
                            }
                            else {
                                m_startPoint = point3;
                            }
                            m_glowPoints = Array.Empty<Point3>();
                        }
                        else {
                            if (point3 != m_startPoint) {
                                Stack<Point3> path = GVAStar.FindPath(m_startPoint.Value, point3, SubsystemTerrain.Terrain);
                                if (path != null) {
                                    m_glowPoints = path.ToArray();
                                    m_endPoint = point3;
                                }
                            }
                            m_startPoint = null;
                        }
                    }
                }
                if (flag) {
                    m_subsystemAudio.PlaySound(
                        "Audio/Click",
                        1f,
                        0f,
                        new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                        2f,
                        true
                    );
                }
                return true;
            }
            return false;
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
            m_flatBatch = m_primitivesRenderer.FlatBatch(0, DepthStencilState.DepthRead, null, BlendState.Additive);
            base.Load(valuesDictionary);
        }

        public void Draw(Camera camera, int drawOrder) {
            if (m_startPoint != null) {
                Vector3 position = new Vector3(m_startPoint.Value) + new Vector3(0.01f);
                m_flatBatch.QueueBoundingBox(new BoundingBox(position, position + new Vector3(0.98f)), Color.Green);
            }
            if (m_glowPoints.Length > 0) {
                foreach (Point3 glowPoint in m_glowPoints) {
                    Vector3 position = new Vector3(glowPoint) + new Vector3(0.01f);
                    m_flatBatch.QueueBoundingBox(new BoundingBox(position, position + new Vector3(0.98f)), Color.Green);
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVCopperHammerDialog(m_type, newType => { m_type = newType; }));
            return true;
        }

        public int[] DrawOrders => new[] { 114 };
    }
}