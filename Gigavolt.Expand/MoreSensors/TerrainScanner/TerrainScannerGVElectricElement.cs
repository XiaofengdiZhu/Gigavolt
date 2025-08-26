using System;
using Engine;

namespace Game {
    public class TerrainScannerGVElectricElement : RotateableGVElectricElement {
        public readonly Terrain m_terrain;
        public uint m_rightInput;
        public uint m_leftInput;
        public uint m_topInput;
        public uint m_bottomInput;
        public uint m_inInput;

        public TerrainScannerGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) => m_terrain = SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(subterrainId);

        public override bool Simulate() {
            m_rightInput = 0u;
            m_leftInput = 0u;
            m_topInput = 0u;
            uint bottomInput = m_bottomInput;
            m_bottomInput = 0u;
            m_inInput = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection =
                        SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Top:
                                m_topInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                            case GVElectricConnectorDirection.Right:
                                m_rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                            case GVElectricConnectorDirection.Bottom:
                                m_bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                            case GVElectricConnectorDirection.Left:
                                m_leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                            case GVElectricConnectorDirection.In:
                                m_inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                        }
                    }
                }
            }
            if (m_bottomInput == 0u
                || bottomInput > 0u
                || m_leftInput == 0u
                || m_inInput == 0u) {
                return false;
            }
            int width = (int)(m_leftInput >> 16);
            int height = (int)(m_leftInput & 0xffffu);
            if (width == 0u
                || height == 0u) {
                return false;
            }
            if (!GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_inInput, out GVArrayData arrayData)) {
                return false;
            }
            int distance = (int)(m_topInput & 0x7FFFu) * (((m_topInput >> 15) & 1u) == 1u ? -1 : 1);
            bool scanNearest = distance == -32767;
            if (scanNearest) {
                distance = 0;
            }
            int offsetX = (int)((m_rightInput >> 16) & 0x7FFFu) * (((m_rightInput >> 31) & 1u) == 1u ? -1 : 1);
            int offsetY = (int)(m_rightInput & 0x7FFFu) * (((m_rightInput >> 15) & 1u) == 1u ? -1 : 1);
            Point3 direction = CellFace.FaceToPoint3(CellFaces[0].Face);
            Point3 startPosition = CellFaces[0].Point + direction * distance;
            Point3 directionX;
            Point3 directionY;
            if (Math.Abs(direction.Y) == 1) {
                directionX = Point3.UnitX;
                directionY = Point3.UnitZ;
                startPosition.X += offsetX;
                startPosition.Z += offsetY;
            }
            else if (Math.Abs(direction.X) == 1) {
                directionX = Point3.UnitZ;
                directionY = -Point3.UnitY;
                startPosition.Z += offsetX;
                startPosition.Y += offsetY;
            }
            else {
                directionX = Point3.UnitX;
                directionY = -Point3.UnitY;
                startPosition.X += offsetX;
                startPosition.Y += offsetY;
            }
            Point3 endPosition = startPosition + directionX * (width - 1) + directionY * (height - 1);
            if (startPosition.Y > 255) {
                startPosition.Y = 255;
            }
            else if (startPosition.Y < 0) {
                startPosition.Y = 0;
            }
            if (endPosition.Y > 255) {
                endPosition.Y = 255;
            }
            else if (endPosition.Y < 0) {
                endPosition.Y = 0;
            }
            if (startPosition.Y - endPosition.Y > 0) {
                height = startPosition.Y - endPosition.Y + 1;
            }
            uint[] image = new uint[width * height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Point3 position = startPosition + directionX * x + directionY * y;
                    int value = scanNearest
                        ? GetNearestCellValue(position, direction, 0)
                        : m_terrain.GetCellValue(position.X, position.Y, position.Z);
                    image[width * y + x] = (uint)(((m_topInput >> 16) & 1u) == 1u ? value : Terrain.ExtractContents(value));
                }
            }
            arrayData.UintArray2Data(image, width, height);
            arrayData.SaveString();
            return false;
        }

        public int GetNearestCellValue(Point3 start, Point3 direction, int distance) {
            if (distance >= 256) {
                return 0;
            }
            Point3 position = start + direction * distance;
            if (position.Y is < 0 or > 255) {
                return 0;
            }
            int value = m_terrain.GetCellValue(position.X, position.Y, position.Z);
            if (Terrain.ReplaceLight(value, 0) == 0) {
                return GetNearestCellValue(start, direction, distance + 1);
            }
            return value;
        }
    }
}