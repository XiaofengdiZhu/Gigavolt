using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class SubsystemGVJumpWireBlockBehavior : SubsystemBlockBehavior, IDrawable {
        public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();
        public Dictionary<uint, List<JumpWireGVElectricElement>> m_tagsDictionary = new Dictionary<uint, List<JumpWireGVElectricElement>>();
        public static int[] m_drawOrders = { 113 };
        public override int[] HandledBlocks => new[] { GVJumpWireBlock.Index };

        public void Draw(Camera camera, int drawOrder) {
            foreach (List<JumpWireGVElectricElement> elements in m_tagsDictionary.Values) {
                for (int i = 0; i < elements.Count; i++) {
                    for (int j = i + 1; j < elements.Count; j++) {
                        m_primitivesRenderer.FlatBatch(0, DepthStencilState.DepthRead, null, BlendState.Additive).QueueLine(elements[i].m_position, elements[j].m_position, Color.Green);
                    }
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public int[] DrawOrders => m_drawOrders;
    }
}