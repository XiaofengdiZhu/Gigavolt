using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Media;
using TemplatesDatabase;
using TextData = Game.SubsystemSignBlockBehavior.TextData;

namespace Game {
    public class SubsystemGVSignBlockCBehavior : SubsystemBlockBehavior, IDrawable, IUpdateable, IGVBlockBehavior {
        public SubsystemGameWidgets m_subsystemViews;
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemGameInfo m_subsystemGameInfo;

        public readonly Dictionary<uint, Dictionary<Point3, TextData>> m_textsByPoint = new() { { 0u, new Dictionary<Point3, TextData>() } };
        public readonly TextData[] m_textureLocations = new TextData[32];
        public readonly BitmapFont m_font = LabelWidget.BitmapFont;
        public RenderTarget2D m_renderTarget;
        public readonly List<Vector3> m_lastUpdatePositions = [];
        public readonly PrimitivesRenderer2D m_primitivesRenderer2D = new();
        public readonly PrimitivesRenderer3D m_primitivesRenderer3D = new();

        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVSignCBlock>()];

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public int[] DrawOrders => [50];

        public SignData GetSignData(Point3 point, uint subterrainId) {
            if (m_textsByPoint[subterrainId].TryGetValue(point, out TextData value)) {
                return new SignData { Lines = value.Lines.ToArray(), Colors = value.Colors.ToArray(), Url = value.Url };
            }
            return null;
        }

        public void SetSignData(Point3 point, uint subterrainId, string[] lines, Color[] colors, string url) {
            TextData textData = new() { Point = point, Url = url };
            for (int i = 0; i < 4; i++) {
                textData.Lines[i] = lines[i];
                textData.Colors[i] = colors[i];
            }
            if (!m_textsByPoint.TryGetValue(subterrainId, out Dictionary<Point3, TextData> points)) {
                points = new Dictionary<Point3, TextData>();
                m_textsByPoint.Add(subterrainId, points);
            }
            points[point] = textData;
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

        public void OnNeighborBlockChanged(int x,
            int y,
            int z,
            int neighborX,
            int neighborY,
            int neighborZ,
            GVSubterrainSystem system) {
            Terrain terrain = system == null ? SubsystemTerrain.Terrain : system.Terrain;
            int cellValueFast = terrain.GetCellValueFast(x, y, z);
            int num = Terrain.ExtractContents(cellValueFast);
            int data = Terrain.ExtractData(cellValueFast);
            Block block = BlocksManager.Blocks[num];
            if (block is not GVSignCBlock) {
                return;
            }
            if (GVSignCBlock.GetPose(data) == 0) {
                Point3 point = CellFace.FaceToPoint3(GVSignCBlock.GetFace(data));
                int x2 = x - point.X;
                int y2 = y - point.Y;
                int z2 = z - point.Z;
                int cellValue = terrain.GetCellValue(x2, y2, z2);
                int cellContents = Terrain.ExtractContents(cellValue);
                if (!BlocksManager.Blocks[cellContents].IsCollidable_(cellValue)) {
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
            else {
                int num2 = GVSignCBlock.GetHanging(data) ? terrain.GetCellValue(x, y + 1, z) : terrain.GetCellValue(x, y - 1, z);
                if (!BlocksManager.Blocks[Terrain.ExtractContents(num2)].IsCollidable_(num2)) {
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
                SignData signData = GetSignData(point, 0u);
                if (signData != null
                    && !string.IsNullOrEmpty(signData.Url)) {
                    WebBrowserManager.LaunchBrowser(signData.Url);
                }
            }
            else if (componentMiner.ComponentPlayer != null) {
                DialogsManager.ShowDialog(componentMiner.ComponentPlayer.GuiWidget, new EditGVSignCDialog(this, point));
            }
            return true;
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) {
            m_textsByPoint[0].Remove(new Point3(x, y, z));
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
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            CreateRenderTarget();
            foreach (ValuesDictionary value11 in valuesDictionary.GetValue<ValuesDictionary>("Texts").Values) {
                Point3 value = value11.GetValue<Point3>("Point");
                uint subterrainId = value11.GetValue("SubterrainId", 0u);
                string value2 = value11.GetValue("Line1", string.Empty);
                string value3 = value11.GetValue("Line2", string.Empty);
                string value4 = value11.GetValue("Line3", string.Empty);
                string value5 = value11.GetValue("Line4", string.Empty);
                Color value6 = value11.GetValue("Color1", Color.Black);
                Color value7 = value11.GetValue("Color2", Color.Black);
                Color value8 = value11.GetValue("Color3", Color.Black);
                Color value9 = value11.GetValue("Color4", Color.Black);
                string value10 = value11.GetValue("Url", string.Empty);
                SetSignData(value, subterrainId, [value2, value3, value4, value5], [value6, value7, value8, value9], value10);
            }
            Display.DeviceReset += Display_DeviceReset;
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            int num = 0;
            ValuesDictionary valuesDictionary2 = new();
            valuesDictionary.SetValue("Texts", valuesDictionary2);
            if (m_textsByPoint.TryGetValue(0u, out Dictionary<Point3, TextData> points)) {
                foreach (TextData value in points.Values) {
                    ValuesDictionary valuesDictionary3 = new();
                    valuesDictionary3.SetValue("Point", value.Point);
                    if (!string.IsNullOrEmpty(value.Lines[0])) {
                        valuesDictionary3.SetValue("Line1", value.Lines[0]);
                    }
                    if (!string.IsNullOrEmpty(value.Lines[1])) {
                        valuesDictionary3.SetValue("Line2", value.Lines[1]);
                    }
                    if (!string.IsNullOrEmpty(value.Lines[2])) {
                        valuesDictionary3.SetValue("Line3", value.Lines[2]);
                    }
                    if (!string.IsNullOrEmpty(value.Lines[3])) {
                        valuesDictionary3.SetValue("Line4", value.Lines[3]);
                    }
                    if (value.Colors[0] != Color.Black) {
                        valuesDictionary3.SetValue("Color1", value.Colors[0]);
                    }
                    if (value.Colors[1] != Color.Black) {
                        valuesDictionary3.SetValue("Color2", value.Colors[1]);
                    }
                    if (value.Colors[2] != Color.Black) {
                        valuesDictionary3.SetValue("Color3", value.Colors[2]);
                    }
                    if (value.Colors[3] != Color.Black) {
                        valuesDictionary3.SetValue("Color4", value.Colors[3]);
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
            foreach (Dictionary<Point3, TextData> points in m_textsByPoint.Values) {
                foreach (TextData value in points.Values) {
                    value.TextureLocation = null;
                }
            }
        }

        public void RenderText(FontBatch2D fontBatch, FlatBatch2D flatBatch, TextData textData) {
            if (!textData.TextureLocation.HasValue) {
                return;
            }
            List<string> list = [];
            List<Color> list2 = [];
            for (int i = 0; i < textData.Lines.Length; i++) {
                if (!string.IsNullOrEmpty(textData.Lines[i])) {
                    list.Add(textData.Lines[i].Replace("\\", "").ToUpper());
                    list2.Add(textData.Colors[i]);
                }
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
                        position: new Vector2(
                            num4 / 2f,
                            j * m_font.GlyphHeight + textData.TextureLocation.Value * (4f * m_font.GlyphHeight) + (num5 - num2) / 2f
                        ),
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
            foreach (Dictionary<Point3, TextData> points in m_textsByPoint.Values) {
                foreach (TextData textData in points.Values) {
                    textData.ToBeRenderedFrame = Time.FrameIndex;
                    if (textData.TextureLocation.HasValue) {
                        continue;
                    }
                    int num2 = m_textureLocations.FirstIndex(d => d == null);
                    if (num2 < 0) {
                        num2 = m_textureLocations.FirstIndex(d => d.ToBeRenderedFrame != Time.FrameIndex);
                    }
                    if (num2 >= 0) {
                        TextData textData2 = m_textureLocations[num2];
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
                    TextData textData3 = m_textureLocations[j];
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
            foreach ((uint subterrainId, Dictionary<Point3, TextData> points) in m_textsByPoint) {
                GVSubterrainSystem subterrainSystem = subterrainId == 0 ? null : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];
                Matrix transform = subterrainId == 0 ? default : subterrainSystem?.GlobalTransform ?? default;
                Matrix orientation = subterrainId == 0 ? default : transform.OrientationMatrix;
                foreach (TextData nearText in points.Values) {
                    if (!nearText.TextureLocation.HasValue) {
                        continue;
                    }
                    int cellValue = m_subsystemTerrain.Terrain.GetCellValue(nearText.Point.X, nearText.Point.Y, nearText.Point.Z);
                    int num = Terrain.ExtractContents(cellValue);
                    if (!(BlocksManager.Blocks[num] is GVBaseSignBlock signBlock)) {
                        continue;
                    }
                    int data = Terrain.ExtractData(cellValue);
                    BlockMesh signSurfaceBlockMesh = signBlock.GetSignSurfaceBlockMesh(data);
                    if (signSurfaceBlockMesh != null) {
                        TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(nearText.Point.X, nearText.Point.Z);
                        if (chunkAtCell != null
                            && chunkAtCell.State >= TerrainChunkState.InvalidVertices1) {
                            nearText.Light = Terrain.ExtractLight(cellValue);
                        }
                        float x = 0f;
                        float x2 = nearText.UsedTextureWidth / (m_font.GlyphHeight * 16f);
                        float x3 = nearText.TextureLocation.Value / 32f;
                        float x4 = (nearText.TextureLocation.Value + nearText.UsedTextureHeight / (m_font.GlyphHeight * 4f)) / 32f;
                        Vector3 vector = new(nearText.Point.X, nearText.Point.Y, nearText.Point.Z);
                        Vector3 vectorPlus05Transformed = subterrainId == 0
                            ? new Vector3(nearText.Point.X + 0.5f, nearText.Point.Y + 0.5f, nearText.Point.Z + 0.5f)
                            : Vector3.Transform(new Vector3(nearText.Point.X + 0.5f, nearText.Point.Y + 0.5f, nearText.Point.Z + 0.5f), transform);
                        if (camera.ViewFrustum.Intersection(vectorPlus05Transformed + camera.ViewDirection)) {
                            Vector3 signSurfaceNormal = signBlock.GetSignSurfaceNormal(data);
                            Vector3 vector2 = MathUtils.Max(
                                    0.01f * Vector3.Dot(camera.ViewPosition - vectorPlus05Transformed, signSurfaceNormal),
                                    0.005f
                                )
                                * signSurfaceNormal;
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
                    }
                }
            }
            m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
        }
    }
}