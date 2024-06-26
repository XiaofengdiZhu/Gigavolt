using Engine;

namespace Game {
    public abstract class GVBaseSignBlock : Block {
        public abstract BlockMesh GetSignSurfaceBlockMesh(int data);

        public abstract Vector3 GetSignSurfaceNormal(int data);
    }
}