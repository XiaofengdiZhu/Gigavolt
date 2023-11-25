using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class ComponentGVDisplayVoltage : Component, IDrawable {
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public SubsystemGV8NumberLedGlow m_subsystemGV8NumberLedGlow;
        public ComponentPlayer m_componentPlayer;
        public ComponentBlockHighlight m_componentBlockHighlight;

        public static int[] m_drawOrders = { 1 };

        public int[] DrawOrders => m_drawOrders;

        public void Draw(Camera camera, int drawOrder) {
            if (GVStaticStorage.DisplayVoltage
                && camera.GameWidget.PlayerData == m_componentPlayer.PlayerData
                && drawOrder == m_drawOrders[0]
                && !camera.UsesMovementControls
                && m_componentPlayer.ComponentHealth.Health > 0
                && m_componentPlayer.ComponentGui.ControlsContainerWidget.IsVisible) {
                if (m_componentPlayer.ComponentMiner.DigCellFace.HasValue) {
                    DisplayVoltage(m_componentPlayer.ComponentMiner.DigCellFace.Value);
                }
                else if (!m_componentPlayer.ComponentAimingSights.IsSightsVisible
                    && m_componentBlockHighlight.m_highlightRaycastResult is TerrainRaycastResult result) {
                    DisplayVoltage(result.CellFace);
                }
            }
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
            m_subsystemGV8NumberLedGlow = Project.FindSubsystem<SubsystemGV8NumberLedGlow>(true);
            m_componentPlayer = Entity.FindComponent<ComponentPlayer>(true);
            m_componentBlockHighlight = Entity.FindComponent<ComponentBlockHighlight>(true);
        }

        public void DisplayVoltage(CellFace cellFace) {
            int blockValue = m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int blockContents = Terrain.ExtractContents(blockValue);
            int blockData = Terrain.ExtractData(blockValue);
            int blockFace = 4;
            Block block = BlocksManager.Blocks[blockContents];
            switch (block) {
                case MountedGVElectricElementBlock mountedBlock: {
                    blockFace = mountedBlock.GetFace(blockValue);
                    if (m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(new CellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                        Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) - 0.3f * CellFace.FaceToVector3(blockFace);
                        Vector3 forward = CellFace.FaceToVector3(blockFace);
                        int rotation = 0;
                        if (mountedBlock is RotateableMountedGVElectricElementBlock) {
                            rotation = RotateableMountedGVElectricElementBlock.GetRotation(blockData);
                        }
                        Vector3 up = blockFace < 4 ? Vector3.UnitY : rotation switch { 1 => Vector3.UnitX, 2 => Vector3.UnitZ, 3 => -Vector3.UnitX, _ => -Vector3.UnitZ };
                        Vector3 right = Vector3.Cross(forward, up);
                        const float size = 0.1f;
                        Vector3 p1 = position + size * (-right - up);
                        Vector3 p2 = position + size * (right - up);
                        Vector3 p3 = position + size * (right + up);
                        Vector3 p4 = position + size * (-right + up);
                        for (int connectorFace = 0; connectorFace < 6; connectorFace++) {
                            GVElectricConnectorType? connectorType = mountedBlock.GetGVConnectorType(
                                m_subsystemTerrain,
                                blockValue,
                                blockFace,
                                connectorFace,
                                cellFace.X,
                                cellFace.Y,
                                cellFace.Z
                            );
                            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(blockFace, rotation, connectorFace);
                            Vector3 offset = connectorDirection == GVElectricConnectorDirection.In ? -0.4f * (up + right) : 0.4f * CellFace.FaceToVector3(connectorFace);
                            if (connectorType.HasValue) {
                                switch (connectorType.Value) {
                                    case GVElectricConnectorType.Output:
                                        DrawVoltage(
                                            element.GetOutputVoltage(connectorFace),
                                            p1 + offset,
                                            p2 + offset,
                                            p3 + offset,
                                            p4 + offset,
                                            Color.Red
                                        );
                                        break;
                                    case GVElectricConnectorType.Input: {
                                        GVElectricConnection connection = element.Connections.Find(connection => connection.ConnectorFace == connectorFace);
                                        if (connection != null) {
                                            DrawVoltage(
                                                connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace),
                                                p1 + offset,
                                                p2 + offset,
                                                p3 + offset,
                                                p4 + offset,
                                                Color.Green
                                            );
                                        }
                                        break;
                                    }
                                    case GVElectricConnectorType.InputOutput: {
                                        GVElectricConnection connection = element.Connections.Find(connection => connection.ConnectorFace == connectorFace);
                                        Vector3 outputOffset = offset;
                                        if (connection != null) {
                                            Vector3 inputOffset = offset;
                                            Vector3 offset2 = 0.11f
                                                * (blockFace < 4 ? connectorFace > 3 ? right : up :
                                                    rotation == connectorFace || rotation + 2 == connectorFace || rotation - 2 == connectorFace ? right : up);
                                            inputOffset += offset2;
                                            outputOffset -= offset2;
                                            DrawVoltage(
                                                connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace),
                                                p1 + inputOffset,
                                                p2 + inputOffset,
                                                p3 + inputOffset,
                                                p4 + inputOffset,
                                                Color.Green
                                            );
                                        }
                                        DrawVoltage(
                                            element.GetOutputVoltage(connectorFace),
                                            p1 + outputOffset,
                                            p2 + outputOffset,
                                            p3 + outputOffset,
                                            p4 + outputOffset,
                                            Color.Red
                                        );
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                case GVBatteryBlock or GVDebugBlock: {
                    if (m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(new CellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                        Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) + 0.55f * CellFace.FaceToVector3(cellFace.Face);
                        Vector3 forward = CellFace.FaceToVector3(cellFace.Face);
                        Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                        Vector3 right = Vector3.Cross(forward, up);
                        const float size = 0.1f;
                        Vector3 p1 = position + size * (-right - up);
                        Vector3 p2 = position + size * (right - up);
                        Vector3 p3 = position + size * (right + up);
                        Vector3 p4 = position + size * (-right + up);
                        DrawVoltage(
                            element.GetOutputVoltage(blockFace),
                            p1,
                            p2,
                            p3,
                            p4,
                            Color.Red
                        );
                    }
                    break;
                }
                case IGVElectricWireElementBlock: break;
                case IGVElectricElementBlock: {
                    blockFace = block switch { GVAttachedSignCBlock => GVAttachedSignCBlock.GetFace(blockData), GVDoorBlock => GVDoorBlock.GetHingeFace(blockData), GVTrapdoorBlock => GVTrapdoorBlock.GetMountingFace(blockData), GVFenceGateBlock => GVFenceGateBlock.GetHingeFace(blockData), _ => blockFace };
                    if (m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(new CellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                        Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) + 0.55f * CellFace.FaceToVector3(cellFace.Face);
                        Vector3 forward = CellFace.FaceToVector3(cellFace.Face);
                        Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                        Vector3 right = Vector3.Cross(forward, up);
                        const float size = 0.1f;
                        Vector3 p1 = position + size * (-right - up);
                        Vector3 p2 = position + size * (right - up);
                        Vector3 p3 = position + size * (right + up);
                        Vector3 p4 = position + size * (-right + up);
                        DrawVoltage(
                            element.CalculateAllInputsVoltage(),
                            p1,
                            p2,
                            p3,
                            p4,
                            Color.Green
                        );
                    }
                    break;
                }
            }
        }

        public void DrawVoltage(uint voltage, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color) {
            if (m_subsystemGV8NumberLedGlow.batchCache.TryGetValue(voltage, out TexturedBatch3D batch)) {
                batch.QueueQuad(
                    p1,
                    p2,
                    p3,
                    p4,
                    Vector2.One,
                    Vector2.UnitY,
                    Vector2.Zero,
                    Vector2.UnitX,
                    color
                );
            }
            else {
                TexturedBatch3D newBatch = SubsystemGV8NumberLedGlow.generateBatch(m_subsystemGV8NumberLedGlow.m_primitivesRenderer, voltage);
                newBatch.QueueQuad(
                    p1,
                    p2,
                    p3,
                    p4,
                    Vector2.One,
                    Vector2.UnitY,
                    Vector2.Zero,
                    Vector2.UnitX,
                    color
                );
                m_subsystemGV8NumberLedGlow.batchCache.Add(voltage, newBatch);
            }
        }
    }
}