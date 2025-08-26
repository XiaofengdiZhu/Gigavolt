using Engine;
using Engine.Graphics;

namespace Game {
    public class CameraGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemDrawing m_subsystemDrawing;
        public readonly SubsystemGameWidgets m_subsystemGameWidgets;
        public readonly SubsystemTerrain m_subsystemTerrain;
        public readonly SubsystemSky m_subsystemSky;
        public readonly SubsystemGVCameraBlockBehavior m_subsystemGVCameraBlockBehavior;
        public readonly GVSubterrainSystem m_subterrainSystem;
        public GameWidget m_gameWidget;
        public GVCamera m_camera;
        public Vector3 m_originalPosition;
        public RenderTarget2D m_renderTarget;
        public bool m_complex;
        public uint m_inputIn;
        public uint m_inputTop;
        public uint m_inputRight;
        public uint m_inputBottom;
        public uint m_inputLeft;
        public int m_lastRotation;
        public int m_mountingFace;

        public static readonly Vector3[] m_upVector3 = [
            Vector3.UnitY,
            Vector3.UnitX,
            -Vector3.UnitY,
            -Vector3.UnitX,
            Vector3.UnitY,
            -Vector3.UnitZ,
            -Vector3.UnitY,
            Vector3.UnitZ,
            Vector3.UnitY,
            -Vector3.UnitX,
            -Vector3.UnitY,
            Vector3.UnitX,
            Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitY,
            -Vector3.UnitZ,
            -Vector3.UnitZ,
            Vector3.UnitX,
            Vector3.UnitZ,
            -Vector3.UnitX,
            Vector3.UnitZ,
            Vector3.UnitX,
            -Vector3.UnitZ,
            -Vector3.UnitX
        ];

        public CameraGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) {
            m_subsystemDrawing = SubsystemGVElectricity.Project.FindSubsystem<SubsystemDrawing>(true);
            m_subsystemGameWidgets = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGameWidgets>(true);
            m_subsystemTerrain = SubsystemGVElectricity.SubsystemTerrain;
            m_subsystemSky = SubsystemGVElectricity.Project.FindSubsystem<SubsystemSky>(true);
            m_subsystemGVCameraBlockBehavior = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVCameraBlockBehavior>(true);
            if (subterrainId != 0) {
                m_subterrainSystem = GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];
            }
        }

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(
                SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z)
            );
            m_mountingFace = cellFace.Face;
            m_lastRotation = RotateableMountedGVElectricElementBlock.GetRotation(data);
            m_complex = GVDisplayLedBlock.GetComplex(data);
            m_originalPosition = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            int index = GVStaticStorage.random.Int(4, int.MaxValue);
            while (m_subsystemTerrain.TerrainUpdater.m_pendingLocations.ContainsKey(index)) {
                index = GVStaticStorage.random.Int(4, int.MaxValue);
            }
            m_gameWidget = new GameWidget(new PlayerData(SubsystemGVElectricity.Project) { PlayerIndex = index }, 0);
            m_subsystemGameWidgets.m_gameWidgets.Add(m_gameWidget);
            m_subsystemGVCameraBlockBehavior.m_gameWidgets.Add(m_gameWidget);
            m_camera = new GVCamera(m_gameWidget);
            m_gameWidget.m_activeCamera = m_camera;
            m_gameWidget.m_cameras = [m_camera];
            Vector3 forward = CellFace.FaceToVector3(m_mountingFace);
            Vector3 originalPosition = m_originalPosition;
            if (SubterrainId != 0) {
                Matrix transform = m_subterrainSystem.GlobalTransform;
                m_originalPosition = Vector3.Transform(m_originalPosition, transform);
            }
            if (m_complex) {
                m_camera.SetupPerspectiveCamera(originalPosition, -Vector3.UnitZ, Vector3.UnitY);
            }
            else {
                m_camera.SetupPerspectiveCamera(originalPosition, forward, m_upVector3[m_mountingFace * 4 + m_lastRotation]);
            }
            m_subsystemTerrain.TerrainUpdater.SetUpdateLocation(
                m_gameWidget.PlayerData.PlayerIndex,
                originalPosition.XZ,
                MathUtils.Min(m_subsystemSky.VisibilityRange, 64f),
                0f
            );
        }

        public override void OnRemoved() {
            m_subsystemTerrain.TerrainUpdater.RemoveUpdateLocation(m_gameWidget.PlayerData.PlayerIndex);
            m_subsystemGameWidgets.m_gameWidgets.Remove(m_gameWidget);
            m_subsystemGVCameraBlockBehavior.m_gameWidgets.Remove(m_gameWidget);
            m_gameWidget.Dispose();
            m_gameWidget = null;
            m_camera = null;
            Utilities.Dispose(ref m_renderTarget);
        }

        public override bool Simulate() {
            int electricRotation = Rotation;
            uint lastInputIn = m_inputIn;
            m_inputIn = 0u;
            if (m_complex) {
                uint lastInputTop = m_inputTop;
                uint lastInputRight = m_inputRight;
                uint lastInputBottom = m_inputBottom;
                uint lastInputLeft = m_inputLeft;
                m_inputTop = 0u;
                m_inputRight = 0u;
                m_inputBottom = 0u;
                m_inputLeft = 0u;
                foreach (GVElectricConnection connection in Connections) {
                    if (connection.ConnectorType != GVElectricConnectorType.Output
                        && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                        GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(
                            CellFaces[0].Face,
                            electricRotation,
                            connection.ConnectorFace
                        );
                        if (connectorDirection.HasValue) {
                            if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                                m_inputIn = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                                m_inputTop = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                                m_inputRight = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                                m_inputBottom = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                                m_inputLeft = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                        }
                    }
                }
                bool changed = false;
                Vector3 newViewPosition = m_camera.ViewPosition;
                Vector3 newViewDirection = m_camera.ViewDirection;
                Vector3 newViewUp = m_camera.ViewUp;
                if (m_inputRight != lastInputRight) {
                    changed = true;
                    newViewPosition.X = m_originalPosition.X + (m_inputRight & 0x7FFFu) / (((m_inputRight >> 15) & 1u) == 1u ? -8f : 8f);
                    newViewPosition.Z = m_originalPosition.Z + ((m_inputRight >> 16) & 0x7FFFu) / (((m_inputRight >> 31) & 1u) == 1u ? -8f : 8f);
                }
                if (m_inputTop != lastInputTop) {
                    changed = true;
                    newViewPosition.Y = m_originalPosition.Y + ((m_inputTop >> 16) & 0x7FFFu) / (((m_inputTop >> 31) & 1u) == 1u ? -8f : 8f);
                    m_camera.m_viewAngel = MathUtils.DegToRad(m_inputTop & 0xFFu);
                }
                if (m_inputLeft != lastInputLeft) {
                    changed = true;
                    m_camera.m_viewSize.X = (int)((m_inputLeft >> 16) & 0xFFFFu);
                    m_camera.m_viewSize.Y = (int)(m_inputLeft & 0xFFFFu);
                }
                if (m_inputBottom != lastInputBottom) {
                    changed = true;
                    Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(
                        (m_inputBottom & 0xFFu) * 0.017453292f * (((m_inputBottom >> 26) & 1u) == 1u ? -1f : 1f),
                        ((m_inputBottom >> 8) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 25) & 1u) == 1u ? -1f : 1f),
                        ((m_inputBottom >> 16) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 24) & 1u) == 1u ? -1f : 1f)
                    );
                    newViewDirection = quaternion.GetForwardVector();
                    newViewUp = quaternion.GetUpVector();
                }
                if (changed) {
                    m_camera.PrepareForDrawing();
                    if (SubterrainId != 0) {
                        Matrix transform = m_subterrainSystem.GlobalTransform;
                        newViewPosition = Vector3.Transform(newViewPosition, transform);
                        Matrix orientation = transform.OrientationMatrix;
                        newViewDirection = Vector3.Transform(newViewDirection, orientation);
                        newViewUp = Vector3.Transform(newViewUp, orientation);
                    }
                    m_camera.SetupPerspectiveCamera(newViewPosition, newViewDirection, newViewUp);
                    m_subsystemTerrain.TerrainUpdater.SetUpdateLocation(
                        m_gameWidget.PlayerData.PlayerIndex,
                        newViewPosition.XZ,
                        MathUtils.Min(m_subsystemSky.VisibilityRange, 64f),
                        0f
                    );
                }
            }
            else {
                foreach (GVElectricConnection connection in Connections) {
                    if (connection.ConnectorType != GVElectricConnectorType.Output
                        && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                        if (SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, electricRotation, connection.ConnectorFace).HasValue) {
                            m_inputIn = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) | m_inputIn;
                        }
                    }
                }
                if (electricRotation != m_lastRotation) {
                    m_camera.PrepareForDrawing();
                    Vector3 position = m_originalPosition;
                    Vector3 forward = CellFace.FaceToVector3(m_mountingFace);
                    Vector3 up = m_upVector3[m_mountingFace * 4 + m_lastRotation];
                    if (SubterrainId != 0) {
                        Matrix transform = m_subterrainSystem.GlobalTransform;
                        position = Vector3.Transform(position, transform);
                        Matrix orientation = transform.OrientationMatrix;
                        forward = Vector3.Transform(forward, orientation);
                        up = Vector3.Transform(up, orientation);
                    }
                    m_camera.SetupPerspectiveCamera(position, forward, up);
                    m_lastRotation = electricRotation;
                }
            }
            if (m_inputIn != lastInputIn
                && m_inputIn > 0
                && GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_inputIn, out GVArrayData data)
                && m_camera.m_viewSize.X > 0
                && m_camera.m_viewSize.Y > 0
                && m_camera.m_viewAngel > 0) {
                //去雾
                foreach (TerrainChunk terrainChunk in m_subsystemTerrain.Terrain.AllocatedChunks) {
                    if (Vector2.DistanceSquared(m_camera.ViewPosition.XZ, terrainChunk.Center) <= MathUtils.Sqr(m_subsystemSky.VisibilityRange)
                        && terrainChunk.State == TerrainChunkState.Valid) {
                        terrainChunk.HazeEnds[0] = float.MaxValue;
                    }
                }
                RenderTarget2D lastRenderTarget = Display.RenderTarget;
                if (m_renderTarget == null
                    || m_renderTarget.Width != m_camera.m_viewSize.X
                    || m_renderTarget.Height != m_camera.m_viewSize.Y) {
                    Utilities.Dispose(ref m_renderTarget);
                    m_renderTarget = new RenderTarget2D(
                        m_camera.m_viewSize.X,
                        m_camera.m_viewSize.Y,
                        1,
                        ColorFormat.Rgba8888,
                        DepthFormat.Depth24Stencil8
                    );
                }
                Display.RenderTarget = m_renderTarget;
                Display.Clear(Color.Black, 1f, 0);
                try {
                    m_subsystemDrawing.Draw(m_camera);
                }
                finally {
                    Display.RenderTarget = lastRenderTarget;
                }
                data.Image2Data(m_renderTarget.GetData(new Rectangle(0, 0, m_renderTarget.Width, m_renderTarget.Height)));
            }
            return false;
        }
    }
}