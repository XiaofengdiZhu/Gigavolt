using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Media;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVSignBlockBehavior : SubsystemBlockBehavior, IDrawable, IUpdateable, IGVBlockBehavior {
        public SubsystemGameWidgets m_subsystemViews;
        public SubsystemGVSubterrain m_subsystemGVSubterrain;
        public SubsystemGameInfo m_subsystemGameInfo;

        public readonly Dictionary<uint, Dictionary<Point3, GVSignTextData>> m_textsByPoint = new() { { 0u, new Dictionary<Point3, GVSignTextData>() } };
        public readonly GVSignTextData[] m_textureLocations = new GVSignTextData[32];
        public readonly BitmapFont m_font = LabelWidget.BitmapFont;
        public RenderTarget2D m_renderTarget;
        public readonly List<Vector3> m_lastUpdatePositions = [];
        public readonly PrimitivesRenderer2D m_primitivesRenderer2D = new();
        public readonly PrimitivesRenderer3D m_primitivesRenderer3D = new();

        public override int[] HandledBlocks => new[] { GVSignBlock.Index };

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public int[] DrawOrders => [51];

        public GVSignTextData GetSignData(Point3 point, uint subterrainId) => m_textsByPoint[subterrainId].TryGetValue(point, out GVSignTextData value) ? value : null;

        public void SetSignData(Point3 point, uint subterrainId, string line, Color color, string url) {
            if (!m_textsByPoint.TryGetValue(subterrainId, out Dictionary<Point3, GVSignTextData> points)) {
                points = new Dictionary<Point3, GVSignTextData>();
                m_textsByPoint.Add(subterrainId, points);
            }
            if (points.TryGetValue(point, out GVSignTextData value)) {
                value.Point = point;
                value.Line = line;
                value.Color = color;
                value.Url = url;
                value.TextureLocation = null;
            }
            else {
                points[point] = new GVSignTextData {
                    Point = point,
                    Line = line,
                    Color = color,
                    Url = url,
                    FloatPosition = new Vector3(point) + new Vector3(0.5f),
                    FloatColor = Color.White,
                    FloatSize = 0,
                    FloatRotation = Vector3.Zero,
                    FloatLight = 0
                };
            }
            m_lastUpdatePositions.Clear();
        }

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ) => OnNeighborBlockChanged(
            x,
            y,
            z,
            neighborX,
            neighborY,
            neighborZ,
            null
        );

        public void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ, GVSubterrainSystem system) {
            Terrain terrain = system == null ? SubsystemTerrain.Terrain : system.Terrain;
            int cellValueFast = terrain.GetCellValueFast(x, y, z);
            int data = Terrain.ExtractData(cellValueFast);
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast)];
            if (block is GVSignBlock) {
                Point3 point = CellFace.FaceToPoint3(GVAttachedSignBlock.GetFace(data));
                int x2 = x - point.X;
                int y2 = y - point.Y;
                int z2 = z - point.Z;
                int cellValue = terrain.GetCellValue(x2, y2, z2);
                if (!BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsCollidable_(cellValue)) {
                    if (system == null) {
                        SubsystemTerrain.DestroyCell(
                            0,
                            x,
                            y,
                            z,
                            0,
                            false,
                            false
                        );
                    }
                    else {
                        system.DestroyCell(
                            0,
                            x,
                            y,
                            z,
                            0,
                            false,
                            false
                        );
                    }
                }
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
            Point3 point = new(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
            if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure) {
                GVSignTextData signData = GetSignData(point, 0);
                if (signData != null
                    && !string.IsNullOrEmpty(signData.Url)) {
                    WebBrowserManager.LaunchBrowser(signData.Url);
                }
            }
            else if (componentMiner.ComponentPlayer != null) {
                DialogsManager.ShowDialog(componentMiner.ComponentPlayer.GuiWidget, new EditGVSignDialog(this, point));
            }
            return true;
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) {
            m_textsByPoint[0].Remove(new Point3(x, y, z));
            m_lastUpdatePositions.Clear();
        }

        public void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) {
            Dictionary<Point3, GVSignTextData> points = m_textsByPoint[system.ID];
            points.Remove(new Point3(x, y, z));
            if (points.Count == 0) {
                m_textsByPoint.Remove(system.ID);
            }
            m_lastUpdatePositions.Clear();
        }

        public void Update(float dt) {
            UpdateRenderTarget();
        }

        public void Draw(Camera camera, int drawOrder) {
            DrawSigns(camera);
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemViews = Project.FindSubsystem<SubsystemGameWidgets>(true);
            m_subsystemGVSubterrain = Project.FindSubsystem<SubsystemGVSubterrain>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            CreateRenderTarget();
            foreach (ValuesDictionary value11 in valuesDictionary.GetValue<ValuesDictionary>("Texts").Values) {
                Point3 point = value11.GetValue<Point3>("Point");
                uint subterrainId = value11.GetValue("SubterrainId", 0u);
                string line1 = value11.GetValue("Line1", string.Empty);
                Color color1 = value11.GetValue("Color1", Color.White);
                string url = value11.GetValue("Url", string.Empty);
                SetSignData(
                    point,
                    subterrainId,
                    line1,
                    color1,
                    url
                );
            }
            Display.DeviceReset += Display_DeviceReset;
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            int num = 0;
            ValuesDictionary valuesDictionary2 = new();
            valuesDictionary.SetValue("Texts", valuesDictionary2);
            if (m_textsByPoint.TryGetValue(0u, out Dictionary<Point3, GVSignTextData> points)) {
                foreach (GVSignTextData value in points.Values) {
                    ValuesDictionary valuesDictionary3 = new();
                    valuesDictionary3.SetValue("Point", value.Point);
                    valuesDictionary3.SetValue("SubterrainId", 0u);
                    if (!string.IsNullOrEmpty(value.Line)) {
                        valuesDictionary3.SetValue("Line1", value.Line);
                    }
                    if (value.Color != Color.White) {
                        valuesDictionary3.SetValue("Color1", value.Color);
                    }
                    if (!string.IsNullOrEmpty(value.Url)) {
                        valuesDictionary3.SetValue("Url", value.Url);
                    }
                    valuesDictionary2.SetValue(num++.ToString(CultureInfo.InvariantCulture), valuesDictionary3);
                }
            }
        }

        public override void Dispose() {
            Utilities.Dispose(ref m_renderTarget);
            Display.DeviceReset -= Display_DeviceReset;
        }

        public void Display_DeviceReset() {
            InvalidateRenderTarget();
        }

        public void CreateRenderTarget() {
            m_renderTarget = new RenderTarget2D(
                (int)m_font.GlyphHeight * 16,
                (int)m_font.GlyphHeight * 4 * 32,
                1,
                ColorFormat.Rgba8888,
                DepthFormat.None
            );
        }

        public void InvalidateRenderTarget() {
            m_lastUpdatePositions.Clear();
            for (int i = 0; i < m_textureLocations.Length; i++) {
                m_textureLocations[i] = null;
            }
            foreach (Dictionary<Point3, GVSignTextData> points in m_textsByPoint.Values) {
                foreach (GVSignTextData value in points.Values) {
                    value.TextureLocation = null;
                }
            }
        }

        public void RenderText(FontBatch2D fontBatch, FlatBatch2D flatBatch, GVSignTextData textData) {
            if (!textData.TextureLocation.HasValue) {
                return;
            }
            List<string> list = [];
            List<Color> list2 = [];
            if (!string.IsNullOrEmpty(textData.Line)) {
                list.Add(textData.Line.Replace("\\", "").ToUpper());
                list2.Add(textData.Color);
            }
            if (list.Count > 0) {
                float num = list.Max(l => l.Length) * m_font.GlyphHeight;
                float num2 = list.Count * m_font.GlyphHeight;
                float num3 = 4f;
                float num4;
                float num5;
                if (num / num2 < num3) {
                    num4 = num2 * num3;
                    num5 = num2;
                }
                else {
                    num4 = num;
                    num5 = num / num3;
                }
                bool flag = !string.IsNullOrEmpty(textData.Url);
                for (int j = 0; j < list.Count; j++) {
                    fontBatch.QueueText(
                        position: new Vector2(num4 / 2f, j * m_font.GlyphHeight + textData.TextureLocation.Value * (4f * m_font.GlyphHeight) + (num5 - num2) / 2f),
                        text: list[j],
                        depth: 0f,
                        color: flag ? new Color(0, 0, 64) : list2[j],
                        anchor: TextAnchor.HorizontalCenter,
                        scale: new Vector2(1f / m_font.Scale),
                        spacing: Vector2.Zero
                    );
                }
                textData.UsedTextureWidth = num4;
                textData.UsedTextureHeight = num5;
            }
        }

        public void UpdateRenderTarget() {
            bool flag = false;
            foreach (GameWidget gameWidget in m_subsystemViews.GameWidgets) {
                bool flag2 = false;
                foreach (Vector3 lastUpdatePosition in m_lastUpdatePositions) {
                    if (Vector3.DistanceSquared(gameWidget.ActiveCamera.ViewPosition, lastUpdatePosition) < 4f) {
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2) {
                    flag = true;
                    break;
                }
            }
            if (!flag) {
                return;
            }
            m_lastUpdatePositions.Clear();
            m_lastUpdatePositions.AddRange(m_subsystemViews.GameWidgets.Select(v => v.ActiveCamera.ViewPosition));
            bool flag3 = false;
            foreach (Dictionary<Point3, GVSignTextData> points in m_textsByPoint.Values) {
                foreach (GVSignTextData textData in points.Values) {
                    textData.ToBeRenderedFrame = Time.FrameIndex;
                    if (textData.TextureLocation.HasValue) {
                        continue;
                    }
                    int num2 = m_textureLocations.FirstIndex(d => d == null);
                    if (num2 < 0) {
                        num2 = m_textureLocations.FirstIndex(d => d.ToBeRenderedFrame != Time.FrameIndex);
                    }
                    if (num2 >= 0) {
                        GVSignTextData textData2 = m_textureLocations[num2];
                        if (textData2 != null) {
                            textData2.TextureLocation = null;
                            m_textureLocations[num2] = null;
                        }
                        m_textureLocations[num2] = textData;
                        textData.TextureLocation = num2;
                        flag3 = true;
                    }
                }
            }
            if (!flag3) {
                return;
            }
            RenderTarget2D renderTarget = Display.RenderTarget;
            Display.RenderTarget = m_renderTarget;
            try {
                Display.Clear(new Vector4(Color.Transparent));
                FlatBatch2D flatBatch = m_primitivesRenderer2D.FlatBatch(0, DepthStencilState.None, null, BlendState.Opaque);
                FontBatch2D fontBatch = m_primitivesRenderer2D.FontBatch(
                    m_font,
                    1,
                    DepthStencilState.None,
                    null,
                    BlendState.Opaque,
                    SamplerState.PointClamp
                );
                for (int j = 0; j < m_textureLocations.Length; j++) {
                    GVSignTextData textData3 = m_textureLocations[j];
                    if (textData3 != null) {
                        RenderText(fontBatch, flatBatch, textData3);
                    }
                }
                m_primitivesRenderer2D.Flush();
            }
            finally {
                Display.RenderTarget = renderTarget;
            }
        }

        public void DrawSigns(Camera camera) {
            if (m_textsByPoint.Count <= 0
                && m_textsByPoint.Sum(p => p.Value.Count) <= 0) {
                return;
            }
            TexturedBatch3D texturedBatch3D = m_primitivesRenderer3D.TexturedBatch(
                m_renderTarget,
                false,
                0,
                DepthStencilState.DepthRead,
                RasterizerState.CullCounterClockwiseScissor,
                null,
                SamplerState.PointClamp
            );
            foreach ((uint subterrainId, Dictionary<Point3, GVSignTextData> points) in m_textsByPoint) {
                GVSubterrainSystem subterrainSystem = subterrainId == 0 ? null : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];
                Matrix transform = subterrainId == 0 ? default : subterrainSystem.GlobalTransform;
                Matrix orientation = subterrainId == 0 ? default : transform.OrientationMatrix;
                foreach (GVSignTextData nearText in points.Values) {
                    if (!nearText.TextureLocation.HasValue) {
                        continue;
                    }
                    int cellValue = m_subsystemGVSubterrain.GetTerrain(subterrainId).GetCellValue(nearText.Point.X, nearText.Point.Y, nearText.Point.Z);
                    int num = Terrain.ExtractContents(cellValue);
                    if (BlocksManager.Blocks[num] is not GVBaseSignBlock signBlock) {
                        continue;
                    }
                    int data = Terrain.ExtractData(cellValue);
                    BlockMesh signSurfaceBlockMesh = signBlock.GetSignSurfaceBlockMesh(data);
                    if (signSurfaceBlockMesh != null) {
                        TerrainChunk chunkAtCell = m_subsystemGVSubterrain.GetTerrain(subterrainId).GetChunkAtCell(nearText.Point.X, nearText.Point.Z);
                        if (chunkAtCell != null
                            && chunkAtCell.State >= TerrainChunkState.InvalidVertices1) {
                            nearText.Light = Terrain.ExtractLight(cellValue);
                        }
                        float x = 0f;
                        float x2 = nearText.UsedTextureWidth / (m_font.GlyphHeight * 16f);
                        float x3 = nearText.TextureLocation.Value / 32f;
                        float x4 = (nearText.TextureLocation.Value + nearText.UsedTextureHeight / (m_font.GlyphHeight * 4f)) / 32f;
                        Vector3 vector = new(nearText.Point.X, nearText.Point.Y, nearText.Point.Z);
                        Vector3 vectorPlus05Transformed = subterrainId == 0 ? new Vector3(nearText.Point.X + 0.5f, nearText.Point.Y + 0.5f, nearText.Point.Z + 0.5f) : Vector3.Transform(new Vector3(nearText.Point.X + 0.5f, nearText.Point.Y + 0.5f, nearText.Point.Z + 0.5f), transform);
                        if (camera.ViewFrustum.Intersection(vectorPlus05Transformed + camera.ViewDirection)) {
                            Vector3 signSurfaceNormal = signBlock.GetSignSurfaceNormal(data);
                            Vector3 vector2 = MathUtils.Max(0.01f * Vector3.Dot(camera.ViewPosition - vectorPlus05Transformed, signSurfaceNormal), 0.005f) * signSurfaceNormal;
                            float num2 = LightingManager.LightIntensityByLightValue[nearText.Light];
                            Color color = new(num2, num2, num2);
                            for (int i = 0; i < signSurfaceBlockMesh.Indices.Count / 3; i++) {
                                BlockMeshVertex blockMeshVertex = signSurfaceBlockMesh.Vertices.Array[signSurfaceBlockMesh.Indices.Array[i * 3]];
                                BlockMeshVertex blockMeshVertex2 = signSurfaceBlockMesh.Vertices.Array[signSurfaceBlockMesh.Indices.Array[i * 3 + 1]];
                                BlockMeshVertex blockMeshVertex3 = signSurfaceBlockMesh.Vertices.Array[signSurfaceBlockMesh.Indices.Array[i * 3 + 2]];
                                Vector3 p;
                                Vector3 p2;
                                Vector3 p3;
                                if (subterrainId == 0) {
                                    p = blockMeshVertex.Position + vector + vector2;
                                    p2 = blockMeshVertex2.Position + vector + vector2;
                                    p3 = blockMeshVertex3.Position + vector + vector2;
                                }
                                else {
                                    p = Vector3.Transform(blockMeshVertex.Position + vector, transform) + Vector3.Transform(vector2, orientation);
                                    p2 = Vector3.Transform(blockMeshVertex2.Position + vector, transform) + Vector3.Transform(vector2, orientation);
                                    p3 = Vector3.Transform(blockMeshVertex3.Position + vector, transform) + Vector3.Transform(vector2, orientation);
                                }
                                Vector2 textureCoordinates = blockMeshVertex.TextureCoordinates;
                                Vector2 textureCoordinates2 = blockMeshVertex2.TextureCoordinates;
                                Vector2 textureCoordinates3 = blockMeshVertex3.TextureCoordinates;
                                textureCoordinates.X = MathUtils.Lerp(x, x2, textureCoordinates.X);
                                textureCoordinates2.X = MathUtils.Lerp(x, x2, textureCoordinates2.X);
                                textureCoordinates3.X = MathUtils.Lerp(x, x2, textureCoordinates3.X);
                                textureCoordinates.Y = MathUtils.Lerp(x3, x4, textureCoordinates.Y);
                                textureCoordinates2.Y = MathUtils.Lerp(x3, x4, textureCoordinates2.Y);
                                textureCoordinates3.Y = MathUtils.Lerp(x3, x4, textureCoordinates3.Y);
                                texturedBatch3D.QueueTriangle(
                                    p,
                                    p2,
                                    p3,
                                    textureCoordinates,
                                    textureCoordinates2,
                                    textureCoordinates3,
                                    color
                                );
                            }
                        }
                        if (nearText.FloatSize > 0
                            && nearText.FloatColor.A > 0) {
                            Vector3 position = nearText.FloatPosition;
                            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(nearText.FloatRotation.X, nearText.FloatRotation.Y, nearText.FloatRotation.Z);
                            Vector3 right = rotationMatrix.Right * x2 * 2 * nearText.FloatSize;
                            Vector3 up = rotationMatrix.Up * (x4 - x3) * 20 * nearText.FloatSize;
                            if (subterrainId != 0) {
                                position = Vector3.Transform(position, transform);
                                right = Vector3.Transform(right, orientation);
                                up = Vector3.Transform(up, orientation);
                            }
                            Vector3[] offsets = [right - up, right + up, -right - up, -right + up];
                            Vector3 min = new(float.MaxValue);
                            Vector3 max = new(float.MinValue);
                            foreach (Vector3 offset in offsets) {
                                min.X = Math.Min(min.X, offset.X);
                                min.Y = Math.Min(min.Y, offset.Y);
                                min.Z = Math.Min(min.Z, offset.Z);
                                max.X = Math.Max(max.X, offset.X);
                                max.Y = Math.Max(max.Y, offset.Y);
                                max.Z = Math.Max(max.Z, offset.Z);
                            }
                            if (camera.ViewFrustum.Intersection(new BoundingBox(position + min, position + max))) {
                                texturedBatch3D.QueueQuad(
                                    position + right - up,
                                    position - right - up,
                                    position - right + up,
                                    position + right + up,
                                    new Vector2(x2, x4),
                                    new Vector2(x, x4),
                                    new Vector2(x, x3),
                                    new Vector2(x2, x3),
                                    Color.MultiplyColorOnly(nearText.FloatColor, nearText.FloatLight)
                                );
                            }
                        }
                        m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
                    }
                }
            }
        }
    }
}