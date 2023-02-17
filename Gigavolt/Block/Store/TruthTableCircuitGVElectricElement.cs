using System;
using System.Collections.Generic;

namespace Game
{
    public class TruthTableCircuitGVElectricElement : RotateableGVElectricElement
    {
        public SubsystemGVTruthTableCircuitBlockBehavior m_subsystemTruthTableCircuitBlockBehavior;

        public uint m_voltage;
        public List<GVTruthTableData.SectionInput> lastInputs = new List<GVTruthTableData.SectionInput>() { Capacity=20};

        public TruthTableCircuitGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
            m_subsystemTruthTableCircuitBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVTruthTableCircuitBlockBehavior>(throwOnError: true);
        }

        public override uint GetOutputVoltage(int face)
        {
            return m_voltage;
        }

        public override bool Simulate()
        {
            uint voltage = m_voltage;
            int rotation = Rotation;
            GVTruthTableData.SectionInput sectionInput = new GVTruthTableData.SectionInput();
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue)
                    {
                        if (connectorDirection == GVElectricConnectorDirection.Top)
                        {
                            sectionInput.i1 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Right)
                        {
                            sectionInput.i2 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Bottom)
                        {
                            sectionInput.i3 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left)
                        {
                            sectionInput.i4 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                    }
                }
            }
            try
            {
                if (lastInputs.Count==0||sectionInput != lastInputs[lastInputs.Count - 1])
                {
                    lastInputs.Add(sectionInput);
                    if (lastInputs.Count > 16)
                    {
                        lastInputs = lastInputs.GetRange(lastInputs.Count - 16, 16);
                    }
                    GVTruthTableData blockData = m_subsystemTruthTableCircuitBlockBehavior.GetBlockData(CellFaces[0].Point);
                    m_voltage = (blockData != null) ? (blockData.Exe(lastInputs)) : 0u;
                }
            }catch (Exception e) { Engine.Log.Error(e); }
            return m_voltage != voltage;
        }
    }
}
