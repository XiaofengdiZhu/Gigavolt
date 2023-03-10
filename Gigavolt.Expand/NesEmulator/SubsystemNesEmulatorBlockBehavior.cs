using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using TemplatesDatabase;
using XamariNES.Emulator;

namespace Game
{
    public class SubsystemNesEmulatorBlockBehavior : SubsystemEditableItemBehavior<GigaVoltageLevelData>, IDrawable
    {
        public SubsystemSky m_subsystemSky;
        public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();
        public Dictionary<GVNesEmulatorGlowPoint, bool> m_glowPoints = new Dictionary<GVNesEmulatorGlowPoint, bool>();
        private readonly NESEmulator _emu;
        private readonly BitmapRenderer _renderer;
        private byte[] _frame = new byte[256 * 240];
        public bool EmuStarted = false;
        public bool RomValid = false;
        public SubsystemNesEmulatorBlockBehavior() : base(1023)
        {
            _emu = new NESEmulator(GetByteFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Gigavolt.Expand.NesEmulator.test2.nes")), GetFrameFromEmulator, XamariNES.Emulator.Enums.enumEmulatorSpeed.Normal);
            RomValid = true;
            _renderer = new BitmapRenderer();
        }
        public override void Load(ValuesDictionary valuesDictionary)
        {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(throwOnError: true);
            //Project.FindSubsystem<SubsystemGVElectricBlockBehavior>(true).HandledBlocks.Append(1023);
        }
        public override int[] HandledBlocks => new int[1]
        {
            1023
        };
        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
        {
            return true;
        }
        private DateTime _lastUpdatedTime = DateTime.MinValue;
        public static int[] m_drawOrders = new int[1]
        {
            110
        };
        public TexturedBatch3D cachedBatch;
        public int[] DrawOrders => m_drawOrders;
        public void Draw(Camera camera, int drawOrder)
        {
            bool powerOn = false;
            bool reset = false;
            byte controler1 = 0;
            byte controler2 = 0;
            foreach (GVNesEmulatorGlowPoint key in m_glowPoints.Keys)
            {
                if (key.GetPowerOn())
                {
                    powerOn = true;
                }
                if (key.GetReset())
                {
                    reset = true;
                }
                controler1 |= key.GetControler1();
                controler2 |= key.GetControler2();
            }
            if (reset)
            {
                _emu.Reset();
                EmuStarted = false;
            }
            else
            {
                if (powerOn)
                {
                    if (EmuStarted)
                    {
                        _emu.Continue();
                    }
                    else
                    {
                        _emu.Start();
                        EmuStarted = true;
                    }
                    ((XamariNES.Controller.NESController)(_emu.Controller1)).ButtonStates = controler1;
                }
                else
                {
                    if (EmuStarted)
                    {
                        _emu.Stop();
                    }
                }
            }
            if (!reset && powerOn && m_glowPoints.Count > 0)
            {
                DateTime now = DateTime.Now;
                if ((now - _lastUpdatedTime).TotalMilliseconds > 33)
                {
                    _lastUpdatedTime = now;
                    if (!RomValid)
                    {
                        _frame = _renderer.GenerateNoise();
                    }
                    var a = ((XamariNES.Controller.NESController)(_emu.Controller1));
                    cachedBatch = m_primitivesRenderer.TexturedBatch(Texture2D.Load(_renderer.Render(_frame)), false, 0, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwiseScissor, BlendState.AlphaBlend, SamplerState.PointClamp);
                }
                foreach (GVNesEmulatorGlowPoint key in m_glowPoints.Keys)
                {
                    if (key.GetPowerOn())
                    {
                        Vector3 vector = key.Position - camera.ViewPosition;
                        float num = Vector3.Dot(vector, camera.ViewDirection);
                        if (num > 0.01f)
                        {
                            float num2 = vector.Length();
                            if (num2 < m_subsystemSky.ViewFogRange.Y)
                            {
                                float num3 = (float)key.GetSize()*0.5f;
                                Vector3 v = (0f - (0.01f + 0.02f * num)) / num2 * vector;
                                Vector3 p = key.Position + num3 * (-key.Right - key.Up * 0.9375f) + v;
                                Vector3 p2 = key.Position + num3 * (key.Right - key.Up * 0.9375f) + v;
                                Vector3 p3 = key.Position + num3 * (key.Right + key.Up * 0.9375f) + v;
                                Vector3 p4 = key.Position + num3 * (-key.Right + key.Up * 0.9375f) + v;
                                switch (key.GetRotation())
                                {
                                    case 1:
                                        cachedBatch.QueueQuad(p, p2, p3, p4, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), Color.White);
                                        break;
                                    case 2:
                                        cachedBatch.QueueQuad(p, p2, p3, p4, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Color.White);
                                        break;
                                    case 3:
                                        cachedBatch.QueueQuad(p, p2, p3, p4, new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), Color.White);
                                        break;
                                    default:
                                        cachedBatch.QueueQuad(p, p2, p3, p4, new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f), Color.White);
                                        break;
                                }
                            }
                        }
                    }
                }
                m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
            }
        }
        public GVNesEmulatorGlowPoint AddGlowPoint()
        {
            var glowPoint = new GVNesEmulatorGlowPoint();
            m_glowPoints.Add(glowPoint, value: true);
            return glowPoint;
        }

        public void RemoveGlowPoint(GVNesEmulatorGlowPoint glowPoint)
        {
            m_glowPoints.Remove(glowPoint);
        }
        /// <summary>
        ///     Delegate to receive frame that's ready from the emulator and
        ///     trigger a draw event.
        ///
        ///     TODO: Because this isn't thread safe, this might lead to some
        ///     screen tearing. Probably need to refactor this.
        /// </summary>
        /// <param name="frame"></param>
        private void GetFrameFromEmulator(byte[] frame)
        {
            _frame = frame;
            //MessagingCenter.Send(this, "InvalidateEmulatorSurface");
        }
        /// <summary>
        ///     Reads an stream resource to a byte array
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public byte[] GetByteFromStream(System.IO.Stream stream)
        {
            byte[] output;
            output = new byte[stream.Length];
            stream.Read(output, 0, (int)stream.Length);
            return output;
        }
    }
}
