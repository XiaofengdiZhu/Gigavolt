using Engine;

namespace Game
{
	public class SwitchGVElectricElement : MountedGVElectricElement
	{
        public SubsystemGVSwitchBlockBehavior m_subsystemGVSwitchBlockBehavior;
		public uint m_voltage;

		public SwitchGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace, int value)
			: base(subsystemGVElectricity, cellFace)
		{
            m_subsystemGVSwitchBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVSwitchBlockBehavior>(throwOnError: true);
            m_voltage = GVSwitchBlock.GetLeverState(value) ? m_subsystemGVSwitchBlockBehavior.GetBlockData(cellFace.Point).Data : 0;
        }

		public override uint GetOutputVoltage(int face)
		{
			return m_voltage;
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			CellFace cellFace = base.CellFaces[0];
			int cellValue = base.SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			int value = GVSwitchBlock.SetLeverState(cellValue, !GVSwitchBlock.GetLeverState(cellValue));
			base.SubsystemGVElectricity.SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, value);
			base.SubsystemGVElectricity.SubsystemAudio.PlaySound("Audio/Click", 1f, 0f, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 2f, autoDelay: true);
			return true;
		}
	}
}
