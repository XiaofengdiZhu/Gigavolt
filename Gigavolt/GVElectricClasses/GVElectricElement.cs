using System.Collections.Generic;
using System.Linq;
using Engine;

namespace Game {
    public abstract class GVElectricElement {
        public SubsystemGVElectricity SubsystemGVElectricity { get; set; }

        public ReadOnlyList<GVCellFace> CellFaces { get; set; }

        public List<GVElectricConnection> Connections { get; set; }

        public GVElectricElement(SubsystemGVElectricity subsystemGVElectric, IEnumerable<GVCellFace> cellFaces) {
            SubsystemGVElectricity = subsystemGVElectric;
            CellFaces = new ReadOnlyList<GVCellFace>(new List<GVCellFace>(cellFaces));
            Connections = new List<GVElectricConnection>();
        }

        public GVElectricElement(SubsystemGVElectricity subsystemGVElectric, GVCellFace cellFace) : this(subsystemGVElectric, new List<GVCellFace> { cellFace }) { }

        public GVElectricElement(SubsystemGVElectricity subsystemGVElectric, IEnumerable<CellFace> cellFaces) {
            SubsystemGVElectricity = subsystemGVElectric;
            CellFaces = new ReadOnlyList<GVCellFace>(cellFaces.Select(cellface => new GVCellFace(cellface)).ToList());
            Connections = new List<GVElectricConnection>();
        }

        public GVElectricElement(SubsystemGVElectricity subsystemGVElectric, CellFace cellFace) : this(subsystemGVElectric, new List<CellFace> { cellFace }) { }

        public virtual uint GetOutputVoltage(int face) => 0u;

        public virtual bool Simulate() => false;

        public virtual void OnAdded() { }

        public virtual void OnRemoved() { }

        public virtual void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ) { }

        public virtual bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) => false;

        public virtual void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody) { }

        public virtual void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) { }

        public virtual void OnConnectionsChanged() { }

        public static bool IsSignalHigh(uint voltage) => voltage >= 8u;

        public int CalculateHighInputsCount() {
            int num = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0
                    && IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace))) {
                    num++;
                }
            }
            return num;
        }

        public uint CalculateAllInputsVoltage() {
            uint num = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    num |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            return num;
        }
    }
}