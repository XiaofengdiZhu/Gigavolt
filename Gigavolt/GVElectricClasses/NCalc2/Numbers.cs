using System;

namespace NCalc {
    public class Numbers {
        static object ConvertIfString(object s) {
            if (s is string
                || s is char) {
                return decimal.Parse(s.ToString());
            }
            return s;
        }

        static object ConvertIfBoolean(object input) {
            if (input is bool boolean) {
                return boolean ? 1 : 0;
            }
            return input;
        }

        public static object Add(object a, object b, EvaluateOptions options) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (options.HasFlag(EvaluateOptions.BooleanCalculation)) {
                a = ConvertIfBoolean(a);
                b = ConvertIfBoolean(b);
            }
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            switch (typeCodeA) {
                case TypeCode.Boolean:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'bool'");
                        case TypeCode.Byte: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.SByte: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Int16: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.UInt16: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Int32: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.UInt32: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Int64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Single: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Double: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Decimal: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                    }
                    break;
                case TypeCode.Byte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'byte' and 'bool'");
                        case TypeCode.Byte: return (byte)a + (byte)b;
                        case TypeCode.SByte: return (byte)a + (sbyte)b;
                        case TypeCode.Int16: return (byte)a + (short)b;
                        case TypeCode.UInt16: return (byte)a + (ushort)b;
                        case TypeCode.Int32: return (byte)a + (int)b;
                        case TypeCode.UInt32: return (byte)a + (uint)b;
                        case TypeCode.Int64: return (byte)a + (long)b;
                        case TypeCode.UInt64: return (byte)a + (ulong)b;
                        case TypeCode.Single: return (byte)a + (float)b;
                        case TypeCode.Double: return (byte)a + (double)b;
                        case TypeCode.Decimal: return (byte)a + (decimal)b;
                    }
                    break;
                case TypeCode.SByte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'bool'");
                        case TypeCode.Byte: return (sbyte)a + (byte)b;
                        case TypeCode.SByte: return (sbyte)a + (sbyte)b;
                        case TypeCode.Int16: return (sbyte)a + (short)b;
                        case TypeCode.UInt16: return (sbyte)a + (ushort)b;
                        case TypeCode.Int32: return (sbyte)a + (int)b;
                        case TypeCode.UInt32: return (sbyte)a + (uint)b;
                        case TypeCode.Int64: return (sbyte)a + (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'ulong'");
                        case TypeCode.Single: return (sbyte)a + (float)b;
                        case TypeCode.Double: return (sbyte)a + (double)b;
                        case TypeCode.Decimal: return (sbyte)a + (decimal)b;
                    }
                    break;
                case TypeCode.Int16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'bool'");
                        case TypeCode.Byte: return (short)a + (byte)b;
                        case TypeCode.SByte: return (short)a + (sbyte)b;
                        case TypeCode.Int16: return (short)a + (short)b;
                        case TypeCode.UInt16: return (short)a + (ushort)b;
                        case TypeCode.Int32: return (short)a + (int)b;
                        case TypeCode.UInt32: return (short)a + (uint)b;
                        case TypeCode.Int64: return (short)a + (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'ulong'");
                        case TypeCode.Single: return (short)a + (float)b;
                        case TypeCode.Double: return (short)a + (double)b;
                        case TypeCode.Decimal: return (short)a + (decimal)b;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ushort' and 'bool'");
                        case TypeCode.Byte: return (ushort)a + (byte)b;
                        case TypeCode.SByte: return (ushort)a + (sbyte)b;
                        case TypeCode.Int16: return (ushort)a + (short)b;
                        case TypeCode.UInt16: return (ushort)a + (ushort)b;
                        case TypeCode.Int32: return (ushort)a + (int)b;
                        case TypeCode.UInt32: return (ushort)a + (uint)b;
                        case TypeCode.Int64: return (ushort)a + (long)b;
                        case TypeCode.UInt64: return (ushort)a + (ulong)b;
                        case TypeCode.Single: return (ushort)a + (float)b;
                        case TypeCode.Double: return (ushort)a + (double)b;
                        case TypeCode.Decimal: return (ushort)a + (decimal)b;
                    }
                    break;
                case TypeCode.Int32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'bool'");
                        case TypeCode.Byte: return (int)a + (byte)b;
                        case TypeCode.SByte: return (int)a + (sbyte)b;
                        case TypeCode.Int16: return (int)a + (short)b;
                        case TypeCode.UInt16: return (int)a + (ushort)b;
                        case TypeCode.Int32: return (int)a + (int)b;
                        case TypeCode.UInt32: return (int)a + (uint)b;
                        case TypeCode.Int64: return (int)a + (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'ulong'");
                        case TypeCode.Single: return (int)a + (float)b;
                        case TypeCode.Double: return (int)a + (double)b;
                        case TypeCode.Decimal: return (int)a + (decimal)b;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'unit' and 'bool'");
                        case TypeCode.Byte: return (uint)a + (byte)b;
                        case TypeCode.SByte: return (uint)a + (sbyte)b;
                        case TypeCode.Int16: return (uint)a + (short)b;
                        case TypeCode.UInt16: return (uint)a + (ushort)b;
                        case TypeCode.Int32: return (uint)a + (int)b;
                        case TypeCode.UInt32: return (uint)a + (uint)b;
                        case TypeCode.Int64: return (uint)a + (long)b;
                        case TypeCode.UInt64: return (uint)a + (ulong)b;
                        case TypeCode.Single: return (uint)a + (float)b;
                        case TypeCode.Double: return (uint)a + (double)b;
                        case TypeCode.Decimal: return (uint)a + (decimal)b;
                    }
                    break;
                case TypeCode.Int64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'bool'");
                        case TypeCode.Byte: return (long)a + (byte)b;
                        case TypeCode.SByte: return (long)a + (sbyte)b;
                        case TypeCode.Int16: return (long)a + (short)b;
                        case TypeCode.UInt16: return (long)a + (ushort)b;
                        case TypeCode.Int32: return (long)a + (int)b;
                        case TypeCode.UInt32: return (long)a + (uint)b;
                        case TypeCode.Int64: return (long)a + (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'ulong'");
                        case TypeCode.Single: return (long)a + (float)b;
                        case TypeCode.Double: return (long)a + (double)b;
                        case TypeCode.Decimal: return (long)a + (decimal)b;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'bool'");
                        case TypeCode.Byte: return (ulong)a + (byte)b;
                        case TypeCode.SByte: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'sbyte'");
                        case TypeCode.Int16: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'short'");
                        case TypeCode.UInt16: return (ulong)a + (ushort)b;
                        case TypeCode.Int32: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'int'");
                        case TypeCode.UInt32: return (ulong)a + (uint)b;
                        case TypeCode.Int64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'ulong'");
                        case TypeCode.UInt64: return (ulong)a + (ulong)b;
                        case TypeCode.Single: return (ulong)a + (float)b;
                        case TypeCode.Double: return (ulong)a + (double)b;
                        case TypeCode.Decimal: return (ulong)a + (decimal)b;
                    }
                    break;
                case TypeCode.Single:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'float' and 'bool'");
                        case TypeCode.Byte: return (float)a + (byte)b;
                        case TypeCode.SByte: return (float)a + (sbyte)b;
                        case TypeCode.Int16: return (float)a + (short)b;
                        case TypeCode.UInt16: return (float)a + (ushort)b;
                        case TypeCode.Int32: return (float)a + (int)b;
                        case TypeCode.UInt32: return (float)a + (uint)b;
                        case TypeCode.Int64: return (float)a + (long)b;
                        case TypeCode.UInt64: return (float)a + (ulong)b;
                        case TypeCode.Single: return (float)a + (float)b;
                        case TypeCode.Double: return (float)a + (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) + (decimal)b;
                    }
                    break;
                case TypeCode.Double:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'double' and 'bool'");
                        case TypeCode.Byte: return (double)a + (byte)b;
                        case TypeCode.SByte: return (double)a + (sbyte)b;
                        case TypeCode.Int16: return (double)a + (short)b;
                        case TypeCode.UInt16: return (double)a + (ushort)b;
                        case TypeCode.Int32: return (double)a + (int)b;
                        case TypeCode.UInt32: return (double)a + (uint)b;
                        case TypeCode.Int64: return (double)a + (long)b;
                        case TypeCode.UInt64: return (double)a + (ulong)b;
                        case TypeCode.Single: return (double)a + (float)b;
                        case TypeCode.Double: return (double)a + (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) + (decimal)b;
                    }
                    break;
                case TypeCode.Decimal:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'decimal' and 'bool'");
                        case TypeCode.Byte: return (decimal)a + (byte)b;
                        case TypeCode.SByte: return (decimal)a + (sbyte)b;
                        case TypeCode.Int16: return (decimal)a + (short)b;
                        case TypeCode.UInt16: return (decimal)a + (ushort)b;
                        case TypeCode.Int32: return (decimal)a + (int)b;
                        case TypeCode.UInt32: return (decimal)a + (uint)b;
                        case TypeCode.Int64: return (decimal)a + (long)b;
                        case TypeCode.UInt64: return (decimal)a + (ulong)b;
                        case TypeCode.Single: return (decimal)a + Convert.ToDecimal(b);
                        case TypeCode.Double: return (decimal)a + Convert.ToDecimal(b);
                        case TypeCode.Decimal: return (decimal)a + (decimal)b;
                    }
                    break;
            }
            return null;
        }

        public static object AddChecked(object a, object b, EvaluateOptions options) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (options.HasFlag(EvaluateOptions.BooleanCalculation)) {
                a = ConvertIfBoolean(a);
                b = ConvertIfBoolean(b);
            }
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            checked {
                switch (typeCodeA) {
                    case TypeCode.Boolean:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'bool'");
                            case TypeCode.Byte: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.SByte: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Int16: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.UInt16: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Int32: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.UInt32: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Int64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Single: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Double: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Decimal: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'bool' and 'byte'");
                        }
                        break;
                    case TypeCode.Byte:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'byte' and 'bool'");
                            case TypeCode.Byte: return (byte)a + (byte)b;
                            case TypeCode.SByte: return (byte)a + (sbyte)b;
                            case TypeCode.Int16: return (byte)a + (short)b;
                            case TypeCode.UInt16: return (byte)a + (ushort)b;
                            case TypeCode.Int32: return (byte)a + (int)b;
                            case TypeCode.UInt32: return (byte)a + (uint)b;
                            case TypeCode.Int64: return (byte)a + (long)b;
                            case TypeCode.UInt64: return (byte)a + (ulong)b;
                            case TypeCode.Single: return (byte)a + (float)b;
                            case TypeCode.Double: return (byte)a + (double)b;
                            case TypeCode.Decimal: return (byte)a + (decimal)b;
                        }
                        break;
                    case TypeCode.SByte:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'bool'");
                            case TypeCode.Byte: return (sbyte)a + (byte)b;
                            case TypeCode.SByte: return (sbyte)a + (sbyte)b;
                            case TypeCode.Int16: return (sbyte)a + (short)b;
                            case TypeCode.UInt16: return (sbyte)a + (ushort)b;
                            case TypeCode.Int32: return (sbyte)a + (int)b;
                            case TypeCode.UInt32: return (sbyte)a + (uint)b;
                            case TypeCode.Int64: return (sbyte)a + (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'sbyte' and 'ulong'");
                            case TypeCode.Single: return (sbyte)a + (float)b;
                            case TypeCode.Double: return (sbyte)a + (double)b;
                            case TypeCode.Decimal: return (sbyte)a + (decimal)b;
                        }
                        break;
                    case TypeCode.Int16:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'bool'");
                            case TypeCode.Byte: return (short)a + (byte)b;
                            case TypeCode.SByte: return (short)a + (sbyte)b;
                            case TypeCode.Int16: return (short)a + (short)b;
                            case TypeCode.UInt16: return (short)a + (ushort)b;
                            case TypeCode.Int32: return (short)a + (int)b;
                            case TypeCode.UInt32: return (short)a + (uint)b;
                            case TypeCode.Int64: return (short)a + (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'short' and 'ulong'");
                            case TypeCode.Single: return (short)a + (float)b;
                            case TypeCode.Double: return (short)a + (double)b;
                            case TypeCode.Decimal: return (short)a + (decimal)b;
                        }
                        break;
                    case TypeCode.UInt16:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ushort' and 'bool'");
                            case TypeCode.Byte: return (ushort)a + (byte)b;
                            case TypeCode.SByte: return (ushort)a + (sbyte)b;
                            case TypeCode.Int16: return (ushort)a + (short)b;
                            case TypeCode.UInt16: return (ushort)a + (ushort)b;
                            case TypeCode.Int32: return (ushort)a + (int)b;
                            case TypeCode.UInt32: return (ushort)a + (uint)b;
                            case TypeCode.Int64: return (ushort)a + (long)b;
                            case TypeCode.UInt64: return (ushort)a + (ulong)b;
                            case TypeCode.Single: return (ushort)a + (float)b;
                            case TypeCode.Double: return (ushort)a + (double)b;
                            case TypeCode.Decimal: return (ushort)a + (decimal)b;
                        }
                        break;
                    case TypeCode.Int32:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'bool'");
                            case TypeCode.Byte: return (int)a + (byte)b;
                            case TypeCode.SByte: return (int)a + (sbyte)b;
                            case TypeCode.Int16: return (int)a + (short)b;
                            case TypeCode.UInt16: return (int)a + (ushort)b;
                            case TypeCode.Int32: return (int)a + (int)b;
                            case TypeCode.UInt32: return (int)a + (uint)b;
                            case TypeCode.Int64: return (int)a + (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'int' and 'ulong'");
                            case TypeCode.Single: return (int)a + (float)b;
                            case TypeCode.Double: return (int)a + (double)b;
                            case TypeCode.Decimal: return (int)a + (decimal)b;
                        }
                        break;
                    case TypeCode.UInt32:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'unit' and 'bool'");
                            case TypeCode.Byte: return (uint)a + (byte)b;
                            case TypeCode.SByte: return (uint)a + (sbyte)b;
                            case TypeCode.Int16: return (uint)a + (short)b;
                            case TypeCode.UInt16: return (uint)a + (ushort)b;
                            case TypeCode.Int32: return (uint)a + (int)b;
                            case TypeCode.UInt32: return (uint)a + (uint)b;
                            case TypeCode.Int64: return (uint)a + (long)b;
                            case TypeCode.UInt64: return (uint)a + (ulong)b;
                            case TypeCode.Single: return (uint)a + (float)b;
                            case TypeCode.Double: return (uint)a + (double)b;
                            case TypeCode.Decimal: return (uint)a + (decimal)b;
                        }
                        break;
                    case TypeCode.Int64:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'bool'");
                            case TypeCode.Byte: return (long)a + (byte)b;
                            case TypeCode.SByte: return (long)a + (sbyte)b;
                            case TypeCode.Int16: return (long)a + (short)b;
                            case TypeCode.UInt16: return (long)a + (ushort)b;
                            case TypeCode.Int32: return (long)a + (int)b;
                            case TypeCode.UInt32: return (long)a + (uint)b;
                            case TypeCode.Int64: return (long)a + (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'long' and 'ulong'");
                            case TypeCode.Single: return (long)a + (float)b;
                            case TypeCode.Double: return (long)a + (double)b;
                            case TypeCode.Decimal: return (long)a + (decimal)b;
                        }
                        break;
                    case TypeCode.UInt64:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'bool'");
                            case TypeCode.Byte: return (ulong)a + (byte)b;
                            case TypeCode.SByte: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'sbyte'");
                            case TypeCode.Int16: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'short'");
                            case TypeCode.UInt16: return (ulong)a + (ushort)b;
                            case TypeCode.Int32: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'int'");
                            case TypeCode.UInt32: return (ulong)a + (uint)b;
                            case TypeCode.Int64: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'ulong' and 'ulong'");
                            case TypeCode.UInt64: return (ulong)a + (ulong)b;
                            case TypeCode.Single: return (ulong)a + (float)b;
                            case TypeCode.Double: return (ulong)a + (double)b;
                            case TypeCode.Decimal: return (ulong)a + (decimal)b;
                        }
                        break;
                    case TypeCode.Single:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'float' and 'bool'");
                            case TypeCode.Byte: return (float)a + (byte)b;
                            case TypeCode.SByte: return (float)a + (sbyte)b;
                            case TypeCode.Int16: return (float)a + (short)b;
                            case TypeCode.UInt16: return (float)a + (ushort)b;
                            case TypeCode.Int32: return (float)a + (int)b;
                            case TypeCode.UInt32: return (float)a + (uint)b;
                            case TypeCode.Int64: return (float)a + (long)b;
                            case TypeCode.UInt64: return (float)a + (ulong)b;
                            case TypeCode.Single: return (float)a + (float)b;
                            case TypeCode.Double: return (float)a + (double)b;
                            case TypeCode.Decimal: return Convert.ToDecimal(a) + (decimal)b;
                        }
                        break;
                    case TypeCode.Double:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'double' and 'bool'");
                            case TypeCode.Byte: return (double)a + (byte)b;
                            case TypeCode.SByte: return (double)a + (sbyte)b;
                            case TypeCode.Int16: return (double)a + (short)b;
                            case TypeCode.UInt16: return (double)a + (ushort)b;
                            case TypeCode.Int32: return (double)a + (int)b;
                            case TypeCode.UInt32: return (double)a + (uint)b;
                            case TypeCode.Int64: return (double)a + (long)b;
                            case TypeCode.UInt64: return (double)a + (ulong)b;
                            case TypeCode.Single: return (double)a + (float)b;
                            case TypeCode.Double: return (double)a + (double)b;
                            case TypeCode.Decimal: return Convert.ToDecimal(a) + (decimal)b;
                        }
                        break;
                    case TypeCode.Decimal:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '+' can't be applied to operands of types 'decimal' and 'bool'");
                            case TypeCode.Byte: return (decimal)a + (byte)b;
                            case TypeCode.SByte: return (decimal)a + (sbyte)b;
                            case TypeCode.Int16: return (decimal)a + (short)b;
                            case TypeCode.UInt16: return (decimal)a + (ushort)b;
                            case TypeCode.Int32: return (decimal)a + (int)b;
                            case TypeCode.UInt32: return (decimal)a + (uint)b;
                            case TypeCode.Int64: return (decimal)a + (long)b;
                            case TypeCode.UInt64: return (decimal)a + (ulong)b;
                            case TypeCode.Single: return (decimal)a + Convert.ToDecimal(b);
                            case TypeCode.Double: return (decimal)a + Convert.ToDecimal(b);
                            case TypeCode.Decimal: return (decimal)a + (decimal)b;
                        }
                        break;
                }
                return null;
            }
        }

        public static object Soustract(object a, object b, EvaluateOptions options) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (options.HasFlag(EvaluateOptions.BooleanCalculation)) {
                a = ConvertIfBoolean(a);
                b = ConvertIfBoolean(b);
            }
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            switch (typeCodeA) {
                case TypeCode.Boolean:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'bool'");
                        case TypeCode.Byte: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.SByte: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Int16: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.UInt16: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Int32: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.UInt32: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Int64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Single: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Double: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        case TypeCode.Decimal: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                    }
                    break;
                case TypeCode.Byte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'byte' and 'bool'");
                        case TypeCode.SByte: return (byte)a - (sbyte)b;
                        case TypeCode.Int16: return (byte)a - (short)b;
                        case TypeCode.UInt16: return (byte)a - (ushort)b;
                        case TypeCode.Int32: return (byte)a - (int)b;
                        case TypeCode.UInt32: return (byte)a - (uint)b;
                        case TypeCode.Int64: return (byte)a - (long)b;
                        case TypeCode.UInt64: return (byte)a - (ulong)b;
                        case TypeCode.Single: return (byte)a - (float)b;
                        case TypeCode.Double: return (byte)a - (double)b;
                        case TypeCode.Decimal: return (byte)a - (decimal)b;
                    }
                    break;
                case TypeCode.SByte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'bool'");
                        case TypeCode.SByte: return (sbyte)a - (sbyte)b;
                        case TypeCode.Int16: return (sbyte)a - (short)b;
                        case TypeCode.UInt16: return (sbyte)a - (ushort)b;
                        case TypeCode.Int32: return (sbyte)a - (int)b;
                        case TypeCode.UInt32: return (sbyte)a - (uint)b;
                        case TypeCode.Int64: return (sbyte)a - (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'ulong'");
                        case TypeCode.Single: return (sbyte)a - (float)b;
                        case TypeCode.Double: return (sbyte)a - (double)b;
                        case TypeCode.Decimal: return (sbyte)a - (decimal)b;
                    }
                    break;
                case TypeCode.Int16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'bool'");
                        case TypeCode.SByte: return (short)a - (sbyte)b;
                        case TypeCode.Int16: return (short)a - (short)b;
                        case TypeCode.UInt16: return (short)a - (ushort)b;
                        case TypeCode.Int32: return (short)a - (int)b;
                        case TypeCode.UInt32: return (short)a - (uint)b;
                        case TypeCode.Int64: return (short)a - (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'ulong'");
                        case TypeCode.Single: return (short)a - (float)b;
                        case TypeCode.Double: return (short)a - (double)b;
                        case TypeCode.Decimal: return (short)a - (decimal)b;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ushort' and 'bool'");
                        case TypeCode.SByte: return (ushort)a - (sbyte)b;
                        case TypeCode.Int16: return (ushort)a - (short)b;
                        case TypeCode.UInt16: return (ushort)a - (ushort)b;
                        case TypeCode.Int32: return (ushort)a - (int)b;
                        case TypeCode.UInt32: return (ushort)a - (uint)b;
                        case TypeCode.Int64: return (ushort)a - (long)b;
                        case TypeCode.UInt64: return (ushort)a - (ulong)b;
                        case TypeCode.Single: return (ushort)a - (float)b;
                        case TypeCode.Double: return (ushort)a - (double)b;
                        case TypeCode.Decimal: return (ushort)a - (decimal)b;
                    }
                    break;
                case TypeCode.Int32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'bool'");
                        case TypeCode.SByte: return (int)a - (sbyte)b;
                        case TypeCode.Int16: return (int)a - (short)b;
                        case TypeCode.UInt16: return (int)a - (ushort)b;
                        case TypeCode.Int32: return (int)a - (int)b;
                        case TypeCode.UInt32: return (int)a - (uint)b;
                        case TypeCode.Int64: return (int)a - (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'ulong'");
                        case TypeCode.Single: return (int)a - (float)b;
                        case TypeCode.Double: return (int)a - (double)b;
                        case TypeCode.Decimal: return (int)a - (decimal)b;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'uint' and 'bool'");
                        case TypeCode.SByte: return (uint)a - (sbyte)b;
                        case TypeCode.Int16: return (uint)a - (short)b;
                        case TypeCode.UInt16: return (uint)a - (ushort)b;
                        case TypeCode.Int32: return (uint)a - (int)b;
                        case TypeCode.UInt32: return (uint)a - (uint)b;
                        case TypeCode.Int64: return (uint)a - (long)b;
                        case TypeCode.UInt64: return (uint)a - (ulong)b;
                        case TypeCode.Single: return (uint)a - (float)b;
                        case TypeCode.Double: return (uint)a - (double)b;
                        case TypeCode.Decimal: return (uint)a - (decimal)b;
                    }
                    break;
                case TypeCode.Int64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'bool'");
                        case TypeCode.SByte: return (long)a - (sbyte)b;
                        case TypeCode.Int16: return (long)a - (short)b;
                        case TypeCode.UInt16: return (long)a - (ushort)b;
                        case TypeCode.Int32: return (long)a - (int)b;
                        case TypeCode.UInt32: return (long)a - (uint)b;
                        case TypeCode.Int64: return (long)a - (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'ulong'");
                        case TypeCode.Single: return (long)a - (float)b;
                        case TypeCode.Double: return (long)a - (double)b;
                        case TypeCode.Decimal: return (long)a - (decimal)b;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'bool'");
                        case TypeCode.SByte: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'double'");
                        case TypeCode.Int16: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'short'");
                        case TypeCode.UInt16: return (ulong)a - (ushort)b;
                        case TypeCode.Int32: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'int'");
                        case TypeCode.UInt32: return (ulong)a - (uint)b;
                        case TypeCode.Int64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'long'");
                        case TypeCode.UInt64: return (ulong)a - (ulong)b;
                        case TypeCode.Single: return (ulong)a - (float)b;
                        case TypeCode.Double: return (ulong)a - (double)b;
                        case TypeCode.Decimal: return (ulong)a - (decimal)b;
                    }
                    break;
                case TypeCode.Single:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'float' and 'bool'");
                        case TypeCode.SByte: return (float)a - (sbyte)b;
                        case TypeCode.Int16: return (float)a - (short)b;
                        case TypeCode.UInt16: return (float)a - (ushort)b;
                        case TypeCode.Int32: return (float)a - (int)b;
                        case TypeCode.UInt32: return (float)a - (uint)b;
                        case TypeCode.Int64: return (float)a - (long)b;
                        case TypeCode.UInt64: return (float)a - (ulong)b;
                        case TypeCode.Single: return (float)a - (float)b;
                        case TypeCode.Double: return (float)a - (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) - (decimal)b;
                    }
                    break;
                case TypeCode.Double:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'double' and 'bool'");
                        case TypeCode.SByte: return (double)a - (sbyte)b;
                        case TypeCode.Int16: return (double)a - (short)b;
                        case TypeCode.UInt16: return (double)a - (ushort)b;
                        case TypeCode.Int32: return (double)a - (int)b;
                        case TypeCode.UInt32: return (double)a - (uint)b;
                        case TypeCode.Int64: return (double)a - (long)b;
                        case TypeCode.UInt64: return (double)a - (ulong)b;
                        case TypeCode.Single: return (double)a - (float)b;
                        case TypeCode.Double: return (double)a - (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) - (decimal)b;
                    }
                    break;
                case TypeCode.Decimal:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'decimal' and 'bool'");
                        case TypeCode.SByte: return (decimal)a - (sbyte)b;
                        case TypeCode.Int16: return (decimal)a - (short)b;
                        case TypeCode.UInt16: return (decimal)a - (ushort)b;
                        case TypeCode.Int32: return (decimal)a - (int)b;
                        case TypeCode.UInt32: return (decimal)a - (uint)b;
                        case TypeCode.Int64: return (decimal)a - (long)b;
                        case TypeCode.UInt64: return (decimal)a - (ulong)b;
                        case TypeCode.Single: return (decimal)a - Convert.ToDecimal(b);
                        case TypeCode.Double: return (decimal)a - Convert.ToDecimal(b);
                        case TypeCode.Decimal: return (decimal)a - (decimal)b;
                    }
                    break;
            }
            return null;
        }

        public static object SoustractChecked(object a, object b, EvaluateOptions options) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (options.HasFlag(EvaluateOptions.BooleanCalculation)) {
                a = ConvertIfBoolean(a);
                b = ConvertIfBoolean(b);
            }
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            checked {
                switch (typeCodeA) {
                    case TypeCode.Boolean:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'bool'");
                            case TypeCode.Byte: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.SByte: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Int16: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.UInt16: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Int32: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.UInt32: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Int64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Single: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Double: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                            case TypeCode.Decimal: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'bool' and 'byte'");
                        }
                        break;
                    case TypeCode.Byte:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'byte' and 'bool'");
                            case TypeCode.SByte: return (byte)a - (sbyte)b;
                            case TypeCode.Int16: return (byte)a - (short)b;
                            case TypeCode.UInt16: return (byte)a - (ushort)b;
                            case TypeCode.Int32: return (byte)a - (int)b;
                            case TypeCode.UInt32: return (byte)a - (uint)b;
                            case TypeCode.Int64: return (byte)a - (long)b;
                            case TypeCode.UInt64: return (byte)a - (ulong)b;
                            case TypeCode.Single: return (byte)a - (float)b;
                            case TypeCode.Double: return (byte)a - (double)b;
                            case TypeCode.Decimal: return (byte)a - (decimal)b;
                        }
                        break;
                    case TypeCode.SByte:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'bool'");
                            case TypeCode.SByte: return (sbyte)a - (sbyte)b;
                            case TypeCode.Int16: return (sbyte)a - (short)b;
                            case TypeCode.UInt16: return (sbyte)a - (ushort)b;
                            case TypeCode.Int32: return (sbyte)a - (int)b;
                            case TypeCode.UInt32: return (sbyte)a - (uint)b;
                            case TypeCode.Int64: return (sbyte)a - (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'sbyte' and 'ulong'");
                            case TypeCode.Single: return (sbyte)a - (float)b;
                            case TypeCode.Double: return (sbyte)a - (double)b;
                            case TypeCode.Decimal: return (sbyte)a - (decimal)b;
                        }
                        break;
                    case TypeCode.Int16:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'bool'");
                            case TypeCode.SByte: return (short)a - (sbyte)b;
                            case TypeCode.Int16: return (short)a - (short)b;
                            case TypeCode.UInt16: return (short)a - (ushort)b;
                            case TypeCode.Int32: return (short)a - (int)b;
                            case TypeCode.UInt32: return (short)a - (uint)b;
                            case TypeCode.Int64: return (short)a - (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'short' and 'ulong'");
                            case TypeCode.Single: return (short)a - (float)b;
                            case TypeCode.Double: return (short)a - (double)b;
                            case TypeCode.Decimal: return (short)a - (decimal)b;
                        }
                        break;
                    case TypeCode.UInt16:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ushort' and 'bool'");
                            case TypeCode.SByte: return (ushort)a - (sbyte)b;
                            case TypeCode.Int16: return (ushort)a - (short)b;
                            case TypeCode.UInt16: return (ushort)a - (ushort)b;
                            case TypeCode.Int32: return (ushort)a - (int)b;
                            case TypeCode.UInt32: return (ushort)a - (uint)b;
                            case TypeCode.Int64: return (ushort)a - (long)b;
                            case TypeCode.UInt64: return (ushort)a - (ulong)b;
                            case TypeCode.Single: return (ushort)a - (float)b;
                            case TypeCode.Double: return (ushort)a - (double)b;
                            case TypeCode.Decimal: return (ushort)a - (decimal)b;
                        }
                        break;
                    case TypeCode.Int32:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'bool'");
                            case TypeCode.SByte: return (int)a - (sbyte)b;
                            case TypeCode.Int16: return (int)a - (short)b;
                            case TypeCode.UInt16: return (int)a - (ushort)b;
                            case TypeCode.Int32: return (int)a - (int)b;
                            case TypeCode.UInt32: return (int)a - (uint)b;
                            case TypeCode.Int64: return (int)a - (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'int' and 'ulong'");
                            case TypeCode.Single: return (int)a - (float)b;
                            case TypeCode.Double: return (int)a - (double)b;
                            case TypeCode.Decimal: return (int)a - (decimal)b;
                        }
                        break;
                    case TypeCode.UInt32:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'uint' and 'bool'");
                            case TypeCode.SByte: return (uint)a - (sbyte)b;
                            case TypeCode.Int16: return (uint)a - (short)b;
                            case TypeCode.UInt16: return (uint)a - (ushort)b;
                            case TypeCode.Int32: return (uint)a - (int)b;
                            case TypeCode.UInt32: return (uint)a - (uint)b;
                            case TypeCode.Int64: return (uint)a - (long)b;
                            case TypeCode.UInt64: return (uint)a - (ulong)b;
                            case TypeCode.Single: return (uint)a - (float)b;
                            case TypeCode.Double: return (uint)a - (double)b;
                            case TypeCode.Decimal: return (uint)a - (decimal)b;
                        }
                        break;
                    case TypeCode.Int64:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'bool'");
                            case TypeCode.SByte: return (long)a - (sbyte)b;
                            case TypeCode.Int16: return (long)a - (short)b;
                            case TypeCode.UInt16: return (long)a - (ushort)b;
                            case TypeCode.Int32: return (long)a - (int)b;
                            case TypeCode.UInt32: return (long)a - (uint)b;
                            case TypeCode.Int64: return (long)a - (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'long' and 'ulong'");
                            case TypeCode.Single: return (long)a - (float)b;
                            case TypeCode.Double: return (long)a - (double)b;
                            case TypeCode.Decimal: return (long)a - (decimal)b;
                        }
                        break;
                    case TypeCode.UInt64:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'bool'");
                            case TypeCode.SByte: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'double'");
                            case TypeCode.Int16: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'short'");
                            case TypeCode.UInt16: return (ulong)a - (ushort)b;
                            case TypeCode.Int32: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'int'");
                            case TypeCode.UInt32: return (ulong)a - (uint)b;
                            case TypeCode.Int64: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'long'");
                            case TypeCode.UInt64: return (ulong)a - (ulong)b;
                            case TypeCode.Single: return (ulong)a - (float)b;
                            case TypeCode.Double: return (ulong)a - (double)b;
                            case TypeCode.Decimal: return (ulong)a - (decimal)b;
                        }
                        break;
                    case TypeCode.Single:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'float' and 'bool'");
                            case TypeCode.SByte: return (float)a - (sbyte)b;
                            case TypeCode.Int16: return (float)a - (short)b;
                            case TypeCode.UInt16: return (float)a - (ushort)b;
                            case TypeCode.Int32: return (float)a - (int)b;
                            case TypeCode.UInt32: return (float)a - (uint)b;
                            case TypeCode.Int64: return (float)a - (long)b;
                            case TypeCode.UInt64: return (float)a - (ulong)b;
                            case TypeCode.Single: return (float)a - (float)b;
                            case TypeCode.Double: return (float)a - (double)b;
                            case TypeCode.Decimal: return Convert.ToDecimal(a) - (decimal)b;
                        }
                        break;
                    case TypeCode.Double:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'double' and 'bool'");
                            case TypeCode.SByte: return (double)a - (sbyte)b;
                            case TypeCode.Int16: return (double)a - (short)b;
                            case TypeCode.UInt16: return (double)a - (ushort)b;
                            case TypeCode.Int32: return (double)a - (int)b;
                            case TypeCode.UInt32: return (double)a - (uint)b;
                            case TypeCode.Int64: return (double)a - (long)b;
                            case TypeCode.UInt64: return (double)a - (ulong)b;
                            case TypeCode.Single: return (double)a - (float)b;
                            case TypeCode.Double: return (double)a - (double)b;
                            case TypeCode.Decimal: return Convert.ToDecimal(a) - (decimal)b;
                        }
                        break;
                    case TypeCode.Decimal:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'decimal' and 'bool'");
                            case TypeCode.SByte: return (decimal)a - (sbyte)b;
                            case TypeCode.Int16: return (decimal)a - (short)b;
                            case TypeCode.UInt16: return (decimal)a - (ushort)b;
                            case TypeCode.Int32: return (decimal)a - (int)b;
                            case TypeCode.UInt32: return (decimal)a - (uint)b;
                            case TypeCode.Int64: return (decimal)a - (long)b;
                            case TypeCode.UInt64: return (decimal)a - (ulong)b;
                            case TypeCode.Single: return (decimal)a - Convert.ToDecimal(b);
                            case TypeCode.Double: return (decimal)a - Convert.ToDecimal(b);
                            case TypeCode.Decimal: return (decimal)a - (decimal)b;
                        }
                        break;
                }
            }
            return null;
        }

        public static object Multiply(object a, object b, EvaluateOptions options) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (options.HasFlag(EvaluateOptions.BooleanCalculation)) {
                a = ConvertIfBoolean(a);
                b = ConvertIfBoolean(b);
            }
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            switch (typeCodeA) {
                case TypeCode.Byte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'byte' and 'bool'");
                        case TypeCode.SByte: return (byte)a * (sbyte)b;
                        case TypeCode.Int16: return (byte)a * (short)b;
                        case TypeCode.UInt16: return (byte)a * (ushort)b;
                        case TypeCode.Int32: return (byte)a * (int)b;
                        case TypeCode.UInt32: return (byte)a * (uint)b;
                        case TypeCode.Int64: return (byte)a * (long)b;
                        case TypeCode.UInt64: return (byte)a * (ulong)b;
                        case TypeCode.Single: return (byte)a * (float)b;
                        case TypeCode.Double: return (byte)a * (double)b;
                        case TypeCode.Decimal: return (byte)a * (decimal)b;
                    }
                    break;
                case TypeCode.SByte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'bool'");
                        case TypeCode.SByte: return (sbyte)a * (sbyte)b;
                        case TypeCode.Int16: return (sbyte)a * (short)b;
                        case TypeCode.UInt16: return (sbyte)a * (ushort)b;
                        case TypeCode.Int32: return (sbyte)a * (int)b;
                        case TypeCode.UInt32: return (sbyte)a * (uint)b;
                        case TypeCode.Int64: return (sbyte)a * (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'ulong'");
                        case TypeCode.Single: return (sbyte)a * (float)b;
                        case TypeCode.Double: return (sbyte)a * (double)b;
                        case TypeCode.Decimal: return (sbyte)a * (decimal)b;
                    }
                    break;
                case TypeCode.Int16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'bool'");
                        case TypeCode.SByte: return (short)a * (sbyte)b;
                        case TypeCode.Int16: return (short)a * (short)b;
                        case TypeCode.UInt16: return (short)a * (ushort)b;
                        case TypeCode.Int32: return (short)a * (int)b;
                        case TypeCode.UInt32: return (short)a * (uint)b;
                        case TypeCode.Int64: return (short)a * (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'ulong'");
                        case TypeCode.Single: return (short)a * (float)b;
                        case TypeCode.Double: return (short)a * (double)b;
                        case TypeCode.Decimal: return (short)a * (decimal)b;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ushort' and 'bool'");
                        case TypeCode.SByte: return (ushort)a * (sbyte)b;
                        case TypeCode.Int16: return (ushort)a * (short)b;
                        case TypeCode.UInt16: return (ushort)a * (ushort)b;
                        case TypeCode.Int32: return (ushort)a * (int)b;
                        case TypeCode.UInt32: return (ushort)a * (uint)b;
                        case TypeCode.Int64: return (ushort)a * (long)b;
                        case TypeCode.UInt64: return (ushort)a * (ulong)b;
                        case TypeCode.Single: return (ushort)a * (float)b;
                        case TypeCode.Double: return (ushort)a * (double)b;
                        case TypeCode.Decimal: return (ushort)a * (decimal)b;
                    }
                    break;
                case TypeCode.Int32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'bool'");
                        case TypeCode.SByte: return (int)a * (sbyte)b;
                        case TypeCode.Int16: return (int)a * (short)b;
                        case TypeCode.UInt16: return (int)a * (ushort)b;
                        case TypeCode.Int32: return (int)a * (int)b;
                        case TypeCode.UInt32: return (int)a * (uint)b;
                        case TypeCode.Int64: return (int)a * (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'ulong'");
                        case TypeCode.Single: return (int)a * (float)b;
                        case TypeCode.Double: return (int)a * (double)b;
                        case TypeCode.Decimal: return (int)a * (decimal)b;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'uint' and 'bool'");
                        case TypeCode.SByte: return (uint)a * (sbyte)b;
                        case TypeCode.Int16: return (uint)a * (short)b;
                        case TypeCode.UInt16: return (uint)a * (ushort)b;
                        case TypeCode.Int32: return (uint)a * (int)b;
                        case TypeCode.UInt32: return (uint)a * (uint)b;
                        case TypeCode.Int64: return (uint)a * (long)b;
                        case TypeCode.UInt64: return (uint)a * (ulong)b;
                        case TypeCode.Single: return (uint)a * (float)b;
                        case TypeCode.Double: return (uint)a * (double)b;
                        case TypeCode.Decimal: return (uint)a * (decimal)b;
                    }
                    break;
                case TypeCode.Int64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'bool'");
                        case TypeCode.SByte: return (long)a * (sbyte)b;
                        case TypeCode.Int16: return (long)a * (short)b;
                        case TypeCode.UInt16: return (long)a * (ushort)b;
                        case TypeCode.Int32: return (long)a * (int)b;
                        case TypeCode.UInt32: return (long)a * (uint)b;
                        case TypeCode.Int64: return (long)a * (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'ulong'");
                        case TypeCode.Single: return (long)a * (float)b;
                        case TypeCode.Double: return (long)a * (double)b;
                        case TypeCode.Decimal: return (long)a * (decimal)b;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'bool'");
                        case TypeCode.SByte: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'sbyte'");
                        case TypeCode.Int16: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'short'");
                        case TypeCode.UInt16: return (ulong)a * (ushort)b;
                        case TypeCode.Int32: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'int'");
                        case TypeCode.UInt32: return (ulong)a * (uint)b;
                        case TypeCode.Int64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'long'");
                        case TypeCode.UInt64: return (ulong)a * (ulong)b;
                        case TypeCode.Single: return (ulong)a * (float)b;
                        case TypeCode.Double: return (ulong)a * (double)b;
                        case TypeCode.Decimal: return (ulong)a * (decimal)b;
                    }
                    break;
                case TypeCode.Single:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'float' and 'bool'");
                        case TypeCode.SByte: return (float)a * (sbyte)b;
                        case TypeCode.Int16: return (float)a * (short)b;
                        case TypeCode.UInt16: return (float)a * (ushort)b;
                        case TypeCode.Int32: return (float)a * (int)b;
                        case TypeCode.UInt32: return (float)a * (uint)b;
                        case TypeCode.Int64: return (float)a * (long)b;
                        case TypeCode.UInt64: return (float)a * (ulong)b;
                        case TypeCode.Single: return (float)a * (float)b;
                        case TypeCode.Double: return (float)a * (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) * (decimal)b;
                    }
                    break;
                case TypeCode.Double:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'double' and 'bool'");
                        case TypeCode.SByte: return (double)a * (sbyte)b;
                        case TypeCode.Int16: return (double)a * (short)b;
                        case TypeCode.UInt16: return (double)a * (ushort)b;
                        case TypeCode.Int32: return (double)a * (int)b;
                        case TypeCode.UInt32: return (double)a * (uint)b;
                        case TypeCode.Int64: return (double)a * (long)b;
                        case TypeCode.UInt64: return (double)a * (ulong)b;
                        case TypeCode.Single: return (double)a * (float)b;
                        case TypeCode.Double: return (double)a * (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) * (decimal)b;
                    }
                    break;
                case TypeCode.Decimal:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'decimal' and 'bool'");
                        case TypeCode.SByte: return (decimal)a * (sbyte)b;
                        case TypeCode.Int16: return (decimal)a * (short)b;
                        case TypeCode.UInt16: return (decimal)a * (ushort)b;
                        case TypeCode.Int32: return (decimal)a * (int)b;
                        case TypeCode.UInt32: return (decimal)a * (uint)b;
                        case TypeCode.Int64: return (decimal)a * (long)b;
                        case TypeCode.UInt64: return (decimal)a * (ulong)b;
                        case TypeCode.Single: return (decimal)a * Convert.ToDecimal(b);
                        case TypeCode.Double: return (decimal)a * Convert.ToDecimal(b);
                        case TypeCode.Decimal: return (decimal)a * (decimal)b;
                    }
                    break;
            }
            return null;
        }

        public static object MultiplyChecked(object a, object b, EvaluateOptions options) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (options.HasFlag(EvaluateOptions.BooleanCalculation)) {
                a = ConvertIfBoolean(a);
                b = ConvertIfBoolean(b);
            }
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            checked {
                switch (typeCodeA) {
                    case TypeCode.Byte:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'byte' and 'bool'");
                            case TypeCode.SByte: return (byte)a * (sbyte)b;
                            case TypeCode.Int16: return (byte)a * (short)b;
                            case TypeCode.UInt16: return (byte)a * (ushort)b;
                            case TypeCode.Int32: return (byte)a * (int)b;
                            case TypeCode.UInt32: return (byte)a * (uint)b;
                            case TypeCode.Int64: return (byte)a * (long)b;
                            case TypeCode.UInt64: return (byte)a * (ulong)b;
                            case TypeCode.Single: return (byte)a * (float)b;
                            case TypeCode.Double: return (byte)a * (double)b;
                            case TypeCode.Decimal: return (byte)a * (decimal)b;
                        }
                        break;
                    case TypeCode.SByte:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'bool'");
                            case TypeCode.SByte: return (sbyte)a * (sbyte)b;
                            case TypeCode.Int16: return (sbyte)a * (short)b;
                            case TypeCode.UInt16: return (sbyte)a * (ushort)b;
                            case TypeCode.Int32: return (sbyte)a * (int)b;
                            case TypeCode.UInt32: return (sbyte)a * (uint)b;
                            case TypeCode.Int64: return (sbyte)a * (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'sbyte' and 'ulong'");
                            case TypeCode.Single: return (sbyte)a * (float)b;
                            case TypeCode.Double: return (sbyte)a * (double)b;
                            case TypeCode.Decimal: return (sbyte)a * (decimal)b;
                        }
                        break;
                    case TypeCode.Int16:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'bool'");
                            case TypeCode.SByte: return (short)a * (sbyte)b;
                            case TypeCode.Int16: return (short)a * (short)b;
                            case TypeCode.UInt16: return (short)a * (ushort)b;
                            case TypeCode.Int32: return (short)a * (int)b;
                            case TypeCode.UInt32: return (short)a * (uint)b;
                            case TypeCode.Int64: return (short)a * (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'short' and 'ulong'");
                            case TypeCode.Single: return (short)a * (float)b;
                            case TypeCode.Double: return (short)a * (double)b;
                            case TypeCode.Decimal: return (short)a * (decimal)b;
                        }
                        break;
                    case TypeCode.UInt16:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ushort' and 'bool'");
                            case TypeCode.SByte: return (ushort)a * (sbyte)b;
                            case TypeCode.Int16: return (ushort)a * (short)b;
                            case TypeCode.UInt16: return (ushort)a * (ushort)b;
                            case TypeCode.Int32: return (ushort)a * (int)b;
                            case TypeCode.UInt32: return (ushort)a * (uint)b;
                            case TypeCode.Int64: return (ushort)a * (long)b;
                            case TypeCode.UInt64: return (ushort)a * (ulong)b;
                            case TypeCode.Single: return (ushort)a * (float)b;
                            case TypeCode.Double: return (ushort)a * (double)b;
                            case TypeCode.Decimal: return (ushort)a * (decimal)b;
                        }
                        break;
                    case TypeCode.Int32:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'bool'");
                            case TypeCode.SByte: return (int)a * (sbyte)b;
                            case TypeCode.Int16: return (int)a * (short)b;
                            case TypeCode.UInt16: return (int)a * (ushort)b;
                            case TypeCode.Int32: return (int)a * (int)b;
                            case TypeCode.UInt32: return (int)a * (uint)b;
                            case TypeCode.Int64: return (int)a * (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'int' and 'ulong'");
                            case TypeCode.Single: return (int)a * (float)b;
                            case TypeCode.Double: return (int)a * (double)b;
                            case TypeCode.Decimal: return (int)a * (decimal)b;
                        }
                        break;
                    case TypeCode.UInt32:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'uint' and 'bool'");
                            case TypeCode.SByte: return (uint)a * (sbyte)b;
                            case TypeCode.Int16: return (uint)a * (short)b;
                            case TypeCode.UInt16: return (uint)a * (ushort)b;
                            case TypeCode.Int32: return (uint)a * (int)b;
                            case TypeCode.UInt32: return (uint)a * (uint)b;
                            case TypeCode.Int64: return (uint)a * (long)b;
                            case TypeCode.UInt64: return (uint)a * (ulong)b;
                            case TypeCode.Single: return (uint)a * (float)b;
                            case TypeCode.Double: return (uint)a * (double)b;
                            case TypeCode.Decimal: return (uint)a * (decimal)b;
                        }
                        break;
                    case TypeCode.Int64:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'bool'");
                            case TypeCode.SByte: return (long)a * (sbyte)b;
                            case TypeCode.Int16: return (long)a * (short)b;
                            case TypeCode.UInt16: return (long)a * (ushort)b;
                            case TypeCode.Int32: return (long)a * (int)b;
                            case TypeCode.UInt32: return (long)a * (uint)b;
                            case TypeCode.Int64: return (long)a * (long)b;
                            case TypeCode.UInt64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'long' and 'ulong'");
                            case TypeCode.Single: return (long)a * (float)b;
                            case TypeCode.Double: return (long)a * (double)b;
                            case TypeCode.Decimal: return (long)a * (decimal)b;
                        }
                        break;
                    case TypeCode.UInt64:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'bool'");
                            case TypeCode.SByte: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'sbyte'");
                            case TypeCode.Int16: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'short'");
                            case TypeCode.UInt16: return (ulong)a * (ushort)b;
                            case TypeCode.Int32: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'int'");
                            case TypeCode.UInt32: return (ulong)a * (uint)b;
                            case TypeCode.Int64: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'ulong' and 'long'");
                            case TypeCode.UInt64: return (ulong)a * (ulong)b;
                            case TypeCode.Single: return (ulong)a * (float)b;
                            case TypeCode.Double: return (ulong)a * (double)b;
                            case TypeCode.Decimal: return (ulong)a * (decimal)b;
                        }
                        break;
                    case TypeCode.Single:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'float' and 'bool'");
                            case TypeCode.SByte: return (float)a * (sbyte)b;
                            case TypeCode.Int16: return (float)a * (short)b;
                            case TypeCode.UInt16: return (float)a * (ushort)b;
                            case TypeCode.Int32: return (float)a * (int)b;
                            case TypeCode.UInt32: return (float)a * (uint)b;
                            case TypeCode.Int64: return (float)a * (long)b;
                            case TypeCode.UInt64: return (float)a * (ulong)b;
                            case TypeCode.Single: return (float)a * (float)b;
                            case TypeCode.Double: return (float)a * (double)b;
                            case TypeCode.Decimal: return Convert.ToDecimal(a) * (decimal)b;
                        }
                        break;
                    case TypeCode.Double:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'double' and 'bool'");
                            case TypeCode.SByte: return (double)a * (sbyte)b;
                            case TypeCode.Int16: return (double)a * (short)b;
                            case TypeCode.UInt16: return (double)a * (ushort)b;
                            case TypeCode.Int32: return (double)a * (int)b;
                            case TypeCode.UInt32: return (double)a * (uint)b;
                            case TypeCode.Int64: return (double)a * (long)b;
                            case TypeCode.UInt64: return (double)a * (ulong)b;
                            case TypeCode.Single: return (double)a * (float)b;
                            case TypeCode.Double: return (double)a * (double)b;
                            case TypeCode.Decimal: return Convert.ToDecimal(a) * (decimal)b;
                        }
                        break;
                    case TypeCode.Decimal:
                        switch (typeCodeB) {
                            case TypeCode.Boolean: throw new InvalidOperationException("Operator '*' can't be applied to operands of types 'decimal' and 'bool'");
                            case TypeCode.SByte: return (decimal)a * (sbyte)b;
                            case TypeCode.Int16: return (decimal)a * (short)b;
                            case TypeCode.UInt16: return (decimal)a * (ushort)b;
                            case TypeCode.Int32: return (decimal)a * (int)b;
                            case TypeCode.UInt32: return (decimal)a * (uint)b;
                            case TypeCode.Int64: return (decimal)a * (long)b;
                            case TypeCode.UInt64: return (decimal)a * (ulong)b;
                            case TypeCode.Single: return (decimal)a * Convert.ToDecimal(b);
                            case TypeCode.Double: return (decimal)a * Convert.ToDecimal(b);
                            case TypeCode.Decimal: return (decimal)a * (decimal)b;
                        }
                        break;
                }
            }
            return null;
        }

        public static object Divide(object a, object b, EvaluateOptions options) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (options.HasFlag(EvaluateOptions.BooleanCalculation)) {
                a = ConvertIfBoolean(a);
                b = ConvertIfBoolean(b);
            }
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            switch (typeCodeA) {
                case TypeCode.Byte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'byte' and 'bool'");
                        case TypeCode.SByte: return (byte)a / (sbyte)b;
                        case TypeCode.Int16: return (byte)a / (short)b;
                        case TypeCode.UInt16: return (byte)a / (ushort)b;
                        case TypeCode.Int32: return (byte)a / (int)b;
                        case TypeCode.UInt32: return (byte)a / (uint)b;
                        case TypeCode.Int64: return (byte)a / (long)b;
                        case TypeCode.UInt64: return (byte)a / (ulong)b;
                        case TypeCode.Single: return (byte)a / (float)b;
                        case TypeCode.Double: return (byte)a / (double)b;
                        case TypeCode.Decimal: return (byte)a / (decimal)b;
                    }
                    break;
                case TypeCode.SByte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'sbyte' and 'bool'");
                        case TypeCode.SByte: return (sbyte)a / (sbyte)b;
                        case TypeCode.Int16: return (sbyte)a / (short)b;
                        case TypeCode.UInt16: return (sbyte)a / (ushort)b;
                        case TypeCode.Int32: return (sbyte)a / (int)b;
                        case TypeCode.UInt32: return (sbyte)a / (uint)b;
                        case TypeCode.Int64: return (sbyte)a / (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'sbyte' and 'ulong'");
                        case TypeCode.Single: return (sbyte)a / (float)b;
                        case TypeCode.Double: return (sbyte)a / (double)b;
                        case TypeCode.Decimal: return (sbyte)a / (decimal)b;
                    }
                    break;
                case TypeCode.Int16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'short' and 'bool'");
                        case TypeCode.SByte: return (short)a / (sbyte)b;
                        case TypeCode.Int16: return (short)a / (short)b;
                        case TypeCode.UInt16: return (short)a / (ushort)b;
                        case TypeCode.Int32: return (short)a / (int)b;
                        case TypeCode.UInt32: return (short)a / (uint)b;
                        case TypeCode.Int64: return (short)a / (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'short' and 'ulong'");
                        case TypeCode.Single: return (short)a / (float)b;
                        case TypeCode.Double: return (short)a / (double)b;
                        case TypeCode.Decimal: return (short)a / (decimal)b;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ushort' and 'bool'");
                        case TypeCode.SByte: return (ushort)a / (sbyte)b;
                        case TypeCode.Int16: return (ushort)a / (short)b;
                        case TypeCode.UInt16: return (ushort)a / (ushort)b;
                        case TypeCode.Int32: return (ushort)a / (int)b;
                        case TypeCode.UInt32: return (ushort)a / (uint)b;
                        case TypeCode.Int64: return (ushort)a / (long)b;
                        case TypeCode.UInt64: return (ushort)a / (ulong)b;
                        case TypeCode.Single: return (ushort)a / (float)b;
                        case TypeCode.Double: return (ushort)a / (double)b;
                        case TypeCode.Decimal: return (ushort)a / (decimal)b;
                    }
                    break;
                case TypeCode.Int32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'int' and 'bool'");
                        case TypeCode.SByte: return (int)a / (sbyte)b;
                        case TypeCode.Int16: return (int)a / (short)b;
                        case TypeCode.UInt16: return (int)a / (ushort)b;
                        case TypeCode.Int32: return (int)a / (int)b;
                        case TypeCode.UInt32: return (int)a / (uint)b;
                        case TypeCode.Int64: return (int)a / (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'int' and 'ulong'");
                        case TypeCode.Single: return (int)a / (float)b;
                        case TypeCode.Double: return (int)a / (double)b;
                        case TypeCode.Decimal: return (int)a / (decimal)b;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'uint' and 'bool'");
                        case TypeCode.SByte: return (uint)a / (sbyte)b;
                        case TypeCode.Int16: return (uint)a / (short)b;
                        case TypeCode.UInt16: return (uint)a / (ushort)b;
                        case TypeCode.Int32: return (uint)a / (int)b;
                        case TypeCode.UInt32: return (uint)a / (uint)b;
                        case TypeCode.Int64: return (uint)a / (long)b;
                        case TypeCode.UInt64: return (uint)a / (ulong)b;
                        case TypeCode.Single: return (uint)a / (float)b;
                        case TypeCode.Double: return (uint)a / (double)b;
                        case TypeCode.Decimal: return (uint)a / (decimal)b;
                    }
                    break;
                case TypeCode.Int64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'long' and 'bool'");
                        case TypeCode.SByte: return (long)a / (sbyte)b;
                        case TypeCode.Int16: return (long)a / (short)b;
                        case TypeCode.UInt16: return (long)a / (ushort)b;
                        case TypeCode.Int32: return (long)a / (int)b;
                        case TypeCode.UInt32: return (long)a / (uint)b;
                        case TypeCode.Int64: return (long)a / (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'long' and 'ulong'");
                        case TypeCode.Single: return (long)a / (float)b;
                        case TypeCode.Double: return (long)a / (double)b;
                        case TypeCode.Decimal: return (long)a / (decimal)b;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '-' can't be applied to operands of types 'ulong' and 'bool'");
                        case TypeCode.SByte: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'sbyte'");
                        case TypeCode.Int16: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'short'");
                        case TypeCode.UInt16: return (ulong)a / (ushort)b;
                        case TypeCode.Int32: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'int'");
                        case TypeCode.UInt32: return (ulong)a / (uint)b;
                        case TypeCode.Int64: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'ulong' and 'long'");
                        case TypeCode.UInt64: return (ulong)a / (ulong)b;
                        case TypeCode.Single: return (ulong)a / (float)b;
                        case TypeCode.Double: return (ulong)a / (double)b;
                        case TypeCode.Decimal: return (ulong)a / (decimal)b;
                    }
                    break;
                case TypeCode.Single:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'float' and 'bool'");
                        case TypeCode.SByte: return (float)a / (sbyte)b;
                        case TypeCode.Int16: return (float)a / (short)b;
                        case TypeCode.UInt16: return (float)a / (ushort)b;
                        case TypeCode.Int32: return (float)a / (int)b;
                        case TypeCode.UInt32: return (float)a / (uint)b;
                        case TypeCode.Int64: return (float)a / (long)b;
                        case TypeCode.UInt64: return (float)a / (ulong)b;
                        case TypeCode.Single: return (float)a / (float)b;
                        case TypeCode.Double: return (float)a / (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) / (decimal)b;
                    }
                    break;
                case TypeCode.Double:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'double' and 'bool'");
                        case TypeCode.SByte: return (double)a / (sbyte)b;
                        case TypeCode.Int16: return (double)a / (short)b;
                        case TypeCode.UInt16: return (double)a / (ushort)b;
                        case TypeCode.Int32: return (double)a / (int)b;
                        case TypeCode.UInt32: return (double)a / (uint)b;
                        case TypeCode.Int64: return (double)a / (long)b;
                        case TypeCode.UInt64: return (double)a / (ulong)b;
                        case TypeCode.Single: return (double)a / (float)b;
                        case TypeCode.Double: return (double)a / (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) / (decimal)b;
                    }
                    break;
                case TypeCode.Decimal:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '/' can't be applied to operands of types 'decimal' and 'bool'");
                        case TypeCode.SByte: return (decimal)a / (sbyte)b;
                        case TypeCode.Int16: return (decimal)a / (short)b;
                        case TypeCode.UInt16: return (decimal)a / (ushort)b;
                        case TypeCode.Int32: return (decimal)a / (int)b;
                        case TypeCode.UInt32: return (decimal)a / (uint)b;
                        case TypeCode.Int64: return (decimal)a / (long)b;
                        case TypeCode.UInt64: return (decimal)a / (ulong)b;
                        case TypeCode.Single: return (decimal)a / Convert.ToDecimal(b);
                        case TypeCode.Double: return (decimal)a / Convert.ToDecimal(b);
                        case TypeCode.Decimal: return (decimal)a / (decimal)b;
                    }
                    break;
            }
            return null;
        }

        public static object Modulo(object a, object b) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            switch (typeCodeA) {
                case TypeCode.Byte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'byte' and 'bool'");
                        case TypeCode.SByte: return (byte)a % (sbyte)b;
                        case TypeCode.Int16: return (byte)a % (short)b;
                        case TypeCode.UInt16: return (byte)a % (ushort)b;
                        case TypeCode.Int32: return (byte)a % (int)b;
                        case TypeCode.UInt32: return (byte)a % (uint)b;
                        case TypeCode.Int64: return (byte)a % (long)b;
                        case TypeCode.UInt64: return (byte)a % (ulong)b;
                        case TypeCode.Single: return (byte)a % (float)b;
                        case TypeCode.Double: return (byte)a % (double)b;
                        case TypeCode.Decimal: return (byte)a % (decimal)b;
                    }
                    break;
                case TypeCode.SByte:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'sbyte' and 'bool'");
                        case TypeCode.SByte: return (sbyte)a % (sbyte)b;
                        case TypeCode.Int16: return (sbyte)a % (short)b;
                        case TypeCode.UInt16: return (sbyte)a % (ushort)b;
                        case TypeCode.Int32: return (sbyte)a % (int)b;
                        case TypeCode.UInt32: return (sbyte)a % (uint)b;
                        case TypeCode.Int64: return (sbyte)a % (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'sbyte' and 'ulong'");
                        case TypeCode.Single: return (sbyte)a % (float)b;
                        case TypeCode.Double: return (sbyte)a % (double)b;
                        case TypeCode.Decimal: return (sbyte)a % (decimal)b;
                    }
                    break;
                case TypeCode.Int16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'short' and 'bool'");
                        case TypeCode.SByte: return (short)a % (sbyte)b;
                        case TypeCode.Int16: return (short)a % (short)b;
                        case TypeCode.UInt16: return (short)a % (ushort)b;
                        case TypeCode.Int32: return (short)a % (int)b;
                        case TypeCode.UInt32: return (short)a % (uint)b;
                        case TypeCode.Int64: return (short)a % (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'short' and 'ulong'");
                        case TypeCode.Single: return (short)a % (float)b;
                        case TypeCode.Double: return (short)a % (double)b;
                        case TypeCode.Decimal: return (short)a % (decimal)b;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ushort' and 'bool'");
                        case TypeCode.SByte: return (ushort)a % (sbyte)b;
                        case TypeCode.Int16: return (ushort)a % (short)b;
                        case TypeCode.UInt16: return (ushort)a % (ushort)b;
                        case TypeCode.Int32: return (ushort)a % (int)b;
                        case TypeCode.UInt32: return (ushort)a % (uint)b;
                        case TypeCode.Int64: return (ushort)a % (long)b;
                        case TypeCode.UInt64: return (ushort)a % (ulong)b;
                        case TypeCode.Single: return (ushort)a % (float)b;
                        case TypeCode.Double: return (ushort)a % (double)b;
                        case TypeCode.Decimal: return (ushort)a % (decimal)b;
                    }
                    break;
                case TypeCode.Int32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'int' and 'bool'");
                        case TypeCode.SByte: return (int)a % (sbyte)b;
                        case TypeCode.Int16: return (int)a % (short)b;
                        case TypeCode.UInt16: return (int)a % (ushort)b;
                        case TypeCode.Int32: return (int)a % (int)b;
                        case TypeCode.UInt32: return (int)a % (uint)b;
                        case TypeCode.Int64: return (int)a % (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'int' and 'ulong'");
                        case TypeCode.Single: return (int)a % (float)b;
                        case TypeCode.Double: return (int)a % (double)b;
                        case TypeCode.Decimal: return (int)a % (decimal)b;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'uint' and 'bool'");
                        case TypeCode.SByte: return (uint)a % (sbyte)b;
                        case TypeCode.Int16: return (uint)a % (short)b;
                        case TypeCode.UInt16: return (uint)a % (ushort)b;
                        case TypeCode.Int32: return (uint)a % (int)b;
                        case TypeCode.UInt32: return (uint)a % (uint)b;
                        case TypeCode.Int64: return (uint)a % (long)b;
                        case TypeCode.UInt64: return (uint)a % (ulong)b;
                        case TypeCode.Single: return (uint)a % (float)b;
                        case TypeCode.Double: return (uint)a % (double)b;
                        case TypeCode.Decimal: return (uint)a % (decimal)b;
                    }
                    break;
                case TypeCode.Int64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'long' and 'bool'");
                        case TypeCode.SByte: return (long)a % (sbyte)b;
                        case TypeCode.Int16: return (long)a % (short)b;
                        case TypeCode.UInt16: return (long)a % (ushort)b;
                        case TypeCode.Int32: return (long)a % (int)b;
                        case TypeCode.UInt32: return (long)a % (uint)b;
                        case TypeCode.Int64: return (long)a % (long)b;
                        case TypeCode.UInt64: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'long' and 'ulong'");
                        case TypeCode.Single: return (long)a % (float)b;
                        case TypeCode.Double: return (long)a % (double)b;
                        case TypeCode.Decimal: return (long)a % (decimal)b;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'bool'");
                        case TypeCode.SByte: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'sbyte'");
                        case TypeCode.Int16: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'short'");
                        case TypeCode.UInt16: return (ulong)a % (ushort)b;
                        case TypeCode.Int32: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'int'");
                        case TypeCode.UInt32: return (ulong)a % (uint)b;
                        case TypeCode.Int64: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'ulong' and 'long'");
                        case TypeCode.UInt64: return (ulong)a % (ulong)b;
                        case TypeCode.Single: return (ulong)a % (float)b;
                        case TypeCode.Double: return (ulong)a % (double)b;
                        case TypeCode.Decimal: return (ulong)a % (decimal)b;
                    }
                    break;
                case TypeCode.Single:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'float' and 'bool'");
                        case TypeCode.SByte: return (float)a % (sbyte)b;
                        case TypeCode.Int16: return (float)a % (short)b;
                        case TypeCode.UInt16: return (float)a % (ushort)b;
                        case TypeCode.Int32: return (float)a % (int)b;
                        case TypeCode.UInt32: return (float)a % (uint)b;
                        case TypeCode.Int64: return (float)a % (long)b;
                        case TypeCode.UInt64: return (float)a % (ulong)b;
                        case TypeCode.Single: return (float)a % (float)b;
                        case TypeCode.Double: return (float)a % (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) % (decimal)b;
                    }
                    break;
                case TypeCode.Double:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'double' and 'bool'");
                        case TypeCode.SByte: return (double)a % (sbyte)b;
                        case TypeCode.Int16: return (double)a % (short)b;
                        case TypeCode.UInt16: return (double)a % (ushort)b;
                        case TypeCode.Int32: return (double)a % (int)b;
                        case TypeCode.UInt32: return (double)a % (uint)b;
                        case TypeCode.Int64: return (double)a % (long)b;
                        case TypeCode.UInt64: return (double)a % (ulong)b;
                        case TypeCode.Single: return (double)a % (float)b;
                        case TypeCode.Double: return (double)a % (double)b;
                        case TypeCode.Decimal: return Convert.ToDecimal(a) % (decimal)b;
                    }
                    break;
                case TypeCode.Decimal:
                    switch (typeCodeB) {
                        case TypeCode.Boolean: throw new InvalidOperationException("Operator '%' can't be applied to operands of types 'decimal' and 'bool'");
                        case TypeCode.SByte: return (decimal)a % (sbyte)b;
                        case TypeCode.Int16: return (decimal)a % (short)b;
                        case TypeCode.UInt16: return (decimal)a % (ushort)b;
                        case TypeCode.Int32: return (decimal)a % (int)b;
                        case TypeCode.UInt32: return (decimal)a % (uint)b;
                        case TypeCode.Int64: return (decimal)a % (long)b;
                        case TypeCode.UInt64: return (decimal)a % (ulong)b;
                        case TypeCode.Single: return (decimal)a % Convert.ToDecimal(b);
                        case TypeCode.Double: return (decimal)a % Convert.ToDecimal(b);
                        case TypeCode.Decimal: return (decimal)a % (decimal)b;
                    }
                    break;
            }
            return null;
        }

        public static object Max(object a, object b) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (a == null
                && b == null) {
                return null;
            }
            if (a == null) {
                return b;
            }
            if (b == null) {
                return a;
            }
            TypeCode typeCode = ConvertToHighestPrecision(ref a, ref b);
            switch (typeCode) {
                case TypeCode.Byte: return Math.Max((byte)a, (byte)b);
                case TypeCode.SByte: return Math.Max((sbyte)a, (sbyte)b);
                case TypeCode.Int16: return Math.Max((short)a, (short)b);
                case TypeCode.UInt16: return Math.Max((ushort)a, (ushort)b);
                case TypeCode.Int32: return Math.Max((int)a, (int)b);
                case TypeCode.UInt32: return Math.Max((uint)a, (uint)b);
                case TypeCode.Int64: return Math.Max((long)a, (long)b);
                case TypeCode.UInt64: return Math.Max((ulong)a, (ulong)b);
                case TypeCode.Single: return Math.Max((float)a, (float)b);
                case TypeCode.Double: return Math.Max((double)a, (double)b);
                case TypeCode.Decimal: return Math.Max((decimal)a, (decimal)b);
            }
            return null;
        }

        public static object Min(object a, object b) {
            a = ConvertIfString(a);
            b = ConvertIfString(b);
            if (a == null
                && b == null) {
                return null;
            }
            if (a == null) {
                return b;
            }
            if (b == null) {
                return a;
            }
            TypeCode typeCode = ConvertToHighestPrecision(ref a, ref b);
            switch (typeCode) {
                case TypeCode.Byte: return Math.Min((byte)a, (byte)b);
                case TypeCode.SByte: return Math.Min((sbyte)a, (sbyte)b);
                case TypeCode.Int16: return Math.Min((short)a, (short)b);
                case TypeCode.UInt16: return Math.Min((ushort)a, (ushort)b);
                case TypeCode.Int32: return Math.Min((int)a, (int)b);
                case TypeCode.UInt32: return Math.Min((uint)a, (uint)b);
                case TypeCode.Int64: return Math.Min((long)a, (long)b);
                case TypeCode.UInt64: return Math.Min((ulong)a, (ulong)b);
                case TypeCode.Single: return Math.Min((float)a, (float)b);
                case TypeCode.Double: return Math.Min((double)a, (double)b);
                case TypeCode.Decimal: return Math.Min((decimal)a, (decimal)b);
            }
            return null;
        }


        static TypeCode ConvertToHighestPrecision(ref object a, ref object b) {
            TypeCode typeCodeA = a.GetTypeCode();
            TypeCode typeCodeB = b.GetTypeCode();
            if (typeCodeA == typeCodeB) {
                return typeCodeA;
            }
            if (!(TypeCodeBitSize(typeCodeA, out bool floatingPointA) is int bitSizeA)) {
                return TypeCode.Empty;
            }
            if (!(TypeCodeBitSize(typeCodeB, out bool floatingPointB) is int bitSizeB)) {
                return TypeCode.Empty;
            }
            if (floatingPointA != floatingPointB) {
                if (floatingPointA) {
                    b = ConvertTo(b, typeCodeA);
                    return typeCodeA;
                }
                a = ConvertTo(a, typeCodeB);
                return typeCodeB;
            }
            if (bitSizeA > bitSizeB) {
                b = ConvertTo(b, typeCodeA);
                return typeCodeA;
            }
            a = ConvertTo(a, typeCodeB);
            return typeCodeB;
        }

        static int? TypeCodeBitSize(TypeCode typeCode, out bool floatingPoint) {
            floatingPoint = false;
            switch (typeCode) {
                case TypeCode.SByte: return 8;
                case TypeCode.Byte: return 8;
                case TypeCode.Int16: return 16;
                case TypeCode.UInt16: return 16;
                case TypeCode.Int32: return 32;
                case TypeCode.UInt32: return 32;
                case TypeCode.Int64: return 64;
                case TypeCode.UInt64: return 64;
                case TypeCode.Single:
                    floatingPoint = true;
                    return 32;
                case TypeCode.Double:
                    floatingPoint = true;
                    return 64;
                case TypeCode.Decimal:
                    floatingPoint = true;
                    return 128;
                default: return null;
            }
        }

        static object ConvertTo(object value, TypeCode toType) {
            switch (toType) {
                case TypeCode.Byte: return Convert.ToByte(value);
                case TypeCode.SByte: return Convert.ToSByte(value);
                case TypeCode.Int16: return Convert.ToInt16(value);
                case TypeCode.UInt16: return Convert.ToUInt16(value);
                case TypeCode.Int32: return Convert.ToInt32(value);
                case TypeCode.UInt32: return Convert.ToUInt32(value);
                case TypeCode.Int64: return Convert.ToInt64(value);
                case TypeCode.UInt64: return Convert.ToUInt64(value);
                case TypeCode.Single: return Convert.ToSingle(value);
                case TypeCode.Double: return Convert.ToDouble(value);
                case TypeCode.Decimal: return Convert.ToDecimal(value);
            }
            return null;
        }
    }
}