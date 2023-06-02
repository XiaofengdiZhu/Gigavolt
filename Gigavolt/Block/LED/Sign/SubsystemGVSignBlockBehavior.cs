using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Media;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVSignBlockBehavior : SubsystemBlockBehavior, IDrawable, IUpdateable {
        public SubsystemGameWidgets m_subsystemViews;

        public SubsystemTerrain m_subsystemTerrain;

        public SubsystemGameInfo m_subsystemGameInfo;

        public Dictionary<Point3, GVSignTextData> m_textsByPoint = new Dictionary<Point3, GVSignTextData>();

        public GVSignTextData[] m_textureLocations = new GVSignTextData[32];

        public List<GVSignTextData> m_nearTexts = new List<GVSignTextData>();

        public BitmapFont m_font = LabelWidget.BitmapFont;

        public RenderTarget2D m_renderTarget;

        public List<Vector3> m_lastUpdatePositions = new List<Vector3>();

        public PrimitivesRenderer2D m_primitivesRenderer2D = new PrimitivesRenderer2D();

        public PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

        public static int[] m_drawOrders = { 50 };

        public override int[] HandledBlocks => new[] { 801 };


        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public int[] DrawOrders => m_drawOrders;

        public GVSignTextData GetSignData(Point3 point) {
            if (m_textsByPoint.TryGetValue(point, out GVSignTextData value)) {
                return new GVSignTextData { Line = value.Line, Color = value.Color, Url = value.Url };
            }
            return null;
        }

        public void SetSignData(Point3 point, string line, Color color, string url) {
            GVSignTextData textData = new GVSignTextData { Point = point, Line = line, Color = color, Url = url };
            m_textsByPoint[point] = textData;
            m_lastUpdatePositions.Clear();
        }

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ) {
            int cellValueFast = SubsystemTerrain.Terrain.GetCellValueFast(x, y, z);
            int num = Terrain.ExtractContents(cellValueFast);
            int data = Terrain.ExtractData(cellValueFast);
            Block block = BlocksManager.Blocks[num];
            if (block is AttachedSignBlock) {
                Point3 point = CellFace.FaceToPoint3(AttachedSignBlock.GetFace(data));
                int x2 = x - point.X;
                int y2 = y - point.Y;
                int z2 = z - point.Z;
                int cellValue = SubsystemTerrain.Terrain.GetCellValue(x2, y2, z2);
                int cellContents = Terrain.ExtractContents(cellValue);
                if (!BlocksManager.Blocks[cellContents].IsCollidable_(cellValue)) {
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
            }
            else if (block is PostedSignBlock) {
                int num2 = PostedSignBlock.GetHanging(data) ? SubsystemTerrain.Terrain.GetCellValue(x, y + 1, z) : SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
                if (!BlocksManager.Blocks[Terrain.ExtractContents(num2)].IsCollidable_(num2)) {
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
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
            Point3 point = new Point3(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
            if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure) {
                GVSignTextData signData = GetSignData(point);
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
            Point3 key = new Point3(x, y, z);
            m_textsByPoint.Remove(key);
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
                string value2 = value11.GetValue("Line1", string.Empty);
                Color value6 = value11.GetValue("Color1", Color.White);
                string value10 = value11.GetValue("Url", string.Empty);
                SetSignData(value, value2, value6, value10);
            }
            Display.DeviceReset += Display_DeviceReset;
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            int num = 0;
            ValuesDictionary valuesDictionary2 = new ValuesDictionary();
            valuesDictionary.SetValue("Texts", valuesDictionary2);
            foreach (GVSignTextData value in m_textsByPoint.Values) {
                ValuesDictionary valuesDictionary3 = new ValuesDictionary();
                valuesDictionary3.SetValue("Point", value.Point);
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
            foreach (GVSignTextData value in m_textsByPoint.Values) {
                value.TextureLocation = null;
            }
        }

        public void RenderText(FontBatch2D fontBatch, FlatBatch2D flatBatch, GVSignTextData textData) {
            if (!textData.TextureLocation.HasValue) {
                return;
            }
            List<string> list = new List<string>();
            List<Color> list2 = new List<Color>();
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
            m_nearTexts.Clear();
            foreach (GVSignTextData value in m_textsByPoint.Values) {
                m_nearTexts.Add(value);
            }
            foreach (GVSignTextData nearText in m_nearTexts) {
                nearText.ToBeRenderedFrame = Time.FrameIndex;
            }
            bool flag3 = false;
            for (int i = 0; i < MathUtils.Min(m_nearTexts.Count, 32); i++) {
                GVSignTextData textData = m_nearTexts[i];
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
            if (m_nearTexts.Count <= 0) {
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
            foreach (GVSignTextData nearText in m_nearTexts) {
                if (!nearText.TextureLocation.HasValue) {
                    continue;
                }
                int cellValue = m_subsystemTerrain.Terrain.GetCellValue(nearText.Point.X, nearText.Point.Y, nearText.Point.Z);
                int num = Terrain.ExtractContents(cellValue);
                if (!(BlocksManager.Blocks[num] is GVSignCBlock signBlock)) {
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
                    float num2 = LightingManager.LightIntensityByLightValue[nearText.Light];
                    Color color = new Color(num2, num2, num2);
                    float x = 0f;
                    float x2 = nearText.UsedTextureWidth / (m_font.GlyphHeight * 16f);
                    float x3 = nearText.TextureLocation.Value / 32f;
                    float x4 = (nearText.TextureLocation.Value + nearText.UsedTextureHeight / (m_font.GlyphHeight * 4f)) / 32f;
                    Vector3 signSurfaceNormal = signBlock.GetSignSurfaceNormal(data);
                    Vector3 vector = new Vector3(nearText.Point.X, nearText.Point.Y, nearText.Point.Z);
                    float num3 = Vector3.Dot(camera.ViewPosition - (vector + new Vector3(0.5f)), signSurfaceNormal);
                    Vector3 vector2 = MathUtils.Max(0.01f * num3, 0.005f) * signSurfaceNormal;
                    for (int i = 0; i < signSurfaceBlockMesh.Indices.Count / 3; i++) {
                        BlockMeshVertex blockMeshVertex = signSurfaceBlockMesh.Vertices.Array[signSurfaceBlockMesh.Indices.Array[i * 3]];
                        BlockMeshVertex blockMeshVertex2 = signSurfaceBlockMesh.Vertices.Array[signSurfaceBlockMesh.Indices.Array[i * 3 + 1]];
                        BlockMeshVertex blockMeshVertex3 = signSurfaceBlockMesh.Vertices.Array[signSurfaceBlockMesh.Indices.Array[i * 3 + 2]];
                        Vector3 p = blockMeshVertex.Position + vector + vector2;
                        Vector3 p2 = blockMeshVertex2.Position + vector + vector2;
                        Vector3 p3 = blockMeshVertex3.Position + vector + vector2;
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
            m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
        }
    }
}