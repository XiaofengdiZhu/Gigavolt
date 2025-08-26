using System;
using System.Collections.Generic;
using Engine;
using Engine.Input;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVTouchpadBlockBehavior : SubsystemBlockBehavior, IUpdateable {
        public SubsystemPlayers m_subsystemPlayers;
        public SubsystemGVElectricity m_subsystemGVElectricity;

        public readonly Dictionary<CellFace, TouchpadGVElectricElement> m_interactingTouchpads = [];
        public readonly Dictionary<CellFace, TouchpadGVElectricElement> m_sightingTouchpads = [];

        public UpdateOrder UpdateOrder => UpdateOrder.CreatureModels + 1;
        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVTouchpadBlock>()];

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(true);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
        }

        public void Update(float dt) {
            Dictionary<CellFace, Vector2> sightingTouchpads = [];
            Dictionary<CellFace, Vector2> interactingTouchpads = [];
            foreach (ComponentPlayer player in m_subsystemPlayers.ComponentPlayers) {
                if (player.ComponentBlockHighlight.m_highlightRaycastResult is TerrainRaycastResult result
                    && Terrain.ExtractContents(result.Value) == HandledBlocks[0]) {
                    CellFace cellFace = result.CellFace;
                    int face = cellFace.Face;
                    if (RotateableMountedGVElectricElementBlock.GetFaceFromDataStatic(Terrain.ExtractData(result.Value)) == face) {
                        Vector3 hitPosition = result.HitPoint();
                        Vector2 position = face switch {
                            0 => new Vector2(hitPosition.X - cellFace.X, cellFace.Y + 1 - hitPosition.Y),
                            1 => new Vector2(cellFace.Z + 1 - hitPosition.Z, cellFace.Y + 1 - hitPosition.Y),
                            2 => new Vector2(hitPosition.X - cellFace.X, cellFace.Y + 1 - hitPosition.Y),
                            3 => new Vector2(hitPosition.Z - cellFace.Z, cellFace.Y + 1 - hitPosition.Y),
                            4 => new Vector2(hitPosition.X - cellFace.X, hitPosition.Z - cellFace.Z),
                            5 => new Vector2(hitPosition.X - cellFace.X, cellFace.Z + 1 - hitPosition.Z),
                            _ => Vector2.Zero
                        };
                        sightingTouchpads.Add(result.CellFace, position);
                        if (player.GameWidget.Input.IsMouseButtonDown(MouseButton.Right)) {
                            interactingTouchpads.Add(result.CellFace, position);
                        }
                    }
                }
            }
            foreach ((CellFace cellFace, TouchpadGVElectricElement element) in m_sightingTouchpads) {
                if (sightingTouchpads.TryGetValue(cellFace, out Vector2 sightPosition)) {
                    element.m_sightPosition = sightPosition;
                }
                else {
                    m_sightingTouchpads.Remove(cellFace);
                    element.m_sightStartTime = null;
                }
                m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
            }
            foreach ((CellFace cellFace, Vector2 sightPosition) in sightingTouchpads) {
                if (!m_sightingTouchpads.ContainsKey(cellFace)
                    && m_subsystemGVElectricity.GetGVElectricElement(cellFace.X, cellFace.Y, cellFace.Z, cellFace.Face, 0) is
                        TouchpadGVElectricElement element) {
                    element.m_sightStartTime = DateTime.Now;
                    element.m_sightPosition = sightPosition;
                    m_sightingTouchpads.Add(cellFace, element);
                    m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
                }
            }
            foreach ((CellFace cellFace, TouchpadGVElectricElement element) in m_interactingTouchpads) {
                if (element.m_interactStop) {
                    if (interactingTouchpads.TryGetValue(cellFace, out Vector2 sightPosition)) {
                        element.m_interactPosition = sightPosition;
                    }
                    else {
                        m_interactingTouchpads.Remove(cellFace);
                        element.m_interactStartTime = null;
                    }
                    m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
                }
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            CellFace cellFace = raycastResult.CellFace;
            int face = raycastResult.CellFace.Face;
            if (RotateableMountedGVElectricElementBlock.GetFaceFromDataStatic(Terrain.ExtractData(raycastResult.Value)) == face
                && m_subsystemGVElectricity.GetGVElectricElement(cellFace.X, cellFace.Y, cellFace.Z, face, 0) is TouchpadGVElectricElement element) {
                if (element.m_interactStop
                    && m_interactingTouchpads.TryAdd(cellFace, element)) {
                    element.m_interactStartTime = DateTime.Now;
                }
                Vector3 hitPosition = raycastResult.HitPoint();
                element.m_interactPosition = face switch {
                    0 => new Vector2(hitPosition.X - cellFace.X, cellFace.Y + 1 - hitPosition.Y),
                    1 => new Vector2(cellFace.Z + 1 - hitPosition.Z, cellFace.Y + 1 - hitPosition.Y),
                    2 => new Vector2(hitPosition.X - cellFace.X, cellFace.Y + 1 - hitPosition.Y),
                    3 => new Vector2(hitPosition.Z - cellFace.Z, cellFace.Y + 1 - hitPosition.Y),
                    4 => new Vector2(hitPosition.X - cellFace.X, hitPosition.Z - cellFace.Z),
                    5 => new Vector2(hitPosition.X - cellFace.X, cellFace.Z + 1 - hitPosition.Z),
                    _ => Vector2.Zero
                };
                if (!element.m_interactStop) {
                    m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
                }
                return true;
            }
            return false;
        }
    }
}