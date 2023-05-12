using System;
using System.Collections.Generic;
using Engine;

namespace Game
{
    public class JumpWireGVElectricElement : RotateableGVElectricElement
    {
        public SubsystemGVJumpWireBlockBehavior m_subsystem;
        public uint m_output = 0u;
        public uint m_tag = 0u;
        public bool m_allowBottomInput = false;
        public bool m_allowTagInput = false;
        public uint m_bottomInput = 0u;
        public JumpWireGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
            m_subsystem = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVJumpWireBlockBehavior>(true);
        }

        public override uint GetOutputVoltage(int face)
        {
            return SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face) == GVElectricConnectorDirection.Top ? m_output : 0u;
        }

        public override bool Simulate()
        {
            try
            {
                uint output = m_output;
                m_output = 0u;
                uint tag = m_tag;
                uint bottomInput = m_bottomInput;
                bool allowBottomInput = m_allowBottomInput;
                bool allowTagInput = m_allowTagInput;
                bool flag = false;
                int rotation = Rotation;
                foreach (GVElectricConnection connection in Connections)
                {
                    if (connection.NeighborConnectorType != GVElectricConnectorType.Input)
                    {
                        GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                        if (connectorDirection.HasValue)
                        {
                            if (connectorDirection.Value == GVElectricConnectorDirection.In)
                            {
                                m_tag = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                if (m_tag != tag)
                                {
                                    flag = true;
                                    if (tag > 0)
                                    {
                                        m_subsystem.m_tagsDictionary[tag].Remove(this);
                                    }

                                    if (m_tag > 0)
                                    {
                                        m_subsystem.m_tagsDictionary.TryGetValue(m_tag, out List<JumpWireGVElectricElement> list);
                                        if (list == null)
                                        {
                                            list = new List<JumpWireGVElectricElement>();
                                            m_subsystem.m_tagsDictionary.Add(m_tag, list);
                                        }

                                        m_subsystem.m_tagsDictionary[m_tag].Add(this);
                                    }
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Left)
                            {
                                m_allowBottomInput = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                                if (m_allowBottomInput != allowBottomInput)
                                {
                                    flag = true;
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Right)
                            {
                                m_allowTagInput = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                                if (m_allowTagInput != allowTagInput)
                                {
                                    flag = true;
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom)
                            {
                                if (m_allowBottomInput)
                                {
                                    m_bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                    m_output = m_bottomInput;
                                }
                                else
                                {
                                    m_bottomInput = 0u;
                                }

                                if (m_bottomInput != bottomInput)
                                {
                                    flag = true;
                                }
                            }
                        }
                    }
                }

                if (m_tag > 0)
                {
                    if (m_allowTagInput)
                    {
                        foreach (JumpWireGVElectricElement element in m_subsystem.m_tagsDictionary[m_tag])
                        {
                            m_output |= element.m_bottomInput;
                        }
                    }

                    if (m_bottomInput != bottomInput || m_tag != tag)
                    {
                        foreach (JumpWireGVElectricElement element in m_subsystem.m_tagsDictionary[m_tag])
                        {
                            if (element.m_allowTagInput)
                            {
                                SubsystemGVElectricity.QueueGVElectricElementForSimulation(element, SubsystemGVElectricity.CircuitStep + 1);
                            }
                        }
                    }
                }
                else if (tag > 0)
                {
                    foreach (JumpWireGVElectricElement element in m_subsystem.m_tagsDictionary[tag])
                    {
                        if (element.m_allowTagInput)
                        {
                            SubsystemGVElectricity.QueueGVElectricElementForSimulation(element, SubsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                }

                if (m_output != output)
                {
                    flag = true;
                }

                return flag;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }
    }
}