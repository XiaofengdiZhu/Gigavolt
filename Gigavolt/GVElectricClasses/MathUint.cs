namespace Game {
    public static class MathUint {
        public static uint Max(uint x1, uint x2) {
            if (x1 <= x2) {
                return x2;
            }
            return x1;
        }

        public static uint Min(uint x1, uint x2) {
            if (x1 >= x2) {
                return x2;
            }
            return x1;
        }

        public static uint Clamp(uint x, uint min, uint max) {
            if (x >= min) {
                if (x <= max) {
                    return x;
                }
                return max;
            }
            return min;
        }

        public static int ToInt(uint input) => input > int.MaxValue ? int.MaxValue : (int)input;
    }
}