using System;

namespace Game {
    public abstract class GVBaseBlock : Block, IGVBaseBlock {
        public Func<int, RecipaediaDescriptionScreen> GetBlockDescriptionScreenHandler { get; set; } = _ => IGVBaseBlock.DefaultRecipaediaDescriptionScreen;
        public override RecipaediaDescriptionScreen GetBlockDescriptionScreen(int value) => GetBlockDescriptionScreenHandler(value);
    }
}