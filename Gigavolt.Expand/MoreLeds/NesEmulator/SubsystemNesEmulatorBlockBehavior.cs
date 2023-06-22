using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;
using XamariNES.Controller;
using XamariNES.Emulator;

namespace Game {
    public class SubsystemNesEmulatorBlockBehavior : SubsystemEditableItemBehavior<EditGVNesEmulatorDialogData>, IDrawable {
        public SubsystemSky m_subsystemSky;
        public SubsystemGameInfo m_subsystemGameInfo;
        public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();
        public Dictionary<GVNesEmulatorGlowPoint, bool> m_glowPoints = new Dictionary<GVNesEmulatorGlowPoint, bool>();
        public readonly NESEmulator _emu;
        readonly BitmapRenderer _renderer;
        byte[] _frame = new byte[256 * 240];
        public bool EmuStarted;
        public bool RomValid;

        public SubsystemNesEmulatorBlockBehavior() : base(GVNesEmulatorBlock.Index) {
            _emu = new NESEmulator(GetByteFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Gigavolt.Expand.MoreLeds.NesEmulator.nestest.nes")), GetFrameFromEmulator);
            RomValid = true;
            _renderer = new BitmapRenderer();
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            EditGVNesEmulatorDialogData data = GetBlockData(new Point3(-GVNesEmulatorBlock.Index));
            if (data != null
                && data.Data.Length > 0) {
                try {
                    LoadRomFromPath(data.Data);
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
        }

        public override int[] HandledBlocks => new[] { GVNesEmulatorBlock.Index };

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            EditGVNesEmulatorDialogData Data = GetBlockData(new Point3(-GVNesEmulatorBlock.Index)) ?? new EditGVNesEmulatorDialogData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVNesEmulatorDialog(Data, this, delegate { SetBlockData(new Point3(-GVNesEmulatorBlock.Index), Data); }));
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            EditGVNesEmulatorDialogData Data = GetBlockData(new Point3(-GVNesEmulatorBlock.Index)) ?? new EditGVNesEmulatorDialogData();
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVNesEmulatorDialog(
                    Data,
                    this,
                    delegate {
                        SetBlockData(new Point3(-GVNesEmulatorBlock.Index), Data);
                        int face = ((GVNesEmulatorBlock)BlocksManager.Blocks[GVNesEmulatorBlock.Index]).GetFace(value);
                        SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                        GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, face);
                        if (GVElectricElement != null) {
                            subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }

        DateTime _lastUpdatedTime = DateTime.MinValue;

        public static int[] m_drawOrders = { 111 };

        public TexturedBatch3D cachedBatch;
        public int[] DrawOrders => m_drawOrders;

        public void Draw(Camera camera, int drawOrder) {
            bool powerOn = false;
            bool reset = false;
            byte controller1 = 0;
            //byte controller2 = 0;
            foreach (GVNesEmulatorGlowPoint key in m_glowPoints.Keys) {
                if (key.GetPowerOn()) {
                    powerOn = true;
                }
                if (key.GetReset()) {
                    reset = true;
                }
                controller1 |= key.GetController1();
                //controller2 |= key.GetController2();
            }
            if (reset) {
                _emu.Reset();
                EmuStarted = false;
            }
            else {
                if (powerOn) {
                    if (EmuStarted) {
                        _emu.Continue();
                    }
                    else {
                        _emu.Start();
                        EmuStarted = true;
                    }
                    ((NESController)_emu.Controller1).ButtonStates = controller1;
                }
                else {
                    if (EmuStarted) {
                        _emu.Stop();
                    }
                }
            }
            if (!reset
                && powerOn
                && m_glowPoints.Count > 0) {
                DateTime now = DateTime.Now;
                if ((now - _lastUpdatedTime).TotalMilliseconds > 33) {
                    _lastUpdatedTime = now;
                    if (!RomValid) {
                        _frame = _renderer.GenerateNoise();
                    }
                    if (cachedBatch != null) {
                        cachedBatch.Texture.Dispose();
                    }
                    cachedBatch = m_primitivesRenderer.TexturedBatch(
                        Texture2D.Load(_renderer.Render(_frame)),
                        false,
                        0,
                        DepthStencilState.DepthRead,
                        RasterizerState.CullCounterClockwiseScissor,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp
                    );
                }
                foreach (GVNesEmulatorGlowPoint key in m_glowPoints.Keys) {
                    if (key.GetPowerOn()) {
                        Vector3 vector = key.Position - camera.ViewPosition;
                        float num = Vector3.Dot(vector, camera.ViewDirection);
                        if (num > 0.01f) {
                            float num2 = vector.Length();
                            if (num2 < m_subsystemSky.ViewFogRange.Y) {
                                float num3 = key.GetSize() * 0.5f;
                                Vector3 v = (0f - (0.01f + 0.02f * num)) / num2 * vector;
                                Vector3 p = key.Position + num3 * (-key.Right - key.Up * 0.9375f) + v;
                                Vector3 p2 = key.Position + num3 * (key.Right - key.Up * 0.9375f) + v;
                                Vector3 p3 = key.Position + num3 * (key.Right + key.Up * 0.9375f) + v;
                                Vector3 p4 = key.Position + num3 * (-key.Right + key.Up * 0.9375f) + v;
                                switch (key.GetRotation()) {
                                    case 1:
                                        cachedBatch.QueueQuad(
                                            p,
                                            p2,
                                            p3,
                                            p4,
                                            new Vector2(1f, 0f),
                                            new Vector2(1f, 1f),
                                            new Vector2(0f, 1f),
                                            new Vector2(0f, 0f),
                                            Color.White
                                        );
                                        break;
                                    case 2:
                                        cachedBatch.QueueQuad(
                                            p,
                                            p2,
                                            p3,
                                            p4,
                                            new Vector2(0f, 0f),
                                            new Vector2(1f, 0f),
                                            new Vector2(1f, 1f),
                                            new Vector2(0f, 1f),
                                            Color.White
                                        );
                                        break;
                                    case 3:
                                        cachedBatch.QueueQuad(
                                            p,
                                            p2,
                                            p3,
                                            p4,
                                            new Vector2(0f, 1f),
                                            new Vector2(0f, 0f),
                                            new Vector2(1f, 0f),
                                            new Vector2(1f, 1f),
                                            Color.White
                                        );
                                        break;
                                    default:
                                        cachedBatch.QueueQuad(
                                            p,
                                            p2,
                                            p3,
                                            p4,
                                            new Vector2(1f, 1f),
                                            new Vector2(0f, 1f),
                                            new Vector2(0f, 0f),
                                            new Vector2(1f, 0f),
                                            Color.White
                                        );
                                        break;
                                }
                            }
                        }
                    }
                }
                m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
            }
        }

        public GVNesEmulatorGlowPoint AddGlowPoint() {
            GVNesEmulatorGlowPoint glowPoint = new GVNesEmulatorGlowPoint();
            m_glowPoints.Add(glowPoint, true);
            return glowPoint;
        }

        public void RemoveGlowPoint(GVNesEmulatorGlowPoint glowPoint) {
            m_glowPoints.Remove(glowPoint);
        }

        public void LoadRomFromPath(string path) {
            byte[] bytes = null;
            try {
                if (path == "nestest") {
                    bytes = GetByteFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Gigavolt.Expand.NesEmulator.nestest.nes"));
                }
                else if (uint.TryParse(path, NumberStyles.HexNumber, null, out uint uintResult)
                    && GVStaticStorage.GVMBIDDataDictionary.TryGetValue(uintResult, out GVArrayData data)) {
                    if (data.m_worldDirectory == null) {
                        data.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                        data.LoadData();
                    }
                    bytes = data.GetBytes();
                }
                else {
                    bytes = GetByteFromStream(Storage.OpenFile(path, OpenFileMode.Read));
                }
            }
            catch (Exception e) {
                Log.Error(e);
            }
            if (bytes != null) {
                _emu._cartridge.LoadROM(bytes);
                _emu.LoadRom(bytes);
            }
        }

        /// <summary>
        ///     Delegate to receive frame that's ready from the emulator and
        ///     trigger a draw event.
        ///     TODO: Because this isn't thread safe, this might lead to some
        ///     screen tearing. Probably need to refactor this.
        /// </summary>
        /// <param name="frame"></param>
        void GetFrameFromEmulator(byte[] frame) {
            _frame = frame;
            //MessagingCenter.Send(this, "InvalidateEmulatorSurface");
        }

        /// <summary>
        ///     Reads an stream resource to a byte array
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] GetByteFromStream(Stream stream) {
            byte[] output = new byte[stream.Length];
            stream.Read(output, 0, (int)stream.Length);
            return output;
        }
    }
}