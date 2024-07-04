using System.Collections.Generic;

namespace Game {
    public class MultiplexerGVElectricElement : RotateableGVElectricElement {
        //ABCDIn
        public uint[] m_inputsVoltage;

        //abcdO
        public uint[] m_nodesVoltage;

        public readonly uint[] default_nodesVoltage = [
            0,
            0,
            0,
            0,
            0
        ];

        //1~20常断，21~28常通
        public readonly bool[] default_switched = [
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true
        ];

        public bool[] m_switches;
        public List<int>[] m_nodesRelations;
        public readonly Queue<int> m_nodesToUpdate;

        //1 parent A child a, 2 parent a child A...
        public readonly int[] switch2parentChildNode = [
            -1,
            0,
            0,
            -1,
            -2,
            1,
            1,
            -2,
            -3,
            2,
            2,
            -3,
            -4,
            3,
            3,
            -4,
            0,
            2,
            2,
            0,
            1,
            3,
            3,
            1,
            0,
            1,
            1,
            0,
            1,
            2,
            2,
            1,
            2,
            3,
            3,
            2,
            3,
            0,
            0,
            3,
            0,
            4,
            4,
            0,
            1,
            4,
            4,
            1,
            2,
            4,
            4,
            2,
            3,
            4,
            4,
            3
        ];

        public bool m_noInInput = true;

        public MultiplexerGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_nodesVoltage = (uint[])default_nodesVoltage.Clone();
            m_inputsVoltage = (uint[])default_nodesVoltage.Clone();
            m_switches = (bool[])default_switched.Clone();
            m_nodesRelations = new List<int>[10];
            m_nodesToUpdate = new Queue<int>();
        }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top
                    && m_switches[1]) {
                    return m_nodesVoltage[0];
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Right
                    && m_switches[3]) {
                    return m_nodesVoltage[1];
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Bottom
                    && m_switches[5]) {
                    return m_nodesVoltage[2];
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Left
                    && m_switches[7]) {
                    return m_nodesVoltage[3];
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            uint[] inputs = (uint[])m_inputsVoltage.Clone();
            m_inputsVoltage = (uint[])default_nodesVoltage.Clone();
            bool flag = false;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                            m_inputsVoltage[4] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                            m_inputsVoltage[0] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                            m_inputsVoltage[1] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                            m_inputsVoltage[2] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                            m_inputsVoltage[3] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                    }
                }
            }
            if (m_inputsVoltage[4] != inputs[4]) {
                flag = true;
                if (m_inputsVoltage[4] == 0) {
                    m_noInInput = true;
                    m_nodesVoltage = (uint[])default_nodesVoltage.Clone();
                    m_switches = (bool[])default_switched.Clone();
                    m_nodesRelations = new List<int>[10];
                }
                else {
                    m_noInInput = false;
                    UpdateSwitches();
                    UpdateNodesRelations();
                }
            }
            for (int i = 0; i < 5; i++) {
                if (flag) {
                    break;
                }
                flag = m_inputsVoltage[i] != inputs[i];
            }
            if (flag) {
                UpdateNodesVoltage();
            }
            return flag;
        }

        public void UpdateSwitches() {
            uint inInput = m_inputsVoltage[4];
            m_switches = new bool[28];
            for (int i = 0; i < 20; i++) {
                m_switches[i] = ((inInput >> i) & 1) == 1;
            }
            for (int i = 20; i < 28; i++) {
                m_switches[i] = ((inInput >> i) & 1) == 0;
            }
        }

        public void UpdateNodesRelations() {
            m_nodesRelations = new List<int>[10];
            if (!(m_switches[1] || m_switches[3] || m_switches[5] || m_switches[7])) {
                m_nodesVoltage = (uint[])default_nodesVoltage.Clone();
            }
            else {
                for (int i = 0; i < 56; i += 2) {
                    if (m_switches[i / 2]) {
                        AddNodesRelation(switch2parentChildNode[i], switch2parentChildNode[i + 1]);
                    }
                }
            }
        }

        public void UpdateNodesVoltage() {
            if (m_nodesToUpdate.Count > 0) {
                m_nodesToUpdate.Clear();
            }
            m_nodesVoltage = (uint[])default_nodesVoltage.Clone();
            for (int i = 0; i < 4; i++) {
                if (m_switches[i * 2]) {
                    SetNodeVoltage(i, m_inputsVoltage[i]);
                }
            }
            while (m_nodesToUpdate.Count > 0) {
                UpdateNodeVoltage(m_nodesToUpdate.Dequeue());
            }
        }

        public void UpdateNodeVoltage(int node) {
            uint newVoltage = 0u;
            foreach (int i in m_nodesRelations[node * 2]) {
                newVoltage |= m_nodesVoltage[i];
            }
            if (newVoltage != m_nodesVoltage[node]) {
                SetNodeVoltage(node, newVoltage);
            }
        }

        public void SetNodeVoltage(int node, uint voltage) {
            m_nodesVoltage[node] = voltage;
            if (m_nodesRelations[node * 2 + 1] != null) {
                foreach (int i in m_nodesRelations[node * 2 + 1]) {
                    m_nodesToUpdate.Enqueue(i);
                }
            }
        }

        public void AddNodesRelation(int parent, int child) {
            if (parent < 0
                || child < 0) {
                return;
            }
            if (m_nodesRelations[child * 2] == null) {
                m_nodesRelations[child * 2] = [];
            }
            m_nodesRelations[child * 2].Add(parent);
            if (m_nodesRelations[parent * 2 + 1] == null) {
                m_nodesRelations[parent * 2 + 1] = [];
            }
            m_nodesRelations[parent * 2 + 1].Add(child);
        }
    }
}