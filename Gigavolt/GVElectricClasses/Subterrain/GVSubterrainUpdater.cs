using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Engine;
using Engine.Graphics;
using GameEntitySystem;

namespace Game {
    public class GVSubterrainUpdater {
        public readonly GVSubterrainSystem m_subterrainSystem;
        public readonly SubsystemAnimatedTextures m_subsystemAnimatedTextures;
        public readonly SubsystemGVBlockBehaviors m_subsystemGVBlockBehaviors;
        public readonly Terrain m_terrain;

        public Task m_task;
        public volatile bool m_quitUpdateThread;
        public AutoResetEvent m_updateEvent = new(true);
        public ManualResetEvent m_pauseEvent = new(true);
        public readonly object m_unpauseLock = new();
        public bool m_unpauseUpdateThread;
        public readonly object m_updateParametersLock = new();
        public TerrainChunk[] m_threadUpdateChunks = [];
        public TerrainChunk[] m_updateChunks = [];
        public bool m_shouldUpdateChunks;
        public readonly List<TerrainUpdater.LightSource> m_lightSources = [];

        public GVSubterrainUpdater(GVSubterrainSystem system, Project project) {
            m_subterrainSystem = system;
            m_subsystemAnimatedTextures = project.FindSubsystem<SubsystemAnimatedTextures>(true);
            m_subsystemGVBlockBehaviors = project.FindSubsystem<SubsystemGVBlockBehaviors>(true);
            m_terrain = system.Terrain;
        }

        public void Update(bool lightChanged) {
            if (lightChanged) {
                DowngradeAllChunksState(TerrainChunkState.InvalidLight, false);
            }
            if (m_task == null) {
                m_quitUpdateThread = false;
                m_task = Task.Run(ThreadUpdateFunction);
                UnpauseUpdateThread();
                m_updateEvent.Set();
            }
            if (m_shouldUpdateChunks) {
                m_updateChunks = m_terrain.AllocatedChunks;
                m_shouldUpdateChunks = false;
                m_updateEvent.Set();
            }
            else {
                lock (m_updateParametersLock) {
                    if (SendReceiveChunkStates()) {
                        UnpauseUpdateThread();
                    }
                }
            }
            foreach (TerrainChunk terrainChunk in m_terrain.AllocatedChunks) {
                if (terrainChunk.State >= TerrainChunkState.InvalidVertices1
                    && !terrainChunk.AreBehaviorsNotified) {
                    terrainChunk.AreBehaviorsNotified = true;
                    NotifyBlockBehaviors(terrainChunk);
                }
            }
        }

        public void DowngradeChunkNeighborhoodState(Point2 coordinates, int radius, TerrainChunkState state, bool forceGeometryRegeneration) {
            for (int i = -radius; i <= radius; i++) {
                for (int j = -radius; j <= radius; j++) {
                    TerrainChunk chunkAtCoords = m_terrain.GetChunkAtCoords(coordinates.X + i, coordinates.Y + j) ?? m_terrain.AllocateChunk(coordinates.X + i, coordinates.Y + j);
                    if (chunkAtCoords.State > state) {
                        chunkAtCoords.State = state;
                        if (forceGeometryRegeneration) {
                            chunkAtCoords.InvalidateSliceContentsHashes();
                        }
                    }
                    chunkAtCoords.WasDowngraded = true;
                }
            }
        }

        public void DowngradeAllChunksState(TerrainChunkState state, bool forceGeometryRegeneration) {
            TerrainChunk[] allocatedChunks = m_terrain.AllocatedChunks;
            foreach (TerrainChunk terrainChunk in allocatedChunks) {
                if (terrainChunk.State > state) {
                    terrainChunk.State = state;
                    if (forceGeometryRegeneration) {
                        terrainChunk.InvalidateSliceContentsHashes();
                    }
                }
                terrainChunk.WasDowngraded = true;
            }
        }

        public void ThreadUpdateFunction() {
            while (!m_quitUpdateThread) {
                m_pauseEvent.WaitOne();
                m_updateEvent.WaitOne();
                try {
                    if (SynchronousUpdateFunction()) {
                        lock (m_unpauseLock) {
                            if (!m_unpauseUpdateThread) {
                                m_pauseEvent.Reset();
                            }
                            m_unpauseUpdateThread = false;
                        }
                    }
                }
                catch (Exception) {
                    // ignored
                }
                finally {
                    m_updateEvent.Set();
                }
            }
        }

        public bool SynchronousUpdateFunction() {
            lock (m_updateParametersLock) {
                m_threadUpdateChunks = m_updateChunks;
                SendReceiveChunkStatesThread();
            }
            TerrainChunk terrainChunk = FindBestChunkToUpdate(out TerrainChunkState desiredState);
            if (terrainChunk != null) {
                double realTime = Time.RealTime;
                do {
                    UpdateChunkSingleStep(terrainChunk, m_subterrainSystem.Light);
                }
                while (terrainChunk.ThreadState < desiredState
                    && Time.RealTime - realTime < 0.01);
                return false;
            }
            return true;
        }

        public bool SendReceiveChunkStates() {
            bool result = false;
            TerrainChunk[] chunks = m_updateChunks;
            foreach (TerrainChunk terrainChunk in chunks) {
                if (terrainChunk.WasDowngraded) {
                    terrainChunk.DowngradedState = terrainChunk.State;
                    terrainChunk.WasDowngraded = false;
                    result = true;
                }
                else if (terrainChunk.UpgradedState.HasValue) {
                    terrainChunk.State = terrainChunk.UpgradedState.Value;
                }
                terrainChunk.UpgradedState = null;
            }
            return result;
        }

        public void SendReceiveChunkStatesThread() {
            foreach (TerrainChunk terrainChunk in m_threadUpdateChunks) {
                if (terrainChunk.DowngradedState.HasValue) {
                    terrainChunk.ThreadState = terrainChunk.DowngradedState.Value;
                    terrainChunk.DowngradedState = null;
                }
                else if (terrainChunk.WasUpgraded) {
                    terrainChunk.UpgradedState = terrainChunk.ThreadState;
                }
                terrainChunk.WasUpgraded = false;
            }
        }

        public TerrainChunk FindBestChunkToUpdate(out TerrainChunkState desiredState) {
            TerrainChunk result = null;
            desiredState = TerrainChunkState.NotLoaded;
            float minDistance = float.MaxValue;
            foreach (TerrainChunk terrainChunk in m_threadUpdateChunks) {
                if (terrainChunk.ThreadState >= TerrainChunkState.Valid) {
                    continue;
                }
                float distanceSquared = Vector2.DistanceSquared(Vector2.Zero, terrainChunk.Center);
                if (distanceSquared < minDistance) {
                    minDistance = distanceSquared;
                    result = terrainChunk;
                }
            }
            if (result != null) {
                desiredState = TerrainChunkState.Valid;
            }
            return result;
        }

        public void UpdateChunkSingleStep(TerrainChunk chunk, int skylightValue) {
            switch (chunk.ThreadState) {
                case TerrainChunkState.NotLoaded: {
                    chunk.ThreadState = TerrainChunkState.InvalidLight;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidContents1: {
                    chunk.ThreadState = TerrainChunkState.InvalidLight;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidContents2: {
                    chunk.ThreadState = TerrainChunkState.InvalidLight;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidContents3: {
                    chunk.ThreadState = TerrainChunkState.InvalidLight;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidContents4: {
                    chunk.ThreadState = TerrainChunkState.InvalidLight;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidLight: {
                    GenerateChunkSunLightAndHeight(chunk, skylightValue);
                    chunk.ThreadState = TerrainChunkState.InvalidPropagatedLight;
                    chunk.WasUpgraded = true;
                    chunk.LightPropagationMask = 0;
                    break;
                }
                case TerrainChunkState.InvalidPropagatedLight: {
                    for (int i = -2; i <= 2; i++) {
                        for (int j = -2; j <= 2; j++) {
                            TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(chunk.Origin.X + i * 16, chunk.Origin.Y + j * 16);
                            if (chunkAtCell != null
                                && chunkAtCell.ThreadState < TerrainChunkState.InvalidPropagatedLight) {
                                UpdateChunkSingleStep(chunkAtCell, skylightValue);
                                return;
                            }
                        }
                    }
                    m_lightSources.Clear();
                    for (int k = -1; k <= 1; k++) {
                        for (int l = -1; l <= 1; l++) {
                            int num = TerrainUpdater.CalculateLightPropagationBitIndex(k, l);
                            if (((chunk.LightPropagationMask >> num) & 1) == 0) {
                                TerrainChunk chunkAtCell2 = m_terrain.GetChunkAtCell(chunk.Origin.X + k * 16, chunk.Origin.Y + l * 16);
                                if (chunkAtCell2 != null) {
                                    GenerateChunkLightSources(chunkAtCell2);
                                    UpdateNeighborsLightPropagationBitmasks(chunkAtCell2);
                                }
                            }
                        }
                    }
                    PropagateLight();
                    chunk.ThreadState = TerrainChunkState.InvalidVertices1;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidVertices1: {
                    lock (chunk.Geometry) {
                        chunk.NewGeometryData = false;
                        GenerateChunkVertices(chunk, true);
                    }
                    chunk.ThreadState = TerrainChunkState.InvalidVertices2;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidVertices2: {
                    lock (chunk.Geometry) {
                        GenerateChunkVertices(chunk, false);
                        chunk.NewGeometryData = true;
                    }
                    chunk.ThreadState = TerrainChunkState.Valid;
                    chunk.WasUpgraded = true;
                    break;
                }
            }
        }

        public void GenerateChunkSunLightAndHeight(TerrainChunk chunk, int skylightValue) {
            for (int x = 0; x < 16; x++) {
                for (int z = 0; z < 16; z++) {
                    int topHeight = 0;
                    int bottomHeight = 255;
                    int nowHeight = 255;
                    int cellIndex = TerrainChunk.CalculateCellIndex(x, 255, z);
                    while (nowHeight >= 0) {
                        int cellValueFast = chunk.GetCellValueFast(cellIndex);
                        if (Terrain.ExtractContents(cellValueFast) != 0) {
                            topHeight = nowHeight;
                            break;
                        }
                        cellValueFast = Terrain.ReplaceLight(cellValueFast, skylightValue);
                        chunk.SetCellValueFast(cellIndex, cellValueFast);
                        nowHeight--;
                        cellIndex--;
                    }
                    nowHeight = 0;
                    cellIndex = TerrainChunk.CalculateCellIndex(x, 0, z);
                    while (nowHeight <= topHeight + 1) {
                        int cellValueFast2 = chunk.GetCellValueFast(cellIndex);
                        if (BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast2)].IsTransparent_(cellValueFast2)) {
                            bottomHeight = nowHeight;
                            break;
                        }
                        cellValueFast2 = Terrain.ReplaceLight(cellValueFast2, skylightValue);
                        chunk.SetCellValueFast(cellIndex, cellValueFast2);
                        nowHeight++;
                        cellIndex++;
                    }
                    nowHeight = topHeight;
                    cellIndex = TerrainChunk.CalculateCellIndex(x, topHeight, z);
                    while (nowHeight >= bottomHeight) {
                        int cellValueFast4 = chunk.GetCellValueFast(cellIndex);
                        cellValueFast4 = Terrain.ReplaceLight(cellValueFast4, skylightValue);
                        chunk.SetCellValueFast(cellIndex, cellValueFast4);
                        nowHeight--;
                        cellIndex--;
                    }
                    chunk.SetTopHeightFast(x, z, topHeight);
                    chunk.SetBottomHeightFast(x, z, bottomHeight);
                }
            }
        }

        public void GenerateChunkLightSources(TerrainChunk chunk) {
            Block[] blocks = BlocksManager.Blocks;
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 16; j++) {
                    int num = i + chunk.Origin.X;
                    int num2 = j + chunk.Origin.Y;
                    TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(num - 1, num2);
                    TerrainChunk chunkAtCell2 = m_terrain.GetChunkAtCell(num + 1, num2);
                    TerrainChunk chunkAtCell3 = m_terrain.GetChunkAtCell(num, num2 - 1);
                    TerrainChunk chunkAtCell4 = m_terrain.GetChunkAtCell(num, num2 + 1);
                    if (chunkAtCell == null
                        || chunkAtCell2 == null
                        || chunkAtCell3 == null
                        || chunkAtCell4 == null) {
                        continue;
                    }
                    int topHeightFast = chunk.GetTopHeightFast(i, j);
                    int bottomHeightFast = chunk.GetBottomHeightFast(i, j);
                    int x = num - 1 - chunkAtCell.Origin.X;
                    int z = num2 - chunkAtCell.Origin.Y;
                    int x2 = num + 1 - chunkAtCell2.Origin.X;
                    int z2 = num2 - chunkAtCell2.Origin.Y;
                    int x3 = num - chunkAtCell3.Origin.X;
                    int z3 = num2 - 1 - chunkAtCell3.Origin.Y;
                    int x4 = num - chunkAtCell4.Origin.X;
                    int z4 = num2 + 1 - chunkAtCell4.Origin.Y;
                    int shaftValueFast = chunkAtCell.GetShaftValueFast(x, z);
                    int shaftValueFast2 = chunkAtCell2.GetShaftValueFast(x2, z2);
                    int shaftValueFast3 = chunkAtCell3.GetShaftValueFast(x3, z3);
                    int shaftValueFast4 = chunkAtCell4.GetShaftValueFast(x4, z4);
                    int x5 = Terrain.ExtractSunlightHeight(shaftValueFast);
                    int x6 = Terrain.ExtractSunlightHeight(shaftValueFast2);
                    int x7 = Terrain.ExtractSunlightHeight(shaftValueFast3);
                    int x8 = Terrain.ExtractSunlightHeight(shaftValueFast4);
                    int num3 = MathUtils.Min(x5, x6, x7, x8);
                    int num4 = bottomHeightFast;
                    int num5 = TerrainChunk.CalculateCellIndex(i, bottomHeightFast, j);
                    while (num4 <= topHeightFast) {
                        int cellValueFast = chunk.GetCellValueFast(num5);
                        int num6 = 0;
                        Block block = blocks[Terrain.ExtractContents(cellValueFast)];
                        if (num4 >= num3
                            && block.IsTransparent_(cellValueFast)) {
                            int cellLightFast = chunkAtCell.GetCellLightFast(x, num4, z);
                            int cellLightFast2 = chunkAtCell2.GetCellLightFast(x2, num4, z2);
                            int cellLightFast3 = chunkAtCell3.GetCellLightFast(x3, num4, z3);
                            int cellLightFast4 = chunkAtCell4.GetCellLightFast(x4, num4, z4);
                            num6 = MathUtils.Max(cellLightFast, cellLightFast2, cellLightFast3, cellLightFast4) - 1 - block.LightAttenuation;
                        }
                        if (block.DefaultEmittedLightAmount > 0) {
                            num6 = Math.Max(num6, block.GetEmittedLightAmount(cellValueFast));
                        }
                        if (num6 > Terrain.ExtractLight(cellValueFast)) {
                            chunk.SetCellValueFast(num5, Terrain.ReplaceLight(cellValueFast, num6));
                            m_lightSources.Add(new TerrainUpdater.LightSource { X = num, Y = num4, Z = num2, Light = num6 });
                        }
                        num4++;
                        num5++;
                    }
                }
            }
        }

        public void UpdateNeighborsLightPropagationBitmasks(TerrainChunk chunk) {
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    TerrainChunk chunkAtCoords = m_terrain.GetChunkAtCoords(chunk.Coords.X + i, chunk.Coords.Y + j);
                    if (chunkAtCoords != null) {
                        int num = TerrainUpdater.CalculateLightPropagationBitIndex(-i, -j);
                        chunkAtCoords.LightPropagationMask |= 1 << num;
                    }
                }
            }
        }

        public void PropagateLight() {
            for (int index = 0; index < m_lightSources.Count; index++) {
                TerrainUpdater.LightSource lightSource = m_lightSources[index];
                int light = lightSource.Light;
                if (light > 1) {
                    PropagateLightSource(lightSource.X - 1, lightSource.Y, lightSource.Z, light);
                    PropagateLightSource(lightSource.X + 1, lightSource.Y, lightSource.Z, light);
                    if (lightSource.Y > 0) {
                        PropagateLightSource(lightSource.X, lightSource.Y - 1, lightSource.Z, light);
                    }
                    if (lightSource.Y < 255) {
                        PropagateLightSource(lightSource.X, lightSource.Y + 1, lightSource.Z, light);
                    }
                    PropagateLightSource(lightSource.X, lightSource.Y, lightSource.Z - 1, light);
                    PropagateLightSource(lightSource.X, lightSource.Y, lightSource.Z + 1, light);
                }
            }
        }

        public void PropagateLightSource(int x, int y, int z, int light) {
            TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(x, z);
            if (chunkAtCell == null) {
                return;
            }
            int index = TerrainChunk.CalculateCellIndex(x & 0xF, y, z & 0xF);
            int cellValueFast = chunkAtCell.GetCellValueFast(index);
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValueFast)];
            if (block.IsTransparent_(cellValueFast)) {
                int num2 = light - block.LightAttenuation - 1;
                if (num2 > Terrain.ExtractLight(cellValueFast)) {
                    m_lightSources.Add(new TerrainUpdater.LightSource { X = x, Y = y, Z = z, Light = num2 });
                    chunkAtCell.SetCellValueFast(index, Terrain.ReplaceLight(cellValueFast, num2));
                }
            }
        }

        public void GenerateChunkVertices(TerrainChunk chunk, bool even) {
            m_subterrainSystem.BlockGeometryGenerator.ResetCache();
            if (!chunk.Draws.TryGetValue(m_subsystemAnimatedTextures.AnimatedBlocksTexture, out TerrainGeometry[] terrainGeometry)) {
                terrainGeometry = new TerrainGeometry[16];
                for (int i = 0; i < 16; i++) {
                    TerrainGeometry t = new(chunk.Draws, i);
                    terrainGeometry[i] = t;
                }
                chunk.Draws.Add(m_subsystemAnimatedTextures.AnimatedBlocksTexture, terrainGeometry);
            }
            TerrainChunk chunkAtCoords = m_terrain.GetChunkAtCoords(chunk.Coords.X - 1, chunk.Coords.Y - 1);
            TerrainChunk chunkAtCoords2 = m_terrain.GetChunkAtCoords(chunk.Coords.X, chunk.Coords.Y - 1);
            TerrainChunk chunkAtCoords3 = m_terrain.GetChunkAtCoords(chunk.Coords.X + 1, chunk.Coords.Y - 1);
            TerrainChunk chunkAtCoords4 = m_terrain.GetChunkAtCoords(chunk.Coords.X - 1, chunk.Coords.Y);
            TerrainChunk chunkAtCoords5 = m_terrain.GetChunkAtCoords(chunk.Coords.X + 1, chunk.Coords.Y);
            TerrainChunk chunkAtCoords6 = m_terrain.GetChunkAtCoords(chunk.Coords.X - 1, chunk.Coords.Y + 1);
            TerrainChunk chunkAtCoords7 = m_terrain.GetChunkAtCoords(chunk.Coords.X, chunk.Coords.Y + 1);
            TerrainChunk chunkAtCoords8 = m_terrain.GetChunkAtCoords(chunk.Coords.X + 1, chunk.Coords.Y + 1);
            int minX = 0;
            int minZ = 0;
            int maxX = 16;
            int maxZ = 16;
            if (chunkAtCoords4 == null) {
                minX++;
            }
            if (chunkAtCoords2 == null) {
                minZ++;
            }
            if (chunkAtCoords5 == null) {
                maxX--;
            }
            if (chunkAtCoords7 == null) {
                maxZ--;
            }
            for (int i = 0; i < 16; i++) {
                if (i % 2 == 0 != even) {
                    continue;
                }
                chunk.SliceContentsHashes[i] = CalculateChunkSliceContentsHash(chunk, i);
                int generateHash = chunk.GeneratedSliceContentsHashes[i];
                if (generateHash != 0
                    && generateHash == chunk.SliceContentsHashes[i]) {
                    continue;
                }
                foreach (KeyValuePair<Texture2D, TerrainGeometry[]> c in chunk.Draws) {
                    TerrainGeometrySubset[] subsets = c.Value[i].Subsets;
                    foreach (TerrainGeometrySubset t in subsets) {
                        t.Vertices.Clear();
                        t.Indices.Clear();
                    }
                }
                for (int xInChunk = minX; xInChunk < maxX; xInChunk++) {
                    for (int zInChunk = minZ; zInChunk < maxZ; zInChunk++) {
                        switch (xInChunk) {
                            case 0:
                                if ((zInChunk == 0 && chunkAtCoords == null)
                                    || (zInChunk == 15 && chunkAtCoords6 == null)) {
                                    continue;
                                }
                                break;
                            case 15:
                                if ((zInChunk == 0 && chunkAtCoords3 == null)
                                    || (zInChunk == 15 && chunkAtCoords8 == null)) {
                                    continue;
                                }
                                break;
                        }
                        int xInWorld = xInChunk + chunk.Origin.X;
                        int zInWorld = zInChunk + chunk.Origin.Y;
                        int bottomHeightFast = chunk.GetBottomHeightFast(xInChunk, zInChunk);
                        int bottomHeight = m_terrain.GetBottomHeight(xInWorld - 1, zInWorld);
                        int bottomHeight2 = m_terrain.GetBottomHeight(xInWorld + 1, zInWorld);
                        int bottomHeight3 = m_terrain.GetBottomHeight(xInWorld, zInWorld - 1);
                        int bottomHeight4 = m_terrain.GetBottomHeight(xInWorld, zInWorld + 1);
                        int bottom = MathUtils.Max(16 * i, MathUtils.Min(bottomHeightFast - 1, MathUtils.Min(bottomHeight, bottomHeight2, bottomHeight3, bottomHeight4)), 0);
                        int top = MathUtils.Min(16 * (i + 1), chunk.GetTopHeightFast(xInChunk, zInChunk) + 1, 255);
                        int zeroIndex = TerrainChunk.CalculateCellIndex(xInChunk, 0, zInChunk);
                        for (int y = bottom; y < top; y++) {
                            int cellValueFast = chunk.GetCellValueFast(zeroIndex + y);
                            int contents = Terrain.ExtractContents(cellValueFast);
                            if (contents != 0) {
                                BlocksManager.Blocks[contents]
                                .GenerateTerrainVertices(
                                    m_subterrainSystem.BlockGeometryGenerator,
                                    terrainGeometry[i],
                                    cellValueFast,
                                    xInWorld,
                                    y,
                                    zInWorld
                                );
                            }
                        }
                    }
                }
            }
        }

        public int CalculateChunkSliceContentsHash(TerrainChunk chunk, int sliceIndex) {
            int num = 1;
            int num2 = chunk.Origin.X - 1;
            int num3 = chunk.Origin.X + 16 + 1;
            int num4 = chunk.Origin.Y - 1;
            int num5 = chunk.Origin.Y + 16 + 1;
            int x = Math.Max(16 * sliceIndex - 1, 0);
            int x2 = Math.Min(16 * (sliceIndex + 1) + 1, 256);
            for (int i = num2; i < num3; i++) {
                for (int j = num4; j < num5; j++) {
                    TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(i, j);
                    if (chunkAtCell != null) {
                        int x3 = i & 0xF;
                        int z = j & 0xF;
                        int shaftValueFast = chunkAtCell.GetShaftValueFast(x3, z);
                        int num6 = Terrain.ExtractBottomHeight(shaftValueFast);
                        int num7 = Terrain.ExtractTopHeight(shaftValueFast);
                        int num8 = Math.Max(x, num6 - 1);
                        int num9 = Math.Min(x2, num7 + 2);
                        int num10 = TerrainChunk.CalculateCellIndex(x3, num8, z);
                        int num11 = num10 + num9 - num8;
                        while (num10 < num11) {
                            num += chunkAtCell.GetCellValueFast(num10++);
                            num *= 31;
                        }
                        num += Terrain.ExtractTemperature(shaftValueFast);
                        num *= 31;
                        num += Terrain.ExtractHumidity(shaftValueFast);
                        num *= 31;
                        num += num8;
                        num *= 31;
                    }
                }
            }
            num += m_terrain.SeasonTemperature;
            num *= 31;
            num += m_terrain.SeasonHumidity;
            num *= 31;
            return num;
        }

        public void UnpauseUpdateThread() {
            lock (m_unpauseLock) {
                m_unpauseUpdateThread = true;
                m_pauseEvent.Set();
            }
        }

        public void NotifyBlockBehaviors(TerrainChunk chunk) {
            bool isLoaded = chunk.IsLoaded;
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 16; j++) {
                    int x = i + chunk.Origin.X;
                    int z = j + chunk.Origin.Y;
                    int num = TerrainChunk.CalculateCellIndex(i, 0, j);
                    int num2 = 0;
                    while (num2 < 255) {
                        int cellValueFast = chunk.GetCellValueFast(num);
                        int contents = Terrain.ExtractContents(cellValueFast);
                        if (contents != 0) {
                            List<IGVBlockBehavior> blockBehaviors = m_subsystemGVBlockBehaviors.GetBlockBehaviors(contents);
                            foreach (IGVBlockBehavior blockBehavior in blockBehaviors) {
                                blockBehavior.OnBlockGenerated(
                                    cellValueFast,
                                    x,
                                    num2,
                                    z,
                                    isLoaded,
                                    m_subterrainSystem
                                );
                            }
                        }
                        num2++;
                        num++;
                    }
                }
            }
        }

        public void Dispose() {
            m_quitUpdateThread = true;
            UnpauseUpdateThread();
            m_updateEvent.Set();
            if (m_task != null) {
                m_task.Wait();
                m_task = null;
            }
            m_pauseEvent.Dispose();
            m_updateEvent.Dispose();
        }
    }
}