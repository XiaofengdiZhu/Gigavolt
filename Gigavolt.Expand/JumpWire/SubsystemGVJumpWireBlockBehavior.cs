using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVJumpWireBlockBehavior : SubsystemBlockBehavior, IDrawable {
        public readonly PrimitivesRenderer3D m_primitivesRenderer = new();
        public FlatBatch3D m_flatBatch;
        public readonly Dictionary<uint, List<JumpWireGVElectricElement>> m_tagsDictionary = new();
        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVJumpWireBlock>()];

        public override void Load(ValuesDictionary valuesDictionary) {
            m_flatBatch = m_primitivesRenderer.FlatBatch(0, DepthStencilState.DepthRead, null, BlendState.Additive);
            base.Load(valuesDictionary);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach (List<JumpWireGVElectricElement> elements in m_tagsDictionary.Values) {
                Dictionary<int, Vector3> positions = new();
                for (int i = 0; i < elements.Count - 1; i++) {
                    if (!positions.TryGetValue(i, out Vector3 position1)) {
                        position1 = GetPosition(elements[i]);
                        positions.Add(i, position1);
                    }
                    for (int j = i + 1; j < elements.Count; j++) {
                        if (!positions.TryGetValue(j, out Vector3 position2)) {
                            position2 = GetPosition(elements[j]);
                            positions.Add(j, position2);
                        }
                        m_flatBatch.QueueLine(position1, position2, Color.Green);
                    }
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public static Vector3 GetPosition(JumpWireGVElectricElement element) {
            Vector3 result = element.m_position;
            uint subterrainId = element.SubterrainId;
            if (subterrainId != 0) {
                result = Vector3.Transform(result, GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform);
            }
            return result;
        }

        public int[] DrawOrders => [2000];
    }
}