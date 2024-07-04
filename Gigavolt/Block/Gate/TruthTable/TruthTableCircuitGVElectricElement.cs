using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public class TruthTableCircuitGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemGVTruthTableCircuitBlockBehavior m_subsystemTruthTableCircuitBlockBehavior;

        public readonly GVTruthTableData m_data;
        public uint m_voltage;
        public List<GVTruthTableData.SectionInput> lastInputs = new() { Capacity = 20 };
        public bool m_dataChanged;

        public TruthTableCircuitGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystemTruthTableCircuitBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVTruthTableCircuitBlockBehavior>(true);
            m_data = m_subsystemTruthTableCircuitBlockBehavior.GetItemData(m_subsystemTruthTableCircuitBlockBehavior.GetIdFromValue(value));
        }

        public override uint GetOutputVoltage(int face) {
            if (face == 123456) {
                m_dataChanged = true;
            }
            return m_voltage;
        }

        public override bool Simulate() {
            if (m_data == null) {
                return false;
            }
            uint voltage = m_voltage;
            int rotation = Rotation;
            GVTruthTableData.SectionInput sectionInput = new();
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Top) {
                            sectionInput.i1 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Right) {
                            sectionInput.i2 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                            sectionInput.i3 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left) {
                            sectionInput.i4 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                    }
                }
            }
            try {
                if (lastInputs.Count == 0
                    || !sectionInput.Equals(lastInputs[^1])) {
                    lastInputs.Add(sectionInput);
                    if (lastInputs.Count > 16) {
                        lastInputs = lastInputs.GetRange(lastInputs.Count - 16, 16);
                    }
                    m_voltage = m_data.Exe(lastInputs);
                }
                else if (m_dataChanged) {
                    m_dataChanged = false;
                    m_voltage = m_data.Exe(lastInputs);
                }
            }
            catch (Exception e) {
                Log.Error(e);
            }
            return m_voltage != voltage;
        }
    }
}