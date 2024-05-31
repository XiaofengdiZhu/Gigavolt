using System;

namespace Game {
    public class GVFourDimensionalMemoryBankGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVFourDimensionalMemoryBankBlockBehavior m_SubsystemGVMemoryBankBlockBehavior;
        public SubsystemGameInfo m_subsystemGameInfo;

        public uint m_voltage;
        public uint m_lastBottomInput;

        public GVFourDimensionalMemoryBankGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            m_SubsystemGVMemoryBankBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVFourDimensionalMemoryBankBlockBehavior>(true);
            m_subsystemGameInfo = subsystemGVElectricity.Project.FindSubsystem<SubsystemGameInfo>(true);
            GVFourDimensionalMemoryBankData blockData = m_SubsystemGVMemoryBankBlockBehavior.GetBlockData(cellFace.Point);
            if (blockData != null) {
                m_voltage = blockData.LastOutput;
            }
        }

        public override void OnAdded() {
            GVFourDimensionalMemoryBankData memoryBankData = m_SubsystemGVMemoryBankBlockBehavior.GetBlockData(CellFaces[0].Point);
            if (memoryBankData != null
                && memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            bool bottomConnected = false;
            uint bottomInput = 0u;
            uint inInput = 0u;
            uint rightInput = 0u;
            uint leftInput = 0u;
            int rotation = Rotation;
            bool hasInput = false;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Right:
                                rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                hasInput = true;
                                break;
                            case GVElectricConnectorDirection.Left:
                                leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                hasInput = true;
                                break;
                            case GVElectricConnectorDirection.Bottom:
                                bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                bottomConnected = true;
                                hasInput = true;
                                break;
                            case GVElectricConnectorDirection.In:
                                inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                hasInput = true;
                                break;
                        }
                    }
                }
            }
            GVFourDimensionalMemoryBankData memoryBankData = m_SubsystemGVMemoryBankBlockBehavior.GetBlockData(CellFaces[0].Point);
            if (memoryBankData != null) {
                if (memoryBankData.m_worldDirectory == null) {
                    memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                    memoryBankData.LoadData();
                }
                int x = (int)(rightInput & 0xffffu);
                int y = (int)(rightInput >> 16);
                int z = (int)(leftInput & 0xffffu);
                int w = (int)(leftInput >> 16);
                if (bottomConnected) {
                    if (bottomInput != m_lastBottomInput) {
                        m_lastBottomInput = bottomInput;
                        if (bottomInput > 0u) {
                            m_voltage = 0u;
                            switch (bottomInput) {
                                case 1u:
                                    m_voltage = memoryBankData.Read(x, y, z, w);
                                    break;
                                case 2u:
                                    memoryBankData.Write(
                                        x,
                                        y,
                                        z,
                                        w,
                                        inInput
                                    );
                                    break;
                                case 256u:
                                    memoryBankData.m_xSize = MathUint.ToIntWithClamp(inInput);
                                    memoryBankData.m_updateTime = DateTime.Now;
                                    break;
                                case 257u:
                                    memoryBankData.m_ySize = MathUint.ToIntWithClamp(inInput);
                                    memoryBankData.m_updateTime = DateTime.Now;
                                    break;
                                case 258u:
                                    memoryBankData.m_xOffset = MathUint.ToIntWithClamp(inInput);
                                    memoryBankData.m_updateTime = DateTime.Now;
                                    break;
                                case 259u:
                                    memoryBankData.m_yOffset = MathUint.ToIntWithClamp(inInput);
                                    memoryBankData.m_updateTime = DateTime.Now;
                                    break;
                                case 260u:
                                    memoryBankData.m_zOffset = MathUint.ToIntWithClamp(inInput);
                                    memoryBankData.m_updateTime = DateTime.Now;
                                    break;
                                case 261u:
                                    memoryBankData.m_wOffset = MathUint.ToIntWithClamp(inInput);
                                    memoryBankData.m_updateTime = DateTime.Now;
                                    break;
                                case 272u:
                                    m_voltage = (uint)memoryBankData.m_xSize;
                                    break;
                                case 273u:
                                    m_voltage = (uint)memoryBankData.m_ySize;
                                    break;
                                case 274u:
                                    m_voltage = (uint)memoryBankData.m_xOffset;
                                    break;
                                case 275u:
                                    m_voltage = (uint)memoryBankData.m_yOffset;
                                    break;
                                case 276u:
                                    m_voltage = (uint)memoryBankData.m_zOffset;
                                    break;
                                case 277u:
                                    m_voltage = (uint)memoryBankData.m_wOffset;
                                    break;
                            }
                        }
                    }
                }
                else {
                    m_voltage = memoryBankData.Read(x, y, z, w);
                }
                if (!hasInput) {
                    m_voltage = memoryBankData.m_ID;
                }
                memoryBankData.LastOutput = m_voltage;
            }
            return m_voltage != voltage;
        }
    }
}