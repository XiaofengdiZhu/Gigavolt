using System;
using System.Linq;
using Engine;
using Esprima.Ast;
using GameEntitySystem;
using Jint;
using JsEngine = Jint.Engine;

namespace Game {
    public class GVJavascriptMicrocontrollerData : IEditableItemData {
        public static readonly int[] DefaultPortsDefinition = { -1, -1, -1, -1, -1 }; //-1:No input or output. 0:Input. 1:Output
        public int[] m_portsDefinition = (int[])DefaultPortsDefinition.Clone();
        public Script m_script;
        public string LastLoadedCode = string.Empty;
        public JsEngine m_jsEngine;

        public Point3 m_position;

        public static Script InitJs = JsEngine.PrepareScript(
            """
            var Game = importNamespace("Game");
            var Engine = importNamespace("Engine");
            var GameEntitySystem = importNamespace("GameEntitySystem");
            var findSubsystem = Game.JsInterface.findSubsystem;//根据名字寻找特定Subsystem，名字不带Subsystem
            var Project = Game.JsInterface.getProject();
            var P0 = 0, P1 = 0, P2 = 0, P3 = 0, P4 = 0;
            """
        );

        public GVJavascriptMicrocontrollerData() {
            m_jsEngine = new JsEngine(
                delegate(Options options) {
                    options.AllowClr();
                    options.AllowClr(typeof(Program).Assembly, typeof(Matrix).Assembly, typeof(Project).Assembly);
                }
            );
            m_jsEngine.Execute(InitJs);
            m_jsEngine.SetValue("GetPosition", GetPosition);
            m_jsEngine.SetValue("GetPortState", GetPortState);
            m_jsEngine.SetValue("SetPortAsDisabled", SetPortAsDisabled);
            m_jsEngine.SetValue("SetPortAsInput", SetPortAsInput);
            m_jsEngine.SetValue("SetPortAsOutput", SetPortAsOutput);
        }

        public IEditableItemData Copy() {
            GVJavascriptMicrocontrollerData result = new() { m_portsDefinition = (int[])m_portsDefinition.Clone(), m_script = JsEngine.PrepareScript(LastLoadedCode) };
            return result;
        }

        public uint[] Exe(uint[] inputs, Point3 position) {
            m_position = position;
            for (int i = 0; i < 5; i++) {
                if (m_portsDefinition[i] == 0) {
                    m_jsEngine.SetValue($"P{OriginDirection2CustomDirection(i)}", inputs[i]);
                }
            }
            m_jsEngine.Execute(m_script);
            uint[] outputs = { 0, 0, 0, 0, 0 };
            for (int i = 0; i < 5; i++) {
                if (m_portsDefinition[i] == 1) {
                    try {
                        outputs[i] = Convert.ToUInt32(m_jsEngine.GetValue($"P{OriginDirection2CustomDirection(i)}").AsNumber());
                    }
                    catch (Exception) {
                        // ignored
                    }
                }
            }
            return outputs;
        }

        public string GetPortState(object port) {
            int num = -1;
            try {
                num = Convert.ToInt32(port);
            }
            catch (Exception) {
                // ignored
            }
            if (num is >= 0 and < 5) {
                return m_portsDefinition[CustomDirection2OriginDirection(num)] switch {
                    0 => "input",
                    1 => "output",
                    _ => "disabled"
                };
            }
            return "error";
        }

        public void SetPortAsDisabled(object port) {
            int num = -1;
            try {
                num = Convert.ToInt32(port);
            }
            catch (Exception) {
                // ignored
            }
            if (num is >= 0 and < 5) {
                m_portsDefinition[CustomDirection2OriginDirection(num)] = -1;
            }
        }

        public void SetPortAsInput(object port) {
            int num = -1;
            try {
                num = Convert.ToInt32(port);
            }
            catch (Exception) {
                // ignored
            }
            if (num is >= 0 and < 5) {
                m_portsDefinition[CustomDirection2OriginDirection(num)] = 0;
            }
        }

        public void SetPortAsOutput(object port) {
            int num = -1;
            try {
                num = Convert.ToInt32(port);
            }
            catch (Exception) {
                // ignored
            }
            if (num is >= 0 and < 5) {
                m_portsDefinition[CustomDirection2OriginDirection(num)] = 1;
            }
        }

        public int[] GetPosition() {
            return new[] { m_position.X, m_position.Y, m_position.Z };
        }

        public void LoadCode(string code, out string error) {
            error = null;
            try {
                m_script = JsEngine.PrepareScript(code);
                LastLoadedCode = code;
            }
            catch (Exception e) {
                error = e.ToString();
                Log.Error(error);
            }
        }

        public void LoadString(string str, out string error) {
            error = null;
            try {
                string[] splitStr = str.Split(new[] { ":::" }, StringSplitOptions.None);
                if (splitStr.Length != 2) {
                    throw new Exception("不是正确的JS单片机存储的数据");
                }
                m_portsDefinition = splitStr[0].Split(';').Select(int.Parse).ToArray();
                if (m_portsDefinition.Length != 5) {
                    throw new Exception("不是正确的JS单片机存储的数据");
                }
                m_script = JsEngine.PrepareScript(splitStr[1]);
                LastLoadedCode = splitStr[1];
            }
            catch (Exception e) {
                error = e.ToString();
                Log.Error(error);
            }
        }

        public string SaveString() => SaveString(true);

        public string SaveString(bool saveLastOutput) => $"{string.Join(";", m_portsDefinition)}:::{LastLoadedCode}";

        public void LoadString(string data) {
            LoadString(data, out _);
        }

        public static int OriginDirection2CustomDirection(int originDirection) {
            return originDirection switch {
                4 => 0,
                1 => 4,
                3 => 2,
                _ => originDirection + 1
            };
        }

        public static int CustomDirection2OriginDirection(int customDirection) {
            return customDirection switch {
                0 => 4,
                4 => 1,
                2 => 3,
                _ => customDirection - 1
            };
        }
    }
}