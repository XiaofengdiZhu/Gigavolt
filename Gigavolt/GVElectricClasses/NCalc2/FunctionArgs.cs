using System;

namespace NCalc {
    public class FunctionArgs : EventArgs {
        object _result;

        public object Result {
            get => _result;
            set {
                _result = value;
                HasResult = true;
            }
        }

        public bool HasResult { get; set; }

        public Expression[] Parameters { get; set; } = new Expression[0];

        public object[] EvaluateParameters() {
            object[] values = new object[Parameters.Length];
            for (int i = 0; i < values.Length; i++) {
                values[i] = Parameters[i].Evaluate();
            }
            return values;
        }
    }
}