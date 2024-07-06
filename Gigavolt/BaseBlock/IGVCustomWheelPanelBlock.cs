using System.Collections.Generic;

namespace Game {
    public interface IGVCustomWheelPanelBlock {
        public List<int> GetCustomWheelPanelValues(int centerValue);
    }
}