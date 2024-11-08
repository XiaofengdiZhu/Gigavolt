using Engine;

namespace Game {
    public abstract class GVBaseSignBlock : GVBaseBlock {
        public abstract BlockMesh GetSignSurfaceBlockMesh(int data);

        public abstract Vector3 GetSignSurfaceNormal(int data);
    }
}