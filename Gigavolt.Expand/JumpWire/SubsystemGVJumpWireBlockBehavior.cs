using System.Collections.Generic;

namespace Game {
    public class SubsystemGVJumpWireBlockBehavior : SubsystemBlockBehavior {
        public Dictionary<uint, List<JumpWireGVElectricElement>> m_tagsDictionary = new Dictionary<uint, List<JumpWireGVElectricElement>>();
        public override int[] HandledBlocks => new[] { GVJumpWireBlock.Index };
    }
}