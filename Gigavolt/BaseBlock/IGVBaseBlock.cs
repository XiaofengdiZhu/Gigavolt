using System;

namespace Game {
    public interface IGVBaseBlock {
        public static RecipaediaDescriptionScreen DefaultRecipaediaDescriptionScreen = new();
        public Func<int, RecipaediaDescriptionScreen> GetBlockDescriptionScreenHandler { get; set; }
    }
}