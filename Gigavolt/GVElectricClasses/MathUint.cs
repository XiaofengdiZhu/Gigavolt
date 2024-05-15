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

        public static int ToIntWithClamp(uint input) => input > int.MaxValue ? int.MaxValue : (int)input;

        public static int ToIntWithSign(uint input, int signOffset = 0) {
            switch (signOffset) {
                case 16: return (int)(input & 0x7FFFu) * ((input & 0x8000u) == 0x8000u ? -1 : 1);
                case 8: return (int)(input & 0x7Fu) * ((input & 0x8u) == 8u ? -1 : 1);
                case 0:
                case 32: return (int)input;
                case < 0:
                case > 32: return 0;
                default: {
                    uint temp = 1u << signOffset;
                    return (int)(input & (uint.MaxValue >> (32 - signOffset))) * ((input & temp) == temp ? 1 : -1);
                }
            }
        }
    }
}