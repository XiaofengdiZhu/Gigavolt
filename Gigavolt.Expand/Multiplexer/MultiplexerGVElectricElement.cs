using System.Collections.Generic;

namespace Game
{
    public class MultiplexerGVElectricElement : RotateableGVElectricElement
    {
        //ABCDIn
        public uint[] m_inputsVoltage = new uint[] { 0, 0, 0, 0, 0 };

        //ABCDabcdO
        public uint[] default_nodesVoltage = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public uint[] m_nodesVoltage;

        //1~20常断，21~28常通
        public bool[] default_switched = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true };

        public bool[] m_switches;
        public List<uint>[] m_nodesRelations;
        public Queue<uint> m_nodesToUpdate;

        //1 parent A child a, 2 parent a child A...
        public uint[] switch2parentChildNode = new uint[] { 0, 4, 4, 0, 1, 5, 5, 1, 2, 6, 6, 2, 3, 7, 7, 3, 4, 6, 6, 4, 5, 7, 7, 5, 4, 5, 5, 4, 5, 6, 6, 5, 6, 7, 7, 6, 7, 4, 4, 7, 4, 8, 8, 4, 5, 8, 8, 5, 6, 8, 8, 6, 7, 8, 8, 7 };

        public bool m_noInInput = true;

        public MultiplexerGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
            m_nodesVoltage = (uint[])default_nodesVoltage.Clone();
            m_switches = (bool[])default_switched.Clone();
            m_nodesRelations = new List<uint>[18];
            m_nodesToUpdate = new Queue<uint>();
        }

        public override uint GetOutputVoltage(int face)
        {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue)
            {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top)
                {
                    return m_nodesVoltage[0];
                }
                else if (connectorDirection.Value == GVElectricConnectorDirection.Right)
                {
                    return m_nodesVoltage[1];
                }
                else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom)
                {
                    return m_nodesVoltage[2];
                }
                else if (connectorDirection.Value == GVElectricConnectorDirection.Left)
                {
                    return m_nodesVoltage[3];
                }
            }
            return 0u;
        }

        public override bool Simulate()
        {
            uint[] inputs = (uint[])m_inputsVoltage.Clone();
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
                            m_inputsVoltage[4] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (m_inputsVoltage[4] != inputs[4])
                            {
                                flag = true;
                                if (m_inputsVoltage[4] == 0)
                                {
                                    m_noInInput = true;
                                    m_nodesVoltage = (uint[])default_nodesVoltage.Clone();
                                    m_switches = (bool[])default_switched.Clone();
                                    m_nodesRelations = new List<uint>[18];
                                }
                                else
                                {
                                    m_noInInput = false;
                                    UpdateSwitches();
                                    UpdateNodesRelations();
                                }
                            }
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Top)
                        {
                            m_inputsVoltage[0] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (!flag)
                            {
                                flag = m_inputsVoltage[0] != inputs[0];
                            }
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Right)
                        {
                            m_inputsVoltage[1] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (!flag)
                            {
                                flag = m_inputsVoltage[1] != inputs[1];
                            }
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom)
                        {
                            m_inputsVoltage[2] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (!flag)
                            {
                                flag = m_inputsVoltage[2] != inputs[2];
                            }
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Left)
                        {
                            m_inputsVoltage[3] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (!flag)
                            {
                                flag = m_inputsVoltage[3] != inputs[3];
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                UpdateNodesVoltage();
            }
            return flag;
        }

        public void UpdateSwitches()
        {
            uint inInput = m_inputsVoltage[4];
            m_switches = new bool[28];
            for (int i = 0; i < 20; i++)
            {
                m_switches[i] = ((inInput >> i) & 1) == 1;
            }
            for (int i = 20; i < 28; i++)
            {
                m_switches[i] = ((inInput >> i) & 1) == 0;
            }
        }

        public void UpdateNodesRelations()
        {
            m_nodesRelations = new List<uint>[18];
            if (!(m_switches[1] || m_switches[3] || m_switches[5] || m_switches[7]))
            {
                m_nodesVoltage = (uint[])default_nodesVoltage.Clone();
            }
            else
            {
                for (int i = 0; i < 56; i += 2)
                {
                    if (m_switches[i / 2])
                    {
                        AddNodesRelation(switch2parentChildNode[i], switch2parentChildNode[i + 1]);
                    }
                }
            }
        }

        public void UpdateNodesVoltage()
        {
            if (m_nodesToUpdate.Count > 0)
            {
                m_nodesToUpdate.Clear();
            }
            m_nodesVoltage = (uint[])default_nodesVoltage.Clone();
            for (uint i = 0; i < 4; i++)
            {
                SetNodeVoltage(i, m_inputsVoltage[i]);
            }
            while (m_nodesToUpdate.Count > 0)
            {
                UpdateNodeVoltage(m_nodesToUpdate.Dequeue());
            }
        }

        public void UpdateNodeVoltage(uint node)
        {
            uint newVoltage = 0u;
            foreach (uint i in m_nodesRelations[node * 2])
            {
                newVoltage |= m_nodesVoltage[i];
            }
            if (newVoltage != m_nodesVoltage[node])
            {
                SetNodeVoltage(node, newVoltage);
            }
        }

        public void SetNodeVoltage(uint node, uint voltage)
        {
            m_nodesVoltage[node] = voltage;
            if (m_nodesRelations[node * 2 + 1] != null)
            {
                foreach (uint i in m_nodesRelations[node * 2 + 1])
                {
                    m_nodesToUpdate.Enqueue(i);
                }
            }
        }

        public void AddNodesRelation(uint parent, uint child)
        {
            if (m_nodesRelations[child * 2] == null)
            {
                m_nodesRelations[child * 2] = new List<uint>();
            }
            m_nodesRelations[child * 2].Add(parent);
            if (m_nodesRelations[parent * 2 + 1] == null)
            {
                m_nodesRelations[parent * 2 + 1] = new List<uint>();
            }
            m_nodesRelations[parent * 2 + 1].Add(child);
        }
    }
}