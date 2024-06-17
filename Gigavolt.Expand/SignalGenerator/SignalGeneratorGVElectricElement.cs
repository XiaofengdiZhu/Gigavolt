using System;
using Engine;

namespace Game {
    public class SignalGeneratorGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemTerrain m_subsystemTerrain;
        public readonly SubsystemGVSignalGeneratorBlockBehavior m_blockBehavior;
        public uint m_topTopOutput;
        public uint m_rightTopInput;
        public uint m_rightBottomInput;
        public uint m_leftTopInput;
        public uint m_leftBottomInput;
        public uint m_bottomBottomInput;
        public uint m_inBottomInput;
        public WaveType m_waveType;
        public GapBehavior m_gapBehavior;
        public bool m_verticalOffsetSign;
        public bool m_randomAmplitude;
        public int m_gap;
        public int m_circle;
        public int m_horizontalOffset;
        public int m_amplitude;
        public uint m_verticalOffset;
        public int m_lastCircuitStep;
        public int m_step;
        public GVCellFace m_bottomCellFace;
        public readonly Point3 m_bottomPoint3;
        public bool firstRun = true;
        public int m_nowAmplitude;
        public Random m_random = new();

        public SignalGeneratorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace[] cellFaces, uint subterrainId) : base(subsystemGVElectricity, cellFaces, subterrainId) {
            m_subsystemTerrain = SubsystemGVElectricity.SubsystemTerrain;
            m_blockBehavior = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVSignalGeneratorBlockBehavior>(true);
            m_bottomCellFace = cellFaces[0];
            m_bottomPoint3 = m_bottomCellFace.Point;
            uint? output = subsystemGVElectricity.ReadPersistentVoltage(m_bottomPoint3, SubterrainId);
            if (output.HasValue) {
                m_topTopOutput = output.Value;
            }
            SubsystemGVSignalGeneratorBlockBehavior.Data data = m_blockBehavior.GetData(m_bottomPoint3);
            if (data != null) {
                m_step = data.Step;
                m_nowAmplitude = data.NowAmplitude;
            }
        }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(m_bottomCellFace.Face, Rotation, face);
            if (connectorDirection.HasValue) {
                switch (connectorDirection.Value) {
                    case GVElectricConnectorDirection.Top: return m_topTopOutput;
                    case GVElectricConnectorDirection.In: return (uint)m_step;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            uint rightTopInput = m_rightTopInput;
            uint rightBottomInput = m_rightBottomInput;
            uint leftTopInput = m_leftTopInput;
            uint leftBottomInput = m_leftBottomInput;
            uint bottomBottomInput = m_bottomBottomInput;
            bool bottomBottomConnected = false;
            uint inBottomInput = m_inBottomInput;
            m_rightTopInput = 0u;
            m_rightBottomInput = 0u;
            m_leftTopInput = 0u;
            m_leftBottomInput = 0u;
            m_bottomBottomInput = 0u;
            m_inBottomInput = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(m_bottomCellFace.Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Right:
                                if (connection.CellFace == m_bottomCellFace) {
                                    m_rightBottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                }
                                else {
                                    m_rightTopInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) & 0xFFFF00FFu;
                                }
                                break;
                            case GVElectricConnectorDirection.Bottom:
                                m_bottomBottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                bottomBottomConnected = true;
                                break;
                            case GVElectricConnectorDirection.Left:
                                if (connection.CellFace == m_bottomCellFace) {
                                    m_leftBottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                }
                                else {
                                    m_leftTopInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                }
                                break;
                            case GVElectricConnectorDirection.In:
                                m_inBottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                        }
                    }
                }
            }
            bool shouldResetClock = false;
            bool shouldCalculate = false;
            bool shouldStop = false;
            if (m_leftTopInput != leftTopInput) {
                m_verticalOffset = m_leftTopInput;
                shouldCalculate = true;
            }
            if (m_leftBottomInput != leftBottomInput) {
                m_amplitude = MathUint.ToIntWithSign(m_leftBottomInput);
                shouldCalculate = true;
            }
            if (m_rightBottomInput != rightBottomInput) {
                m_horizontalOffset = MathUint.ToIntWithSign(m_rightBottomInput >> 16, 16);
                int circle = (int)(m_rightBottomInput & 0xFFFFu);
                if (m_circle != circle) {
                    m_circle = circle;
                    shouldResetClock = true;
                }
                shouldCalculate = true;
            }
            if (bottomBottomConnected) {
                if (m_inBottomInput != inBottomInput
                    && inBottomInput == 0u) {
                    shouldResetClock = true;
                }
            }
            else {
                switch (m_inBottomInput) {
                    case 0u:
                        shouldResetClock = true;
                        shouldStop = true;
                        break;
                    case < 8u:
                        shouldStop = true;
                        break;
                }
            }
            if (m_rightTopInput != rightTopInput) {
                int gap = MathUint.ToIntWithSign(m_rightTopInput >> 16, 16);
                if (m_gap != gap) {
                    m_gap = gap;
                    shouldResetClock = true;
                }
                m_verticalOffsetSign = ((m_rightTopInput >> 7) & 1u) == 1u;
                m_randomAmplitude = ((m_rightTopInput >> 6) & 1u) == 1u;
                uint gapBehavior = (m_rightTopInput >> 4) & 3u;
                m_gapBehavior = (GapBehavior)(gapBehavior > 2u ? 0u : gapBehavior);
                uint waveType = m_rightTopInput & 0xFu;
                m_waveType = (WaveType)(waveType > 3u ? 0u : waveType);
                shouldCalculate = true;
            }
            if (shouldStop) {
                if (shouldResetClock) {
                    m_step = 0;
                    m_blockBehavior.SetStep(m_bottomPoint3, 0);
                }
                return false;
            }
            int circleWithGap = m_circle + m_gap;
            int step = 0;
            if ((!shouldResetClock && circleWithGap > 0) || firstRun) {
                firstRun = false;
                if (bottomBottomConnected) {
                    if (m_bottomBottomInput == 0
                        || m_bottomBottomInput == bottomBottomInput) {
                        return false;
                    }
                    step = m_step + MathUint.ToIntWithSign(m_bottomBottomInput);
                }
                else if (m_lastCircuitStep != SubsystemGVElectricity.CircuitStep) {
                    m_lastCircuitStep = SubsystemGVElectricity.CircuitStep;
                    step = (m_blockBehavior.GetStep(m_bottomPoint3) ?? -1) + 1;
                    SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, m_lastCircuitStep + 1);
                }
                if (m_randomAmplitude) {
                    if (step < 0
                        || step >= circleWithGap) {
                        m_nowAmplitude = m_random.Int(1, m_amplitude);
                        m_blockBehavior.SetNowAmplitude(m_bottomPoint3, m_nowAmplitude);
                    }
                }
                else {
                    m_nowAmplitude = m_amplitude;
                }
                step %= circleWithGap;
                if (step < 0) {
                    step += circleWithGap;
                }
            }
            m_blockBehavior.SetStep(m_bottomPoint3, step);
            if (step != m_step) {
                m_step = step;
                shouldCalculate = true;
            }
            if (shouldCalculate && circleWithGap > 0) {
                uint topTopOutput = m_topTopOutput;
                m_topTopOutput = 0u;
                int trueStep = (step + m_horizontalOffset) % circleWithGap;
                if (trueStep < 0) {
                    trueStep += circleWithGap;
                }
                long verticalOffset = m_verticalOffset * (m_verticalOffsetSign ? -1L : 1L);
                if (trueStep > m_circle) {
                    switch (m_gapBehavior) {
                        case GapBehavior.Keep:
                            m_topTopOutput = topTopOutput;
                            return false;
                        case GapBehavior.Zero:
                            m_topTopOutput = 0;
                            break;
                        case GapBehavior.VerticalOffset:
                            m_topTopOutput = verticalOffset <= 0L ? 0u : (uint)verticalOffset;
                            break;
                    }
                }
                else {
                    if (m_circle > 0) {
                        long temp = m_waveType switch {
                            WaveType.Sine => (long)(m_nowAmplitude * Math.Sin(trueStep / (double)m_circle * Math.PI * 2d)),
                            WaveType.Triangle => trueStep >= m_circle / 2d ? (long)(2d * m_nowAmplitude * (1d - trueStep / (double)m_circle)) : (long)(2d * m_nowAmplitude * trueStep / m_circle),
                            WaveType.Sawtooth => (long)(m_nowAmplitude * trueStep / (double)m_circle),
                            WaveType.Square => trueStep >= m_circle / 2d ? 0u : m_nowAmplitude,
                            _ => 0u
                        };
                        temp += verticalOffset;
                        if (temp < 0) {
                            m_topTopOutput = 0;
                        }
                        else {
                            m_topTopOutput = (uint)temp;
                        }
                    }
                }
                if (m_topTopOutput != topTopOutput) {
                    SubsystemGVElectricity.WritePersistentVoltage(m_bottomCellFace.Point, m_topTopOutput, SubterrainId);
                    return true;
                }
            }
            return m_step != step;
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            GVCellFace bottomCellFace = CellFaces[0];
            int face = bottomCellFace.Face;
            int data = Terrain.ExtractData(m_subsystemTerrain.Terrain.GetCellValue(bottomCellFace.X, bottomCellFace.Y, bottomCellFace.Z));
            int rotation = RotateableMountedGVElectricElementBlock.GetRotation(data);
            int nextRotation = (rotation + 1) % 4;
            Point3 nextUp = GVSignalGeneratorBlock.m_upPoint3[face * 4 + nextRotation] + bottomCellFace.Point;
            if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(nextUp.X, nextUp.Y, nextUp.Z)) == 0) {
                Point3 faceDirection = -CellFace.FaceToPoint3(face);
                int faceValue = m_subsystemTerrain.Terrain.GetCellValue(nextUp.X + faceDirection.X, nextUp.Y + faceDirection.Y, nextUp.Z + faceDirection.Z);
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(faceValue)];
                if ((block.IsCollidable_(faceValue) && !block.IsFaceTransparent(m_subsystemTerrain, face, faceValue))
                    || (face == 4 && block is FenceBlock)) {
                    m_subsystemTerrain.ChangeCell(nextUp.X, nextUp.Y, nextUp.Z, Terrain.MakeBlockValue(GVSignalGeneratorBlock.Index, 0, RotateableMountedGVElectricElementBlock.SetRotation(GVSignalGeneratorBlock.SetIsTopPart(data, true), nextRotation)));
                    Rotation = nextRotation;
                    Point3 upDirection = GVSignalGeneratorBlock.m_upPoint3[face * 4 + rotation];
                    m_subsystemTerrain.ChangeCell(upDirection.X + bottomCellFace.X, upDirection.Y + bottomCellFace.Y, upDirection.Z + bottomCellFace.Z, 0);
                    return true;
                }
            }
            return false;
        }

        public enum WaveType { Sine, Triangle, Sawtooth, Square }

        public enum GapBehavior { Zero, Keep, VerticalOffset }
    }
}