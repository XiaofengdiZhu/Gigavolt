using System;
using Engine;

namespace Game {
    public class GVBlockGeometryGenerator : BlockGeometryGenerator {
        public readonly SubsystemGVElectricity SubsystemGVElectricity;
        public readonly GVSubterrainSystem m_subterrainSystem;

        public GVBlockGeometryGenerator(Terrain terrain, SubsystemTerrain subsystemTerrain, SubsystemElectricity subsystemElectricity, SubsystemFurnitureBlockBehavior subsystemFurnitureBlockBehavior, SubsystemMetersBlockBehavior subsystemMetersBlockBehavior, SubsystemPalette subsystemPalette, SubsystemGVElectricity subsystemGVElectricity, GVSubterrainSystem subterrainSystem) : base(
            terrain,
            subsystemTerrain,
            subsystemElectricity,
            subsystemFurnitureBlockBehavior,
            subsystemMetersBlockBehavior,
            subsystemPalette
        ) {
            SubsystemGVElectricity = subsystemGVElectricity;
            m_subterrainSystem = subterrainSystem;
        }

        public override void GenerateCubeVertices(Block block, int value, int x, int y, int z, Color color, TerrainGeometrySubset[] subsetsByFace) {
            int blockIndex = block.BlockIndex;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCell(x, z);
            TerrainChunk chunkAtCell2 = Terrain.GetChunkAtCell(x, z + 1);
            TerrainChunk chunkAtCell3 = Terrain.GetChunkAtCell(x + 1, z);
            TerrainChunk chunkAtCell4 = Terrain.GetChunkAtCell(x, z - 1);
            TerrainChunk chunkAtCell5 = Terrain.GetChunkAtCell(x - 1, z);
            int cellValueFast = chunkAtCell2.GetCellValueFast(x & 0xF, y, (z + 1) & 0xF);
            int textureSlotCount = block.GetTextureSlotCount(value);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    0,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices = subsetsByFace[0].Vertices;
                DynamicArray<int> indices = subsetsByFace[0].Indices;
                int faceTextureSlot = block.GetFaceTextureSlot(0, value);
                int count = vertices.Count;
                vertices.Count += 4;
                SetupCubeVertexFace0(
                    x,
                    y,
                    z + 1,
                    1f,
                    0,
                    faceTextureSlot,
                    textureSlotCount,
                    color,
                    ref vertices.Array[count]
                ); //A
                SetupCubeVertexFace0(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    1,
                    faceTextureSlot,
                    textureSlotCount,
                    color,
                    ref vertices.Array[count + 1]
                ); //B
                SetupCubeVertexFace0(
                    x + 1,
                    y + 1,
                    z + 1,
                    1f,
                    2,
                    faceTextureSlot,
                    textureSlotCount,
                    color,
                    ref vertices.Array[count + 2]
                ); //C
                SetupCubeVertexFace0(
                    x,
                    y + 1,
                    z + 1,
                    1f,
                    3,
                    faceTextureSlot,
                    textureSlotCount,
                    color,
                    ref vertices.Array[count + 3]
                ); //D
                int count2 = indices.Count;
                indices.Count += 6;
                indices.Array[count2] = count; //A
                indices.Array[count2 + 1] = count + 2; //C
                indices.Array[count2 + 2] = count + 1; //B
                indices.Array[count2 + 3] = count + 2; //C
                indices.Array[count2 + 4] = count; //A
                indices.Array[count2 + 5] = count + 3; //D
            }
            cellValueFast = chunkAtCell3.GetCellValueFast((x + 1) & 0xF, y, z & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    1,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices2 = subsetsByFace[1].Vertices;
                DynamicArray<int> indices2 = subsetsByFace[1].Indices;
                int faceTextureSlot2 = block.GetFaceTextureSlot(1, value);
                int count3 = vertices2.Count;
                vertices2.Count += 4;
                SetupCubeVertexFace1(
                    x + 1,
                    y,
                    z,
                    1f,
                    1,
                    faceTextureSlot2,
                    textureSlotCount,
                    color,
                    ref vertices2.Array[count3]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y + 1,
                    z,
                    1f,
                    2,
                    faceTextureSlot2,
                    textureSlotCount,
                    color,
                    ref vertices2.Array[count3 + 1]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y + 1,
                    z + 1,
                    1f,
                    3,
                    faceTextureSlot2,
                    textureSlotCount,
                    color,
                    ref vertices2.Array[count3 + 2]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    0,
                    faceTextureSlot2,
                    textureSlotCount,
                    color,
                    ref vertices2.Array[count3 + 3]
                );
                int count4 = indices2.Count;
                indices2.Count += 6;
                indices2.Array[count4] = count3;
                indices2.Array[count4 + 1] = count3 + 2;
                indices2.Array[count4 + 2] = count3 + 1;
                indices2.Array[count4 + 3] = count3 + 2;
                indices2.Array[count4 + 4] = count3;
                indices2.Array[count4 + 5] = count3 + 3;
            }
            cellValueFast = chunkAtCell4.GetCellValueFast(x & 0xF, y, (z - 1) & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    2,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices3 = subsetsByFace[2].Vertices;
                DynamicArray<int> indices3 = subsetsByFace[2].Indices;
                int faceTextureSlot3 = block.GetFaceTextureSlot(2, value);
                int count5 = vertices3.Count;
                vertices3.Count += 4;
                SetupCubeVertexFace2(
                    x,
                    y,
                    z,
                    1f,
                    1,
                    faceTextureSlot3,
                    textureSlotCount,
                    color,
                    ref vertices3.Array[count5]
                );
                SetupCubeVertexFace2(
                    x + 1,
                    y,
                    z,
                    1f,
                    0,
                    faceTextureSlot3,
                    textureSlotCount,
                    color,
                    ref vertices3.Array[count5 + 1]
                );
                SetupCubeVertexFace2(
                    x + 1,
                    y + 1,
                    z,
                    1f,
                    3,
                    faceTextureSlot3,
                    textureSlotCount,
                    color,
                    ref vertices3.Array[count5 + 2]
                );
                SetupCubeVertexFace2(
                    x,
                    y + 1,
                    z,
                    1f,
                    2,
                    faceTextureSlot3,
                    textureSlotCount,
                    color,
                    ref vertices3.Array[count5 + 3]
                );
                int count6 = indices3.Count;
                indices3.Count += 6;
                indices3.Array[count6] = count5;
                indices3.Array[count6 + 1] = count5 + 1;
                indices3.Array[count6 + 2] = count5 + 2;
                indices3.Array[count6 + 3] = count5 + 2;
                indices3.Array[count6 + 4] = count5 + 3;
                indices3.Array[count6 + 5] = count5;
            }
            cellValueFast = chunkAtCell5.GetCellValueFast((x - 1) & 0xF, y, z & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    3,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices4 = subsetsByFace[3].Vertices;
                DynamicArray<int> indices4 = subsetsByFace[3].Indices;
                int faceTextureSlot4 = block.GetFaceTextureSlot(3, value);
                int count7 = vertices4.Count;
                vertices4.Count += 4;
                SetupCubeVertexFace3(
                    x,
                    y,
                    z,
                    1f,
                    0,
                    faceTextureSlot4,
                    textureSlotCount,
                    color,
                    ref vertices4.Array[count7]
                );
                SetupCubeVertexFace3(
                    x,
                    y + 1,
                    z,
                    1f,
                    3,
                    faceTextureSlot4,
                    textureSlotCount,
                    color,
                    ref vertices4.Array[count7 + 1]
                );
                SetupCubeVertexFace3(
                    x,
                    y + 1,
                    z + 1,
                    1f,
                    2,
                    faceTextureSlot4,
                    textureSlotCount,
                    color,
                    ref vertices4.Array[count7 + 2]
                );
                SetupCubeVertexFace3(
                    x,
                    y,
                    z + 1,
                    1f,
                    1,
                    faceTextureSlot4,
                    textureSlotCount,
                    color,
                    ref vertices4.Array[count7 + 3]
                );
                int count8 = indices4.Count;
                indices4.Count += 6;
                indices4.Array[count8] = count7;
                indices4.Array[count8 + 1] = count7 + 1;
                indices4.Array[count8 + 2] = count7 + 2;
                indices4.Array[count8 + 3] = count7 + 2;
                indices4.Array[count8 + 4] = count7 + 3;
                indices4.Array[count8 + 5] = count7;
            }
            cellValueFast = y < 255 ? chunkAtCell.GetCellValueFast(x & 0xF, y + 1, z & 0xF) : m_subterrainSystem.Light << 10;
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    4,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices5 = subsetsByFace[4].Vertices;
                DynamicArray<int> indices5 = subsetsByFace[4].Indices;
                int faceTextureSlot5 = block.GetFaceTextureSlot(4, value);
                int count9 = vertices5.Count;
                vertices5.Count += 4;
                SetupCubeVertexFace4(
                    x,
                    y + 1,
                    z,
                    1f,
                    3,
                    faceTextureSlot5,
                    textureSlotCount,
                    color,
                    ref vertices5.Array[count9]
                );
                SetupCubeVertexFace4(
                    x + 1,
                    y + 1,
                    z,
                    1f,
                    2,
                    faceTextureSlot5,
                    textureSlotCount,
                    color,
                    ref vertices5.Array[count9 + 1]
                );
                SetupCubeVertexFace4(
                    x + 1,
                    y + 1,
                    z + 1,
                    1f,
                    1,
                    faceTextureSlot5,
                    textureSlotCount,
                    color,
                    ref vertices5.Array[count9 + 2]
                );
                SetupCubeVertexFace4(
                    x,
                    y + 1,
                    z + 1,
                    1f,
                    0,
                    faceTextureSlot5,
                    textureSlotCount,
                    color,
                    ref vertices5.Array[count9 + 3]
                );
                int count10 = indices5.Count;
                indices5.Count += 6;
                indices5.Array[count10] = count9;
                indices5.Array[count10 + 1] = count9 + 1;
                indices5.Array[count10 + 2] = count9 + 2;
                indices5.Array[count10 + 3] = count9 + 2;
                indices5.Array[count10 + 4] = count9 + 3;
                indices5.Array[count10 + 5] = count9;
            }
            cellValueFast = y > 0 ? chunkAtCell.GetCellValueFast(x & 0xF, y - 1, z & 0xF) : m_subterrainSystem.Light << 10;
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    5,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices6 = subsetsByFace[5].Vertices;
                DynamicArray<int> indices6 = subsetsByFace[5].Indices;
                int faceTextureSlot6 = block.GetFaceTextureSlot(5, value);
                int count11 = vertices6.Count;
                vertices6.Count += 4;
                SetupCubeVertexFace5(
                    x,
                    y,
                    z,
                    1f,
                    0,
                    faceTextureSlot6,
                    textureSlotCount,
                    color,
                    ref vertices6.Array[count11]
                );
                SetupCubeVertexFace5(
                    x + 1,
                    y,
                    z,
                    1f,
                    1,
                    faceTextureSlot6,
                    textureSlotCount,
                    color,
                    ref vertices6.Array[count11 + 1]
                );
                SetupCubeVertexFace5(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    2,
                    faceTextureSlot6,
                    textureSlotCount,
                    color,
                    ref vertices6.Array[count11 + 2]
                );
                SetupCubeVertexFace5(
                    x,
                    y,
                    z + 1,
                    1f,
                    3,
                    faceTextureSlot6,
                    textureSlotCount,
                    color,
                    ref vertices6.Array[count11 + 3]
                );
                int count12 = indices6.Count;
                indices6.Count += 6;
                indices6.Array[count12] = count11;
                indices6.Array[count12 + 1] = count11 + 2;
                indices6.Array[count12 + 2] = count11 + 1;
                indices6.Array[count12 + 3] = count11 + 2;
                indices6.Array[count12 + 4] = count11;
                indices6.Array[count12 + 5] = count11 + 3;
            }
        }

        public override void GenerateCubeVertices(Block block, int value, int x, int y, int z, float height11, float height21, float height22, float height12, Color sideColor, Color topColor11, Color topColor21, Color topColor22, Color topColor12, int overrideTopTextureSlot, TerrainGeometrySubset[] subsetsByFace) {
            int blockIndex = block.BlockIndex;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCell(x, z);
            TerrainChunk chunkAtCell2 = Terrain.GetChunkAtCell(x, z + 1);
            TerrainChunk chunkAtCell3 = Terrain.GetChunkAtCell(x + 1, z);
            TerrainChunk chunkAtCell4 = Terrain.GetChunkAtCell(x, z - 1);
            TerrainChunk chunkAtCell5 = Terrain.GetChunkAtCell(x - 1, z);
            int cellValueFast = chunkAtCell2.GetCellValueFast(x & 0xF, y, (z + 1) & 0xF);
            int textureSlotCount = block.GetTextureSlotCount(value);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    0,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices = subsetsByFace[0].Vertices;
                DynamicArray<int> indices = subsetsByFace[0].Indices;
                int faceTextureSlot = block.GetFaceTextureSlot(0, value);
                int count = vertices.Count;
                vertices.Count += 4;
                SetupCubeVertexFace0(
                    x,
                    y,
                    z + 1,
                    1f,
                    0,
                    faceTextureSlot,
                    textureSlotCount,
                    sideColor,
                    ref vertices.Array[count]
                );
                SetupCubeVertexFace0(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    1,
                    faceTextureSlot,
                    textureSlotCount,
                    sideColor,
                    ref vertices.Array[count + 1]
                );
                SetupCubeVertexFace0(
                    x + 1,
                    y + 1,
                    z + 1,
                    height22,
                    2,
                    faceTextureSlot,
                    textureSlotCount,
                    sideColor,
                    ref vertices.Array[count + 2]
                );
                SetupCubeVertexFace0(
                    x,
                    y + 1,
                    z + 1,
                    height12,
                    3,
                    faceTextureSlot,
                    textureSlotCount,
                    sideColor,
                    ref vertices.Array[count + 3]
                );
                int count2 = indices.Count;
                indices.Count += 6;
                indices.Array[count2] = count;
                indices.Array[count2 + 1] = count + 2;
                indices.Array[count2 + 2] = count + 1;
                indices.Array[count2 + 3] = count + 2;
                indices.Array[count2 + 4] = count;
                indices.Array[count2 + 5] = count + 3;
            }
            cellValueFast = chunkAtCell3.GetCellValueFast((x + 1) & 0xF, y, z & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    1,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices2 = subsetsByFace[1].Vertices;
                DynamicArray<int> indices2 = subsetsByFace[1].Indices;
                int faceTextureSlot2 = block.GetFaceTextureSlot(1, value);
                int count3 = vertices2.Count;
                vertices2.Count += 4;
                SetupCubeVertexFace1(
                    x + 1,
                    y,
                    z,
                    1f,
                    1,
                    faceTextureSlot2,
                    textureSlotCount,
                    sideColor,
                    ref vertices2.Array[count3]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y + 1,
                    z,
                    height21,
                    2,
                    faceTextureSlot2,
                    textureSlotCount,
                    sideColor,
                    ref vertices2.Array[count3 + 1]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y + 1,
                    z + 1,
                    height22,
                    3,
                    faceTextureSlot2,
                    textureSlotCount,
                    sideColor,
                    ref vertices2.Array[count3 + 2]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    0,
                    faceTextureSlot2,
                    textureSlotCount,
                    sideColor,
                    ref vertices2.Array[count3 + 3]
                );
                int count4 = indices2.Count;
                indices2.Count += 6;
                indices2.Array[count4] = count3;
                indices2.Array[count4 + 1] = count3 + 2;
                indices2.Array[count4 + 2] = count3 + 1;
                indices2.Array[count4 + 3] = count3 + 2;
                indices2.Array[count4 + 4] = count3;
                indices2.Array[count4 + 5] = count3 + 3;
            }
            cellValueFast = chunkAtCell4.GetCellValueFast(x & 0xF, y, (z - 1) & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    2,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices3 = subsetsByFace[2].Vertices;
                DynamicArray<int> indices3 = subsetsByFace[2].Indices;
                int faceTextureSlot3 = block.GetFaceTextureSlot(2, value);
                int count5 = vertices3.Count;
                vertices3.Count += 4;
                SetupCubeVertexFace2(
                    x,
                    y,
                    z,
                    1f,
                    1,
                    faceTextureSlot3,
                    textureSlotCount,
                    sideColor,
                    ref vertices3.Array[count5]
                );
                SetupCubeVertexFace2(
                    x + 1,
                    y,
                    z,
                    1f,
                    0,
                    faceTextureSlot3,
                    textureSlotCount,
                    sideColor,
                    ref vertices3.Array[count5 + 1]
                );
                SetupCubeVertexFace2(
                    x + 1,
                    y + 1,
                    z,
                    height21,
                    3,
                    faceTextureSlot3,
                    textureSlotCount,
                    sideColor,
                    ref vertices3.Array[count5 + 2]
                );
                SetupCubeVertexFace2(
                    x,
                    y + 1,
                    z,
                    height11,
                    2,
                    faceTextureSlot3,
                    textureSlotCount,
                    sideColor,
                    ref vertices3.Array[count5 + 3]
                );
                int count6 = indices3.Count;
                indices3.Count += 6;
                indices3.Array[count6] = count5;
                indices3.Array[count6 + 1] = count5 + 1;
                indices3.Array[count6 + 2] = count5 + 2;
                indices3.Array[count6 + 3] = count5 + 2;
                indices3.Array[count6 + 4] = count5 + 3;
                indices3.Array[count6 + 5] = count5;
            }
            cellValueFast = chunkAtCell5.GetCellValueFast((x - 1) & 0xF, y, z & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    3,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices4 = subsetsByFace[3].Vertices;
                DynamicArray<int> indices4 = subsetsByFace[3].Indices;
                int faceTextureSlot4 = block.GetFaceTextureSlot(3, value);
                int count7 = vertices4.Count;
                vertices4.Count += 4;
                SetupCubeVertexFace3(
                    x,
                    y,
                    z,
                    1f,
                    0,
                    faceTextureSlot4,
                    textureSlotCount,
                    sideColor,
                    ref vertices4.Array[count7]
                );
                SetupCubeVertexFace3(
                    x,
                    y + 1,
                    z,
                    height11,
                    3,
                    faceTextureSlot4,
                    textureSlotCount,
                    sideColor,
                    ref vertices4.Array[count7 + 1]
                );
                SetupCubeVertexFace3(
                    x,
                    y + 1,
                    z + 1,
                    height12,
                    2,
                    faceTextureSlot4,
                    textureSlotCount,
                    sideColor,
                    ref vertices4.Array[count7 + 2]
                );
                SetupCubeVertexFace3(
                    x,
                    y,
                    z + 1,
                    1f,
                    1,
                    faceTextureSlot4,
                    textureSlotCount,
                    sideColor,
                    ref vertices4.Array[count7 + 3]
                );
                int count8 = indices4.Count;
                indices4.Count += 6;
                indices4.Array[count8] = count7;
                indices4.Array[count8 + 1] = count7 + 1;
                indices4.Array[count8 + 2] = count7 + 2;
                indices4.Array[count8 + 3] = count7 + 2;
                indices4.Array[count8 + 4] = count7 + 3;
                indices4.Array[count8 + 5] = count7;
            }
            cellValueFast = y < 255 ? chunkAtCell.GetCellValueFast(x & 0xF, y + 1, z & 0xF) : m_subterrainSystem.Light << 10;
            if (((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    4,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                ))
                || height11 < 1f
                || height12 < 1f
                || height21 < 1f
                || height22 < 1f) {
                DynamicArray<TerrainVertex> vertices5 = subsetsByFace[4].Vertices;
                DynamicArray<int> indices5 = subsetsByFace[4].Indices;
                int textureSlot = overrideTopTextureSlot >= 0 ? overrideTopTextureSlot : block.GetFaceTextureSlot(4, value);
                int count9 = vertices5.Count;
                vertices5.Count += 4;
                SetupCubeVertexFace4(
                    x,
                    y + 1,
                    z,
                    height11,
                    3,
                    textureSlot,
                    textureSlotCount,
                    topColor11,
                    ref vertices5.Array[count9]
                );
                SetupCubeVertexFace4(
                    x + 1,
                    y + 1,
                    z,
                    height21,
                    2,
                    textureSlot,
                    textureSlotCount,
                    topColor21,
                    ref vertices5.Array[count9 + 1]
                );
                SetupCubeVertexFace4(
                    x + 1,
                    y + 1,
                    z + 1,
                    height22,
                    1,
                    textureSlot,
                    textureSlotCount,
                    topColor22,
                    ref vertices5.Array[count9 + 2]
                );
                SetupCubeVertexFace4(
                    x,
                    y + 1,
                    z + 1,
                    height12,
                    0,
                    textureSlot,
                    textureSlotCount,
                    topColor12,
                    ref vertices5.Array[count9 + 3]
                );
                int count10 = indices5.Count;
                indices5.Count += 6;
                indices5.Array[count10] = count9;
                indices5.Array[count10 + 1] = count9 + 1;
                indices5.Array[count10 + 2] = count9 + 2;
                indices5.Array[count10 + 3] = count9 + 2;
                indices5.Array[count10 + 4] = count9 + 3;
                indices5.Array[count10 + 5] = count9;
            }
            cellValueFast = y > 0 ? chunkAtCell.GetCellValueFast(x & 0xF, y - 1, z & 0xF) : m_subterrainSystem.Light << 10;
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    4,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices6 = subsetsByFace[5].Vertices;
                DynamicArray<int> indices6 = subsetsByFace[5].Indices;
                int faceTextureSlot5 = block.GetFaceTextureSlot(5, value);
                int count11 = vertices6.Count;
                vertices6.Count += 4;
                SetupCubeVertexFace5(
                    x,
                    y,
                    z,
                    1f,
                    0,
                    faceTextureSlot5,
                    textureSlotCount,
                    sideColor,
                    ref vertices6.Array[count11]
                );
                SetupCubeVertexFace5(
                    x + 1,
                    y,
                    z,
                    1f,
                    1,
                    faceTextureSlot5,
                    textureSlotCount,
                    sideColor,
                    ref vertices6.Array[count11 + 1]
                );
                SetupCubeVertexFace5(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    2,
                    faceTextureSlot5,
                    textureSlotCount,
                    sideColor,
                    ref vertices6.Array[count11 + 2]
                );
                SetupCubeVertexFace5(
                    x,
                    y,
                    z + 1,
                    1f,
                    3,
                    faceTextureSlot5,
                    textureSlotCount,
                    sideColor,
                    ref vertices6.Array[count11 + 3]
                );
                int count12 = indices6.Count;
                indices6.Count += 6;
                indices6.Array[count12] = count11;
                indices6.Array[count12 + 1] = count11 + 2;
                indices6.Array[count12 + 2] = count11 + 1;
                indices6.Array[count12 + 3] = count11 + 2;
                indices6.Array[count12 + 4] = count11;
                indices6.Array[count12 + 5] = count11 + 3;
            }
        }

        public override void GenerateCubeVertices(Block block, int value, int x, int y, int z, int rotationX, int rotationY, int rotationZ, Color color, TerrainGeometrySubset[] subsetsByFace) {
            int blockIndex = block.BlockIndex;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCell(x, z);
            TerrainChunk chunkAtCell2 = Terrain.GetChunkAtCell(x, z + 1);
            TerrainChunk chunkAtCell3 = Terrain.GetChunkAtCell(x + 1, z);
            TerrainChunk chunkAtCell4 = Terrain.GetChunkAtCell(x, z - 1);
            TerrainChunk chunkAtCell5 = Terrain.GetChunkAtCell(x - 1, z);
            int cellValueFast = chunkAtCell2.GetCellValueFast(x & 0xF, y, (z + 1) & 0xF);
            int textureSlotCount = block.GetTextureSlotCount(value);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    0,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices = subsetsByFace[0].Vertices;
                DynamicArray<int> indices = subsetsByFace[0].Indices;
                int faceTextureSlot = block.GetFaceTextureSlot(0, value);
                int count = vertices.Count;
                vertices.Count += 4;
                SetupCubeVertexFace0(
                    x,
                    y,
                    z + 1,
                    1f,
                    rotationZ,
                    faceTextureSlot,
                    textureSlotCount,
                    color,
                    ref vertices.Array[count]
                );
                SetupCubeVertexFace0(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    1 + rotationZ,
                    faceTextureSlot,
                    textureSlotCount,
                    color,
                    ref vertices.Array[count + 1]
                );
                SetupCubeVertexFace0(
                    x + 1,
                    y + 1,
                    z + 1,
                    1f,
                    2 + rotationZ,
                    faceTextureSlot,
                    textureSlotCount,
                    color,
                    ref vertices.Array[count + 2]
                );
                SetupCubeVertexFace0(
                    x,
                    y + 1,
                    z + 1,
                    1f,
                    3 + rotationZ,
                    faceTextureSlot,
                    textureSlotCount,
                    color,
                    ref vertices.Array[count + 3]
                );
                int count2 = indices.Count;
                indices.Count += 6;
                indices.Array[count2] = count;
                indices.Array[count2 + 1] = count + 2;
                indices.Array[count2 + 2] = count + 1;
                indices.Array[count2 + 3] = count + 2;
                indices.Array[count2 + 4] = count;
                indices.Array[count2 + 5] = count + 3;
            }
            cellValueFast = chunkAtCell3.GetCellValueFast((x + 1) & 0xF, y, z & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    1,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices2 = subsetsByFace[1].Vertices;
                DynamicArray<int> indices2 = subsetsByFace[1].Indices;
                int faceTextureSlot2 = block.GetFaceTextureSlot(1, value);
                int count3 = vertices2.Count;
                vertices2.Count += 4;
                SetupCubeVertexFace1(
                    x + 1,
                    y,
                    z,
                    1f,
                    1 + rotationX,
                    faceTextureSlot2,
                    textureSlotCount,
                    color,
                    ref vertices2.Array[count3]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y + 1,
                    z,
                    1f,
                    2 + rotationX,
                    faceTextureSlot2,
                    textureSlotCount,
                    color,
                    ref vertices2.Array[count3 + 1]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y + 1,
                    z + 1,
                    1f,
                    3 + rotationX,
                    faceTextureSlot2,
                    textureSlotCount,
                    color,
                    ref vertices2.Array[count3 + 2]
                );
                SetupCubeVertexFace1(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    rotationX,
                    faceTextureSlot2,
                    textureSlotCount,
                    color,
                    ref vertices2.Array[count3 + 3]
                );
                int count4 = indices2.Count;
                indices2.Count += 6;
                indices2.Array[count4] = count3;
                indices2.Array[count4 + 1] = count3 + 2;
                indices2.Array[count4 + 2] = count3 + 1;
                indices2.Array[count4 + 3] = count3 + 2;
                indices2.Array[count4 + 4] = count3;
                indices2.Array[count4 + 5] = count3 + 3;
            }
            cellValueFast = chunkAtCell4.GetCellValueFast(x & 0xF, y, (z - 1) & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    2,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices3 = subsetsByFace[2].Vertices;
                DynamicArray<int> indices3 = subsetsByFace[2].Indices;
                int faceTextureSlot3 = block.GetFaceTextureSlot(2, value);
                int count5 = vertices3.Count;
                vertices3.Count += 4;
                SetupCubeVertexFace2(
                    x,
                    y,
                    z,
                    1f,
                    1 + rotationZ,
                    faceTextureSlot3,
                    textureSlotCount,
                    color,
                    ref vertices3.Array[count5]
                );
                SetupCubeVertexFace2(
                    x + 1,
                    y,
                    z,
                    1f,
                    rotationZ,
                    faceTextureSlot3,
                    textureSlotCount,
                    color,
                    ref vertices3.Array[count5 + 1]
                );
                SetupCubeVertexFace2(
                    x + 1,
                    y + 1,
                    z,
                    1f,
                    3 + rotationZ,
                    faceTextureSlot3,
                    textureSlotCount,
                    color,
                    ref vertices3.Array[count5 + 2]
                );
                SetupCubeVertexFace2(
                    x,
                    y + 1,
                    z,
                    1f,
                    2 + rotationZ,
                    faceTextureSlot3,
                    textureSlotCount,
                    color,
                    ref vertices3.Array[count5 + 3]
                );
                int count6 = indices3.Count;
                indices3.Count += 6;
                indices3.Array[count6] = count5;
                indices3.Array[count6 + 1] = count5 + 1;
                indices3.Array[count6 + 2] = count5 + 2;
                indices3.Array[count6 + 3] = count5 + 2;
                indices3.Array[count6 + 4] = count5 + 3;
                indices3.Array[count6 + 5] = count5;
            }
            cellValueFast = chunkAtCell5.GetCellValueFast((x - 1) & 0xF, y, z & 0xF);
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    3,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices4 = subsetsByFace[3].Vertices;
                DynamicArray<int> indices4 = subsetsByFace[3].Indices;
                int faceTextureSlot4 = block.GetFaceTextureSlot(3, value);
                int count7 = vertices4.Count;
                vertices4.Count += 4;
                SetupCubeVertexFace3(
                    x,
                    y,
                    z,
                    1f,
                    rotationX,
                    faceTextureSlot4,
                    textureSlotCount,
                    color,
                    ref vertices4.Array[count7]
                );
                SetupCubeVertexFace3(
                    x,
                    y + 1,
                    z,
                    1f,
                    3 + rotationX,
                    faceTextureSlot4,
                    textureSlotCount,
                    color,
                    ref vertices4.Array[count7 + 1]
                );
                SetupCubeVertexFace3(
                    x,
                    y + 1,
                    z + 1,
                    1f,
                    2 + rotationX,
                    faceTextureSlot4,
                    textureSlotCount,
                    color,
                    ref vertices4.Array[count7 + 2]
                );
                SetupCubeVertexFace3(
                    x,
                    y,
                    z + 1,
                    1f,
                    1 + rotationX,
                    faceTextureSlot4,
                    textureSlotCount,
                    color,
                    ref vertices4.Array[count7 + 3]
                );
                int count8 = indices4.Count;
                indices4.Count += 6;
                indices4.Array[count8] = count7;
                indices4.Array[count8 + 1] = count7 + 1;
                indices4.Array[count8 + 2] = count7 + 2;
                indices4.Array[count8 + 3] = count7 + 2;
                indices4.Array[count8 + 4] = count7 + 3;
                indices4.Array[count8 + 5] = count7;
            }
            cellValueFast = y < 255 ? chunkAtCell.GetCellValueFast(x & 0xF, y + 1, z & 0xF) : m_subterrainSystem.Light << 10;
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    4,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices5 = subsetsByFace[4].Vertices;
                DynamicArray<int> indices5 = subsetsByFace[4].Indices;
                int faceTextureSlot5 = block.GetFaceTextureSlot(4, value);
                int count9 = vertices5.Count;
                vertices5.Count += 4;
                SetupCubeVertexFace4(
                    x,
                    y + 1,
                    z,
                    1f,
                    3 + rotationY,
                    faceTextureSlot5,
                    textureSlotCount,
                    color,
                    ref vertices5.Array[count9]
                );
                SetupCubeVertexFace4(
                    x + 1,
                    y + 1,
                    z,
                    1f,
                    2 + rotationY,
                    faceTextureSlot5,
                    textureSlotCount,
                    color,
                    ref vertices5.Array[count9 + 1]
                );
                SetupCubeVertexFace4(
                    x + 1,
                    y + 1,
                    z + 1,
                    1f,
                    1 + rotationY,
                    faceTextureSlot5,
                    textureSlotCount,
                    color,
                    ref vertices5.Array[count9 + 2]
                );
                SetupCubeVertexFace4(
                    x,
                    y + 1,
                    z + 1,
                    1f,
                    rotationY,
                    faceTextureSlot5,
                    textureSlotCount,
                    color,
                    ref vertices5.Array[count9 + 3]
                );
                int count10 = indices5.Count;
                indices5.Count += 6;
                indices5.Array[count10] = count9;
                indices5.Array[count10 + 1] = count9 + 1;
                indices5.Array[count10 + 2] = count9 + 2;
                indices5.Array[count10 + 3] = count9 + 2;
                indices5.Array[count10 + 4] = count9 + 3;
                indices5.Array[count10 + 5] = count9;
            }
            cellValueFast = y > 0 ? chunkAtCell.GetCellValueFast(x & 0xF, y - 1, z & 0xF) : m_subterrainSystem.Light << 10;
            if ((block.GenerateFacesForSameNeighbors || Terrain.ExtractContents(cellValueFast) != blockIndex)
                && block.ShouldGenerateFace(
                    SubsystemTerrain,
                    5,
                    value,
                    cellValueFast,
                    x,
                    y,
                    z
                )) {
                DynamicArray<TerrainVertex> vertices6 = subsetsByFace[5].Vertices;
                DynamicArray<int> indices6 = subsetsByFace[5].Indices;
                int faceTextureSlot6 = block.GetFaceTextureSlot(5, value);
                int count11 = vertices6.Count;
                vertices6.Count += 4;
                SetupCubeVertexFace5(
                    x,
                    y,
                    z,
                    1f,
                    rotationY,
                    faceTextureSlot6,
                    textureSlotCount,
                    color,
                    ref vertices6.Array[count11]
                );
                SetupCubeVertexFace5(
                    x + 1,
                    y,
                    z,
                    1f,
                    1 + rotationY,
                    faceTextureSlot6,
                    textureSlotCount,
                    color,
                    ref vertices6.Array[count11 + 1]
                );
                SetupCubeVertexFace5(
                    x + 1,
                    y,
                    z + 1,
                    1f,
                    2 + rotationY,
                    faceTextureSlot6,
                    textureSlotCount,
                    color,
                    ref vertices6.Array[count11 + 2]
                );
                SetupCubeVertexFace5(
                    x,
                    y,
                    z + 1,
                    1f,
                    3 + rotationY,
                    faceTextureSlot6,
                    textureSlotCount,
                    color,
                    ref vertices6.Array[count11 + 3]
                );
                int count12 = indices6.Count;
                indices6.Count += 6;
                indices6.Array[count12] = count11;
                indices6.Array[count12 + 1] = count11 + 2;
                indices6.Array[count12 + 2] = count11 + 1;
                indices6.Array[count12 + 3] = count11 + 2;
                indices6.Array[count12 + 4] = count11;
                indices6.Array[count12 + 5] = count11 + 3;
            }
        }

        public override int CalculateVertexLightFace0(int x, int y, int z) {
            int light = 0;
            int shadow = 0;
            int subterrainLightValue = m_subterrainSystem.Light << 10;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCell(x - 1, z);
            int num = TerrainChunk.CalculateCellIndex((x - 1) & 0xF, y, z & 0xF);
            int cellValueFast = num > 0 ? chunkAtCell.GetCellValueFast(num - 1) : subterrainLightValue;
            int cellValueFast2 = chunkAtCell.GetCellValueFast(num);
            CalculateCubeVertexLight(cellValueFast, ref light, ref shadow);
            CalculateCubeVertexLight(cellValueFast2, ref light, ref shadow);
            TerrainChunk chunkAtCell2 = Terrain.GetChunkAtCell(x, z);
            int num2 = TerrainChunk.CalculateCellIndex(x & 0xF, y, z & 0xF);
            int cellValueFast3 = num2 > 0 ? chunkAtCell2.GetCellValueFast(num2 - 1) : subterrainLightValue;
            int cellValueFast4 = chunkAtCell2.GetCellValueFast(num2);
            CalculateCubeVertexLight(cellValueFast3, ref light, ref shadow);
            CalculateCubeVertexLight(cellValueFast4, ref light, ref shadow);
            return CombineLightAndShadow(light, shadow);
        }

        public override int CalculateVertexLightFace1(int x, int y, int z) {
            int light = 0;
            int shadow = 0;
            int subterrainLightValue = m_subterrainSystem.Light << 10;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCell(x, z - 1);
            int num = TerrainChunk.CalculateCellIndex(x & 0xF, y, (z - 1) & 0xF);
            int cellValueFast = num > 0 ? chunkAtCell.GetCellValueFast(num - 1) : subterrainLightValue;
            int cellValueFast2 = chunkAtCell.GetCellValueFast(num);
            CalculateCubeVertexLight(cellValueFast, ref light, ref shadow);
            CalculateCubeVertexLight(cellValueFast2, ref light, ref shadow);
            TerrainChunk chunkAtCell2 = Terrain.GetChunkAtCell(x, z);
            int num2 = TerrainChunk.CalculateCellIndex(x & 0xF, y, z & 0xF);
            int cellValueFast3 = num2 > 0 ? chunkAtCell2.GetCellValueFast(num2 - 1) : subterrainLightValue;
            int cellValueFast4 = chunkAtCell2.GetCellValueFast(num2);
            CalculateCubeVertexLight(cellValueFast3, ref light, ref shadow);
            CalculateCubeVertexLight(cellValueFast4, ref light, ref shadow);
            return CombineLightAndShadow(light, shadow);
        }

        public override int CalculateVertexLightFace2(int x, int y, int z) {
            int light = 0;
            int shadow = 0;
            int subterrainLightValue = m_subterrainSystem.Light << 10;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCell(x - 1, z - 1);
            int num = TerrainChunk.CalculateCellIndex((x - 1) & 0xF, y, (z - 1) & 0xF);
            int cellValueFast = num > 0 ? chunkAtCell.GetCellValueFast(num - 1) : subterrainLightValue;
            int cellValueFast2 = chunkAtCell.GetCellValueFast(num);
            CalculateCubeVertexLight(cellValueFast, ref light, ref shadow);
            CalculateCubeVertexLight(cellValueFast2, ref light, ref shadow);
            TerrainChunk chunkAtCell2 = Terrain.GetChunkAtCell(x, z - 1);
            int num2 = TerrainChunk.CalculateCellIndex(x & 0xF, y, (z - 1) & 0xF);
            int cellValueFast3 = num2 > 0 ? chunkAtCell2.GetCellValueFast(num2 - 1) : subterrainLightValue;
            int cellValueFast4 = chunkAtCell2.GetCellValueFast(num2);
            CalculateCubeVertexLight(cellValueFast3, ref light, ref shadow);
            CalculateCubeVertexLight(cellValueFast4, ref light, ref shadow);
            return CombineLightAndShadow(light, shadow);
        }

        public override int CalculateVertexLightFace3(int x, int y, int z) {
            int light = 0;
            int shadow = 0;
            int subterrainLightValue = m_subterrainSystem.Light << 10;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCell(x - 1, z - 1);
            int num = TerrainChunk.CalculateCellIndex((x - 1) & 0xF, y, (z - 1) & 0xF);
            int cellValueFast = num > 0 ? chunkAtCell.GetCellValueFast(num - 1) : subterrainLightValue;
            int cellValueFast2 = chunkAtCell.GetCellValueFast(num);
            CalculateCubeVertexLight(cellValueFast, ref light, ref shadow);
            CalculateCubeVertexLight(cellValueFast2, ref light, ref shadow);
            TerrainChunk chunkAtCell2 = Terrain.GetChunkAtCell(x - 1, z);
            int num2 = TerrainChunk.CalculateCellIndex((x - 1) & 0xF, y, z & 0xF);
            int cellValueFast3 = num2 > 0 ? chunkAtCell2.GetCellValueFast(num2 - 1) : subterrainLightValue;
            int cellValueFast4 = chunkAtCell2.GetCellValueFast(num2);
            CalculateCubeVertexLight(cellValueFast3, ref light, ref shadow);
            CalculateCubeVertexLight(cellValueFast4, ref light, ref shadow);
            return CombineLightAndShadow(light, shadow);
        }

        public override int CalculateVertexLightFace4(int x, int y, int z) {
            int light = 0;
            int shadow = 0;
            CalculateCubeVertexLight(Terrain.GetCellValueFastChunkExists(x - 1, y, z - 1), ref light, ref shadow);
            CalculateCubeVertexLight(Terrain.GetCellValueFastChunkExists(x, y, z - 1), ref light, ref shadow);
            CalculateCubeVertexLight(Terrain.GetCellValueFastChunkExists(x - 1, y, z), ref light, ref shadow);
            CalculateCubeVertexLight(Terrain.GetCellValueFastChunkExists(x, y, z), ref light, ref shadow);
            return CombineLightAndShadow(light, shadow);
        }

        public override int CalculateVertexLightFace5(int x, int y, int z) {
            int light = 0;
            int shadow = 0;
            int subterrainLightValue = m_subterrainSystem.Light << 10;
            CalculateCubeVertexLight(y > 0 ? Terrain.GetCellValueFastChunkExists(x - 1, y - 1, z - 1) : subterrainLightValue, ref light, ref shadow);
            CalculateCubeVertexLight(y > 0 ? Terrain.GetCellValueFastChunkExists(x, y - 1, z - 1) : subterrainLightValue, ref light, ref shadow);
            CalculateCubeVertexLight(y > 0 ? Terrain.GetCellValueFastChunkExists(x - 1, y - 1, z) : subterrainLightValue, ref light, ref shadow);
            CalculateCubeVertexLight(y > 0 ? Terrain.GetCellValueFastChunkExists(x, y - 1, z) : subterrainLightValue, ref light, ref shadow);
            return CombineLightAndShadow(light, shadow);
        }

        public static void GenerateGVWireVertices(BlockGeometryGenerator generator, int value, int x, int y, int z, int mountingFace, float centerBoxSize, Vector2 centerOffset, TerrainGeometrySubset subset) {
            SubsystemGVElectricity SubsystemGVElectricity;
            if (generator is GVBlockGeometryGenerator GVGenerator) {
                SubsystemGVElectricity = GVGenerator.SubsystemGVElectricity;
            }
            else {
                SubsystemGVElectricity = generator.SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
            }
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (SubsystemGVElectricity == null) {
                return;
            }
            Color innerTopColor = GVWireBlock.WireColor;
            Color innerBottomColor = innerTopColor;
            int innerContents = Terrain.ExtractContents(value);
            IGVElectricElementBlock innerBlock = BlocksManager.Blocks[innerContents] as IGVElectricElementBlock;
            switch (innerBlock) {
                case null: return;
                case GVWireHarnessBlock:
                    innerTopColor = GVWireHarnessBlock.WireColor1;
                    innerBottomColor = GVWireHarnessBlock.WireColor2;
                    break;
                case IPaintableBlock paintableBlock: {
                    int? color2 = paintableBlock.GetPaintColor(value);
                    if (color2.HasValue) {
                        innerTopColor = SubsystemPalette.GetColor(generator, color2);
                        innerBottomColor = innerTopColor;
                    }
                    break;
                }
                default: {
                    int mask = innerBlock.GetConnectionMask(value);
                    if (mask != int.MaxValue
                        && mask != 1
                        && mask % 2 != 0) {
                        innerTopColor = GVWireHarnessBlock.WireColor1;
                        innerBottomColor = GVWireHarnessBlock.WireColor2;
                    }
                    break;
                }
            }
            int GVWireHarnessBlockIndex = GVBlocksManager.GetBlockIndex<GVWireHarnessBlock>();
            int GVWireBlockIndex = GVBlocksManager.GetBlockIndex<GVWireBlock>();
            float innerLightValue = LightingManager.LightIntensityByLightValue[Terrain.ExtractLight(value)];
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
            DynamicArray<GVElectricConnectionPath> m_GVtmpConnectionPaths = [];
            SubsystemGVElectricity.GetAllConnectedNeighbors(
                x,
                y,
                z,
                mountingFace,
                generator.Terrain,
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
                        if (innerContents != GVWireBlockIndex) {
                            int outerValue = generator.Terrain.GetCellValue(x + tmpConnectionPath.NeighborOffsetX, y + tmpConnectionPath.NeighborOffsetY, z + tmpConnectionPath.NeighborOffsetZ);
                            IGVElectricElementBlock outerBlock = BlocksManager.Blocks[Terrain.ExtractContents(outerValue)] as IGVElectricElementBlock;
                            if (outerBlock == null) {
                                continue;
                            }
                            if (outerBlock is IPaintableBlock paintableBlock) {
                                int? color4 = paintableBlock.GetPaintColor(outerValue);
                                outerTopColor = color4.HasValue ? SubsystemPalette.GetColor(generator, color4) : GVWireBlock.WireColor;
                                outerBottomColor = outerTopColor;
                                innerTopColor2 = innerContents == GVWireHarnessBlockIndex ? GVWireHarnessBlock.WireColor1 : outerTopColor;
                                innerBottomColor2 = innerContents == GVWireHarnessBlockIndex ? GVWireHarnessBlock.WireColor2 : outerBottomColor;
                            }
                            else if (innerContents == GVWireHarnessBlockIndex
                                && outerBlock is not GVWireHarnessBlock) {
                                int mask = outerBlock.GetConnectionMask(outerValue);
                                if (mask == int.MaxValue
                                    || mask == 1
                                    || mask % 2 == 0) {
                                    outerTopColor = GVWireBlock.WireColor;
                                    outerBottomColor = outerTopColor;
                                }
                            }
                        }
                        Vector3 vector3 = connectorDirection != GVElectricConnectorDirection.In ? CellFace.FaceToVector3(tmpConnectionPath.ConnectorFace) : -Vector3.Normalize(vector2);
                        Vector3 vector4 = Vector3.Cross(vector, vector3);
                        float s = centerBoxSize >= 0f ? Math.Max(0.03125f, centerBoxSize / 2f) : centerBoxSize / 2f;
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
                            Vector3 vector9 = 0.25f * GetRandomWireOffset(0.5f * (v5 + v6), vector);
                            v5 += vector9;
                            v6 += vector9;
                            vector7 += vector9;
                        }
                        Vector2 vector10 = v2 + v3 * new Vector2(Math.Max(0.0625f, centerBoxSize), 0f);
                        Vector2 vector11 = v2 + v3 * new Vector2(num5 * 2f, 0f);
                        Vector2 vector12 = v2 + v3 * new Vector2(num5 * 2f, 1f);
                        Vector2 vector13 = v2 + v3 * new Vector2(Math.Max(0.0625f, centerBoxSize), 1f);
                        Vector2 vector14 = v2 + v3 * new Vector2(centerBoxSize, 0.5f);
                        Vector2 vector15 = v2 + v3 * new Vector2(num6 * 2f, 0.5f);
                        int num7 = Terrain.ExtractLight(generator.Terrain.GetCellValue(x + tmpConnectionPath.NeighborOffsetX, y + tmpConnectionPath.NeighborOffsetY, z + tmpConnectionPath.NeighborOffsetZ));
                        float num8 = LightingManager.LightIntensityByLightValue[num7];
                        float num9 = 0.5f * (innerLightValue + num8);
                        float num10 = LightingManager.CalculateLighting(-vector4);
                        float num11 = LightingManager.CalculateLighting(vector4);
                        float num12 = LightingManager.CalculateLighting(vector);
                        float num13 = num10 * innerLightValue;
                        float num14 = num10 * num9;
                        float num15 = num11 * num9;
                        float num16 = num11 * innerLightValue;
                        float num17 = num12 * innerLightValue;
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
                        SetupVertex(
                            v5.X,
                            v5.Y,
                            v5.Z,
                            color5,
                            vector10.X,
                            vector10.Y,
                            ref array[count]
                        );
                        SetupVertex(
                            vector5.X,
                            vector5.Y,
                            vector5.Z,
                            color6,
                            vector11.X,
                            vector11.Y,
                            ref array[count + 1]
                        );
                        SetupVertex(
                            vector6.X,
                            vector6.Y,
                            vector6.Z,
                            color7,
                            vector12.X,
                            vector12.Y,
                            ref array[count + 2]
                        );
                        SetupVertex(
                            v6.X,
                            v6.Y,
                            v6.Z,
                            color8,
                            vector13.X,
                            vector13.Y,
                            ref array[count + 3]
                        );
                        SetupVertex(
                            vector7.X,
                            vector7.Y,
                            vector7.Z,
                            color9,
                            vector14.X,
                            vector14.Y,
                            ref array[count + 4]
                        );
                        SetupVertex(
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
                || (num4 == 0 && innerContents != GVWireBlockIndex && innerContents != GVWireHarnessBlockIndex)) {
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
                        Vector3 vector18 = 0.25f * GetRandomWireOffset(0.5f * (v8 + v9), vector);
                        v8 += vector18;
                        v9 += vector18;
                        vector17 += vector18;
                    }
                    Vector2 vector19 = v2 + v3 * new Vector2(0.0625f, 0f);
                    Vector2 vector20 = v2 + v3 * new Vector2(0.0625f, 1f);
                    Vector2 vector21 = v2 + v3 * new Vector2(0f, 0.5f);
                    float num19 = LightingManager.CalculateLighting(vector16) * innerLightValue;
                    float num20 = LightingManager.CalculateLighting(vector) * innerLightValue;
                    Color color11 = new((byte)(innerBottomColor.R * num19), (byte)(innerBottomColor.G * num19), (byte)(innerBottomColor.B * num19));
                    Color color12 = new((byte)(innerTopColor.R * num20), (byte)(innerTopColor.G * num20), (byte)(innerTopColor.B * num20));
                    int count3 = subset.Vertices.Count;
                    subset.Vertices.Count += 3;
                    TerrainVertex[] array3 = subset.Vertices.Array;
                    SetupVertex(
                        v8.X,
                        v8.Y,
                        v8.Z,
                        color11,
                        vector19.X,
                        vector19.Y,
                        ref array3[count3]
                    );
                    SetupVertex(
                        v9.X,
                        v9.Y,
                        v9.Z,
                        color11,
                        vector20.X,
                        vector20.Y,
                        ref array3[count3 + 1]
                    );
                    SetupVertex(
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