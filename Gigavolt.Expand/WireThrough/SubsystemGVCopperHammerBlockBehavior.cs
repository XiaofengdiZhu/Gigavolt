using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;
using SixLabors.ImageSharp.PixelFormats;
using TemplatesDatabase;
using Color = SixLabors.ImageSharp.Color;

namespace Game {
    public class SubsystemGVCopperHammerBlockBehavior : SubsystemBlockBehavior, IDrawable {
        SubsystemAudio m_subsystemAudio;
        SubsystemGVElectricity m_subsystemGVElectricity;
        public readonly PrimitivesRenderer3D m_primitivesRenderer = new();
        public FlatBatch3D m_flatBatch;
        public int m_texture;
        public bool m_isHarness;
        public Point3? m_startPoint;
        public Point3? m_endPoint;
        public Point3[] m_glowPoints = Array.Empty<Point3>();
        public override int[] HandledBlocks => Array.Empty<int>();

        public override bool OnUse(Ray3 ray, ComponentMiner componentMiner) {
            int? blockColor = GVCopperHammerBlock.GetColor(Terrain.ExtractData(componentMiner.ActiveBlockValue));
            if (blockColor.HasValue) {
                foreach (GVElectricElement element in m_subsystemGVElectricity.m_GVElectricElements) {
                    switch (element) {
                        case ButtonGVElectricElement button when button.CellFaces[0].Mask == 1 << blockColor.Value:
                            button.Press();
                            break;
                        case SwitchGVElectricElement switchElement when switchElement.CellFaces[0].Mask == 1 << blockColor.Value:
                            switchElement.Switch();
                            break;
                    }
                }
                return true;
            }
            TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Digging);
            if (terrainRaycastResult.HasValue) {
                bool flag = false;
                CellFace cellFace = terrainRaycastResult.Value.CellFace;
                int value = terrainRaycastResult.Value.Value;
                int contents = Terrain.ExtractContents(value);
                int GVWireBlockIndex = GVBlocksManager.GetBlockIndex<GVEWireThroughBlock>();
                int GVEWireThroughBlockIndex = GVBlocksManager.GetBlockIndex<GVEWireThroughBlock>();
                if (contents == GVWireBlockIndex) {
                    flag = true;
                    SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, Terrain.MakeBlockValue(GVEWireThroughBlockIndex, Terrain.ExtractLight(value), Terrain.ExtractData(value)));
                }
                else if (contents == GVBlocksManager.GetBlockIndex<GVWireHarnessBlock>()) {
                    flag = true;
                    SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, Terrain.MakeBlockValue(GVEWireThroughBlockIndex, Terrain.ExtractLight(value), GVEWireThroughBlock.SetIsWireHarness(GVEWireThroughBlock.SetWireFacesBitmask(0, GVWireHarnessBlock.GetWireFacesBitmask(value)), true)));
                }
                else if (contents == GVBlocksManager.GetBlockIndex<GVEWireThroughBlock>()) {
                    flag = true;
                    int data = Terrain.ExtractData(value);
                    if (!GVEWireThroughBlock.GetIsCross(data)) {
                        int mask = GVEWireThroughBlock.GetWireFacesBitmask(data);
                        SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, GVEWireThroughBlock.GetTexture(data) == 3 ? GVEWireThroughBlock.GetIsWireHarness(data) ? GVWireHarnessBlock.SetWireFacesBitmask(GVBlocksManager.GetBlockIndex<GVWireHarnessBlock>(), mask) : GVWireBlock.SetWireFacesBitmask(Terrain.MakeBlockValue(GVWireBlockIndex, 0, GVWireBlock.SetColor(0, GVEWireThroughBlock.GetColor(data))), mask) : Terrain.ReplaceData(value, GVEWireThroughBlock.SetTexture(data, (GVEWireThroughBlock.GetTexture(data) + 1) % 4)));
                    }
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
                                        SubsystemTerrain.ChangeCell(m_glowPoints[i].X, m_glowPoints[i].Y, m_glowPoints[i].Z, Terrain.MakeBlockValue(GVEWireThroughBlockIndex, 0, GVEWireThroughBlock.SetWireFacesBitmask(GVEWireThroughBlock.SetTexture(GVEWireThroughBlock.SetIsWireHarness(0, m_isHarness), m_texture), (1 << face1) | (1 << face2))));
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
            base.Load(valuesDictionary);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
            m_flatBatch = m_primitivesRenderer.FlatBatch(0, DepthStencilState.DepthRead, null, BlendState.Additive);
            if (GVEWireThroughBlock.m_harnessTexture == null) {
                Image image;
                if (Project.FindSubsystem<SubsystemBlocksTexture>(true).BlocksTexture.Tag is Image image2) {
                    image = new Image(image2);
                }
                else {
                    image = ContentManager.Get<Image>("Textures/Blocks");
                }
                int d = image.Width / 512;
                if (d < 1) {
                    int slotHeight = image.Height / 16;
                    int startX = slotHeight * 8 + slotHeight / 2 - 1;
                    int endX = startX + 2;
                    for (int x = startX; x < endX; x++) {
                        for (int i = 0; i < 6; i++) {
                            if (i == 4) {
                                continue;
                            }
                            int startY = slotHeight * 8 + slotHeight / 2 - 1 + i * slotHeight;
                            int endY = startY + 2;
                            for (int y = startY; y < endY; y++) {
                                image.SetPixelFast(x, y, Color.Orange.ToPixel<Rgba32>());
                            }
                        }
                    }
                }
                else {
                    int slotHeight = 32 * d;
                    int startX = image.Width / 256 * 135;
                    int endX = startX + d * 4;
                    for (int x = startX; x < endX; x++) {
                        for (int i = 0; i < 6; i++) {
                            if (i == 4) {
                                continue;
                            }
                            int startY = image.Height / 256 * 135 + i * slotHeight;
                            int endY = startY + d * 4;
                            for (int y = startY; y < endY; y++) {
                                if ((x == startX || x == endX - 1)
                                    && (y == startY || y == endY - 1)) {
                                    continue;
                                }
                                image.SetPixelFast(x, y, Color.Orange.ToPixel<Rgba32>());
                            }
                        }
                    }
                }
                GVEWireThroughBlock.m_harnessTexture = Texture2D.Load(image);
            }
        }

        public void Draw(Camera camera, int drawOrder) {
            if (m_startPoint != null) {
                Vector3 position = new Vector3(m_startPoint.Value) + new Vector3(0.01f);
                m_flatBatch.QueueBoundingBox(new BoundingBox(position, position + new Vector3(0.98f)), Engine.Color.Green);
            }
            if (m_glowPoints.Length > 0) {
                foreach (Point3 glowPoint in m_glowPoints) {
                    Vector3 position = new Vector3(glowPoint) + new Vector3(0.01f);
                    m_flatBatch.QueueBoundingBox(new BoundingBox(position, position + new Vector3(0.98f)), Engine.Color.Green);
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVCopperHammerDialog(
                    m_texture,
                    m_isHarness,
                    (newTexture, newIsHarness) => {
                        m_texture = newTexture;
                        m_isHarness = newIsHarness;
                    }
                )
            );
            return true;
        }

        public int[] DrawOrders => [114];

        public override void Dispose() {
            GVEWireThroughBlock.m_harnessTexture = null;
            base.Dispose();
        }
    }
}