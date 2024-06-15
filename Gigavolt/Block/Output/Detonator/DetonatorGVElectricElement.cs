using System.Collections.Generic;
using Engine;

namespace Game {
    public class DetonatorGVElectricElement : MountedGVElectricElement {
        public readonly GVSubterrainSystem m_subterrainSystem;
        public DetonatorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subterrainSystem = subterrainId == 0 ? null : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];

        public void Detonate(uint pressure) {
            SubsystemExplosions m_subsystemExplosions = SubsystemGVElectricity.Project.FindSubsystem<SubsystemExplosions>(true);
            GVCellFace cellFace = CellFaces[0];
            Point3 position = CellFaces[0].Point;
            if (SubterrainId != 0) {
                position = Terrain.ToCell(Vector3.Transform(new Vector3(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f), m_subterrainSystem.GlobalTransform));
            }
            Block block = BlocksManager.Blocks[GVDetonatorBlock.Index];
            if (pressure == 0) {
                m_subsystemExplosions.AddExplosion(
                    position.X,
                    position.Y,
                    position.Z,
                    block.GetExplosionPressure(GVDetonatorBlock.Index),
                    block.GetExplosionIncendiary(GVDetonatorBlock.Index),
                    false
                );
            }
            else {
                if (SubterrainId == 0) {
                    SubsystemGVElectricity.SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, AirBlock.Index);
                }
                else {
                    m_subterrainSystem.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, AirBlock.Index);
                }
                m_subsystemExplosions.AddExplosion(
                    position.X,
                    position.Y,
                    position.Z,
                    pressure,
                    block.GetExplosionIncendiary(GVDetonatorBlock.Index),
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
                    }
                    foreach (Point3 point in points) {
                        if (SubterrainId == 0) {
                            m_subterrainSystem.DestroyCell(
                                int.MaxValue,
                                point.X,
                                point.Y,
                                point.Z,
                                AirBlock.Index,
                                false,
                                false
                            );
                        }
                        else {
                            SubsystemGVElectricity.SubsystemTerrain.DestroyCell(
                                int.MaxValue,
                                point.X,
                                point.Y,
                                point.Z,
                                AirBlock.Index,
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