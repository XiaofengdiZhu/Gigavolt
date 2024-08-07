using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Engine.Input;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class ComponentGigavolt : Component, IDrawable, IUpdateable {
        public SubsystemGVSubterrain m_subsystemGVSubterrain;
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public SubsystemGVBatteryBlockBehavior m_subsystemGVBatteryBlockBehavior;
        public SubsystemGVSwitchBlockBehavior m_subsystemGVSwitchBlockBehavior;
        public SubsystemGVButtonBlockBehavior m_subsystemGVButtonBlockBehavior;
        public TexturedBatch3D m_8NumberBatch;
        public TexturedBatch3D m_mouseScrollBatch;
        public ComponentPlayer m_componentPlayer;
        public ComponentBlockHighlight m_componentBlockHighlight;

        public bool m_isCreativeMode;
        public bool m_categoryColorNotSet = true;
        public uint? m_forceDisplayVoltage;
        public CanvasWidget m_controlsContainer;
        public DragHostWidget m_dragHostWidget;
        public GVWheelPanelWidget m_wheelPanelWidget;
        public DateTime? m_dragStartTime;
        public Vector2 m_dragStartPosition;


        public int[] DrawOrders => [2001];
        public UpdateOrder UpdateOrder => UpdateOrder.Reset;

        public void Draw(Camera camera, int drawOrder) {
            if ((GVStaticStorage.DisplayVoltage || m_forceDisplayVoltage.HasValue)
                && camera.GameWidget.PlayerData == m_componentPlayer.PlayerData
                && drawOrder == DrawOrders[0]
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
                m_8NumberBatch.Flush(camera.ViewProjectionMatrix);
                m_mouseScrollBatch.Flush(camera.ViewProjectionMatrix);
            }
        }

        public void Update(float dt) {
            if (m_isCreativeMode) {
                //设置创造模式背包中的分类颜色
                if (m_categoryColorNotSet) {
                    if (m_componentPlayer.ComponentGui.ModalPanelWidget is CreativeInventoryWidget widget) {
                        foreach (CreativeInventoryWidget.Category c in widget.m_categories) {
                            if (c.Name.StartsWith("GV ")) {
                                if (c.Color.B == 243) {
                                    break;
                                }
                                if (c.Name.EndsWith("Expand")
                                    || c.Name.EndsWith("Multiple")) {
                                    c.Color = new Color(233, 85, 227);
                                }
                                else {
                                    c.Color = new Color(30, 213, 243);
                                }
                            }
                        }
                        m_categoryColorNotSet = false;
                    }
                }
                //快捷轮盘
                if (GVStaticStorage.WheelPanelEnabled) {
                    if (m_dragHostWidget.m_dragWidget == null) {
                        m_wheelPanelWidget.m_hide = false;
                        m_dragStartTime = null;
                    }
                    else {
                        if (m_dragStartTime == null) {
                            m_dragStartTime = DateTime.Now;
                            m_dragStartPosition = m_dragHostWidget.m_dragPosition;
                        }
                        else {
                            Vector2 transformedDragPosition = m_controlsContainer.ScreenToWidget(m_dragHostWidget.m_dragPosition);
                            const float edgeDistance = (GVWheelPanelWidget.FirstRingDiameter + GVWheelPanelWidget.RingSpacing) / 2f;
                            if (Vector2.DistanceSquared(m_dragHostWidget.m_dragPosition, m_dragStartPosition) < 50
                                && transformedDragPosition.X > edgeDistance
                                && transformedDragPosition.X < m_controlsContainer.ActualSize.X - edgeDistance
                                && transformedDragPosition.Y > edgeDistance
                                && transformedDragPosition.Y < m_controlsContainer.ActualSize.Y - edgeDistance) {
                                if ((DateTime.Now - m_dragStartTime.Value).TotalMilliseconds > 600
                                    && m_dragHostWidget.m_dragData is InventoryDragData data) {
                                    int centerBlockValue = data.Inventory.GetSlotValue(data.SlotIndex);
                                    Block centerBlock = BlocksManager.Blocks[Terrain.ExtractContents(centerBlockValue)];
                                    IGVCustomWheelPanelBlock customWheelPanelBlock = centerBlock as IGVCustomWheelPanelBlock;
                                    List<int> outerBlocksValue = customWheelPanelBlock == null ? BlocksManager.Blocks[Terrain.ExtractContents(centerBlockValue)].GetCreativeValues().ToList() : customWheelPanelBlock.GetCustomWheelPanelValues(centerBlockValue);
                                    m_wheelPanelWidget.CenterBlockValue = centerBlockValue;
                                    m_wheelPanelWidget.OuterBlocksValue = outerBlocksValue;
                                    m_wheelPanelWidget.IsVisible = true;
                                    m_controlsContainer.SetWidgetPosition(m_wheelPanelWidget, transformedDragPosition - m_wheelPanelWidget.Size / 2);
                                    m_wheelPanelWidget.m_originalInventoryDragData = data;
                                }
                            }
                            else if (!m_wheelPanelWidget.IsVisible
                                && !m_wheelPanelWidget.m_hide) {
                                m_dragStartTime = DateTime.Now;
                                m_dragStartPosition = m_dragHostWidget.m_dragPosition;
                            }
                        }
                    }
                }
            }
            //Ctrl键+鼠标滚轮编辑电压
            if (!m_componentPlayer.ComponentAimingSights.IsSightsVisible
                && m_componentPlayer.GameWidget.Input.IsKeyDown(Key.Control)
                && m_componentBlockHighlight.m_highlightRaycastResult is TerrainRaycastResult result) {
                int wheelMovement = m_componentPlayer.GameWidget.Input.MouseWheelMovement;
                CellFace cellFace = result.CellFace;
                int contents = Terrain.ExtractContents(result.Value);
                SubsystemGVEditableItemBehavior<GigaVoltageLevelData> behavior;
                if (contents == GVButtonBlock.Index) {
                    int id = m_subsystemGVButtonBlockBehavior.GetIdFromValue(result.Value);
                    GVButtonData blockData = m_subsystemGVButtonBlockBehavior.GetItemData(id, true);
                    if (wheelMovement == 0) {
                        m_forceDisplayVoltage = blockData.GigaVoltageLevel;
                    }
                    else {
                        if (wheelMovement > 0) {
                            if (blockData.GigaVoltageLevel == uint.MaxValue) {
                                return;
                            }
                            blockData.GigaVoltageLevel++;
                        }
                        else {
                            if (blockData.GigaVoltageLevel == 0u) {
                                return;
                            }
                            blockData.GigaVoltageLevel--;
                        }
                        m_forceDisplayVoltage = blockData.GigaVoltageLevel;
                        m_componentPlayer.GameWidget.Input.Clear();
                        blockData.SaveString();
                        m_subsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, m_subsystemGVButtonBlockBehavior.SetIdToValue(result.Value, m_subsystemGVButtonBlockBehavior.StoreItemDataAtUniqueId(blockData, id)));
                    }
                }
                else {
                    switch (contents) {
                        case GVBatteryBlock.Index:
                            behavior = m_subsystemGVBatteryBlockBehavior;
                            break;
                        case GVSwitchBlock.Index:
                            behavior = m_subsystemGVSwitchBlockBehavior;
                            break;
                        default:
                            m_forceDisplayVoltage = null;
                            return;
                    }
                    int id = behavior.GetIdFromValue(result.Value);
                    GigaVoltageLevelData blockData = behavior.GetItemData(id, true);
                    if (wheelMovement == 0) {
                        m_forceDisplayVoltage = blockData.Data;
                    }
                    else {
                        if (wheelMovement > 0) {
                            if (blockData.Data == uint.MaxValue) {
                                return;
                            }
                            blockData.Data++;
                        }
                        else {
                            if (blockData.Data == 0u) {
                                return;
                            }
                            blockData.Data--;
                        }
                        m_forceDisplayVoltage = blockData.Data;
                        m_componentPlayer.GameWidget.Input.Clear();
                        blockData.SaveString();
                        m_subsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, behavior.SetIdToValue(result.Value, behavior.StoreItemDataAtUniqueId(blockData, id)));
                    }
                }
            }
            else {
                m_forceDisplayVoltage = null;
            }
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            m_subsystemGVSubterrain = Project.FindSubsystem<SubsystemGVSubterrain>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
            m_subsystemGVSwitchBlockBehavior = Project.FindSubsystem<SubsystemGVSwitchBlockBehavior>(true);
            m_subsystemGVButtonBlockBehavior = Project.FindSubsystem<SubsystemGVButtonBlockBehavior>(true);
            m_subsystemGVBatteryBlockBehavior = Project.FindSubsystem<SubsystemGVBatteryBlockBehavior>(true);
            m_8NumberBatch = Project.FindSubsystem<SubsystemGV8NumberLedGlow>(true).batchCache;
            m_mouseScrollBatch = new TexturedBatch3D {
                BlendState = BlendState.AlphaBlend,
                SamplerState = SamplerState.PointClamp,
                Texture = ContentManager.Get<Texture2D>("Textures/Gui/GVMouseScroll"),
                DepthStencilState = DepthStencilState.Default,
                RasterizerState = RasterizerState.CullCounterClockwiseScissor,
                UseAlphaTest = false
            };
            m_componentPlayer = Entity.FindComponent<ComponentPlayer>(true);
            m_componentBlockHighlight = Entity.FindComponent<ComponentBlockHighlight>(true);
            m_isCreativeMode = Project.FindSubsystem<SubsystemGameInfo>(true).WorldSettings.GameMode == GameMode.Creative;
            m_controlsContainer = m_componentPlayer.GuiWidget.Children.Find<CanvasWidget>("ControlsContainer");
            m_dragHostWidget = m_componentPlayer.DragHostWidget;
            m_wheelPanelWidget = new GVWheelPanelWidget(Project, m_componentPlayer.ComponentGui) { IsVisible = false, SubsystemTerrain = m_subsystemTerrain };
            m_componentPlayer.ComponentGui.ControlsContainerWidget.AddChildren(m_wheelPanelWidget);
        }

        public void DisplayVoltage(CellFace cellFace) {
            int blockValue = m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int blockContents = Terrain.ExtractContents(blockValue);
            int blockData = Terrain.ExtractData(blockValue);
            int blockFace = 4;
            Block block = BlocksManager.Blocks[blockContents];
            if (m_forceDisplayVoltage.HasValue
                && blockContents != GVSwitchBlock.Index
                && blockContents != GVButtonBlock.Index
                && blockContents != GVBatteryBlock.Index) {
                return;
            }
            Dictionary<GVCellFace, GVElectricElement> elements = m_subsystemGVElectricity.m_GVElectricElementsByCellFace[0];
            switch (block) {
                case MountedGVElectricElementBlock mountedBlock: {
                    blockFace = mountedBlock.GetFace(blockValue);
                    if (!elements.TryGetValue(new GVCellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                        for (int i = 0; i < 16; i++) {
                            if (elements.TryGetValue(
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
                        if (m_forceDisplayVoltage.HasValue) {
                            SubsystemGV8NumberLedGlow.Draw8Number(
                                m_8NumberBatch,
                                m_forceDisplayVoltage.Value,
                                position,
                                size,
                                right,
                                up,
                                Color.Cyan
                            );
                            Vector3 mouseScrollPosition = position + up * 0.4f + right * 0.1f;
                            Vector3 mouseScrollRight = right * 0.2f;
                            Vector3 mouseScrollUp = up * 0.2f;
                            m_mouseScrollBatch.QueueQuad(
                                mouseScrollPosition,
                                mouseScrollPosition - mouseScrollRight,
                                mouseScrollPosition - mouseScrollRight - mouseScrollUp,
                                mouseScrollPosition - mouseScrollUp,
                                Vector2.Zero,
                                Vector2.UnitX,
                                Vector2.One,
                                Vector2.UnitY,
                                Color.White
                            );
                            break;
                        }
                        for (int connectorFace = 0; connectorFace < 6; connectorFace++) {
                            GVElectricConnectorType? connectorType = mountedBlock.GetGVConnectorType(
                                m_subsystemGVSubterrain,
                                blockValue,
                                blockFace,
                                connectorFace,
                                cellFace.X,
                                cellFace.Y,
                                cellFace.Z,
                                0
                            );
                            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(blockFace, rotation, connectorFace);
                            Vector3 offset = connectorDirection == GVElectricConnectorDirection.In ? -0.4f * (up + right) : 0.4f * CellFace.FaceToVector3(connectorFace);
                            if (connectorType.HasValue) {
                                switch (connectorType.Value) {
                                    case GVElectricConnectorType.Output:
                                        SubsystemGV8NumberLedGlow.Draw8Number(
                                            m_8NumberBatch,
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
                                                m_8NumberBatch,
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
                                                m_8NumberBatch,
                                                connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace),
                                                position + inputOffset,
                                                size,
                                                right,
                                                up,
                                                Color.Green
                                            );
                                        }
                                        SubsystemGV8NumberLedGlow.Draw8Number(
                                            m_8NumberBatch,
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
                    if (elements.TryGetValue(new GVCellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                        Vector3 forward = CellFace.FaceToVector3(cellFace.Face);
                        Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) + 0.55f * forward;
                        Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                        Vector3 right = Vector3.Cross(forward, up);
                        const float size = 0.1f;
                        SubsystemGV8NumberLedGlow.Draw8Number(
                            m_8NumberBatch,
                            m_forceDisplayVoltage ?? element.GetOutputVoltage(blockFace),
                            position,
                            size,
                            right,
                            up,
                            m_forceDisplayVoltage.HasValue ? Color.Cyan : Color.Red
                        );
                        if (m_forceDisplayVoltage.HasValue) {
                            Vector3 mouseScrollPosition = position + up * 0.4f + right * 0.1f;
                            Vector3 mouseScrollRight = right * 0.2f;
                            Vector3 mouseScrollUp = up * 0.2f;
                            m_mouseScrollBatch.QueueQuad(
                                mouseScrollPosition,
                                mouseScrollPosition - mouseScrollRight,
                                mouseScrollPosition - mouseScrollRight - mouseScrollUp,
                                mouseScrollPosition - mouseScrollUp,
                                Vector2.Zero,
                                Vector2.UnitX,
                                Vector2.One,
                                Vector2.UnitY,
                                Color.White
                            );
                        }
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
                            if (elements.TryGetValue(
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
                                    Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) - (i % 4 * 2 - 3) / 8f * right - (i / 4 * 2 - 3) / 8f * up;
                                    if (wireBlock.IsWireThrough()) {
                                        position += forward * 0.55f;
                                    }
                                    const float size = 0.1f;
                                    SubsystemGV8NumberLedGlow.Draw8Number(
                                        m_8NumberBatch,
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
                        if (!elements.TryGetValue(new GVCellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                            for (int i = 0; i < 16; i++) {
                                if (elements.TryGetValue(
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
                            Vector3 position = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
                            if (wireBlock.IsWireThrough()) {
                                position += forward * 0.55f;
                            }
                            Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                            Vector3 right = Vector3.Cross(forward, up);
                            const float size = 0.1f;
                            SubsystemGV8NumberLedGlow.Draw8Number(
                                m_8NumberBatch,
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
                        GVAttachedSignBlock => GVAttachedSignBlock.GetFace(blockData),
                        GVSignCBlock => GVSignCBlock.GetPose(blockData) == 1 ? blockFace : GVSignCBlock.GetFace(blockData),
                        GVDoorBlock => GVDoorBlock.GetHingeFace(blockData),
                        GVTrapdoorBlock => GVTrapdoorBlock.GetMountingFace(blockData),
                        GVFenceGateBlock => GVFenceGateBlock.GetHingeFace(blockData),
                        _ => blockFace
                    };
                    if (elements.TryGetValue(new GVCellFace(cellFace.X, cellFace.Y, cellFace.Z, blockFace), out GVElectricElement element)) {
                        Vector3 forward = CellFace.FaceToVector3(cellFace.Face);
                        Vector3 position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) + 0.55f * forward;
                        Vector3 up = cellFace.Face < 4 ? Vector3.UnitY : Vector3.UnitX;
                        Vector3 right = Vector3.Cross(forward, up);
                        const float size = 0.1f;
                        SubsystemGV8NumberLedGlow.Draw8Number(
                            m_8NumberBatch,
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