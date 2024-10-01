using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        public readonly AutoResetEvent m_updateEvent = new(true);
        public readonly ManualResetEvent m_pauseEvent = new(true);
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
            if (Monitor.TryEnter(m_updateParametersLock, 0)) {
                try {
                    if (SendReceiveChunkStates()) {
                        UnpauseUpdateThread();
                    }
                }
                finally {
                    Monitor.Exit(m_updateParametersLock);
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
                    break;
                }
                case TerrainChunkState.InvalidPropagatedLight: {
                    for (int i = -1; i <= 1; i++) {
                        for (int j = -1; j <= 1; j++) {
                            TerrainChunk chunkAtCoords = m_terrain.GetChunkAtCoords(chunk.Coords.X + i, chunk.Coords.Y + j);
                            if (chunkAtCoords != null
                                && chunkAtCoords.ThreadState < TerrainChunkState.InvalidPropagatedLight) {
                                UpdateChunkSingleStep(chunkAtCoords, skylightValue);
                                return;
                            }
                        }
                    }
                    m_lightSources.Clear();
                    GenerateChunkLightSources(chunk);
                    GenerateChunkEdgeLightSources(chunk, 0);
                    GenerateChunkEdgeLightSources(chunk, 1);
                    GenerateChunkEdgeLightSources(chunk, 2);
                    GenerateChunkEdgeLightSources(chunk, 3);
                    PropagateLightSources();
                    chunk.ThreadState = TerrainChunkState.InvalidVertices1;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidVertices1: {
                    for (int k = -1; k <= 1; k++) {
                        for (int l = -1; l <= 1; l++) {
                            TerrainChunk chunkAtCoords2 = m_terrain.GetChunkAtCoords(chunk.Coords.X + k, chunk.Coords.Y + l);
                            if (chunkAtCoords2 != null
                                && chunkAtCoords2.ThreadState < TerrainChunkState.InvalidVertices1) {
                                UpdateChunkSingleStep(chunkAtCoords2, skylightValue);
                                return;
                            }
                        }
                    }
                    CalculateChunkSliceContentsHashes(chunk);
                    lock (chunk.Geometry) {
                        chunk.NewGeometryData = false;
                        GenerateChunkVertices(chunk, 0);
                        ModsManager.HookAction(
                            "GenerateChunkVertices",
                            modLoader => {
                                modLoader.GenerateChunkVertices(chunk, true);
                                return true;
                            }
                        );
                    }
                    chunk.ThreadState = TerrainChunkState.InvalidVertices2;
                    chunk.WasUpgraded = true;
                    break;
                }
                case TerrainChunkState.InvalidVertices2: {
                    lock (chunk.Geometry) {
                        GenerateChunkVertices(chunk, 1);
                        ModsManager.HookAction(
                            "GenerateChunkVertices",
                            modLoader => {
                                modLoader.GenerateChunkVertices(chunk, true);
                                return false;
                            }
                        );
                        chunk.NewGeometryData = true;
                    }
                    chunk.ThreadState = TerrainChunkState.Valid;
                    chunk.WasUpgraded = true;
                    break;
                }
            }
        }

        public void GenerateChunkSunLightAndHeight(TerrainChunk chunk, int skylightValue) {
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 16; j++) {
                    int num = 0;
                    int num2 = 255;
                    int num3 = 0;
                    int num4 = 255;
                    int num5 = TerrainChunk.CalculateCellIndex(i, 255, j);
                    while (num4 >= 0) {
                        int cellValueFast = chunk.GetCellValueFast(num5);
                        if (Terrain.ExtractContents(cellValueFast) != 0) {
                            num = num4;
                            break;
                        }
                        cellValueFast = Terrain.ReplaceLight(cellValueFast, skylightValue);
                        chunk.SetCellValueFast(num5, cellValueFast);
                        num4--;
                        num5--;
                    }
                    num4 = 0;
                    num5 = TerrainChunk.CalculateCellIndex(i, 0, j);
                    while (num4 <= num + 1) {
                        int cellValueFast2 = chunk.GetCellValueFast(num5);
                        int num6 = Terrain.ExtractContents(cellValueFast2);
                        if (BlocksManager.Blocks[num6].IsTransparent_(cellValueFast2)) {
                            num2 = num4;
                            break;
                        }
                        cellValueFast2 = Terrain.ReplaceLight(cellValueFast2, 0);
                        chunk.SetCellValueFast(num5, cellValueFast2);
                        num4++;
                        num5++;
                    }
                    int num7 = skylightValue;
                    num4 = num;
                    num5 = TerrainChunk.CalculateCellIndex(i, num, j);
                    if (num7 > 0) {
                        while (num4 >= num2) {
                            int cellValueFast3 = chunk.GetCellValueFast(num5);
                            int num8 = Terrain.ExtractContents(cellValueFast3);
                            if (num8 != 0) {
                                Block block = BlocksManager.Blocks[num8];
                                if (!block.IsTransparent_(cellValueFast3)
                                    || block.LightAttenuation >= num7) {
                                    break;
                                }
                                num7 -= block.LightAttenuation;
                            }
                            cellValueFast3 = Terrain.ReplaceLight(cellValueFast3, num7);
                            chunk.SetCellValueFast(num5, cellValueFast3);
                            num4--;
                            num5--;
                        }
                    }
                    num3 = num4 + 1;
                    while (num4 >= num2) {
                        int cellValueFast4 = chunk.GetCellValueFast(num5);
                        cellValueFast4 = Terrain.ReplaceLight(cellValueFast4, 0);
                        chunk.SetCellValueFast(num5, cellValueFast4);
                        num4--;
                        num5--;
                    }
                    chunk.SetTopHeightFast(i, j, num);
                    chunk.SetBottomHeightFast(i, j, num2);
                    chunk.SetSunlightHeightFast(i, j, num3);
                }
            }
        }

        public void GenerateChunkLightSources(TerrainChunk chunk) {
            Block[] blocks = BlocksManager.Blocks;
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 16; j++) {
                    int topHeightFast = chunk.GetTopHeightFast(i, j);
                    int bottomHeightFast = chunk.GetBottomHeightFast(i, j);
                    int num = i + chunk.Origin.X;
                    int num2 = j + chunk.Origin.Y;
                    int k = bottomHeightFast;
                    int num3 = TerrainChunk.CalculateCellIndex(i, bottomHeightFast, j);
                    while (k <= topHeightFast) {
                        int cellValueFast = chunk.GetCellValueFast(num3);
                        Block block = blocks[Terrain.ExtractContents(cellValueFast)];
                        if (block.DefaultEmittedLightAmount > 0) {
                            int emittedLightAmount = block.GetEmittedLightAmount(cellValueFast);
                            if (emittedLightAmount > Terrain.ExtractLight(cellValueFast)) {
                                chunk.SetCellValueFast(num3, Terrain.ReplaceLight(cellValueFast, emittedLightAmount));
                                if (emittedLightAmount > 1) {
                                    m_lightSources.Add(new TerrainUpdater.LightSource { X = num, Y = k, Z = num2, Light = emittedLightAmount });
                                }
                            }
                        }
                        k++;
                        num3++;
                    }
                    TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(num - 1, num2);
                    TerrainChunk chunkAtCell2 = m_terrain.GetChunkAtCell(num + 1, num2);
                    TerrainChunk chunkAtCell3 = m_terrain.GetChunkAtCell(num, num2 - 1);
                    TerrainChunk chunkAtCell4 = m_terrain.GetChunkAtCell(num, num2 + 1);
                    if (chunkAtCell != null
                        && chunkAtCell2 != null
                        && chunkAtCell3 != null
                        && chunkAtCell4 != null) {
                        int num4 = num - 1 - chunkAtCell.Origin.X;
                        int num5 = num2 - chunkAtCell.Origin.Y;
                        int num6 = num + 1 - chunkAtCell2.Origin.X;
                        int num7 = num2 - chunkAtCell2.Origin.Y;
                        int num8 = num - chunkAtCell3.Origin.X;
                        int num9 = num2 - 1 - chunkAtCell3.Origin.Y;
                        int num10 = num - chunkAtCell4.Origin.X;
                        int num11 = num2 + 1 - chunkAtCell4.Origin.Y;
                        int num12 = Terrain.ExtractSunlightHeight(chunkAtCell.GetShaftValueFast(num4, num5));
                        int num13 = Terrain.ExtractSunlightHeight(chunkAtCell2.GetShaftValueFast(num6, num7));
                        int num14 = Terrain.ExtractSunlightHeight(chunkAtCell3.GetShaftValueFast(num8, num9));
                        int num15 = Terrain.ExtractSunlightHeight(chunkAtCell4.GetShaftValueFast(num10, num11));
                        int num16 = MathUtils.Min(num12, num13, num14, num15);
                        int l = num16;
                        int num17 = TerrainChunk.CalculateCellIndex(i, num16, j);
                        while (l <= topHeightFast) {
                            int cellValueFast2 = chunk.GetCellValueFast(num17);
                            Block block2 = blocks[Terrain.ExtractContents(cellValueFast2)];
                            if (block2.IsTransparent) {
                                int cellLightFast = chunkAtCell.GetCellLightFast(num4, l, num5);
                                int cellLightFast2 = chunkAtCell2.GetCellLightFast(num6, l, num7);
                                int cellLightFast3 = chunkAtCell3.GetCellLightFast(num8, l, num9);
                                int cellLightFast4 = chunkAtCell4.GetCellLightFast(num10, l, num11);
                                int num18 = MathUtils.Max(cellLightFast, cellLightFast2, cellLightFast3, cellLightFast4) - 1 - block2.LightAttenuation;
                                if (num18 > Terrain.ExtractLight(cellValueFast2)) {
                                    chunk.SetCellValueFast(num17, Terrain.ReplaceLight(cellValueFast2, num18));
                                    if (num18 > 1) {
                                        m_lightSources.Add(new TerrainUpdater.LightSource { X = num, Y = l, Z = num2, Light = num18 });
                                    }
                                }
                            }
                            l++;
                            num17++;
                        }
                    }
                }
            }
        }

        public virtual void GenerateChunkEdgeLightSources(TerrainChunk chunk, int face) {
            Block[] blocks = BlocksManager.Blocks;
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            TerrainChunk terrainChunk;
            switch (face) {
                case 0:
                    terrainChunk = chunk.Terrain.GetChunkAtCoords(chunk.Coords.X, chunk.Coords.Y + 1);
                    num2 = 15;
                    num4 = 0;
                    break;
                case 1:
                    terrainChunk = chunk.Terrain.GetChunkAtCoords(chunk.Coords.X + 1, chunk.Coords.Y);
                    num = 15;
                    num3 = 0;
                    break;
                case 2:
                    terrainChunk = chunk.Terrain.GetChunkAtCoords(chunk.Coords.X, chunk.Coords.Y - 1);
                    num2 = 0;
                    num4 = 15;
                    break;
                default:
                    terrainChunk = chunk.Terrain.GetChunkAtCoords(chunk.Coords.X - 1, chunk.Coords.Y);
                    num = 0;
                    num3 = 15;
                    break;
            }
            if (terrainChunk == null
                || terrainChunk.ThreadState < TerrainChunkState.InvalidPropagatedLight) {
                return;
            }
            for (int i = 0; i < 16; i++) {
                switch (face) {
                    case 0:
                        num = i;
                        num3 = i;
                        break;
                    case 1:
                        num2 = i;
                        num4 = i;
                        break;
                    case 2:
                        num = i;
                        num3 = i;
                        break;
                    default:
                        num2 = i;
                        num4 = i;
                        break;
                }
                int num5 = num + chunk.Origin.X;
                int num6 = num2 + chunk.Origin.Y;
                int bottomHeightFast = chunk.GetBottomHeightFast(num, num2);
                int num7 = TerrainChunk.CalculateCellIndex(num, 0, num2);
                int num8 = TerrainChunk.CalculateCellIndex(num3, 0, num4);
                for (int j = bottomHeightFast; j < 256; j++) {
                    int cellValueFast = chunk.GetCellValueFast(num7 + j);
                    int num9 = Terrain.ExtractContents(cellValueFast);
                    if (blocks[num9].IsTransparent) {
                        int num10 = Terrain.ExtractLight(cellValueFast);
                        int num11 = Terrain.ExtractLight(terrainChunk.GetCellValueFast(num8 + j)) - 1;
                        if (num11 > num10) {
                            chunk.SetCellValueFast(num7 + j, Terrain.ReplaceLight(cellValueFast, num11));
                            if (num11 > 1) {
                                m_lightSources.Add(new TerrainUpdater.LightSource { X = num5, Y = j, Z = num6, Light = num11 });
                            }
                        }
                    }
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
            int num = Terrain.ExtractContents(cellValueFast);
            Block block = BlocksManager.Blocks[num];
            if (block.IsTransparent_(cellValueFast)) {
                int num2 = light - block.LightAttenuation - 1;
                if (num2 > Terrain.ExtractLight(cellValueFast)) {
                    m_lightSources.Add(new TerrainUpdater.LightSource { X = x, Y = y, Z = z, Light = num2 });
                    chunkAtCell.SetCellValueFast(index, Terrain.ReplaceLight(cellValueFast, num2));
                }
            }
        }

        public virtual void PropagateLightSources() {
            for (int i = 0; i < m_lightSources.Count && i < 120000; i++) {
                TerrainUpdater.LightSource lightSource = m_lightSources[i];
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
            for (int i = 0; i < m_lightSources.Count && i < 120000; i++) {
                TerrainUpdater.LightSource lightSource = m_lightSources[i];
                int light = lightSource.Light;
                int x = lightSource.X;
                int y = lightSource.Y;
                int z = lightSource.Z;
                int num2 = x & 15;
                int num3 = z & 15;
                TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(x, z);
                if (num2 == 0) {
                    PropagateLightSource(
                        m_terrain.GetChunkAtCell(x - 1, z),
                        x - 1,
                        y,
                        z,
                        light
                    );
                }
                else {
                    PropagateLightSource(
                        chunkAtCell,
                        x - 1,
                        y,
                        z,
                        light
                    );
                }
                if (num2 == 15) {
                    PropagateLightSource(
                        m_terrain.GetChunkAtCell(x + 1, z),
                        x + 1,
                        y,
                        z,
                        light
                    );
                }
                else {
                    PropagateLightSource(
                        chunkAtCell,
                        x + 1,
                        y,
                        z,
                        light
                    );
                }
                if (num3 == 0) {
                    PropagateLightSource(
                        m_terrain.GetChunkAtCell(x, z - 1),
                        x,
                        y,
                        z - 1,
                        light
                    );
                }
                else {
                    PropagateLightSource(
                        chunkAtCell,
                        x,
                        y,
                        z - 1,
                        light
                    );
                }
                if (num3 == 15) {
                    PropagateLightSource(
                        m_terrain.GetChunkAtCell(x, z + 1),
                        x,
                        y,
                        z + 1,
                        light
                    );
                }
                else {
                    PropagateLightSource(
                        chunkAtCell,
                        x,
                        y,
                        z + 1,
                        light
                    );
                }
                if (y > 0) {
                    PropagateLightSource(
                        chunkAtCell,
                        x,
                        y - 1,
                        z,
                        light
                    );
                }
                if (y < 255) {
                    PropagateLightSource(
                        chunkAtCell,
                        x,
                        y + 1,
                        z,
                        light
                    );
                }
            }
        }

        [MethodImpl(256)]
        void PropagateLightSource(TerrainChunk chunk, int x, int y, int z, int light) {
            if (chunk != null) {
                int num = TerrainChunk.CalculateCellIndex(x & 15, y, z & 15);
                int cellValueFast = chunk.GetCellValueFast(num);
                int num2 = Terrain.ExtractContents(cellValueFast);
                Block block = BlocksManager.Blocks[num2];
                if (block.IsTransparent) {
                    int num3 = light - block.LightAttenuation - 1;
                    if (num3 > Terrain.ExtractLight(cellValueFast)) {
                        if (num3 > 1) {
                            m_lightSources.Add(new TerrainUpdater.LightSource { X = x, Y = y, Z = z, Light = num3 });
                        }
                        chunk.SetCellValueFast(num, Terrain.ReplaceLight(cellValueFast, num3));
                    }
                }
            }
        }

        public void GenerateChunkVertices(TerrainChunk chunk, int stage) {
            m_subterrainSystem.BlockGeometryGenerator.ResetCache();
            if (!chunk.Draws.TryGetValue(m_subsystemAnimatedTextures.AnimatedBlocksTexture, out TerrainGeometry[] terrainGeometry)) {
                terrainGeometry = new TerrainGeometry[16];
                for (int i = 0; i < 16; i++) {
                    TerrainGeometry t = new(chunk.Draws, i);
                    terrainGeometry[i] = t;
                }
                chunk.Draws.Add(m_subsystemAnimatedTextures.AnimatedBlocksTexture, terrainGeometry);
            }
            TerrainChunk chunkAtCoords1 = m_terrain.GetChunkAtCoords(chunk.Coords.X - 1, chunk.Coords.Y - 1);
            TerrainChunk chunkAtCoords2 = m_terrain.GetChunkAtCoords(chunk.Coords.X, chunk.Coords.Y - 1);
            TerrainChunk chunkAtCoords3 = m_terrain.GetChunkAtCoords(chunk.Coords.X + 1, chunk.Coords.Y - 1);
            TerrainChunk chunkAtCoords4 = m_terrain.GetChunkAtCoords(chunk.Coords.X - 1, chunk.Coords.Y);
            TerrainChunk chunkAtCoords5 = m_terrain.GetChunkAtCoords(chunk.Coords.X + 1, chunk.Coords.Y);
            TerrainChunk chunkAtCoords6 = m_terrain.GetChunkAtCoords(chunk.Coords.X - 1, chunk.Coords.Y + 1);
            TerrainChunk chunkAtCoords7 = m_terrain.GetChunkAtCoords(chunk.Coords.X, chunk.Coords.Y + 1);
            TerrainChunk chunkAtCoords8 = m_terrain.GetChunkAtCoords(chunk.Coords.X + 1, chunk.Coords.Y + 1);
            int num1 = 0;
            int num2 = 0;
            int num3 = 16;
            int num4 = 16;
            if (chunkAtCoords4 == null) {
                ++num1;
            }
            if (chunkAtCoords2 == null) {
                ++num2;
            }
            if (chunkAtCoords5 == null) {
                --num3;
            }
            if (chunkAtCoords7 == null) {
                --num4;
            }
            for (int index = 0; index < 16; ++index) {
                if (index % 2 == stage) {
                    chunk.SliceContentsHashes[index] = CalculateChunkSliceContentsHash(chunk, index);
                    int generateHash = chunk.GeneratedSliceContentsHashes[index];
                    if (generateHash != 0
                        && generateHash == chunk.SliceContentsHashes[index]) {
                        continue;
                    }
                    foreach (KeyValuePair<Texture2D, TerrainGeometry[]> c in chunk.Draws) {
                        TerrainGeometrySubset[] subsets = c.Value[index].Subsets;
                        for (int p = 0; p < subsets.Length; p++) {
                            subsets[p].Vertices.Clear();
                            subsets[p].Indices.Clear();
                        }
                    }
                    for (int x1 = num1; x1 < num3; ++x1) {
                        for (int z1 = num2; z1 < num4; ++z1) {
                            switch (x1) {
                                case 0:
                                    if ((z1 == 0 && chunkAtCoords1 == null)
                                        || (z1 == 15 && chunkAtCoords6 == null)) {
                                        break;
                                    }
                                    goto default;
                                case 15:
                                    if ((z1 == 0 && chunkAtCoords3 == null)
                                        || (z1 == 15 && chunkAtCoords8 == null)) {
                                        break;
                                    }
                                    goto default;
                                default:
                                    int x2 = x1 + chunk.Origin.X;
                                    int z2 = z1 + chunk.Origin.Y;
                                    int x2_1 = MathUtils.Min(chunk.GetBottomHeightFast(x1, z1) - 1, MathUtils.Min(m_terrain.GetBottomHeight(x2 - 1, z2), m_terrain.GetBottomHeight(x2 + 1, z2), m_terrain.GetBottomHeight(x2, z2 - 1), m_terrain.GetBottomHeight(x2, z2 + 1)));
                                    int x2_2 = chunk.GetTopHeightFast(x1, z1) + 1;
                                    int num5 = MathUtils.Max(16 * index, x2_1, 1);
                                    int num6 = MathUtils.Min(16 * (index + 1), x2_2, byte.MaxValue);
                                    int cellIndex = TerrainChunk.CalculateCellIndex(x1, 0, z1);
                                    for (int y = num5; y < num6; ++y) {
                                        int cellValueFast = chunk.GetCellValueFast(cellIndex + y);
                                        int contents = Terrain.ExtractContents(cellValueFast);
                                        if (contents != 0) {
                                            BlocksManager.Blocks[contents]
                                            .GenerateTerrainVertices(
                                                m_subterrainSystem.BlockGeometryGenerator,
                                                terrainGeometry[index],
                                                cellValueFast,
                                                x2,
                                                y,
                                                z2
                                            );
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        void CalculateChunkSliceContentsHashes(TerrainChunk chunk) {
            int num = 1;
            num += m_terrain.SeasonTemperature;
            num *= 31;
            num += m_terrain.SeasonHumidity;
            num *= 31;
            TerrainChunkGeometry geometry = chunk.Geometry;
            for (int i = 0; i < 16; i++) {
                chunk.SliceContentsHashes[i] = num;
            }
            int num2 = chunk.Origin.X - 1;
            int num3 = chunk.Origin.X + 16 + 1;
            int num4 = chunk.Origin.Y - 1;
            int num5 = chunk.Origin.Y + 16 + 1;
            for (int j = num2; j < num3; j++) {
                for (int k = num4; k < num5; k++) {
                    TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(j, k);
                    if (chunkAtCell != null) {
                        int num6 = j & 15;
                        int num7 = k & 15;
                        int shaftValueFast = chunkAtCell.GetShaftValueFast(num6, num7);
                        int num8 = Terrain.ExtractTopHeight(shaftValueFast);
                        int num9 = Terrain.ExtractBottomHeight(shaftValueFast);
                        int num10 = num6 > 0 ? chunkAtCell.GetBottomHeightFast(num6 - 1, num7) : m_terrain.GetBottomHeight(j - 1, k);
                        int num11 = num7 > 0 ? chunkAtCell.GetBottomHeightFast(num6, num7 - 1) : m_terrain.GetBottomHeight(j, k - 1);
                        int num12 = num6 < 15 ? chunkAtCell.GetBottomHeightFast(num6 + 1, num7) : m_terrain.GetBottomHeight(j + 1, k);
                        int num13 = num7 < 15 ? chunkAtCell.GetBottomHeightFast(num6, num7 + 1) : m_terrain.GetBottomHeight(j, k + 1);
                        int num14 = MathUtils.Min(MathUtils.Min(num10, num11, num12, num13), num9 - 1);
                        int num15 = num8 + 2;
                        num14 = MathUtils.Max(num14, 0);
                        num15 = MathUtils.Min(num15, 256);
                        int num16 = MathUtils.Max((num14 - 1) / 16, 0);
                        int num17 = MathUtils.Min((num15 + 1) / 16, 15);
                        int num18 = 1;
                        num18 += Terrain.ExtractTemperature(shaftValueFast);
                        num18 *= 31;
                        num18 += Terrain.ExtractHumidity(shaftValueFast);
                        num18 *= 31;
                        for (int l = num16; l <= num17; l++) {
                            int num19 = num18;
                            int num20 = MathUtils.Max(l * 16 - 1, num14);
                            int num21 = MathUtils.Min(l * 16 + 16 + 1, num15);
                            int m = TerrainChunk.CalculateCellIndex(num6, num20, num7);
                            int num22 = m + num21 - num20;
                            while (m < num22) {
                                num19 += chunkAtCell.GetCellValueFast(m++);
                                num19 *= 31;
                            }
                            num19 += num20;
                            num19 *= 31;
                            chunk.SliceContentsHashes[l] += num19;
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
            int x = MathUtils.Max(16 * sliceIndex - 1, 0);
            int x2 = MathUtils.Min(16 * (sliceIndex + 1) + 1, 256);
            for (int i = num2; i < num3; i++) {
                for (int j = num4; j < num5; j++) {
                    TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(i, j);
                    if (chunkAtCell != null) {
                        int x3 = i & 0xF;
                        int z = j & 0xF;
                        int shaftValueFast = chunkAtCell.GetShaftValueFast(x3, z);
                        int num6 = Terrain.ExtractBottomHeight(shaftValueFast);
                        int num7 = Terrain.ExtractTopHeight(shaftValueFast);
                        int num8 = MathUtils.Max(x, num6 - 1);
                        int num9 = MathUtils.Min(x2, num7 + 2);
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