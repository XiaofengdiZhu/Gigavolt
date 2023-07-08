using Engine;

namespace Game {
    public class GVDispenserCBlock : CubeBlock, IGVElectricElementBlock {
        public enum Mode { Dispense, Shoot }

        public const int Index = 841;

        public override int GetFaceTextureSlot(int face, int value) {
            int direction = GetDirection(Terrain.ExtractData(value));
            if (face != direction) {
                return 42;
            }
            return 59;
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float num = Vector3.Dot(forward, Vector3.UnitZ);
            float num2 = Vector3.Dot(forward, Vector3.UnitX);
            float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
            float num4 = Vector3.Dot(forward, -Vector3.UnitX);
            float num5 = Vector3.Dot(forward, Vector3.UnitY);
            float num6 = Vector3.Dot(forward, -Vector3.UnitY);
            float num7 = MathUtils.Min(MathUtils.Min(num, num2, num3), MathUtils.Min(num4, num5, num6));
            int direction = 0;
            if (num == num7) {
                direction = 0;
            }
            else if (num2 == num7) {
                direction = 1;
            }
            else if (num3 == num7) {
                direction = 2;
            }
            else if (num4 == num7) {
                direction = 3;
            }
            else if (num5 == num7) {
                direction = 4;
            }
            else if (num6 == num7) {
                direction = 5;
            }
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(Index, 0, SetDirection(0, direction));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public static int GetDirection(int data) => data & 7;

        public static int SetDirection(int data, int direction) => (data & -8) | (direction & 7);

        public static Mode GetMode(int data) {
            if ((data & 8) != 0) {
                return Mode.Shoot;
            }
            return Mode.Dispense;
        }

        public static int SetMode(int data, Mode mode) => (data & -9) | (mode != 0 ? 8 : 0);

        public static bool GetAcceptsDrops(int data) => (data & 0x10) != 0;

        public static int SetAcceptsDrops(int data, bool acceptsDrops) => (data & -17) | (acceptsDrops ? 16 : 0);

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemElectricity, int value, int x, int y, int z) => new DispenserGVCElectricElement(subsystemElectricity, new Point3(x, y, z));

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) => GVElectricConnectorType.Input;

        public int GetConnectionMask(int value) => int.MaxValue;
    }
}