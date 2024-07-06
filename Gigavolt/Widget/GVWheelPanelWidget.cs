using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVWheelPanelWidget : CanvasWidget {
        public const float FirstRingDiameter = 108f;
        public const float RingSpacing = 72f;

        public int m_ringCount;
        public GVBlockIconWidget m_lastFocusedWidget;
        public Vector2 m_dragPosition;
        public InventoryDragData m_inventoryDragData;
        public readonly ComponentGui m_componentGui;

        public int CenterBlockValue {
            get => CenterBlockWidget.Value;
            set {
                value = Terrain.ReplaceLight(value, 15);
                if (value != CenterBlockWidget.Value) {
                    CenterBlockWidget.IsVisible = value != 0;
                    CenterBlockWidget.Value = value;
                    SetPosition(CenterBlockWidget, new Vector2(m_ringCount * RingSpacing + (FirstRingDiameter - CenterBlockWidget.Size.X) / 2, m_ringCount * RingSpacing + (FirstRingDiameter - CenterBlockWidget.FullHeight) * 0.5f - CenterBlockWidget.NameLabelMarginY));
                }
            }
        }

        public readonly GVBlockIconWidget CenterBlockWidget = new() { AlwaysInFocus = true, Size = new Vector2(60f), IsVisible = false };
        public List<int> m_outerBlocksValue;

        public List<int> OuterBlocksValue {
            get => m_outerBlocksValue;
            set {
                value = value.Select(x => Terrain.ReplaceLight(x, 15)).ToList();
                value.Remove(CenterBlockValue);
                m_ringCount = value.Count > 8 ? 2 : 1;
                Size = new Vector2(m_ringCount * 2 * RingSpacing + FirstRingDiameter);
                m_outerBlocksValue = value;
                foreach (GVBlockIconWidget widget in OuterBlocksWidgets) {
                    RemoveChildren(widget);
                }
                OuterBlocksWidgets.Clear();
                int firstLevelCount = Math.Min(value.Count >= 28 ? 12 : 8, value.Count);
                int secondLevelCount = Math.Max(value.Count - firstLevelCount, firstLevelCount);
                float center = RingSpacing * m_ringCount + FirstRingDiameter / 2f;
                const float firstLevelRadius = (RingSpacing + FirstRingDiameter) / 2f;
                const float secondLevelRadius = RingSpacing * 1.5f + FirstRingDiameter / 2f;
                for (int i = 0; i < Math.Min(value.Count, 36); i++) {
                    int blockValue = value[i];
                    GVBlockIconWidget widget = new() { Value = blockValue, SubsystemTerrain = SubsystemTerrain, Size = new Vector2(48f) };
                    OuterBlocksWidgets.Add(widget);
                    AddChildren(widget);
                    if (i < firstLevelCount) {
                        SetPosition(widget, new Vector2(center + MathF.Cos(2 * MathF.PI * i / firstLevelCount) * firstLevelRadius - widget.Size.X / 2, center - MathF.Sin(2 * MathF.PI * i / firstLevelCount) * firstLevelRadius - CenterBlockWidget.FullHeight * 0.5f - CenterBlockWidget.NameLabelMarginY));
                    }
                    else {
                        SetPosition(widget, new Vector2(center + MathF.Cos(2 * MathF.PI * i / secondLevelCount) * secondLevelRadius - widget.Size.X / 2, center - MathF.Sin(2 * MathF.PI * i / secondLevelCount) * secondLevelRadius - CenterBlockWidget.FullHeight * 0.5f - CenterBlockWidget.NameLabelMarginY));
                    }
                }
                SetPosition(CenterBlockWidget, new Vector2(m_ringCount * RingSpacing + (FirstRingDiameter - CenterBlockWidget.Size.X) / 2, m_ringCount * RingSpacing + (FirstRingDiameter - CenterBlockWidget.FullHeight) * 0.5f - CenterBlockWidget.NameLabelMarginY));
            }
        }

        public readonly List<GVBlockIconWidget> OuterBlocksWidgets = [];
        public SubsystemTerrain m_subsystemTerrain;

        public SubsystemTerrain SubsystemTerrain {
            get => m_subsystemTerrain;
            set {
                m_subsystemTerrain = value;
                CenterBlockWidget.SubsystemTerrain = value;
                foreach (GVBlockIconWidget blockIconWidget in OuterBlocksWidgets) {
                    blockIconWidget.SubsystemTerrain = value;
                }
            }
        }

        public GVWheelPanelWidget(ComponentGui componentGui) {
            IsDrawRequired = true;
            AddChildren(CenterBlockWidget);
            m_componentGui = componentGui;
        }

        public override void Update() {
            if (Input.Drag == null) {
                IsVisible = false;
                if (m_lastFocusedWidget != null) {
                    int oldValue = Terrain.ReplaceLight(m_inventoryDragData.Inventory.GetSlotValue(m_inventoryDragData.SlotIndex), 0);
                    int newValue = Terrain.ReplaceLight(m_lastFocusedWidget.Value, 0);
                    if (newValue > 0
                        && newValue != oldValue) {
                        if (m_inventoryDragData.DragMode == DragMode.AllItems) {
                            m_inventoryDragData.Inventory.AddSlotItems(m_inventoryDragData.SlotIndex, newValue, m_inventoryDragData.Inventory.RemoveSlotItems(m_inventoryDragData.SlotIndex, int.MaxValue));
                            AudioManager.PlaySound("Audio/UI/ItemMoved", 1f, 0f, 0f);
                        }
                        else {
                            int oldCount = m_inventoryDragData.Inventory.GetSlotCount(m_inventoryDragData.SlotIndex);
                            if (oldCount == 1
                                || oldCount == 9999) {
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
                    m_lastFocusedWidget = null;
                }
            }
            else {
                m_dragPosition = Input.Drag.Value;
                m_dragPosition.X = Math.Clamp(m_dragPosition.X, GlobalBounds.Min.X, GlobalBounds.Max.X - 1f);
                m_dragPosition.Y = Math.Clamp(m_dragPosition.Y, GlobalBounds.Min.Y, GlobalBounds.Max.Y - 1f);
                GVBlockIconWidget newFocusedWidget = HitTestGlobal(m_dragPosition, widget => widget is GVBlockIconWidget) as GVBlockIconWidget;
                if (newFocusedWidget != m_lastFocusedWidget) {
                    if (newFocusedWidget != null) {
                        newFocusedWidget.HasFocus = true;
                        AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    }
                    if (m_lastFocusedWidget != null) {
                        m_lastFocusedWidget.HasFocus = false;
                    }
                    m_lastFocusedWidget = newFocusedWidget;
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

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            if (CenterBlockValue > 0) {
                CenterBlockWidget.IsVisible = true;
            }
            else {
                CenterBlockWidget.IsVisible = false;
            }
            base.MeasureOverride(parentAvailableSize);
        }
    }
}