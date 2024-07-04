using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public class JumpWireGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemGVJumpWireBlockBehavior m_subsystem;
        public Vector3 m_position;
        public uint m_output;
        public uint m_tag;
        public bool m_allowBottomInput;
        public bool m_allowTagInput;
        public uint m_bottomInput;

        public JumpWireGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystem = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVJumpWireBlockBehavior>(true);
            m_position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f) - CellFace.FaceToVector3(cellFace.Face) * 0.48f;
        }

        public override uint GetOutputVoltage(int face) => SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face) == GVElectricConnectorDirection.Top ? m_output : 0u;

        public override void OnRemoved() {
            if (m_subsystem.m_tagsDictionary.TryGetValue(m_tag, out List<JumpWireGVElectricElement> value)) {
                value.Remove(this);
            }
        }

        public override bool Simulate() {
            try {
                uint output = m_output;
                m_output = 0u;
                uint tag = m_tag;
                m_tag = 0u;
                uint bottomInput = m_bottomInput;
                m_bottomInput = 0u;
                bool allowBottomInput = m_allowBottomInput;
                m_allowBottomInput = false;
                bool allowTagInput = m_allowTagInput;
                m_allowTagInput = false;
                bool flag = false;
                int rotation = Rotation;
                foreach (GVElectricConnection connection in Connections) {
                    if (connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                        GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                        if (connectorDirection.HasValue) {
                            if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                                m_tag = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                if (m_tag != tag) {
                                    flag = true;
                                    if (tag > 0) {
                                        m_subsystem.m_tagsDictionary[tag].Remove(this);
                                    }
                                    if (m_tag > 0) {
                                        m_subsystem.m_tagsDictionary.TryGetValue(m_tag, out List<JumpWireGVElectricElement> list);
                                        if (list == null) {
                                            list = new List<JumpWireGVElectricElement>();
                                            m_subsystem.m_tagsDictionary.Add(m_tag, list);
                                        }
                                        m_subsystem.m_tagsDictionary[m_tag].Add(this);
                                    }
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                                m_allowBottomInput = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                                if (m_allowBottomInput != allowBottomInput) {
                                    flag = true;
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                                m_allowTagInput = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                                if (m_allowTagInput != allowTagInput) {
                                    flag = true;
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                                m_bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                        }
                    }
                }
                if (m_allowBottomInput) {
                    m_output = m_bottomInput;
                    if (m_bottomInput != bottomInput) {
                        flag = true;
                    }
                }
                else {
                    m_bottomInput = 0u;
                }
                if (m_tag > 0) {
                    if (m_allowTagInput) {
                        foreach (JumpWireGVElectricElement element in m_subsystem.m_tagsDictionary[m_tag]) {
                            m_output |= element.m_bottomInput;
                        }
                    }
                    if (m_bottomInput != bottomInput
                        || m_tag != tag) {
                        foreach (JumpWireGVElectricElement element in m_subsystem.m_tagsDictionary[m_tag]) {
                            if (element.m_allowTagInput) {
                                SubsystemGVElectricity.QueueGVElectricElementForSimulation(element, SubsystemGVElectricity.CircuitStep + 1);
                            }
                        }
                    }
                }
                if (m_tag != tag
                    && tag > 0) {
                    foreach (JumpWireGVElectricElement element in m_subsystem.m_tagsDictionary[tag]) {
                        if (element.m_allowTagInput) {
                            SubsystemGVElectricity.QueueGVElectricElementForSimulation(element, SubsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                }
                if (m_output != output) {
                    flag = true;
                }
                return flag;
            }
            catch (Exception e) {
                Log.Error(e);
                return false;
            }
        }
    }
}