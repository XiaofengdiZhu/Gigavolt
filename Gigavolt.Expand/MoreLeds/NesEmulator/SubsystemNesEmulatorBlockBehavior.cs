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
        public SubsystemGameInfo m_subsystemGameInfo;
        public readonly Dictionary<uint, HashSet<GVNesEmulatorGlowPoint>> m_glowPoints = new();
        public readonly NESEmulator _emu;
        readonly BitmapRenderer _renderer;
        byte[] _frame = new byte[256 * 240];
        public bool EmuStarted;
        public readonly bool RomValid;

        public SubsystemNesEmulatorBlockBehavior() : base(BlocksManager.GetBlockIndex<GVNesEmulatorBlock>()) {
            _emu = new NESEmulator(GetByteFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Game.MoreLeds.NesEmulator.nestest.nes")), GetFrameFromEmulator);
            RomValid = true;
            _renderer = new BitmapRenderer();
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            EditGVNesEmulatorDialogData data = GetBlockData(new Point3(-875));
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

        public override int[] HandledBlocks => [BlocksManager.GetBlockIndex<GVNesEmulatorBlock>()];

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            EditGVNesEmulatorDialogData Data = GetBlockData(new Point3(-875)) ?? new EditGVNesEmulatorDialogData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVNesEmulatorDialog(Data, this, delegate { SetBlockData(new Point3(-875), Data); }));
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            EditGVNesEmulatorDialogData Data = GetBlockData(new Point3(-875)) ?? new EditGVNesEmulatorDialogData();
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVNesEmulatorDialog(
                    Data,
                    this,
                    delegate {
                        SetBlockData(new Point3(-875), Data);
                        int face = GVBlocksManager.GetBlock<GVNesEmulatorBlock>().GetFace(value);
                        SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                        GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(
                            x,
                            y,
                            z,
                            face,
                            0
                        );
                        if (GVElectricElement != null) {
                            subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }

        DateTime _lastUpdatedTime = DateTime.MinValue;

        public static readonly int[] m_drawOrders = [111];

        public readonly TexturedBatch3D cachedBatch = new() {
            BlendState = BlendState.AlphaBlend,
            DepthStencilState = DepthStencilState.DepthRead,
            Layer = 0,
            RasterizerState = RasterizerState.CullCounterClockwiseScissor,
            SamplerState = SamplerState.PointClamp
        };

        public int[] DrawOrders => m_drawOrders;

        public void Draw(Camera camera, int drawOrder) {
            bool powerOn = false;
            bool reset = false;
            byte controller1 = 0;
            //byte controller2 = 0;
            foreach ((uint _, HashSet<GVNesEmulatorGlowPoint> points) in m_glowPoints) {
                if (points.Count == 0) {
                    continue;
                }
                foreach (GVNesEmulatorGlowPoint key in points) {
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
                    cachedBatch.Texture?.Dispose();
                    cachedBatch.Texture = Texture2D.Load(_renderer.Render(_frame));
                }
                foreach ((uint subterrainId, HashSet<GVNesEmulatorGlowPoint> points) in m_glowPoints) {
                    if (points.Count == 0) {
                        continue;
                    }
                    Matrix transform = subterrainId == 0 ? default : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform;
                    foreach (GVNesEmulatorGlowPoint key in points) {
                        if (key.GetPowerOn()) {
                            Vector3 position = key.Position;
                            float halfSize = key.GetSize() * 0.5f;
                            Vector3 right = key.Right * halfSize;
                            Vector3 up = key.Up * halfSize * 0.9375f;
                            if (subterrainId != 0) {
                                position = Vector3.Transform(position, transform);
                                Matrix orientation = transform.OrientationMatrix;
                                right = Vector3.Transform(right, orientation);
                                up = Vector3.Transform(up, orientation);
                            }
                            Vector3[] offsets = [right - up, right + up, -right - up, -right + up];
                            Vector3 min = Vector3.Zero;
                            Vector3 max = Vector3.Zero;
                            foreach (Vector3 offset in offsets) {
                                min.X = Math.Min(min.X, offset.X);
                                min.Y = Math.Min(min.Y, offset.Y);
                                min.Z = Math.Min(min.Z, offset.Z);
                                max.X = Math.Max(max.X, offset.X);
                                max.Y = Math.Max(max.Y, offset.Y);
                                max.Z = Math.Max(max.Z, offset.Z);
                            }
                            if (camera.ViewFrustum.Intersection(new BoundingBox(position + min, position + max))) {
                                Vector3 direction = position - camera.ViewPosition;
                                position -= (0.01f + 0.02f * Vector3.Dot(direction, camera.ViewDirection)) / direction.Length() * direction;
                                Vector3 p = position - right - up;
                                Vector3 p2 = position + right - up;
                                Vector3 p3 = position + right + up;
                                Vector3 p4 = position - right + up;
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
                cachedBatch.Flush(camera.ViewProjectionMatrix);
            }
        }

        public GVNesEmulatorGlowPoint AddGlowPoint(uint subterrainId) {
            GVNesEmulatorGlowPoint glowPoint = new();
            if (m_glowPoints.TryGetValue(subterrainId, out HashSet<GVNesEmulatorGlowPoint> points)) {
                points.Add(glowPoint);
            }
            else {
                m_glowPoints.Add(subterrainId, [glowPoint]);
            }
            return glowPoint;
        }

        public void RemoveGlowPoint(GVNesEmulatorGlowPoint glowPoint, uint subterrainId) {
            m_glowPoints[subterrainId]?.Remove(glowPoint);
        }

        public void LoadRomFromPath(string path) {
            byte[] bytes = null;
            try {
                // ReSharper disable once StringLiteralTypo
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
                    using (Stream stream = Storage.OpenFile(path, OpenFileMode.Read)) {
                        bytes = GetByteFromStream(stream);
                    }
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
        ///     Reads a stream resource to a byte array
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] GetByteFromStream(Stream stream) {
            byte[] output = new byte[stream.Length];
            _ = stream.Read(output, 0, (int)stream.Length);
            return output;
        }
    }
}