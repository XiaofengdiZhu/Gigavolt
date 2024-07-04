using System.Xml.Linq;
using Engine;

namespace Game {
    public class GVDispenserWidget : CanvasWidget {
        public readonly SubsystemTerrain m_subsystemTerrain;

        public readonly ComponentGVDispenser m_componentDispenser;

        public readonly ComponentBlockEntity m_componentBlockEntity;

        public readonly GridPanelWidget m_inventoryGrid;

        public readonly GridPanelWidget m_dispenserGrid;

        public readonly ButtonWidget m_dispenseButton;

        public readonly ButtonWidget m_shootButton;

        public readonly CheckboxWidget m_acceptsDropsBox;

        public GVDispenserWidget(IInventory inventory, ComponentGVDispenser componentDispenser) {
            m_componentDispenser = componentDispenser;
            m_componentBlockEntity = componentDispenser.Entity.FindComponent<ComponentBlockEntity>(true);
            m_subsystemTerrain = componentDispenser.Project.FindSubsystem<SubsystemTerrain>(true);
            XElement node = ContentManager.Get<XElement>("Widgets/GVDispenserWidget");
            LoadContents(this, node);
            m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
            m_dispenserGrid = Children.Find<GridPanelWidget>("DispenserGrid");
            m_dispenseButton = Children.Find<ButtonWidget>("DispenseButton");
            m_shootButton = Children.Find<ButtonWidget>("ShootButton");
            m_acceptsDropsBox = Children.Find<CheckboxWidget>("AcceptsDropsBox");
            int num = 0;
            for (int i = 0; i < m_dispenserGrid.RowsCount; i++) {
                for (int j = 0; j < m_dispenserGrid.ColumnsCount; j++) {
                    InventorySlotWidget inventorySlotWidget = new();
                    inventorySlotWidget.AssignInventorySlot(componentDispenser, num++);
                    m_dispenserGrid.Children.Add(inventorySlotWidget);
                    m_dispenserGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
                }
            }
            num = 10;
            for (int k = 0; k < m_inventoryGrid.RowsCount; k++) {
                for (int l = 0; l < m_inventoryGrid.ColumnsCount; l++) {
                    InventorySlotWidget inventorySlotWidget2 = new();
                    inventorySlotWidget2.AssignInventorySlot(inventory, num++);
                    m_inventoryGrid.Children.Add(inventorySlotWidget2);
                    m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
                }
            }
        }

        public override void Update() {
            int value = m_subsystemTerrain.Terrain.GetCellValue(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z);
            int data = Terrain.ExtractData(value);
            if (m_dispenseButton.IsClicked) {
                data = GVDispenserBlock.SetMode(data, GVDispenserBlock.Mode.Dispense);
                value = Terrain.ReplaceData(value, data);
                m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, value);
            }
            if (m_shootButton.IsClicked) {
                data = GVDispenserBlock.SetMode(data, GVDispenserBlock.Mode.Shoot);
                value = Terrain.ReplaceData(value, data);
                m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, value);
            }
            if (m_acceptsDropsBox.IsClicked) {
                data = GVDispenserBlock.SetAcceptsDrops(data, !GVDispenserBlock.GetAcceptsDrops(data));
                value = Terrain.ReplaceData(value, data);
                m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, value);
            }
            GVDispenserBlock.Mode mode = GVDispenserBlock.GetMode(data);
            m_dispenseButton.IsChecked = mode == GVDispenserBlock.Mode.Dispense;
            m_shootButton.IsChecked = mode == GVDispenserBlock.Mode.Shoot;
            m_acceptsDropsBox.IsChecked = GVDispenserBlock.GetAcceptsDrops(data);
            if (!m_componentDispenser.IsAddedToProject) {
                ParentWidget.Children.Remove(this);
            }
        }
    }
}