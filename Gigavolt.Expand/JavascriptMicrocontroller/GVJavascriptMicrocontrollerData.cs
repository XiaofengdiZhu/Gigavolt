using System;
using System.Linq;
using Acornima.Ast;
using Engine;
using Jint;
using JsEngine = Jint.Engine;

namespace Game {
    public class GVJavascriptMicrocontrollerData : IEditableItemData {
        public static readonly int[] DefaultPortsDefinition = [
            -1,
            -1,
            -1,
            -1,
            -1
        ]; //-1:No input or output. 0:Input. 1:Output

        public int[] m_portsDefinition = (int[])DefaultPortsDefinition.Clone();
        public int m_executeAgain;
        public Prepared<Script> m_script;
        public string LastLoadedCode = string.Empty;
        public readonly JsEngine m_jsEngine;

        public Point3 m_position;

        public static Prepared<Script> InitJs = JsEngine.PrepareScript(
            """
            var Game = importNamespace("Game");
            var Engine = importNamespace("Engine");
            var GameEntitySystem = importNamespace("GameEntitySystem");
            function findSubsystem(name) {//根据名字寻找特定Subsystem，名字不带Subsystem
                let type = Game["Subsystem" + name];
                let project = getProject();
                if (!type || !project) {
                    return null;
                }
                return System.Convert.ChangeType(project.FindSubsystem(type, null, false), type);
            }
            var Project = Game.GameManager.Project;
            var P0 = 0, P1 = 0, P2 = 0, P3 = 0, P4 = 0;
            """
        );

        public GVJavascriptMicrocontrollerData() {
            m_jsEngine = new JsEngine(
                delegate(Options options) {
                    options.AllowClr(AppDomain.CurrentDomain.GetAssemblies());
                    options.TimeoutInterval(TimeSpan.FromSeconds(5));
                }
            );
            m_jsEngine.Execute(InitJs);
            m_jsEngine.SetValue("getPosition", GetPosition);
            m_jsEngine.SetValue("getPortState", GetPortState);
            m_jsEngine.SetValue("setPortDisabled", SetPortDisabled);
            m_jsEngine.SetValue("setPortInput", SetPortInput);
            m_jsEngine.SetValue("setPortOutput", SetPortOutput);
            m_jsEngine.SetValue("executeAgain", ExecuteAgain);
        }

        public IEditableItemData Copy() => new GVJavascriptMicrocontrollerData { m_portsDefinition = (int[])m_portsDefinition.Clone(), m_script = JsEngine.PrepareScript(LastLoadedCode), LastLoadedCode = LastLoadedCode };

        public uint[] Exe(uint[] inputs, Point3 position) {
            m_position = position;
            for (int i = 0; i < 5; i++) {
                if (m_portsDefinition[i] == 0) {
                    m_jsEngine.SetValue($"P{OriginDirection2CustomDirection(i)}", inputs[i]);
                }
            }
            try {
                m_jsEngine.Execute(m_script);
            }
            catch (TimeoutException) {
                Log.Error("Javascript运行超时（5秒）");
            }
            catch (Exception e) {
                Log.Error(e);
            }
            uint[] outputs = [
                0,
                0,
                0,
                0,
                0
            ];
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

        public void SetPortDisabled(object port) {
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

        public void SetPortInput(object port) {
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

        public void SetPortOutput(object port) {
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

        public Point3 GetPosition() => new(m_position.X, m_position.Y, m_position.Z);

        public void ExecuteAgain(object delay) {
            try {
                m_executeAgain = Convert.ToInt32(delay);
            }
            catch (Exception) {
                // ignored
            }
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
                string[] splitStr = str.Split([":::"], StringSplitOptions.None);
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