using System.Collections.Generic;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVBlockBehaviors : Subsystem {
        public readonly HashSet<IGVBlockBehavior> BlockBehaviors = [];
        public List<IGVBlockBehavior>[] m_blockBehaviorsByContents;
        public List<IGVBlockBehavior> GetBlockBehaviors(int contents) => m_blockBehaviorsByContents[contents];

        public override void Load(ValuesDictionary valuesDictionary) {
            m_blockBehaviorsByContents = new List<IGVBlockBehavior>[BlocksManager.Blocks.Length];
            SubsystemBlockBehaviors originalSubsystem = Project.FindSubsystem<SubsystemBlockBehaviors>(true);
            for (int i = 0; i < BlocksManager.Blocks.Length; i++) {
                m_blockBehaviorsByContents[i] = [];
                foreach (SubsystemBlockBehavior originalBehavior in originalSubsystem.m_blockBehaviorsByContents[i]) {
                    if (originalBehavior is IGVBlockBehavior blockBehavior) {
                        m_blockBehaviorsByContents[i].Add(blockBehavior);
                        BlockBehaviors.Add(blockBehavior);
                    }
                }
            }
        }
    }
}