using System.Collections.Generic;
using Engine;

namespace Game
{
    public class SubsystemGVJumpWireBlockBehavior:SubsystemBlockBehavior
    {
        public Dictionary<uint,List<JumpWireGVElectricElement>> m_tagsDictionary = new Dictionary<uint, List<JumpWireGVElectricElement>>();
        public override int[] HandledBlocks => new int[]{GVJumpWireBlock.Index};
    }
}