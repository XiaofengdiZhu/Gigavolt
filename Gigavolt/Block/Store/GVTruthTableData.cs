using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Engine;
using NCalc;

namespace Game {
    public class GVTruthTableData : IEditableItemData {
        public class SectionInput {
            public uint i1;
            public uint i2;
            public uint i3;
            public uint i4;
            public SectionInput() { }

            public SectionInput(uint i1, uint i2, uint i3, uint i4) {
                this.i1 = i1;
                this.i2 = i2;
                this.i3 = i3;
                this.i4 = i4;
            }

            public override bool Equals(object obj) {
                if (obj == null
                    || GetType() != obj.GetType()) {
                    return false;
                }
                return Equals((SectionInput)obj);
            }

            public bool Equals(SectionInput other) => i1 == other.i1 && i2 == other.i2 && i3 == other.i3 && i4 == other.i4;
            public override int GetHashCode() => base.GetHashCode();

            public override string ToString() => $"{i1},{i2},{i3},{i4}";
        }

        public class Line {
            public List<Func<SectionInput, bool>[]> i = new List<Func<SectionInput, bool>[]>();
            public Func<SectionInput, uint> o = u => 0u;

            public uint? Exe(List<SectionInput> inputs) {
                int length = Math.Min(i.Count, inputs.Count);
                try {
                    for (int j = 0; j < length; j++) {
                        SectionInput input = inputs[inputs.Count - length + j];
                        for (int k = 0; k < 4; k++) {
                            if (!i[j][k](input)) {
                                return null;
                            }
                        }
                    }
                    return o(inputs[inputs.Count - 1]);
                }
                catch (Exception e) {
                    Log.Error(e);
                    return null;
                }
            }
        }

        public List<Line> Data;
        public uint LastOutput { get; set; }
        public string LastLoadedString = string.Empty;

        public IEditableItemData Copy() {
            GVTruthTableData result = new GVTruthTableData { LastLoadedString = LastLoadedString, LastOutput = LastOutput };
            result.LoadString(LastLoadedString);
            return result;
        }

        public uint Exe(List<SectionInput> inputs) {
            for (int i = 0; i < Data.Count; i++) {
                uint? output = Data[i].Exe(inputs);
                if (output.HasValue) {
                    return output.Value;
                }
            }
            return 0u;
        }

        public Regex hexRegex = new Regex(@"0[xX][\dabcdefABCDEF]+");
        public Regex binRegex = new Regex(@"0[bB][01]+");

        public void LoadString(string str, out string error) {
            error = null;
            List<Line> newData = new List<Line>();
            string replacedString = str;
            replacedString = hexRegex.Replace(replacedString, m => long.Parse(m.Value.Substring(2), NumberStyles.HexNumber).ToString());
            replacedString = binRegex.Replace(replacedString, m => Convert.ToUInt32(m.Value.Substring(2), 2).ToString());
            replacedString = replacedString.Replace("\n", "");
            string[] linesString = replacedString.Split(new[] { "::" }, StringSplitOptions.None);
            foreach (string lineString in linesString) {
                Line line = new Line();
                string[] temp = lineString.Split(':');
                if (temp.Length < 2) {
                    error = $"{lineString}未找到输出";
                    Log.Error(error);
                    return;
                }
                Expression oe;
                try {
                    oe = new Expression(CookForExpression(temp[1], "o"));
                }
                catch (Exception e) {
                    error = $"{temp[1]}存在错误:\n{e}";
                    Log.Error(error);
                    return;
                }
                line.o = oe.ToLambda<SectionInput, uint>();
                string[] sectionStrings = temp[0].Split(new[] { ";;" }, StringSplitOptions.None);
                if (sectionStrings.Length > 16) {
                    error = "其中一套输入规则参试获取15次输入变化前的输入";
                    Log.Error(error);
                    return;
                }
                for (int i = 0; i < sectionStrings.Length; i++) {
                    Func<SectionInput, bool>[] iF = new Func<SectionInput, bool>[4];
                    string sectionString = sectionStrings[i];
                    string[] inputStrings = sectionString.Split(';');
                    for (int j = 0; j < 4; j++) {
                        if (j >= inputStrings.Length) {
                            iF[j] = u => true;
                        }
                        else {
                            string inputString = inputStrings[j];
                            if (inputString.Length == 0) {
                                iF[j] = u => true;
                                continue;
                            }
                            Expression ie = new Expression(CookForExpression(inputString, $"i{j + 1}"));
                            if (ie.HasErrors()) {
                                error = $"{inputString}存在错误:\n{ie.Error}";
                                Log.Error(error);
                                return;
                            }
                            iF[j] = ie.ToLambda<SectionInput, bool>();
                        }
                    }
                    line.i.Add(iF);
                }
                newData.Add(line);
            }
            Data = newData;
            LastLoadedString = str;
        }

        public Regex addBracketRegex = new Regex(@"(i\d)");
        public string[] notCookOperators = { "=", "!", "not", ">", "<", "and", "&&", "||", "or" };

        public string CookForExpression(string input, string category) {
            if (category.StartsWith("i")) {
                if (input == "true"
                    || input == "") {
                    return "true";
                }
                if (input.StartsWith("=")
                    || input.StartsWith("!=")
                    || input.StartsWith("<")
                    || input.StartsWith(">")) {
                    input = $"{category}{input}";
                }
                else if (!notCookOperators.Any(item => input.Contains(item))) {
                    input = $"{category}={input}";
                }
            }
            else if (category == "o"
                && input.StartsWith("=")) {
                input = input.Substring(1);
            }
            return addBracketRegex.Replace(input, "[$1]");
        }

        public string SaveString() => SaveString(true);

        public string SaveString(bool saveLastOutput) => LastLoadedString;

        public void LoadString(string data) {
            LoadString(data, out _);
        }
    }
}