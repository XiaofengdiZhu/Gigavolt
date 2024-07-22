using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using GameEntitySystem;

namespace Game {
    public class GVWheelPanelWidget : CanvasWidget {
        public const float FirstRingDiameter = 108f;
        public const float RingSpacing = 72f;

        public int m_ringCount;
        public GVBlockIconWidget m_lastFocusedBlockIconWidget;
        public GVBlockHelperWidget m_lastFocusedBlockHelperWidget;
        public Vector2 m_dragPosition;
        public InventoryDragData m_inventoryDragData;
        public readonly Project m_project;
        public readonly ComponentGui m_componentGui;

        public int CenterBlockValue {
            get => CenterBlockWidget.Value;
            set {
                value = Terrain.ReplaceLight(value, 15);
                if (value != CenterBlockWidget.Value) {
                    CenterBlockWidget.Value = value;
                    int noLightValue = Terrain.ReplaceLight(value, 0);
                    RecipesWidget.RecipesCount = CraftingRecipesManager.Recipes.Count(r => r.ResultValue == noLightValue);
                    AdjustFixedWidgets();
                }
            }
        }

        public readonly GVBlockIconWidget CenterBlockWidget = new() { AlwaysInFocus = true, Size = new Vector2(60f) };
        public List<int> m_outerBlocksValue;

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
                m_ringCount = blocksValue.Count > 8 ? 2 : 1;
                Size = new Vector2(m_ringCount * 2 * RingSpacing + FirstRingDiameter);
                m_outerBlocksValue = blocksValue;
                foreach (GVBlockIconWidget widget in OuterBlocksWidgets) {
                    RemoveChildren(widget);
                }
                OuterBlocksWidgets.Clear();
                int firstLevelCount = Math.Min(blocksValue.Count >= 28 ? 12 : 8, blocksValue.Count);
                int secondLevelCount = Math.Max(blocksValue.Count - firstLevelCount, firstLevelCount);
                float center = RingSpacing * m_ringCount + FirstRingDiameter / 2f;
                const float firstLevelRadius = (RingSpacing + FirstRingDiameter) / 2f;
                const float secondLevelRadius = RingSpacing * 1.5f + FirstRingDiameter / 2f;
                for (int i = 0; i < Math.Min(blocksValue.Count, 36); i++) {
                    int blockValue = blocksValue[i];
                    GVBlockIconWidget widget = new() { Value = blockValue, SubsystemTerrain = SubsystemTerrain, Size = new Vector2(48f) };
                    OuterBlocksWidgets.Add(widget);
                    AddChildren(widget);
                    if (i < firstLevelCount) {
                        SetPosition(widget, new Vector2(center + MathF.Cos(2 * MathF.PI * i / firstLevelCount) * firstLevelRadius - widget.Size.X / 2, center - MathF.Sin(2 * MathF.PI * i / firstLevelCount) * firstLevelRadius - CenterBlockWidget.FullHeight * 0.5f - CenterBlockWidget.NameLabelMarginY));
                    }
                    else {
                        int i2 = i - firstLevelCount;
                        SetPosition(widget, new Vector2(center + MathF.Cos(2 * MathF.PI * i2 / secondLevelCount) * secondLevelRadius - widget.Size.X / 2, center - MathF.Sin(2 * MathF.PI * i2 / secondLevelCount) * secondLevelRadius - CenterBlockWidget.FullHeight * 0.5f - CenterBlockWidget.NameLabelMarginY));
                    }
                }
                AdjustFixedWidgets();
            }
        }

        public readonly List<GVBlockIconWidget> OuterBlocksWidgets = [];
        public readonly GVBlockHelperWidget DescriptionWidget = new() { Mode = GVBlockHelperWidget.DisplayMode.Description };
        public readonly GVBlockHelperWidget RecipesWidget = new() { Mode = GVBlockHelperWidget.DisplayMode.Recipes, RecipesCount = 0 };
        public readonly GVBlockHelperWidget DuplicateWidget = new() { Mode = GVBlockHelperWidget.DisplayMode.Duplicate };
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
            m_project = project;
            m_componentGui = componentGui;
        }

        public override void Update() {
            if (Input.Drag == null) {
                IsVisible = false;
                if (m_lastFocusedBlockIconWidget != null) {
                    m_lastFocusedBlockIconWidget.HasFocus = false;
                    int oldValue = Terrain.ReplaceLight(m_inventoryDragData.Inventory.GetSlotValue(m_inventoryDragData.SlotIndex), 0);
                    int newValue = Terrain.ReplaceLight(m_lastFocusedBlockIconWidget.Value, 0);
                    if (newValue > 0
                        && newValue != oldValue) {
                        if (m_inventoryDragData.DragMode == DragMode.AllItems) {
                            m_inventoryDragData.Inventory.AddSlotItems(m_inventoryDragData.SlotIndex, newValue, m_inventoryDragData.Inventory.RemoveSlotItems(m_inventoryDragData.SlotIndex, int.MaxValue));
                            AudioManager.PlaySound("Audio/UI/ItemMoved", 1f, 0f, 0f);
                        }
                        else {
                            int oldCount = m_inventoryDragData.Inventory.GetSlotCount(m_inventoryDragData.SlotIndex);
                            if (oldCount is 1 or 9999) {
                                m_inventoryDragData.Inventory.RemoveSlotItems(m_inventoryDragData.SlotIndex, int.MaxValue);
                                m_inventoryDragData.Inventory.AddSlotItems(m_inventoryDragData.SlotIndex, newValue, 1);
                                AudioManager.PlaySound("Audio/UI/ItemMoved", 1f, 0f, 0f);
                            }
                            else {
                                if (ComponentInventoryBase.FindAcquireSlotForItem(m_inventoryDragData.Inventory, oldValue) >= 0) {
                                    int count = m_inventoryDragData.Inventory.RemoveSlotItems(m_inventoryDragData.SlotIndex, int.MaxValue);
                                    m_inventoryDragData.Inventory.AddSlotItems(m_inventoryDragData.SlotIndex, newValue, 1);
                                    ComponentInventoryBase.AcquireItems(m_inventoryDragData.Inventory, oldValue, count - 1);
                                    AudioManager.PlaySound("Audio/UI/ItemMoved", 1f, 0f, 0f);
                                }
                                else {
                                    m_componentGui.DisplaySmallMessage(LanguageControl.Get(GetType().Name, "1"), Color.White, false, false);
                                }
                            }
                        }
                    }
                    m_lastFocusedBlockIconWidget = null;
                }
                if (m_lastFocusedBlockHelperWidget != null) {
                    m_lastFocusedBlockHelperWidget.HasFocus = false;
                    int value = Terrain.ReplaceLight(CenterBlockValue, 0);
                    switch (m_lastFocusedBlockHelperWidget.Mode) {
                        case GVBlockHelperWidget.DisplayMode.Recipes:
                            ScreensManager.SwitchScreen("RecipaediaRecipes", value);
                            AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                            break;
                        case GVBlockHelperWidget.DisplayMode.Description:
                            ScreensManager.SwitchScreen("RecipaediaDescription", [value, new List<int> { value }]);
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
                if (newFocusedBlockIconWidget != m_lastFocusedBlockIconWidget) {
                    if (newFocusedBlockIconWidget != null) {
                        newFocusedBlockIconWidget.HasFocus = true;
                        AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    }
                    if (m_lastFocusedBlockIconWidget != null) {
                        m_lastFocusedBlockIconWidget.HasFocus = false;
                    }
                    m_lastFocusedBlockIconWidget = newFocusedBlockIconWidget;
                    m_lastFocusedBlockHelperWidget = null;
                }
                GVBlockHelperWidget newFocusedBlockHelperWidget = hitTestWidget as GVBlockHelperWidget;
                if (newFocusedBlockHelperWidget != m_lastFocusedBlockHelperWidget) {
                    if (newFocusedBlockHelperWidget != null) {
                        newFocusedBlockHelperWidget.HasFocus = true;
                    }
                    if (m_lastFocusedBlockHelperWidget != null) {
                        m_lastFocusedBlockHelperWidget.HasFocus = false;
                    }
                    m_lastFocusedBlockHelperWidget = newFocusedBlockHelperWidget;
                    m_lastFocusedBlockIconWidget = null;
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
                case 1:
                    SetPosition(DescriptionWidget, new Vector2(-36f, Size.Y - DescriptionWidget.Size.Y + 28f));
                    SetPosition(RecipesWidget, Size - RecipesWidget.Size + new Vector2(36f, 28f));
                    SetPosition(DuplicateWidget, new Vector2(-36f, -28f));
                    break;
                case 2:
                    SetPosition(DescriptionWidget, new Vector2(-16f, Size.Y - DescriptionWidget.Size.Y + 8f));
                    SetPosition(RecipesWidget, Size - RecipesWidget.Size + new Vector2(16f, 8f));
                    SetPosition(DuplicateWidget, new Vector2(-16f, -8f));
                    break;
            }
        }
    }
}