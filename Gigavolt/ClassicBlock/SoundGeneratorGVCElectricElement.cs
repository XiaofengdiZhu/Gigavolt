using System;
using Engine;

// ReSharper disable RedundantExplicitArraySize

namespace Game {
    public class SoundGeneratorGVCElectricElement : RotateableGVElectricElement {
        public SubsystemNoise m_subsystemNoise;

        public SubsystemParticles m_subsystemParticles;

        public SoundParticleSystem m_particleSystem;

        public Random m_random = new();

        public uint m_lastToneInput;

        public double m_playAllowedTime;

        public string[] m_tones = new string[16] {
            "",
            "Bell",
            "Organ",
            "Ping",
            "String",
            "Trumpet",
            "Voice",
            "Piano",
            "PianoLong",
            "Drums",
            "",
            "",
            "",
            "",
            "",
            "Piano"
        };

        public int[] m_maxOctaves = new int[16] {
            0,
            5,
            5,
            5,
            5,
            5,
            5,
            6,
            6,
            0,
            0,
            0,
            0,
            0,
            0,
            6
        };

        public string[] m_drums = new string[10] {
            "Snare",
            "BassDrum",
            "ClosedHiHat",
            "PedalHiHat",
            "OpenHiHat",
            "LowTom",
            "HighTom",
            "CrashCymbal",
            "RideCymbal",
            "HandClap"
        };

        public SoundGeneratorGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            m_subsystemNoise = subsystemGVElectricity.Project.FindSubsystem<SubsystemNoise>(true);
            m_subsystemParticles = subsystemGVElectricity.Project.FindSubsystem<SubsystemParticles>(true);
            Vector3 vector = CellFace.FaceToVector3(cellFace.Face);
            Vector3 position = new Vector3(cellFace.Point) + new Vector3(0.5f) - 0.2f * vector;
            m_particleSystem = new SoundParticleSystem(subsystemGVElectricity.SubsystemTerrain, position, vector);
        }

        public override bool Simulate() {
            uint num = 0;
            uint num2 = 15;
            uint num3 = 2;
            uint num4 = 0;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection.Value == GVElectricConnectorDirection.In
                            || connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                            num4 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) & 0xfu;
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                            num = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) & 0xfu;
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                            num3 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) & 0xfu;
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                            num2 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) & 0xfu;
                        }
                    }
                }
            }
            if (m_lastToneInput == 0
                && num4 != 0
                && num != 15
                && SubsystemGVElectricity.SubsystemTime.GameTime >= m_playAllowedTime) {
                m_playAllowedTime = SubsystemGVElectricity.SubsystemTime.GameTime + 0.079999998211860657;
                string text = m_tones[num4];
                float num5 = 0f;
                string text2 = null;
                if (text == "Drums") {
                    num5 = 1f;
                    if (num < m_drums.Length) {
                        text2 = $"Audio/SoundGenerator/Drums{m_drums[num]}";
                    }
                }
                else if (!string.IsNullOrEmpty(text)) {
                    float num6 = 130.8125f * MathF.Pow(1.05946314f, num + 12f * num3);
                    int num7 = 0;
                    for (int i = 4; i <= m_maxOctaves[num4]; i++) {
                        float num8 = num6 / (523.25f * MathF.Pow(2f, i - 5f));
                        if (num7 == 0
                            || (num8 >= 0.5f && num8 < num5)) {
                            num7 = i;
                            num5 = num8;
                        }
                    }
                    text2 = $"Audio/SoundGenerator/{text}C{num7}";
                }
                if (num5 != 0f
                    && !string.IsNullOrEmpty(text2)) {
                    GVCellFace cellFace = CellFaces[0];
                    Vector3 position = new(cellFace.X, cellFace.Y, cellFace.Z);
                    float volume = num2 / 15f;
                    float pitch = MathUtils.Clamp(MathF.Log(num5) / MathF.Log(2f), -1f, 1f);
                    float minDistance = 0.5f + 5f * num2 / 15f;
                    SubsystemGVElectricity.SubsystemAudio.PlaySound(
                        text2,
                        volume,
                        pitch,
                        position,
                        minDistance,
                        true
                    );
                    float loudness = num2 < 8 ? 0.25f : 0.5f;
                    float range = MathUtils.Lerp(2f, 20f, num2 / 15f);
                    m_subsystemNoise.MakeNoise(position, loudness, range);
                    if (m_particleSystem.SubsystemParticles == null) {
                        m_subsystemParticles.AddParticleSystem(m_particleSystem);
                    }
                    Vector3 hsv = new(22.5f * num + m_random.Float(0f, 22f), 0.5f + num2 / 30f, 1f);
                    m_particleSystem.AddNote(new Color(Color.HsvToRgb(hsv)));
                }
            }
            m_lastToneInput = num4;
            return false;
        }
    }
}