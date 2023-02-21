using Engine;
using System;
using System.Collections.Generic;
using System.Text;
using NCalc;
using System.Text.RegularExpressions;
using System.Linq;

namespace Game
{
    public class GVTruthTableData : IEditableItemData
    {
        public class SectionInput
        {
            public uint i1;
            public uint i2;
            public uint i3;
            public uint i4;
            public SectionInput() { }
            public SectionInput(uint i1, uint i2, uint i3, uint i4)
            {
                this.i1 = i1;
                this.i2 = i2;
                this.i3 = i3;
                this.i4 = i4;
            }
            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                return this.Equals((SectionInput)obj);
            }
            public bool Equals(SectionInput other)
            {
                return this.i1 == other.i1 && this.i2 == other.i2 && this.i3 == other.i3 && this.i4 == other.i4;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            public override string ToString()
            {
                return $"{i1},{i2},{i3},{i4}";
            }
        }
        public class Line
        {
            public List<Func<SectionInput, bool>[]> i = new List<Func<SectionInput, bool>[]>();
            public Func<SectionInput, uint> o = u => 0u;
            public uint? Exe(List<SectionInput> inputs)
            {
                int length = Math.Min(i.Count, inputs.Count);
                try
                {
                    for (int j = 0; j < length; j++)
                    {
                        SectionInput input = inputs[inputs.Count - length + j];
                        for (int k = 0; k < 4; k++)
                        {
                            if (!i[j][k](input))
                            {
                                return null;
                            }
                        }
                    }
                    return o(inputs[length - 1]);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return null;
                }
            }
        }
        public List<Line> Data = null;
        public uint LastOutput { get; set; }
        public string LastLoadedString { get; set; }
        public IEditableItemData Copy()
        {
            var result = new GVTruthTableData
            {
                LastLoadedString = LastLoadedString,
                LastOutput = LastOutput
            };
            result.LoadString(LastLoadedString, out string error);
            return result;
        }
        public uint Exe(List<SectionInput> inputs)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                uint? output = Data[i].Exe(inputs);
                if (output.HasValue)
                {
                    return output.Value;
                }
            }
            return 0u;
        }
        public Regex hexRegex = new Regex(@"0x[\dabcdefABCDEF]+");
        public void LoadString(string data, out string error)
        {
            error = null;
            List<Line> newData = new List<Line>();
            data = hexRegex.Replace(data, new MatchEvaluator(
                (Match m) =>
                {
                    return long.Parse(m.Value.Substring(2), System.Globalization.NumberStyles.HexNumber).ToString();
                }
            ));
            data = data.Replace("\n", "");
            string[] linesString = data.Split(new string[] { "::" }, StringSplitOptions.None);
            foreach (string lineString in linesString)
            {
                Line line = new Line();
                string[] temp = lineString.Split(':');
                if (temp.Length < 2)
                {
                    error = $"{lineString}Î´ÕÒµ½Êä³ö";
                    Log.Error(error);
                    return;
                }
                Expression oe = new Expression(CookForExpression(temp[1], "o"));
                if (oe.HasErrors())
                {
                    error = $"{temp[1]}´æÔÚ´íÎó:{oe.Error}";
                    Log.Error(error);
                    return;
                }
                line.o = oe.ToLambda<SectionInput, uint>();
                string[] sectionStrings = temp[0].Split(new string[] { ";;" }, StringSplitOptions.None);
                for (int i = 0; i < sectionStrings.Length; i++)
                {
                    Func<SectionInput, bool>[] iF = new Func<SectionInput, bool>[4];
                    string sectionString = sectionStrings[i];
                    string[] inputStrings = sectionString.Split(';');
                    for (int j = 0; j < 4; j++)
                    {
                        if (j >= inputStrings.Length)
                        {
                            iF[j] = u => true;
                        }
                        else
                        {
                            string inputString = inputStrings[j];
                            if (inputString.Length == 0)
                            {
                                iF[j] = u => true;
                                continue;
                            }
                            Expression ie = new Expression(CookForExpression(inputString, $"i{j + 1}"));
                            if (ie.HasErrors())
                            {
                                error = $"{inputString}´æÔÚ´íÎó:{ie.Error}";
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
            LastLoadedString = data;
        }
        public Regex addBracketRegex = new Regex(@"(i\d)");
        public string[] notCookOperators = new string[] { "=", "!", "not", ">", "<", "and", "&&", "||", "or" };
        public string CookForExpression(string input, string category)
        {
            if (category.StartsWith("i"))
            {
                if (input == "true" || input == "")
                {
                    return "true";
                }
                if (input.StartsWith("=") || input.StartsWith("!=") || input.StartsWith("<") || input.StartsWith(">"))
                {
                    input = $"{category}{input}";
                }
                else if (!notCookOperators.Any<string>(item => input.Contains(item)))
                {
                    input = $"{category}={input}";
                }
            }
            else if (category == "o" && input.StartsWith("="))
            {
                input = input.Substring(1);
            }
            return addBracketRegex.Replace(input, "[$1]");
        }
        public string SaveString()
        {
            return SaveString(true);
        }
        public string SaveString(bool saveLastOutput)
        {
            return LastLoadedString;
        }

        public void LoadString(string data)
        {
            LoadString(data, out _);
        }
    }
}