using System.Collections.Generic;
using Engine;

namespace Game {
    public class DetonatorGVElectricElement : MountedGVElectricElement {
        public DetonatorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) { }

        public void Detonate(uint pressure) {
            SubsystemExplosions m_subsystemExplosions = SubsystemGVElectricity.Project.FindSubsystem<SubsystemExplosions>(true);
            GVCellFace cellFace = CellFaces[0];
            if (pressure == 0) {
                int value = Terrain.MakeBlockValue(GVDetonatorBlock.Index);
                m_subsystemExplosions.TryExplodeBlock(cellFace.X, cellFace.Y, cellFace.Z, value);
            }
            else {
                SubsystemGVElectricity.Project.FindSubsystem<SubsystemTerrain>(true).ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, 0);
                m_subsystemExplosions.AddExplosion(
                    cellFace.X,
                    cellFace.Y,
                    cellFace.Z,
                    pressure,
                    false,
                    false
                );
            }
        }

        public override bool Simulate() {
            uint num = 0u;
            HashSet<Point3> points = new();
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    uint input = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    num |= input;
                    if (input == uint.MaxValue) {
                        points.Add(connection.NeighborCellFace.Point);
                    }
                }
            }
            if (num > 0u) {
                if (num == uint.MaxValue) {
                    foreach (ComponentPlayer player in SubsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true).ComponentPlayers) {
                        player.ComponentHealth.Injure(float.MaxValue, null, true, LanguageControl.Get(GetType().Name, 1));
                        SubsystemTerrain subsystemTerrain = SubsystemGVElectricity.Project.FindSubsystem<SubsystemTerrain>(true);
                        foreach (Point3 point in points) {
                            subsystemTerrain.DestroyCell(
                                int.MaxValue,
                                point.X,
                                point.Y,
                                point.Z,
                                0,
                                false,
                                false
                            );
                        }
                    }
                }
                else {
                    Detonate(num);
                }
            }
            return false;
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) {
            Detonate(0u);
        }
    }
}