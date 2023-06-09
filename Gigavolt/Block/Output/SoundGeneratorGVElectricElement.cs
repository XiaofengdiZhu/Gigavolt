using System;
using Engine;
using Engine.Audio;

namespace Game {
    public class SoundGeneratorGVElectricElement : RotateableGVElectricElement {
        public SubsystemNoise m_subsystemNoise;
        public SubsystemAudio m_subsystemAudio;
        public SubsystemGameInfo m_subsystemGameInfo;

        public uint m_lastBottomInput;
        public uint m_lastInInput;

        public Sound m_sound;
        public float m_volume = 1f;
        public bool m_playing;
        public DateTime m_lastNoiseTime = DateTime.MinValue;

        public SoundGeneratorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            m_subsystemNoise = subsystemGVElectricity.Project.FindSubsystem<SubsystemNoise>(true);
            m_subsystemAudio = subsystemGVElectricity.Project.FindSubsystem<SubsystemAudio>(true);
            m_subsystemGameInfo = subsystemGVElectricity.Project.FindSubsystem<SubsystemGameInfo>(true);
            GVStaticStorage.GVSGCFEEList.Add(this);
        }

        public override bool Simulate() {
            uint leftInput = 0;
            uint topInput = uint.MaxValue;
            uint rightInput = 0;
            uint bottomInput = 0;
            uint inInput = 0;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                            inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                            bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                            leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                            rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                            topInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                    }
                }
            }
            if (bottomInput == 0) {
                if (m_lastBottomInput > 0) {
                    if (m_sound != null && m_playing) {
                        m_sound.Stop();
                        m_playing = false;
                    }
                }
                if (inInput != m_lastInInput) {
                    if (m_sound != null) {
                        m_sound.Dispose();
                    }
                    if (inInput > 0) {
                        if (GVStaticStorage.GVMBIDDataDictionary.TryGetValue(inInput, out GVArrayData GVMBData)) {
                            try {
                                if (GVMBData.m_worldDirectory == null) {
                                    GVMBData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                                    GVMBData.LoadData();
                                }
                                short[] shorts = GVMBData.Data2Shorts();
                                int startIndex = MathUint.ToInt(topInput);
                                int itemsCount = MathUint.ToInt(rightInput);
                                if (itemsCount > shorts.Length
                                    || startIndex + itemsCount > shorts.Length) {
                                    itemsCount = shorts.Length - startIndex;
                                }
                                if (itemsCount < 0) {
                                    itemsCount = 0;
                                }
                                m_sound = new Sound(
                                    new SoundBuffer(
                                        shorts,
                                        startIndex,
                                        itemsCount,
                                        2,
                                        MathUint.ToInt(leftInput)
                                    )
                                );
                            }
                            catch (Exception ex) {
                                string error = $"{CellFaces[0].Point}的GV声音发生器加载ID为{inInput.ToString("X", null)}的存储器中的音频数据时出错";
                                foreach (ComponentPlayer componentPlayer in SubsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true).ComponentPlayers) {
                                    componentPlayer.ComponentGui.DisplaySmallMessage(error, Color.White, true, true);
                                }
                                Log.Error($"{error}，加载起始位置为{topInput}（均为十进制），加载short数量为{rightInput}，声道数为2，采样率为{leftInput}，详细报错：\n{ex}");
                            }
                        }
                        else {
                            m_sound = null;
                            string error = $"{CellFaces[0].Point}的GV声音发生器无法找到ID为{inInput.ToString("X", null)}的存储器";
                            Log.Error(error);
                            foreach (ComponentPlayer componentPlayer in SubsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true).ComponentPlayers) {
                                componentPlayer.ComponentGui.DisplaySmallMessage(error, Color.White, true, true);
                            }
                        }
                    }
                    m_lastInInput = inInput;
                }
                m_lastBottomInput = 0;
            }
            else {
                if (bottomInput != m_lastBottomInput) {
                    if (m_sound != null) {
                        m_volume = bottomInput / (float)uint.MaxValue;
                        if (m_lastBottomInput == 0) {
                            CellFace cellFace = CellFaces[0];
                            m_sound.Volume = m_volume * m_subsystemAudio.CalculateVolume(m_subsystemAudio.CalculateListenerDistance(new Vector3(cellFace.X, cellFace.Y, cellFace.Z)), 0.5f + 5f * m_volume);
                            m_sound.Play();
                            m_playing = true;
                        }
                    }
                    m_lastBottomInput = bottomInput;
                }
            }
            if (m_playing && (DateTime.Now - m_lastNoiseTime).TotalSeconds > 1) {
                m_lastNoiseTime = DateTime.Now;
                CellFace cellFace = CellFaces[0];
                m_subsystemNoise.MakeNoise(new Vector3(cellFace.X, cellFace.Y, cellFace.Z), m_volume < 0.5f ? 0.25f : 0.5f, MathUtils.Lerp(2f, 20f, m_volume));
            }
            return false;
        }

        public override void OnRemoved() {
            if (m_sound != null) {
                if (m_playing) {
                    m_sound.Stop();
                }
                m_sound.Dispose();
            }
            GVStaticStorage.GVSGCFEEList.Remove(this);
        }
    }
}