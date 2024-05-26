using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVJumpWireBlockBehavior : SubsystemBlockBehavior, IDrawable {
        public PrimitivesRenderer3D m_primitivesRenderer = new();
        public FlatBatch3D m_flatBatch;
        public Dictionary<uint, List<JumpWireGVElectricElement>> m_tagsDictionary = new();
        public static int[] m_drawOrders = [113];
        public override int[] HandledBlocks => new[] { GVJumpWireBlock.Index };

        public override void Load(ValuesDictionary valuesDictionary) {
            m_flatBatch = m_primitivesRenderer.FlatBatch(0, DepthStencilState.DepthRead, null, BlendState.Additive);
            base.Load(valuesDictionary);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach (List<JumpWireGVElectricElement> elements in m_tagsDictionary.Values) {
                for (int i = 0; i < elements.Count; i++) {
                    for (int j = i + 1; j < elements.Count; j++) {
                        m_flatBatch.QueueLine(elements[i].m_position, elements[j].m_position, Color.Green);
                    }
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public int[] DrawOrders => m_drawOrders;
    }
}