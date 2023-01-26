using Engine;
using System;

namespace Game
{
    public class ButtonGVElectricElement : MountedGVElectricElement
    {
        public SubsystemGVButtonBlockBehavior m_subsystemGVButtonBlockBehavior;

        public uint m_voltage;

        public bool m_wasPressed;

        public ButtonGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace, int value)
            : base(subsystemGVElectricity, cellFace)
        {
            m_subsystemGVButtonBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVButtonBlockBehavior>(throwOnError: true);
        }

        public void Press()
        {
            if (!m_wasPressed && m_voltage == 0u)
            {
                m_wasPressed = true;
                CellFace cellFace = base.CellFaces[0];
                base.SubsystemGVElectricity.SubsystemAudio.PlaySound("Audio/Click", 1f, 0f, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 2f, autoDelay: true);
                base.SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, base.SubsystemGVElectricity.CircuitStep + 1);
            }
        }

        public override uint GetOutputVoltage(int face)
        {
            return m_voltage;
        }

        public override bool Simulate()
        {
            uint voltage = m_voltage;
            if (m_wasPressed)
            {
                m_wasPressed = false;
                GigaVoltageLevelData blockData = m_subsystemGVButtonBlockBehavior.GetBlockData(CellFaces[0].Point);
                if (blockData != null)
                {
                    m_voltage = blockData.Data;
                    base.SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, base.SubsystemGVElectricity.CircuitStep + 10);
                }
                else
                {
                    m_voltage = 0u;
                }
            }
            else
            {
                m_voltage = 0u;
            }
            return m_voltage != voltage;
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
        {
            Press();
            return true;
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
        {
            Press();
        }
    }
}