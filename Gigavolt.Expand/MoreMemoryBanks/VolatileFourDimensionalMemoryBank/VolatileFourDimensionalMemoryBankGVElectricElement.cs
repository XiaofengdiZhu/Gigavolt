using System;

namespace Game {
    public class VolatileFourDimensionalMemoryBankGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior m_SubsystemGVMemoryBankBlockBehavior;

        public readonly GVVolatileFourDimensionalMemoryBankData m_data;
        public uint m_voltage;
        public uint m_lastBottomInput;

        public VolatileFourDimensionalMemoryBankGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_SubsystemGVMemoryBankBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior>(true);
            m_data = m_SubsystemGVMemoryBankBlockBehavior.GetItemData(m_SubsystemGVMemoryBankBlockBehavior.GetIdFromValue(value));
        }

        public override void OnAdded() { }

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
                                m_voltage = m_data.Read(x, y, z, w);
                                break;
                            case 2u:
                                m_data.Write(
                                    x,
                                    y,
                                    z,
                                    w,
                                    inInput
                                );
                                break;
                            case 256u:
                                m_data.m_xSize = MathUint.ToIntWithClamp(inInput);
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 257u:
                                m_data.m_ySize = MathUint.ToIntWithClamp(inInput);
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 258u:
                                m_data.m_xOffset = MathUint.ToIntWithClamp(inInput);
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 259u:
                                m_data.m_yOffset = MathUint.ToIntWithClamp(inInput);
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 260u:
                                m_data.m_zOffset = MathUint.ToIntWithClamp(inInput);
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 261u:
                                m_data.m_wOffset = MathUint.ToIntWithClamp(inInput);
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 272u:
                                m_voltage = (uint)m_data.m_xSize;
                                break;
                            case 273u:
                                m_voltage = (uint)m_data.m_ySize;
                                break;
                            case 274u:
                                m_voltage = (uint)m_data.m_xOffset;
                                break;
                            case 275u:
                                m_voltage = (uint)m_data.m_yOffset;
                                break;
                            case 276u:
                                m_voltage = (uint)m_data.m_zOffset;
                                break;
                            case 277u:
                                m_voltage = (uint)m_data.m_wOffset;
                                break;
                        }
                    }
                }
            }
            else {
                m_voltage = m_data.Read(x, y, z, w);
            }
            if (!hasInput) {
                m_voltage = m_data.m_ID;
            }
            m_data.LastOutput = m_voltage;
            return m_voltage != voltage;
        }
    }
}