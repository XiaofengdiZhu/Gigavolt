using Engine;

namespace Game {
    public abstract class GenerateGVWireVerticesBlock : Block {
        public readonly DynamicArray<GVElectricConnectionPath> m_GVtmpConnectionPaths = [];
        public readonly Random m_GVRandom = new();

        public virtual void GenerateGVWireVertices(BlockGeometryGenerator generator, int value, int x, int y, int z, int mountingFace, float centerBoxSize, Vector2 centerOffset, TerrainGeometrySubset subset) {
            SubsystemGVElectricity SubsystemGVElectricity = generator.SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
            if (SubsystemGVElectricity == null) {
                return;
            }
            Color innerTopColor = GVWireBlock.WireColor;
            Color innerBottomColor = innerTopColor;
            int num = Terrain.ExtractContents(value);
            switch (num) {
                case GVWireHarnessBlock.Index:
                    innerTopColor = GVWireHarnessBlock.WireColor1;
                    innerBottomColor = GVWireHarnessBlock.WireColor2;
                    break;
                case GVWireBlock.Index: {
                    int? color2 = GVWireBlock.GetColor(Terrain.ExtractData(value));
                    if (color2.HasValue) {
                        innerTopColor = SubsystemPalette.GetColor(generator, color2);
                        innerBottomColor = innerTopColor;
                    }
                    break;
                }
            }
            int num2 = Terrain.ExtractLight(value);
            float num3 = LightingManager.LightIntensityByLightValue[num2];
            Vector3 v = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) - 0.5f * CellFace.FaceToVector3(mountingFace);
            Vector3 vector = CellFace.FaceToVector3(mountingFace);
            Vector2 v2 = new(0.9376f, 0.0001f);
            Vector2 v3 = new(0.03125f, 0.00550781237f);
            Point3 point = CellFace.FaceToPoint3(mountingFace);
            int cellContents = generator.Terrain.GetCellContents(x - point.X, y - point.Y, z - point.Z);
            bool flag = cellContents is 2 or 7 or 8 or 6 or 62 or 72;
            Vector3 v4 = CellFace.FaceToVector3(SubsystemGVElectricity.GetConnectorFace(mountingFace, GVElectricConnectorDirection.Top));
            Vector3 vector2 = CellFace.FaceToVector3(SubsystemGVElectricity.GetConnectorFace(mountingFace, GVElectricConnectorDirection.Left)) * centerOffset.X + v4 * centerOffset.Y;
            int num4 = 0;
            m_GVtmpConnectionPaths.Clear();
            SubsystemGVElectricity.GetAllConnectedNeighbors(
                x,
                y,
                z,
                mountingFace,
                m_GVtmpConnectionPaths
            );
            foreach (GVElectricConnectionPath tmpConnectionPath in m_GVtmpConnectionPaths) {
                if ((num4 & (1 << tmpConnectionPath.ConnectorFace)) == 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(mountingFace, 0, tmpConnectionPath.ConnectorFace);
                    if (!(centerOffset == Vector2.Zero)
                        || connectorDirection != GVElectricConnectorDirection.In) {
                        num4 |= 1 << tmpConnectionPath.ConnectorFace;
                        Color innerTopColor2 = innerTopColor;
                        Color innerBottomColor2 = innerBottomColor;
                        Color outerTopColor = innerTopColor2;
                        Color outerBottomColor = innerBottomColor2;
                        if (num != GVWireBlock.Index) {
                            int cellValue = generator.Terrain.GetCellValue(x + tmpConnectionPath.NeighborOffsetX, y + tmpConnectionPath.NeighborOffsetY, z + tmpConnectionPath.NeighborOffsetZ);
                            Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
                            if (block is GVWireBlock) {
                                int? color4 = WireBlock.GetColor(Terrain.ExtractData(cellValue));
                                outerTopColor = color4.HasValue ? SubsystemPalette.GetColor(generator, color4) : GVWireBlock.WireColor;
                                outerBottomColor = outerTopColor;
                                innerTopColor2 = num == GVWireHarnessBlock.Index ? GVWireHarnessBlock.WireColor1 : outerTopColor;
                                innerBottomColor2 = num == GVWireHarnessBlock.Index ? GVWireHarnessBlock.WireColor2 : outerBottomColor;
                            }
                            else if (block is IGVElectricElementBlock and not GVWireHarnessBlock
                                && num == GVWireHarnessBlock.Index) {
                                outerTopColor = GVWireBlock.WireColor;
                                outerBottomColor = outerTopColor;
                            }
                        }
                        Vector3 vector3 = connectorDirection != GVElectricConnectorDirection.In ? CellFace.FaceToVector3(tmpConnectionPath.ConnectorFace) : -Vector3.Normalize(vector2);
                        Vector3 vector4 = Vector3.Cross(vector, vector3);
                        float s = centerBoxSize >= 0f ? MathUtils.Max(0.03125f, centerBoxSize / 2f) : centerBoxSize / 2f;
                        float num5 = connectorDirection == GVElectricConnectorDirection.In ? 0.03125f : 0.5f;
                        float num6 = connectorDirection == GVElectricConnectorDirection.In ? 0f :
                            tmpConnectionPath.ConnectorFace == tmpConnectionPath.NeighborFace ? num5 + 0.03125f :
                            tmpConnectionPath.ConnectorFace != CellFace.OppositeFace(tmpConnectionPath.NeighborFace) ? num5 : num5 - 0.03125f;
                        Vector3 v5 = v - vector4 * 0.03125f + vector3 * s + vector2;
                        Vector3 vector5 = v - vector4 * 0.03125f + vector3 * num5;
                        Vector3 vector6 = v + vector4 * 0.03125f + vector3 * num5;
                        Vector3 v6 = v + vector4 * 0.03125f + vector3 * s + vector2;
                        Vector3 vector7 = v + vector * 0.03125f + vector3 * (centerBoxSize / 2f) + vector2;
                        Vector3 vector8 = v + vector * 0.03125f + vector3 * num6;
                        if (flag && centerBoxSize == 0f) {
                            Vector3 vector9 = 0.25f * BlockGeometryGenerator.GetRandomWireOffset(0.5f * (v5 + v6), vector);
                            v5 += vector9;
                            v6 += vector9;
                            vector7 += vector9;
                        }
                        Vector2 vector10 = v2 + v3 * new Vector2(MathUtils.Max(0.0625f, centerBoxSize), 0f);
                        Vector2 vector11 = v2 + v3 * new Vector2(num5 * 2f, 0f);
                        Vector2 vector12 = v2 + v3 * new Vector2(num5 * 2f, 1f);
                        Vector2 vector13 = v2 + v3 * new Vector2(MathUtils.Max(0.0625f, centerBoxSize), 1f);
                        Vector2 vector14 = v2 + v3 * new Vector2(centerBoxSize, 0.5f);
                        Vector2 vector15 = v2 + v3 * new Vector2(num6 * 2f, 0.5f);
                        int num7 = Terrain.ExtractLight(generator.Terrain.GetCellValue(x + tmpConnectionPath.NeighborOffsetX, y + tmpConnectionPath.NeighborOffsetY, z + tmpConnectionPath.NeighborOffsetZ));
                        float num8 = LightingManager.LightIntensityByLightValue[num7];
                        float num9 = 0.5f * (num3 + num8);
                        float num10 = LightingManager.CalculateLighting(-vector4);
                        float num11 = LightingManager.CalculateLighting(vector4);
                        float num12 = LightingManager.CalculateLighting(vector);
                        float num13 = num10 * num3;
                        float num14 = num10 * num9;
                        float num15 = num11 * num9;
                        float num16 = num11 * num3;
                        float num17 = num12 * num3;
                        float num18 = num12 * num9;
                        Color color5 = new((byte)(innerBottomColor2.R * num13), (byte)(innerBottomColor2.G * num13), (byte)(innerBottomColor2.B * num13)); //内部底部
                        Color color6 = new((byte)(outerBottomColor.R * num14), (byte)(outerBottomColor.G * num14), (byte)(outerBottomColor.B * num14)); //外面底部
                        Color color7 = new((byte)(outerBottomColor.R * num15), (byte)(outerBottomColor.G * num15), (byte)(outerBottomColor.B * num15)); //外面底部
                        Color color8 = new((byte)(innerBottomColor2.R * num16), (byte)(innerBottomColor2.G * num16), (byte)(innerBottomColor2.B * num16)); //内部底部
                        Color color9 = new((byte)(innerTopColor2.R * num17), (byte)(innerTopColor2.G * num17), (byte)(innerTopColor2.B * num17)); //内部顶部
                        Color color10 = new((byte)(outerTopColor.R * num18), (byte)(outerTopColor.G * num18), (byte)(outerTopColor.B * num18)); //外面顶部
                        int count = subset.Vertices.Count;
                        subset.Vertices.Count += 6;
                        TerrainVertex[] array = subset.Vertices.Array;
                        BlockGeometryGenerator.SetupVertex(
                            v5.X,
                            v5.Y,
                            v5.Z,
                            color5,
                            vector10.X,
                            vector10.Y,
                            ref array[count]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            vector5.X,
                            vector5.Y,
                            vector5.Z,
                            color6,
                            vector11.X,
                            vector11.Y,
                            ref array[count + 1]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            vector6.X,
                            vector6.Y,
                            vector6.Z,
                            color7,
                            vector12.X,
                            vector12.Y,
                            ref array[count + 2]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            v6.X,
                            v6.Y,
                            v6.Z,
                            color8,
                            vector13.X,
                            vector13.Y,
                            ref array[count + 3]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            vector7.X,
                            vector7.Y,
                            vector7.Z,
                            color9,
                            vector14.X,
                            vector14.Y,
                            ref array[count + 4]
                        );
                        BlockGeometryGenerator.SetupVertex(
                            vector8.X,
                            vector8.Y,
                            vector8.Z,
                            color10,
                            vector15.X,
                            vector15.Y,
                            ref array[count + 5]
                        );
                        int count2 = subset.Indices.Count;
                        subset.Indices.Count += connectorDirection == GVElectricConnectorDirection.In ? 15 : 12;
                        int[] array2 = subset.Indices.Array;
                        array2[count2] = count;
                        array2[count2 + 1] = count + 5;
                        array2[count2 + 2] = count + 1;
                        array2[count2 + 3] = count + 5;
                        array2[count2 + 4] = count;
                        array2[count2 + 5] = count + 4;
                        array2[count2 + 6] = count + 4;
                        array2[count2 + 7] = count + 2;
                        array2[count2 + 8] = count + 5;
                        array2[count2 + 9] = count + 2;
                        array2[count2 + 10] = count + 4;
                        array2[count2 + 11] = count + 3;
                        if (connectorDirection == GVElectricConnectorDirection.In) {
                            array2[count2 + 12] = count + 2;
                            array2[count2 + 13] = count + 1;
                            array2[count2 + 14] = count + 5;
                        }
                    }
                }
            }
            if (centerBoxSize != 0f
                || (num4 == 0 && num != GVWireBlock.Index && num != GVWireHarnessBlock.Index)) {
                return;
            }
            for (int i = 0; i < 6; i++) {
                if (i != mountingFace
                    && i != CellFace.OppositeFace(mountingFace)
                    && (num4 & (1 << i)) == 0) {
                    Vector3 vector16 = CellFace.FaceToVector3(i);
                    Vector3 v7 = Vector3.Cross(vector, vector16);
                    Vector3 v8 = v - v7 * 0.03125f + vector16 * 0.03125f;
                    Vector3 v9 = v + v7 * 0.03125f + vector16 * 0.03125f;
                    Vector3 vector17 = v + vector * 0.03125f;
                    if (flag) {
                        Vector3 vector18 = 0.25f * BlockGeometryGenerator.GetRandomWireOffset(0.5f * (v8 + v9), vector);
                        v8 += vector18;
                        v9 += vector18;
                        vector17 += vector18;
                    }
                    Vector2 vector19 = v2 + v3 * new Vector2(0.0625f, 0f);
                    Vector2 vector20 = v2 + v3 * new Vector2(0.0625f, 1f);
                    Vector2 vector21 = v2 + v3 * new Vector2(0f, 0.5f);
                    float num19 = LightingManager.CalculateLighting(vector16) * num3;
                    float num20 = LightingManager.CalculateLighting(vector) * num3;
                    Color color11 = new((byte)(innerBottomColor.R * num19), (byte)(innerBottomColor.G * num19), (byte)(innerBottomColor.B * num19));
                    Color color12 = new((byte)(innerTopColor.R * num20), (byte)(innerTopColor.G * num20), (byte)(innerTopColor.B * num20));
                    int count3 = subset.Vertices.Count;
                    subset.Vertices.Count += 3;
                    TerrainVertex[] array3 = subset.Vertices.Array;
                    BlockGeometryGenerator.SetupVertex(
                        v8.X,
                        v8.Y,
                        v8.Z,
                        color11,
                        vector19.X,
                        vector19.Y,
                        ref array3[count3]
                    );
                    BlockGeometryGenerator.SetupVertex(
                        v9.X,
                        v9.Y,
                        v9.Z,
                        color11,
                        vector20.X,
                        vector20.Y,
                        ref array3[count3 + 1]
                    );
                    BlockGeometryGenerator.SetupVertex(
                        vector17.X,
                        vector17.Y,
                        vector17.Z,
                        color12,
                        vector21.X,
                        vector21.Y,
                        ref array3[count3 + 2]
                    );
                    int count4 = subset.Indices.Count;
                    subset.Indices.Count += 3;
                    int[] array4 = subset.Indices.Array;
                    array4[count4] = count3;
                    array4[count4 + 1] = count3 + 2;
                    array4[count4 + 2] = count3 + 1;
                }
            }
        }
    }
}