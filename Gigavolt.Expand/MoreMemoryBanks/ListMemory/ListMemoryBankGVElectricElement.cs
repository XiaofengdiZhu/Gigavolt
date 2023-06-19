using System.Collections.Generic;

namespace Game {
    public class ListMemoryBankGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVListMemoryBankBlockBehavior m_SubsystemGVMemoryBankBlockBehavior;
        public SubsystemGameInfo m_subsystemGameInfo;

        public uint m_voltage;
        public uint m_lastBottomInput;

        public ListMemoryBankGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            m_SubsystemGVMemoryBankBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVListMemoryBankBlockBehavior>(true);
            m_subsystemGameInfo = subsystemGVElectricity.Project.FindSubsystem<SubsystemGameInfo>(true);
            GVListMemoryBankData blockData = m_SubsystemGVMemoryBankBlockBehavior.GetBlockData(cellFace.Point);
            if (blockData != null) {
                m_voltage = blockData.LastOutput;
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            bool bottomConnected = false;
            uint inInput = 0u;
            uint rightInput = 0u;
            uint leftInput = 0u;
            uint bottomInput = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Right:
                                rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Left:
                                leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Bottom: {
                                bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                bottomConnected = true;
                                break;
                            }
                            case GVElectricConnectorDirection.In:
                                inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                        }
                    }
                }
            }
            GVListMemoryBankData memoryBankData = m_SubsystemGVMemoryBankBlockBehavior.GetBlockData(CellFaces[0].Point);
            if (memoryBankData == null) {
                memoryBankData = new GVListMemoryBankData(GVStaticStorage.GetUniqueGVMBID(), m_subsystemGameInfo.DirectoryName, new List<uint>(256));
                m_SubsystemGVMemoryBankBlockBehavior.SetBlockData(CellFaces[0].Point, memoryBankData);
            }
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
                if (!memoryBankData.m_isDataInitialized) {
                    memoryBankData.Data = new List<uint>(256);
                }
            }
            if (bottomConnected) {
                if (bottomInput == 0u
                    || bottomInput > 2u) {
                    m_lastBottomInput = bottomInput;
                    m_voltage = 0u;
                }
                else if (bottomInput != m_lastBottomInput) {
                    m_lastBottomInput = bottomInput;
                    uint smallIndex = MathUint.Min(leftInput, rightInput);
                    uint bigIndex = MathUint.Max(leftInput, rightInput);
                    if (leftInput == 0u) {
                        smallIndex = bigIndex;
                    }
                    switch (bottomInput) {
                        case 1u:
                            m_voltage = memoryBankData.Read(rightInput);
                            break;
                        case 2u:
                            for (uint i = bigIndex; i >= smallIndex; i--) {
                                memoryBankData.Write(i, inInput);
                            }
                            break;
                    }
                }
            }
            else {
                m_voltage = memoryBankData.Read(leftInput);
            }
            memoryBankData.LastOutput = m_voltage;
            return m_voltage != voltage;
        }
    }
}