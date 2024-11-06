using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using GameEntitySystem;

namespace Game {
    public class GVWheelPanelWidget : CanvasWidget {
        public class GVWheelPanelInventory : IInventory {
            public int Value;
            public int Count;
            public int GetSlotValue(int slotIndex) => Value;

            public int GetSlotCount(int slotIndex) => Count;

            public int GetSlotCapacity(int slotIndex, int value) {
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
                return block.IsNonDuplicable_(value) ? 1 : block.GetMaxStacking(value);
            }

            public int GetSlotProcessCapacity(int slotIndex, int value) {
                if (Count > 0
                    && Value != 0) {
                    SubsystemBlockBehavior[] blockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(true).GetBlockBehaviors(Terrain.ExtractContents(Value));
                    for (int i = 0; i < blockBehaviors.Length; i++) {
                        int processInventoryItemCapacity = blockBehaviors[i].GetProcessInventoryItemCapacity(this, slotIndex, value);
                        if (processInventoryItemCapacity > 0) {
                            return processInventoryItemCapacity;
                        }
                    }
                }
                return 0;
            }

            public void AddSlotItems(int slotIndex, int value, int count) {
                if (value == Value) {
                    Count = Math.Min(GetSlotCapacity(0, value), Count + count);
                }
                else {
                    Value = value;
                    Count = count;
                }
            }

            public void ProcessSlotItems(int slotIndex, int value, int count, int processCount, out int processedValue, out int processedCount) {
                if (Count > 0
                    && Value != 0) {
                    foreach (SubsystemBlockBehavior subsystemBlockBehavior in Project.FindSubsystem<SubsystemBlockBehaviors>(true).GetBlockBehaviors(Terrain.ExtractContents(Value))) {
                        int processInventoryItemCapacity = subsystemBlockBehavior.GetProcessInventoryItemCapacity(this, slotIndex, value);
                        if (processInventoryItemCapacity > 0) {
                            subsystemBlockBehavior.ProcessInventoryItem(
                                this,
                                slotIndex,
                                value,
                                count,
                                MathUtils.Min(processInventoryItemCapacity, processCount),
                                out processedValue,
                                out processedCount
                            );
                            return;
                        }
                    }
                }
                processedValue = value;
                processedCount = count;
            }

            public int RemoveSlotItems(int slotIndex, int count) {
                int result = Math.Min(count, Count);
                Count -= result;
                return result;
            }

            public void DropAllItems(Vector3 position) {
                Value = 0;
                Count = 0;
            }

            public Project m_project;
            public Project Project => m_project;
            public int SlotsCount => 1;

            public int VisibleSlotsCount {
                get => 1;
                set { }
            }

            public int ActiveSlotIndex {
                get => 0;
                set { }
            }
        }

        public const float FirstRingDiameter = 108f;
        public const float RingSpacing = 72f;

        public int m_ringCount;
        public GVBlockIconWidget m_lastFocusedBlockIconWidget;
        public GVBlockHelperWidget m_lastFocusedBlockHelperWidget;
        public Vector2 m_dragPosition;
        public InventoryDragData m_originalInventoryDragData;
        public InventoryDragData m_inventoryDragData;
        public bool m_hide;
        public readonly Project m_project;
        public readonly ComponentGui m_componentGui;
        public readonly GVWheelPanelInventory m_inventory;

        public int CenterBlockValue {
            get => CenterBlockWidget.Value;
            set {
                value = Terrain.ReplaceLight(value, 15);
                if (value != CenterBlockWidget.Value) {
                    CenterBlockWidget.Value = value;
                    int noLightValue = Terrain.ReplaceLight(value, 0);
                    RecipesWidget.RecipesCount = CraftingRecipesManager.Recipes.Count(r => r.ResultValue == noLightValue);
                    m_inventory.Value = noLightValue;
                    m_inventory.Count = 1;
                    AdjustFixedWidgets();
                }
            }
        }

        public readonly GVBlockIconWidget CenterBlockWidget = new() { AlwaysInFocus = true, Size = new Vector2(60f) };
        public List<int> m_outerBlocksValue;
        public int m_firstLevelCount;
        public int m_secondLevelCount;

        public List<int> OuterBlocksValue {
            get => m_outerBlocksValue;
            set {
                List<int> blocksValue = [];
                foreach (int blockValue in value) {
                    int newBlockValue = Terrain.ReplaceLight(blockValue, 15);
                    if (newBlockValue != CenterBlockValue) {
                        blocksValue.Add(newBlockValue);
                    }
                }
                m_ringCount = blocksValue.Count switch {
                    0 => 0,
                    <= 8 => 1,
                    _ => 2
                };
                Size = new Vector2(m_ringCount * 2 * RingSpacing + FirstRingDiameter);
                m_outerBlocksValue = blocksValue;
                foreach (GVBlockIconWidget widget in OuterBlocksWidgets) {
                    RemoveChildren(widget);
                }
                OuterBlocksWidgets.Clear();
                m_firstLevelCount = m_ringCount > 0 ? Math.Min(blocksValue.Count >= 28 ? 12 : 8, blocksValue.Count) : 0;
                m_secondLevelCount = m_ringCount > 1 ? Math.Min(Math.Max(blocksValue.Count - m_firstLevelCount, m_firstLevelCount), 36 - m_firstLevelCount) : 0;
                if (m_secondLevelCount is > 8 and < 24) {
                    m_secondLevelCount++;
                }
                float center = RingSpacing * m_ringCount + FirstRingDiameter / 2f;
                const float firstLevelRadius = (RingSpacing + FirstRingDiameter) / 2f;
                const float secondLevelRadius = RingSpacing * 1.5f + FirstRingDiameter / 2f;
                for (int i = 0; i < Math.Min(blocksValue.Count, 35); i++) {
                    int blockValue = blocksValue[i];
                    GVBlockIconWidget widget = new() { Value = blockValue, SubsystemTerrain = SubsystemTerrain, Size = new Vector2(48f) };
                    OuterBlocksWidgets.Add(widget);
                    AddChildren(widget);
                    if (i < m_firstLevelCount) {
                        SetPosition(widget, new Vector2(center + MathF.Cos(2 * MathF.PI * i / m_firstLevelCount) * firstLevelRadius - widget.Size.X / 2, center - MathF.Sin(2 * MathF.PI * i / m_firstLevelCount) * firstLevelRadius - CenterBlockWidget.FullHeight * 0.5f - CenterBlockWidget.NameLabelMarginY));
                    }
                    else {
                        int i2 = i - m_firstLevelCount;
                        SetPosition(widget, new Vector2(center + MathF.Cos(2 * MathF.PI * i2 / m_secondLevelCount) * secondLevelRadius - widget.Size.X / 2, center - MathF.Sin(2 * MathF.PI * i2 / m_secondLevelCount) * secondLevelRadius - CenterBlockWidget.FullHeight * 0.5f - CenterBlockWidget.NameLabelMarginY));
                    }
                }
                AdjustFixedWidgets();
            }
        }

        public readonly List<GVBlockIconWidget> OuterBlocksWidgets = [];
        public readonly GVBlockHelperWidget DescriptionWidget = new() { Mode = GVBlockHelperWidget.DisplayMode.Description };
        public readonly GVBlockHelperWidget RecipesWidget = new() { Mode = GVBlockHelperWidget.DisplayMode.Recipes, RecipesCount = 0 };
        public readonly GVBlockHelperWidget DuplicateWidget = new() { Mode = GVBlockHelperWidget.DisplayMode.Duplicate };
        public readonly GVBlockHelperWidget CancelWidget = new() { Mode = GVBlockHelperWidget.DisplayMode.Cancel };
        public SubsystemTerrain m_subsystemTerrain;

        public SubsystemTerrain SubsystemTerrain {
            get => m_subsystemTerrain;
            init {
                m_subsystemTerrain = value;
                CenterBlockWidget.SubsystemTerrain = value;
                foreach (GVBlockIconWidget blockIconWidget in OuterBlocksWidgets) {
                    blockIconWidget.SubsystemTerrain = value;
                }
            }
        }

        public GVWheelPanelWidget(Project project, ComponentGui componentGui) {
            IsDrawRequired = true;
            AddChildren(CenterBlockWidget);
            AddChildren(DescriptionWidget);
            AddChildren(RecipesWidget);
            AddChildren(DuplicateWidget);
            AddChildren(CancelWidget);
            m_inventory = new GVWheelPanelInventory { m_project = project };
            m_inventoryDragData = new InventoryDragData { DragMode = DragMode.AllItems, Inventory = m_inventory, SlotIndex = 0 };
            m_project = project;
            m_componentGui = componentGui;
        }

        public override void Update() {
            DragHostWidget dragHostWidget = m_componentGui.m_componentPlayer.DragHostWidget;
            if (Input.Drag == null) {
                IsVisible = false;
                if (m_lastFocusedBlockHelperWidget != null) {
                    m_lastFocusedBlockHelperWidget.HasFocus = false;
                    dragHostWidget.m_dragWidget.IsVisible = false;
                    dragHostWidget.m_dragWidget = null;
                    dragHostWidget.m_dragData = null;
                    dragHostWidget.m_dragEndedHandler = null;
                    int value = Terrain.ReplaceLight(CenterBlockValue, 0);
                    switch (m_lastFocusedBlockHelperWidget.Mode) {
                        case GVBlockHelperWidget.DisplayMode.Recipes:
                            if (m_lastFocusedBlockHelperWidget.RecipesCount > 0) {
                                ScreensManager.SwitchScreen("RecipaediaRecipes", value);
                                AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                            }
                            break;
                        case GVBlockHelperWidget.DisplayMode.Description:
                            ScreensManager.SwitchScreen("RecipaediaDescription", value, new List<int> { value });
                            AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                            break;
                        case GVBlockHelperWidget.DisplayMode.Duplicate:
                            int content = Terrain.ExtractContents(value);
                            int newValue;
                            if (BlocksManager.Blocks[content] is IGVCustomWheelPanelBlock block) {
                                newValue = block.GetCustomCopyBlock(m_project, value);
                            }
                            else {
                                newValue = value;
                            }
                            GameWidget gameWidget = m_componentGui.m_componentPlayer.PlayerData.GameWidget;
                            Vector3 vector2 = Vector3.Normalize(gameWidget.ActiveCamera.ScreenToWorld(new Vector3(m_dragPosition.X, m_dragPosition.Y, 1f), Matrix.Identity) - gameWidget.ActiveCamera.ViewPosition) * 4f;
                            m_project.FindSubsystem<SubsystemPickables>(true)
                            .AddPickable(
                                newValue,
                                1,
                                gameWidget.ActiveCamera.ViewPosition,
                                vector2,
                                null
                            );
                            AudioManager.PlaySound("Audio/UI/ItemMoved", 1f, 0f, 0f);
                            break;
                    }
                    m_lastFocusedBlockHelperWidget = null;
                }
            }
            else {
                m_dragPosition = Input.Drag.Value;
                Widget hitTestWidget = HitTestGlobal(m_dragPosition, widget => widget is GVBlockIconWidget or GVBlockHelperWidget);
                GVBlockIconWidget newFocusedBlockIconWidget = hitTestWidget as GVBlockIconWidget;
                if (newFocusedBlockIconWidget != m_lastFocusedBlockIconWidget
                    && newFocusedBlockIconWidget != null) {
                    if (m_lastFocusedBlockIconWidget != null) {
                        m_lastFocusedBlockIconWidget.HasFocus = false;
                    }
                    m_lastFocusedBlockIconWidget = newFocusedBlockIconWidget;
                    newFocusedBlockIconWidget.HasFocus = true;
                    if (newFocusedBlockIconWidget == CenterBlockWidget) {
                        dragHostWidget.m_dragData = m_originalInventoryDragData;
                    }
                    else {
                        int noLightValue = Terrain.ReplaceLight(newFocusedBlockIconWidget.Value, 0);
                        if (m_inventory.Count > 1) {
                            m_inventory.Count = Math.Min(m_inventory.Count, m_inventory.GetSlotCapacity(0, noLightValue));
                        }
                        m_inventory.Value = noLightValue;
                        dragHostWidget.m_dragData = m_inventoryDragData;
                        if (m_secondLevelCount > 8) {
                            int index = OuterBlocksWidgets.IndexOf(newFocusedBlockIconWidget);
                            if (index < m_firstLevelCount) {
                                AdjustOuterBlocksWidgets(index);
                            }
                        }
                    }
                    if (dragHostWidget.m_dragWidget is ContainerWidget containerWidget) {
                        containerWidget.Children.Find<BlockIconWidget>("InventoryDragWidget.Icon").Value = newFocusedBlockIconWidget.Value;
                        containerWidget.Children.Find<LabelWidget>("InventoryDragWidget.Name").Text = "";
                    }
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    m_lastFocusedBlockHelperWidget = null;
                    return;
                }
                GVBlockHelperWidget newFocusedBlockHelperWidget = hitTestWidget as GVBlockHelperWidget;
                if (newFocusedBlockHelperWidget != m_lastFocusedBlockHelperWidget) {
                    if (newFocusedBlockHelperWidget != null) {
                        newFocusedBlockHelperWidget.HasFocus = true;
                        dragHostWidget.m_dragData = m_originalInventoryDragData;
                        if (dragHostWidget.m_dragWidget is ContainerWidget containerWidget) {
                            containerWidget.Children.Find<BlockIconWidget>("InventoryDragWidget.Icon").Value = CenterBlockValue;
                            containerWidget.Children.Find<LabelWidget>("InventoryDragWidget.Name").Text = BlocksManager.Blocks[Terrain.ExtractContents(CenterBlockValue)].GetDisplayName(m_subsystemTerrain, CenterBlockValue);
                        }
                        if (newFocusedBlockHelperWidget.Mode == GVBlockHelperWidget.DisplayMode.Cancel) {
                            IsVisible = false;
                            m_hide = true;
                        }
                    }
                    if (m_lastFocusedBlockHelperWidget != null) {
                        m_lastFocusedBlockHelperWidget.HasFocus = false;
                    }
                    m_lastFocusedBlockHelperWidget = newFocusedBlockHelperWidget;
                    if (m_lastFocusedBlockIconWidget != null) {
                        m_lastFocusedBlockIconWidget.HasFocus = false;
                        m_lastFocusedBlockIconWidget = null;
                    }
                    return;
                }
                if (newFocusedBlockIconWidget == null) {
                    if (m_lastFocusedBlockIconWidget != null) {
                        if (HitTestGlobal(m_dragPosition, widget => widget == this) == null) {
                            if (dragHostWidget.m_dragWidget is ContainerWidget containerWidget) {
                                containerWidget.Children.Find<LabelWidget>("InventoryDragWidget.Name").Text = BlocksManager.Blocks[Terrain.ExtractContents(m_lastFocusedBlockIconWidget.Value)].GetDisplayName(m_subsystemTerrain, m_lastFocusedBlockIconWidget.Value);
                            }
                        }
                    }
                }
                int mouseWheelMovement = Input.MouseWheelMovement;
                if (mouseWheelMovement != 0) {
                    if (dragHostWidget.m_dragData == m_originalInventoryDragData) {
                        IInventory inventory = m_originalInventoryDragData.Inventory;
                        if (inventory is ComponentCreativeInventory) {
                            if (mouseWheelMovement > 0) {
                                int value = inventory.GetSlotValue(m_originalInventoryDragData.SlotIndex);
                                if (!BlocksManager.Blocks[Terrain.ExtractContents(value)].IsNonDuplicable_(value)) {
                                    m_inventory.Value = value;
                                    m_inventory.Count = 1;
                                    dragHostWidget.m_dragData = m_inventoryDragData;
                                }
                            }
                        }
                        else {
                            int index = m_originalInventoryDragData.SlotIndex;
                            int value = inventory.GetSlotValue(index);
                            int count = inventory.GetSlotCount(index);
                            int capacity = inventory.GetSlotCapacity(index, value);
                            if (mouseWheelMovement > 0
                                && count < capacity) {
                                inventory.AddSlotItems(index, value, 1);
                                count++;
                            }
                            else if (Input.MouseWheelMovement < 0
                                && count > 1) {
                                inventory.RemoveSlotItems(index, 1);
                                count--;
                            }
                            if (dragHostWidget.m_dragWidget is ContainerWidget containerWidget) {
                                LabelWidget labelWidget = containerWidget.Children.Find<LabelWidget>("InventoryDragWidget.Count");
                                labelWidget.Text = count.ToString();
                            }
                        }
                    }
                    if (dragHostWidget.m_dragData == m_inventoryDragData) {
                        m_inventory.Count = mouseWheelMovement > 0 ? Math.Min(m_inventory.Count + 1, m_inventory.GetSlotCapacity(0, m_inventory.Value)) : Math.Max(m_inventory.Count - 1, 1);
                        if (dragHostWidget.m_dragWidget is ContainerWidget containerWidget) {
                            LabelWidget labelWidget = containerWidget.Children.Find<LabelWidget>("InventoryDragWidget.Count");
                            labelWidget.Text = m_inventory.Count.ToString();
                            labelWidget.IsVisible = m_inventory.Count > 1;
                        }
                    }
                }
            }
        }

        public override void Draw(DrawContext dc) {
            Vector2 center = Vector2.Transform(new Vector2(RingSpacing * m_ringCount + FirstRingDiameter / 2f), GlobalTransform);
            Color color1 = new Color(0, 0, 0, 128) * GlobalColorTransform;
            Color color2 = new Color(0, 0, 0, 96) * GlobalColorTransform;
            Color color3 = new Color(0, 0, 0, 64) * GlobalColorTransform;
            FlatBatch2D flatBatch2D = dc.PrimitivesRenderer2D.FlatBatch(100);
            for (int ring = 0; ring <= m_ringCount; ring++) {
                float radius = (FirstRingDiameter / 2f + RingSpacing * ring) * GlobalTransform.Right.Length();
                flatBatch2D.QueueEllipse(
                    center,
                    new Vector2(radius),
                    0f,
                    color1,
                    64
                );
                flatBatch2D.QueueEllipse(
                    center,
                    new Vector2(radius - 0.5f),
                    0f,
                    color2,
                    64
                );
                flatBatch2D.QueueEllipse(
                    center,
                    new Vector2(radius + 0.5f),
                    0f,
                    color3,
                    64
                );
                if (ring == m_ringCount) {
                    flatBatch2D.QueueDisc(
                        center,
                        new Vector2(radius),
                        0f,
                        color3,
                        64
                    );
                }
            }
            base.Draw(dc);
        }

        public void AdjustFixedWidgets() {
            SetPosition(CenterBlockWidget, new Vector2(m_ringCount * RingSpacing + (FirstRingDiameter - CenterBlockWidget.Size.X) / 2, m_ringCount * RingSpacing + (FirstRingDiameter - CenterBlockWidget.FullHeight) * 0.5f - CenterBlockWidget.NameLabelMarginY));
            switch (m_ringCount) {
                case 0:
                    SetPosition(DescriptionWidget, new Vector2(-56f, Size.Y - DescriptionWidget.Size.Y + 48f));
                    SetPosition(RecipesWidget, Size - RecipesWidget.Size + new Vector2(56f, 48f));
                    SetPosition(DuplicateWidget, new Vector2(-56f, -48f));
                    SetPosition(CancelWidget, new Vector2(Size.X - CancelWidget.Size.X + 56f, -48f));
                    break;
                case 1:
                    SetPosition(DescriptionWidget, new Vector2(-36f, Size.Y - DescriptionWidget.Size.Y + 28f));
                    SetPosition(RecipesWidget, Size - RecipesWidget.Size + new Vector2(36f, 28f));
                    SetPosition(DuplicateWidget, new Vector2(-36f, -28f));
                    SetPosition(CancelWidget, new Vector2(Size.X - CancelWidget.Size.X + 36f, -28f));
                    break;
                case 2:
                    SetPosition(DescriptionWidget, new Vector2(-16f, Size.Y - DescriptionWidget.Size.Y + 8f));
                    SetPosition(RecipesWidget, Size - RecipesWidget.Size + new Vector2(16f, 8f));
                    SetPosition(DuplicateWidget, new Vector2(-16f, -8f));
                    SetPosition(CancelWidget, new Vector2(Size.X - CancelWidget.Size.X + 16f, -8f));
                    break;
            }
        }

        public void AdjustOuterBlocksWidgets(int index) {
            int blocksCount = OuterBlocksValue.Count;
            if (blocksCount <= 8) {
                return;
            }
            float center = RingSpacing * m_ringCount + FirstRingDiameter / 2f;
            const float secondLevelRadius = RingSpacing * 1.5f + FirstRingDiameter / 2f;
            if (m_secondLevelCount > m_firstLevelCount) {
                index = (int)MathF.Round((float)index / m_firstLevelCount * m_secondLevelCount);
            }
            for (int i = 0; i < Math.Min(m_secondLevelCount, blocksCount - m_firstLevelCount); i++) {
                if (i == index) {
                    continue;
                }
                GVBlockIconWidget widget = OuterBlocksWidgets[i + m_firstLevelCount - (i > index ? 1 : 0)];
                SetPosition(widget, new Vector2(center + MathF.Cos(2 * MathF.PI * i / m_secondLevelCount) * secondLevelRadius - widget.Size.X / 2, center - MathF.Sin(2 * MathF.PI * i / m_secondLevelCount) * secondLevelRadius - CenterBlockWidget.FullHeight * 0.5f - CenterBlockWidget.NameLabelMarginY));
            }
        }
    }
}