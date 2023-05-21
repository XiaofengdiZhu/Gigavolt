using System;

namespace NCalc {
    public class ParameterArgs : EventArgs {
        object _result;

        public object Result {
            get => _result;
            set {
                _result = value;
                HasResult = true;
            }
        }

        public bool HasResult { get; set; }
    }
}