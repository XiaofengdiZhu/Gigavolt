using Engine;

namespace Game {
    public abstract class GVSignCBlock : Block {
        public abstract BlockMesh GetSignSurfaceBlockMesh(int data);

        public abstract Vector3 GetSignSurfaceNormal(int data);
    }
}