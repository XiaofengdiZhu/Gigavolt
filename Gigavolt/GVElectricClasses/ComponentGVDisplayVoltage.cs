using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class ComponentGVDisplayVoltage : Component, IDrawable {
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public SubsystemGV8NumberLedGlow m_subsystemGV8NumberLedGlow;
        TexturedBatch3D m_8Numberbatch;
        public ComponentPlayer m_componentPlayer;
        public ComponentBlockHighlight m_componentBlockHighlight;

        public static int[] m_drawOrders = [1];

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
                m_8Numberbatch.Flush(camera.ViewProjectionMatrix);
            }
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
            m_subsystemGV8NumberLedGlow = Project.FindSubsystem<SubsystemGV8NumberLedGlow>(true);
            m_8Numberbatch = m_subsystemGV8NumberLedGlow.batchCache;
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
                    GVElectricElement element = null;
                    if (!m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(new GVCellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out element)) {
                        for (int i = 0; i < 16; i++) {
                            if (m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(
                                    new GVCellFace(
                                        cellFace.X,
                                        cellFace.Y,
                                        cellFace.Z,
                                        blockFace,
                                        1 << i
                                    ),
                                    out element
                                )) {
                                break;
                            }
                        }
                    }
                    if (element != null) {
                        Vector3 forward = CellFace.FaceToVector3(blockFace);
                        Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) - 0.3f * forward;
                        int rotation = 0;
                        if (mountedBlock is RotateableMountedGVElectricElementBlock) {
                            rotation = RotateableMountedGVElectricElementBlock.GetRotation(blockData);
                        }
                        Vector3 up = blockFace < 4 ? Vector3.UnitY : rotation switch {
                            1 => Vector3.UnitZ,
                            2 => -Vector3.UnitX,
                            3 => -Vector3.UnitZ,
                            _ => Vector3.UnitX
                        };
                        Vector3 right = Vector3.Cross(forward, up);
                        const float size = 0.1f;
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
                                        SubsystemGV8NumberLedGlow.Draw8Number(
                                            m_8Numberbatch,
                                            element.GetOutputVoltage(connectorFace),
                                            position + offset,
                                            size,
                                            right,
                                            up,
                                            Color.Red
                                        );
                                        break;
                                    case GVElectricConnectorType.Input: {
                                        GVElectricConnection connection = element.Connections.Find(connection => connection.ConnectorFace == connectorFace);
                                        if (connection != null) {
                                            SubsystemGV8NumberLedGlow.Draw8Number(
                                                m_8Numberbatch,
                                                connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace),
                                                position + offset,
                                                size,
                                                right,
                                                up,
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
                                            SubsystemGV8NumberLedGlow.Draw8Number(
                                                m_8Numberbatch,
                                                connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace),
                                                position + inputOffset,
                                                size,
                                                right,
                                                up,
                                                Color.Green
                                            );
                                        }
                                        SubsystemGV8NumberLedGlow.Draw8Number(
                                            m_8Numberbatch,
                                            element.GetOutputVoltage(connectorFace),
                                            position + outputOffset,
                                            size,
                                            right,
                                            up,
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
                    if (m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(new GVCellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                        Vector3 forward = CellFace.FaceToVector3(cellFace.Face);
                        Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) + 0.55f * forward;
                        Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                        Vector3 right = Vector3.Cross(forward, up);
                        const float size = 0.1f;
                        SubsystemGV8NumberLedGlow.Draw8Number(
                            m_8Numberbatch,
                            element.GetOutputVoltage(blockFace),
                            position,
                            size,
                            right,
                            up,
                            Color.Red
                        );
                    }
                    break;
                }
                case IGVElectricWireElementBlock wireBlock: {
                    if (block is GVWireThroughBlock) {
                        blockFace = GVWireThroughBlock.GetWiredFace(blockData);
                    }
                    else {
                        int mask = GVWireBlock.GetWireFacesBitmask(blockValue);
                        for (int i = 0; i < 6; i++) {
                            if ((mask & (1 << i)) != 0) {
                                blockFace = i;
                                break;
                            }
                        }
                    }
                    if (wireBlock.IsWireHarness(blockValue)) {
                        for (int i = 0; i < 16; i++) {
                            if (m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(
                                    new GVCellFace(
                                        cellFace.X,
                                        cellFace.Y,
                                        cellFace.Z,
                                        blockFace,
                                        1 << i
                                    ),
                                    out GVElectricElement element
                                )) {
                                uint voltage = element.GetOutputVoltage(blockFace);
                                if (voltage > 0) {
                                    Vector3 forward = CellFace.FaceToVector3(cellFace.Face);
                                    Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                                    Vector3 right = Vector3.Cross(forward, up);
                                    Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) + 0.55f * forward - (i % 4 * 2 - 3) / 8f * right - (i / 4 * 2 - 3) / 8f * up;
                                    const float size = 0.1f;
                                    SubsystemGV8NumberLedGlow.Draw8Number(
                                        m_8Numberbatch,
                                        voltage,
                                        position,
                                        size,
                                        right,
                                        up,
                                        SubsystemPalette.GetColor(m_subsystemTerrain, i)
                                    );
                                }
                            }
                        }
                    }
                    else {
                        GVElectricElement element = null;
                        if (!m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(new GVCellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out element)) {
                            for (int i = 0; i < 16; i++) {
                                if (m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(
                                        new GVCellFace(
                                            cellFace.X,
                                            cellFace.Y,
                                            cellFace.Z,
                                            blockFace,
                                            1 << i
                                        ),
                                        out element
                                    )) {
                                    break;
                                }
                            }
                        }
                        if (element != null) {
                            Vector3 forward = CellFace.FaceToVector3(cellFace.Face);
                            Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) + 0.55f * forward;
                            Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                            Vector3 right = Vector3.Cross(forward, up);
                            const float size = 0.1f;
                            SubsystemGV8NumberLedGlow.Draw8Number(
                                m_8Numberbatch,
                                element.GetOutputVoltage(blockFace),
                                position,
                                size,
                                right,
                                up,
                                Color.Red
                            );
                        }
                    }
                    break;
                }
                case IGVElectricElementBlock: {
                    blockFace = block switch {
                        GVAttachedSignCBlock => GVAttachedSignCBlock.GetFace(blockData),
                        GVDoorBlock => GVDoorBlock.GetHingeFace(blockData),
                        GVTrapdoorBlock => GVTrapdoorBlock.GetMountingFace(blockData),
                        GVFenceGateBlock => GVFenceGateBlock.GetHingeFace(blockData),
                        _ => blockFace
                    };
                    if (m_subsystemGVElectricity.m_GVElectricElementsByCellFace.TryGetValue(new GVCellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                        Vector3 forward = CellFace.FaceToVector3(cellFace.Face);
                        Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) + 0.55f * forward;
                        Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                        Vector3 right = Vector3.Cross(forward, up);
                        const float size = 0.1f;
                        SubsystemGV8NumberLedGlow.Draw8Number(
                            m_8Numberbatch,
                            element.CalculateAllInputsVoltage(),
                            position,
                            size,
                            right,
                            up,
                            Color.Green
                        );
                    }
                    break;
                }
            }
        }
    }
}