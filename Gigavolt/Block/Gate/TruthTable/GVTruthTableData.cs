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
            public long i1;
            public long i2;
            public long i3;
            public long i4;
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

            public override int GetHashCode() {
                int hash = 17;
                // ReSharper disable NonReadonlyMemberInGetHashCode
                hash = hash * 23 + i1.GetHashCode();
                hash = hash * 23 + i2.GetHashCode();
                hash = hash * 23 + i3.GetHashCode();
                hash = hash * 23 + i4.GetHashCode();
                return hash;
            }

            public override string ToString() => $"{i1},{i2},{i3},{i4}";
        }

        public class Line {
            public readonly List<Func<SectionInput, bool>[]> i = [];
            public Func<SectionInput, uint> o = _ => 0u;

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
                    return o(inputs[^1]);
                }
                catch (Exception e) {
                    Log.Error(e);
                    return null;
                }
            }
        }

        public List<Line> Data;
        public string LastLoadedString = string.Empty;

        public IEditableItemData Copy() {
            GVTruthTableData result = new() { LastLoadedString = LastLoadedString };
            result.LoadString(LastLoadedString);
            return result;
        }

        public uint Exe(List<SectionInput> inputs) {
            if (Data != null) {
                foreach (Line line in Data) {
                    uint? output = line.Exe(inputs);
                    if (output.HasValue) {
                        return output.Value;
                    }
                }
            }
            return 0u;
        }

        public Regex hexRegex = new(@"0[xX][\dabcdefABCDEF]+");
        public Regex binRegex = new(@"0[bB][01]+");

        public void LoadString(string str, out string error) {
            error = null;
            List<Line> newData = [];
            string replacedString = str;
            replacedString = hexRegex.Replace(replacedString, m => long.Parse(m.Value.Substring(2), NumberStyles.HexNumber).ToString());
            replacedString = binRegex.Replace(replacedString, m => Convert.ToUInt32(m.Value.Substring(2), 2).ToString());
            replacedString = replacedString.Replace("\n", "").Replace("PI()", "3.141592653589793").Replace("E()", "2.718281828459045");
            string[] linesString = replacedString.Split(["::"], StringSplitOptions.None);
            foreach (string lineString in linesString) {
                Line line = new();
                string[] temp = lineString.Split(':');
                if (temp.Length < 2) {
                    error = $"{lineString}未找到输出";
                    Log.Error(error);
                    return;
                }
                try {
                    line.o = new Expression(CookForExpression(temp[1], "o")).ToLambda<SectionInput, uint>();
                }
                catch (Exception e) {
                    error = $"{temp[1]}存在错误:\n{e}";
                    Log.Error(error);
                    return;
                }
                string[] sectionStrings = temp[0].Split([";;"], StringSplitOptions.None);
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
                            iF[j] = _ => true;
                        }
                        else {
                            string inputString = inputStrings[j];
                            if (inputString.Length == 0) {
                                iF[j] = _ => true;
                                continue;
                            }
                            try {
                                iF[j] = new Expression(CookForExpression(inputString, $"i{j + 1}")).ToLambda<SectionInput, bool>();
                            }
                            catch (Exception e) {
                                error = $"{inputString}存在错误:\n{e}";
                                Log.Error(error);
                                return;
                            }
                        }
                    }
                    line.i.Add(iF);
                }
                newData.Add(line);
            }
            Data = newData;
            LastLoadedString = str;
        }

        public Regex addBracketRegex = new(@"(i\d)");

        public string[] notCookOperators = [
            "=",
            "!",
            "not",
            ">",
            "<",
            "and",
            "&&",
            "||",
            "or"
        ];

        public string CookForExpression(string input, string category) {
            if (category.StartsWith("i")) {
                if (input is "true" or "") {
                    return "true";
                }
                if (input.StartsWith('=')
                    || input.StartsWith("!=")
                    || input.StartsWith('<')
                    || input.StartsWith('>')) {
                    input = $"{category}{input}";
                }
                else if (!notCookOperators.Any(item => input.Contains(item))) {
                    input = $"{category}={input}";
                }
            }
            else if (category == "o"
                && input.StartsWith('=')) {
                input = input.Substring(1);
            }
            return addBracketRegex.Replace(input, "[$1]");
        }

        public string SaveString() => LastLoadedString;

        public void LoadString(string data) {
            LoadString(data, out _);
        }
    }
}