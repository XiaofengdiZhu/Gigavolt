using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCalc.Domain;
using L = System.Linq.Expressions;

namespace NCalc {
    class LambdaExpressionVistor : LogicalExpressionVisitor {
        readonly IDictionary<string, object> _parameters;
        readonly L.Expression _context;
        readonly EvaluateOptions _options = EvaluateOptions.None;

        readonly Dictionary<Type, HashSet<Type>> _implicitPrimitiveConversionTable = new Dictionary<Type, HashSet<Type>> {
            {
                typeof(sbyte), new HashSet<Type> {
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            }, {
                typeof(byte), new HashSet<Type> {
                    typeof(short),
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            }, {
                typeof(short), new HashSet<Type> {
                    typeof(int),
                    typeof(long),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            }, {
                typeof(ushort), new HashSet<Type> {
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            { typeof(int), new HashSet<Type> { typeof(long), typeof(float), typeof(double), typeof(decimal) } }, {
                typeof(uint), new HashSet<Type> {
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            { typeof(long), new HashSet<Type> { typeof(float), typeof(double), typeof(decimal) } }, {
                typeof(char), new HashSet<Type> {
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            { typeof(float), new HashSet<Type> { typeof(double) } },
            { typeof(ulong), new HashSet<Type> { typeof(float), typeof(double), typeof(decimal) } }
        };

        bool Ordinal => (_options & EvaluateOptions.MatchStringsOrdinal) == EvaluateOptions.MatchStringsOrdinal;
        bool IgnoreCaseString => (_options & EvaluateOptions.MatchStringsWithIgnoreCase) == EvaluateOptions.MatchStringsWithIgnoreCase;
        bool Checked => (_options & EvaluateOptions.OverflowProtection) == EvaluateOptions.OverflowProtection;

        public LambdaExpressionVistor(IDictionary<string, object> parameters, EvaluateOptions options) {
            _parameters = parameters;
            _options = options;
        }

        public LambdaExpressionVistor(L.ParameterExpression context, EvaluateOptions options) {
            _context = context;
            _options = options;
        }

        public L.Expression Result { get; private set; }

        public override void Visit(LogicalExpression expression) {
            throw new NotImplementedException();
        }

        public override void Visit(TernaryExpression expression) {
            expression.LeftExpression.Accept(this);
            L.Expression test = Result;
            expression.MiddleExpression.Accept(this);
            L.Expression ifTrue = Result;
            expression.RightExpression.Accept(this);
            L.Expression ifFalse = Result;
            Result = L.Expression.Condition(test, ifTrue, ifFalse);
        }

        public override void Visit(BinaryExpression expression) {
            expression.LeftExpression.Accept(this);
            L.Expression left = Result;
            expression.RightExpression.Accept(this);
            L.Expression right = Result;
            switch (expression.Type) {
                case BinaryExpressionType.And:
                    Result = L.Expression.AndAlso(left, right);
                    break;
                case BinaryExpressionType.Or:
                    Result = L.Expression.OrElse(left, right);
                    break;
                case BinaryExpressionType.NotEqual:
                    Result = WithCommonNumericType(left, right, L.Expression.NotEqual, expression.Type);
                    break;
                case BinaryExpressionType.LesserOrEqual:
                    Result = WithCommonNumericType(left, right, L.Expression.LessThanOrEqual, expression.Type);
                    break;
                case BinaryExpressionType.GreaterOrEqual:
                    Result = WithCommonNumericType(left, right, L.Expression.GreaterThanOrEqual, expression.Type);
                    break;
                case BinaryExpressionType.Lesser:
                    Result = WithCommonNumericType(left, right, L.Expression.LessThan, expression.Type);
                    break;
                case BinaryExpressionType.Greater:
                    Result = WithCommonNumericType(left, right, L.Expression.GreaterThan, expression.Type);
                    break;
                case BinaryExpressionType.Equal:
                    Result = WithCommonNumericType(left, right, L.Expression.Equal, expression.Type);
                    break;
                case BinaryExpressionType.Minus:
                    if (Checked) {
                        Result = WithCommonNumericType(left, right, L.Expression.SubtractChecked);
                    }
                    else {
                        Result = WithCommonNumericType(left, right, L.Expression.Subtract);
                    }
                    break;
                case BinaryExpressionType.Plus:
                    if (Checked) {
                        Result = WithCommonNumericType(left, right, L.Expression.AddChecked);
                    }
                    else {
                        Result = WithCommonNumericType(left, right, L.Expression.Add);
                    }
                    break;
                case BinaryExpressionType.Modulo:
                    Result = WithCommonNumericType(left, right, L.Expression.Modulo);
                    break;
                case BinaryExpressionType.Div:
                    Result = WithCommonNumericType(left, right, L.Expression.Divide);
                    break;
                case BinaryExpressionType.Times:
                    if (Checked) {
                        Result = WithCommonNumericType(left, right, L.Expression.MultiplyChecked);
                    }
                    else {
                        Result = WithCommonNumericType(left, right, L.Expression.Multiply);
                    }
                    break;
                case BinaryExpressionType.BitwiseOr:
                    Result = L.Expression.Or(left, right);
                    break;
                case BinaryExpressionType.BitwiseAnd:
                    Result = L.Expression.And(left, right);
                    break;
                case BinaryExpressionType.BitwiseXOr:
                    Result = L.Expression.ExclusiveOr(left, right);
                    break;
                case BinaryExpressionType.LeftShift:
                    Result = L.Expression.LeftShift(left, right);
                    break;
                case BinaryExpressionType.RightShift:
                    Result = L.Expression.RightShift(left, right);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override void Visit(UnaryExpression expression) {
            expression.Expression.Accept(this);
            switch (expression.Type) {
                case UnaryExpressionType.Not:
                    Result = L.Expression.Not(Result);
                    break;
                case UnaryExpressionType.Negate:
                    Result = L.Expression.Negate(Result);
                    break;
                case UnaryExpressionType.BitwiseNot:
                    Result = L.Expression.Not(Result);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override void Visit(ValueExpression expression) {
            Result = L.Expression.Constant(expression.Value);
        }

        public override void Visit(Function function) {
            int argCount = function.Expressions.Length;
            L.Expression[] args = new L.Expression[argCount];
            for (int i = 0; i < argCount; i++) {
                function.Expressions[i].Accept(this);
                args[i] = Result;
            }
            string functionName = function.Identifier.Name.ToLowerInvariant();
            if (functionName == "if") {
                Type[] numberTypePriority = { typeof(double), typeof(float), typeof(long), typeof(int), typeof(short) };
                int index1 = Array.IndexOf(numberTypePriority, args[1].Type);
                int index2 = Array.IndexOf(numberTypePriority, args[2].Type);
                if (index1 >= 0
                    && index2 >= 0
                    && index1 != index2) {
                    args[1] = L.Expression.Convert(args[1], numberTypePriority[Math.Min(index1, index2)]);
                    args[2] = L.Expression.Convert(args[2], numberTypePriority[Math.Min(index1, index2)]);
                }
                Result = L.Expression.Condition(args[0], args[1], args[2]);
                return;
            }
            if (functionName == "in") {
                L.NewArrayExpression items = L.Expression.NewArrayInit(args[0].Type, new ArraySegment<L.Expression>(args, 1, args.Length - 1));
                MethodInfo smi = typeof(Array).GetRuntimeMethod("IndexOf", new[] { typeof(Array), typeof(object) });
                L.MethodCallExpression r = L.Expression.Call(smi, L.Expression.Convert(items, typeof(Array)), L.Expression.Convert(args[0], typeof(object)));
                Result = L.Expression.GreaterThanOrEqual(r, L.Expression.Constant(0));
                return;
            }

            //Context methods take precedence over built-in functions because they're user-customisable.
            ExtendedMethodInfo mi = FindMethod(function.Identifier.Name, args);
            if (mi != null) {
                Result = L.Expression.Call(_context, mi.BaseMethodInfo, mi.PreparedArguments);
                return;
            }
            if (argCount == 0) {
                throw new TargetParameterCountException();
            }
            L.UnaryExpression arg0 = L.Expression.Convert(args[0], typeof(double));
            L.UnaryExpression arg1 = argCount >= 2 ? L.Expression.Convert(args[1], typeof(double)) : null;
            switch (functionName) {
                case "min":
                    if (argCount < 2) {
                        throw new TargetParameterCountException();
                    }
                    Result = L.Expression.Condition(L.Expression.LessThan(arg0, arg1), arg0, arg1);
                    break;
                case "max":
                    if (argCount < 2) {
                        throw new TargetParameterCountException();
                    }
                    Result = L.Expression.Condition(L.Expression.GreaterThan(arg0, arg1), arg0, arg1);
                    break;
                case "pow":
                    if (argCount < 2) {
                        throw new TargetParameterCountException();
                    }
                    Result = L.Expression.Power(arg0, arg1);
                    break;
                case "abs":
                    Result = MethExpressionWithOneParameter("Abs", arg0);
                    break;
                case "acos":
                    Result = MethExpressionWithOneParameter("Acos", arg0);
                    break;
                case "asin":
                    Result = MethExpressionWithOneParameter("Asin", arg0);
                    break;
                case "atan":
                    Result = MethExpressionWithOneParameter("Atan", arg0);
                    break;
                case "atan2":
                    if (argCount < 2) {
                        throw new TargetParameterCountException();
                    }
                    Result = MethExpressionWithTwoParameter("Atan2", arg0, arg1);
                    break;
                case "ceiling":
                    Result = MethExpressionWithOneParameter("Ceiling", arg0);
                    break;
                case "cos":
                    Result = MethExpressionWithOneParameter("Cos", arg0);
                    break;
                case "cosh":
                    Result = MethExpressionWithOneParameter("Cosh", arg0);
                    break;
                case "exp":
                    Result = MethExpressionWithOneParameter("Exp", arg0);
                    break;
                case "floor":
                    Result = MethExpressionWithOneParameter("Floor", arg0);
                    break;
                case "ieeremainder":
                    if (argCount < 2) {
                        throw new TargetParameterCountException();
                    }
                    Result = MethExpressionWithTwoParameter("IEEERemainder", arg0, arg1);
                    break;
                case "log":
                    Result = argCount >= 2 ? MethExpressionWithTwoParameter("Log", arg0, arg1) : MethExpressionWithOneParameter("Log", arg0);
                    break;
                case "log10":
                    Result = MethExpressionWithOneParameter("Log10", arg0);
                    break;
                case "round":
                    if (argCount >= 2) {
                        L.Expression.Call(typeof(Math).GetRuntimeMethod("Round", new[] { typeof(double), typeof(int) }), arg0, L.Expression.Convert(args[1], typeof(int)));
                    }
                    else {
                        Result = MethExpressionWithOneParameter("Round", arg0);
                    }
                    break;
                case "sign":
                    Result = MethExpressionWithOneParameter("Sign", arg0);
                    break;
                case "sin":
                    Result = MethExpressionWithOneParameter("Sin", arg0);
                    break;
                case "sinh":
                    Result = MethExpressionWithOneParameter("Sinh", arg0);
                    break;
                case "sqrt":
                    Result = MethExpressionWithOneParameter("Sqrt", arg0);
                    break;
                case "tan":
                    Result = MethExpressionWithOneParameter("Tan", arg0);
                    break;
                case "tanh":
                    Result = MethExpressionWithOneParameter("Tanh", arg0);
                    break;
                case "truncate":
                    Result = MethExpressionWithOneParameter("Truncate", arg0);
                    break;
                default: throw new MissingMethodException($"method not found: {functionName}");
            }
        }

        public L.MethodCallExpression MethExpressionWithOneParameter(string methodName, L.Expression arg) {
            return L.Expression.Call(typeof(Math).GetRuntimeMethod(methodName, new[] { typeof(double) }), arg);
        }

        public L.MethodCallExpression MethExpressionWithTwoParameter(string methodName, L.Expression arg1, L.Expression arg2) {
            return L.Expression.Call(typeof(Math).GetRuntimeMethod(methodName, new[] { typeof(double), typeof(double) }), arg1, arg2);
        }

        public override void Visit(Identifier function) {
            if (_context == null) {
                Result = L.Expression.Constant(_parameters[function.Name]);
            }
            else {
                Result = L.Expression.PropertyOrField(_context, function.Name);
            }
        }

        ExtendedMethodInfo FindMethod(string methodName, L.Expression[] methodArgs) {
            if (_context == null) {
                return null;
            }
            TypeInfo contextTypeInfo = _context.Type.GetTypeInfo();
            TypeInfo objectTypeInfo = typeof(object).GetTypeInfo();
            do {
                IEnumerable<MethodInfo> methods = contextTypeInfo.DeclaredMethods.Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase) && m.IsPublic && !m.IsStatic);
                List<ExtendedMethodInfo> candidates = new List<ExtendedMethodInfo>();
                foreach (MethodInfo potentialMethod in methods) {
                    ParameterInfo[] methodParams = potentialMethod.GetParameters();
                    Tuple<int, L.Expression[]> preparedArguments = PrepareMethodArgumentsIfValid(methodParams, methodArgs);
                    if (preparedArguments != null) {
                        ExtendedMethodInfo candidate = new ExtendedMethodInfo { BaseMethodInfo = potentialMethod, PreparedArguments = preparedArguments.Item2, Score = preparedArguments.Item1 };
                        if (candidate.Score == 0) {
                            return candidate;
                        }
                        candidates.Add(candidate);
                    }
                }
                if (candidates.Any()) {
                    return candidates.OrderBy(method => method.Score).First();
                }
                contextTypeInfo = contextTypeInfo.BaseType.GetTypeInfo();
            }
            while (contextTypeInfo != objectTypeInfo);
            return null;
        }

        /// <summary>
        ///     Returns a tuple where the first item is a score, and the second is a list of prepared arguments.
        ///     Score is a simplified indicator of how close the arguments' types are to the parameters'. A score of 0 indicates a perfect match between arguments and parameters.
        ///     Prepared arguments refers to having the arguments implicitly converted where necessary, and "params" arguments collated into one array.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        Tuple<int, L.Expression[]> PrepareMethodArgumentsIfValid(ParameterInfo[] parameters, L.Expression[] arguments) {
            if (!parameters.Any()
                && !arguments.Any()) {
                return Tuple.Create(0, arguments);
            }
            if (!parameters.Any()) {
                return null;
            }
            ParameterInfo lastParameter = parameters.Last();
            bool hasParamsKeyword = lastParameter.IsDefined(typeof(ParamArrayAttribute));
            if (hasParamsKeyword && parameters.Length > arguments.Length) {
                return null;
            }
            L.Expression[] newArguments = new L.Expression[parameters.Length];
            L.Expression[] paramsKeywordArgument = null;
            Type paramsElementType = null;
            int paramsParameterPosition = 0;
            if (!hasParamsKeyword) {
                if (parameters.Length != arguments.Length) {
                    return null;
                }
            }
            else {
                paramsParameterPosition = lastParameter.Position;
                paramsElementType = lastParameter.ParameterType.GetElementType();
                paramsKeywordArgument = new L.Expression[arguments.Length - parameters.Length + 1];
            }
            int functionMemberScore = 0;
            for (int i = 0; i < arguments.Length; i++) {
                bool isParamsElement = hasParamsKeyword && i >= paramsParameterPosition;
                L.Expression argument = arguments[i];
                Type argumentType = argument.Type;
                Type parameterType = isParamsElement ? paramsElementType : parameters[i].ParameterType;
                if (argumentType != parameterType) {
                    bool canCastImplicitly = TryCastImplicitly(argumentType, parameterType, ref argument);
                    if (!canCastImplicitly) {
                        return null;
                    }
                    functionMemberScore++;
                }
                if (!isParamsElement) {
                    newArguments[i] = argument;
                }
                else {
                    paramsKeywordArgument[i - paramsParameterPosition] = argument;
                }
            }
            if (hasParamsKeyword) {
                newArguments[paramsParameterPosition] = L.Expression.NewArrayInit(paramsElementType, paramsKeywordArgument);
            }
            return Tuple.Create(functionMemberScore, newArguments);
        }

        bool TryCastImplicitly(Type from, Type to, ref L.Expression argument) {
            bool convertingFromPrimitiveType = _implicitPrimitiveConversionTable.TryGetValue(from, out HashSet<Type> possibleConversions);
            if (!convertingFromPrimitiveType
                || !possibleConversions.Contains(to)) {
                argument = null;
                return false;
            }
            argument = L.Expression.Convert(argument, to);
            return true;
        }

        L.Expression WithCommonNumericType(L.Expression left, L.Expression right, Func<L.Expression, L.Expression, L.Expression> action, BinaryExpressionType expressiontype = BinaryExpressionType.Unknown) {
            left = UnwrapNullable(left);
            right = UnwrapNullable(right);
            if (_options.HasFlag(EvaluateOptions.BooleanCalculation)) {
                if (left.Type == typeof(bool)) {
                    left = L.Expression.Condition(left, L.Expression.Constant(1.0), L.Expression.Constant(0.0));
                }
                if (right.Type == typeof(bool)) {
                    right = L.Expression.Condition(right, L.Expression.Constant(1.0), L.Expression.Constant(0.0));
                }
            }
            Type[] precedence = { typeof(decimal), typeof(double), typeof(float), typeof(ulong), typeof(long), typeof(uint), typeof(int), typeof(ushort), typeof(short), typeof(byte), typeof(sbyte) };
            int l = Array.IndexOf(precedence, left.Type);
            int r = Array.IndexOf(precedence, right.Type);
            if (l >= 0
                && r >= 0) {
                Type type = precedence[Math.Min(l, r)];
                if (left.Type != type) {
                    left = L.Expression.Convert(left, type);
                }
                if (right.Type != type) {
                    right = L.Expression.Convert(right, type);
                }
            }
            L.Expression comparer = null;
            if (IgnoreCaseString) {
                if (Ordinal) {
                    comparer = L.Expression.Property(null, typeof(StringComparer), "OrdinalIgnoreCase");
                }
                else {
                    comparer = L.Expression.Property(null, typeof(StringComparer), "CurrentCultureIgnoreCase");
                }
            }
            else {
                comparer = L.Expression.Property(null, typeof(StringComparer), "Ordinal");
            }
            if (comparer != null
                && (typeof(string).Equals(left.Type) || typeof(string).Equals(right.Type))) {
                switch (expressiontype) {
                    case BinaryExpressionType.Equal: return L.Expression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Equals", new[] { typeof(string), typeof(string) }), new[] { left, right });
                    case BinaryExpressionType.NotEqual: return L.Expression.Not(L.Expression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Equals", new[] { typeof(string), typeof(string) }), new[] { left, right }));
                    case BinaryExpressionType.GreaterOrEqual: return L.Expression.GreaterThanOrEqual(L.Expression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare", new[] { typeof(string), typeof(string) }), new[] { left, right }), L.Expression.Constant(0));
                    case BinaryExpressionType.LesserOrEqual: return L.Expression.LessThanOrEqual(L.Expression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare", new[] { typeof(string), typeof(string) }), new[] { left, right }), L.Expression.Constant(0));
                    case BinaryExpressionType.Greater: return L.Expression.GreaterThan(L.Expression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare", new[] { typeof(string), typeof(string) }), new[] { left, right }), L.Expression.Constant(0));
                    case BinaryExpressionType.Lesser: return L.Expression.LessThan(L.Expression.Call(comparer, typeof(StringComparer).GetRuntimeMethod("Compare", new[] { typeof(string), typeof(string) }), new[] { left, right }), L.Expression.Constant(0));
                }
            }
            return action(left, right);
        }

        L.Expression UnwrapNullable(L.Expression expression) {
            TypeInfo ti = expression.Type.GetTypeInfo();
            if (ti.IsGenericType
                && ti.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                return L.Expression.Condition(L.Expression.Property(expression, "HasValue"), L.Expression.Property(expression, "Value"), L.Expression.Default(expression.Type.GetTypeInfo().GenericTypeArguments[0]));
            }
            return expression;
        }
    }
}