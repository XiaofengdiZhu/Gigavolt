using System;
using System.Collections.Generic;
using Engine;
using GameEntitySystem;

namespace Game {
    public class GVSubterrainSystem : IDisposable {
        public readonly SubsystemBlockBehaviors m_subsystemBlockBehaviors;
        public readonly SubsystemTerrain m_subsystemTerrain;
        public readonly SubsystemPickables m_subsystemPickables;
        public readonly SubsystemParticles m_subsystemParticles;
        public readonly SubsystemGameWidgets m_subsystemViews;
        public readonly SubsystemGVBlockBehaviors m_subsystemGVBlockBehaviors;
        public SubsystemGVElectricity m_subsystemGVElectricity;

        public readonly uint ID;
        public readonly GVSubterrainSystem Parent;
        public readonly Point3 Anchor;
        public readonly Dictionary<Point3, GVSubterrainSystem> Children = [];
        public readonly Terrain Terrain = new();
        public readonly GVSubterrainUpdater TerrainUpdater;
        public readonly GVSubterrainRenderer TerrainRenderer;
        public readonly GVBlockGeometryGenerator BlockGeometryGenerator;
        public bool m_hasDropped = false;

        public int m_light;
        public bool m_lightChanged;

        public int Light {
            get => m_light;
            set {
                if (value != m_light) {
                    m_light = value;
                    m_lightChanged = true;
                }
            }
        }

        public bool UseParentLight = true;
        public readonly Matrix AnchorTransform;
        public readonly Matrix OriginTransform;
        public Matrix m_baseTransform;

        public Matrix BaseTransform {
            get => m_baseTransform;
            set {
                if (value != m_baseTransform) {
                    m_baseTransform = value;
                    GlobalTransform = Parent == null ? OriginTransform * value * AnchorTransform : OriginTransform * value * AnchorTransform * Parent.GlobalTransform;
                    m_invertedBaseTransform = null;
                }
            }
        }

        public Matrix? m_invertedBaseTransform;

        public Matrix InvertedBaseTransform {
            get {
                m_invertedBaseTransform ??= Matrix.Invert(BaseTransform);
                return m_invertedBaseTransform.Value;
            }
        }

        public bool m_globalTransformChanged;
        public Matrix m_globalTransform;

        public Matrix GlobalTransform {
            get => m_globalTransform;
            set {
                if (value != m_globalTransform) {
                    m_globalTransform = value;
                    m_invertedGlobalTransform = null;
                    m_globalTransformChanged = true;
                }
            }
        }

        public Matrix? m_invertedGlobalTransform;

        public Matrix InvertedGlobalTransform {
            get {
                m_invertedGlobalTransform ??= Matrix.Invert(GlobalTransform);
                return m_invertedGlobalTransform.Value;
            }
        }

        public readonly Dictionary<Point3, bool> m_modifiedCells = [];
        public readonly DynamicArray<Point3> m_modifiedList = [];

        public GVSubterrainSystem(Project project, Matrix transform = default, Point3 anchor = default, Vector3 originOffset = default, GVSubterrainSystem parent = null, Point2 min = default, Point2 max = default, Dictionary<Point3, int> blocks = null) {
            m_subsystemBlockBehaviors = project.FindSubsystem<SubsystemBlockBehaviors>(true);
            m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemPickables = project.FindSubsystem<SubsystemPickables>(true);
            m_subsystemParticles = project.FindSubsystem<SubsystemParticles>(true);
            m_subsystemViews = project.FindSubsystem<SubsystemGameWidgets>(true);
            m_subsystemGVBlockBehaviors = project.FindSubsystem<SubsystemGVBlockBehaviors>(true);
            m_subsystemGVElectricity = project.FindSubsystem<SubsystemGVElectricity>(true);
            Terrain.SeasonHumidity = 12;
            Terrain.SeasonTemperature = 12;
            TerrainUpdater = new GVSubterrainUpdater(this, project);
            TerrainRenderer = new GVSubterrainRenderer(this, project);
            BlockGeometryGenerator = new GVBlockGeometryGenerator(
                Terrain,
                null,
                null,
                project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true),
                null,
                project.FindSubsystem<SubsystemPalette>(true),
                project.FindSubsystem<SubsystemGVElectricity>(true),
                this
            );
            ID = GVStaticStorage.GetUniqueGVMBID();
            GVStaticStorage.GVSubterrainSystemDictionary.Add(ID, this);
            m_subsystemGVElectricity.AddSubterrain(ID);
            Anchor = anchor;
            AnchorTransform = Matrix.CreateTranslation(anchor.X + 0.5f, anchor.Y + 0.5f, anchor.Z + 0.5f);
            OriginTransform = Matrix.CreateTranslation(originOffset);
            if (parent != null
                && !parent.Children.ContainsKey(anchor)) {
                Parent = parent;
                parent.Children.Add(anchor, this);
            }
            BaseTransform = transform == default ? Matrix.Identity : transform;
            for (int k = min.X >> 4; k <= max.X >> 4; k++) {
                for (int l = min.Y >> 4; l <= max.Y >> 4; l++) {
                    TerrainUpdater.DowngradeChunkNeighborhoodState(new Point2(k, l), 0, TerrainChunkState.NotLoaded, false);
                    TerrainUpdater.DowngradeChunkNeighborhoodState(new Point2(k, l), 1, TerrainChunkState.InvalidLight, false);
                    TerrainUpdater.m_shouldUpdateChunks = true;
                }
            }
            if (blocks != null) {
                foreach (KeyValuePair<Point3, int> pair in blocks) {
                    GenerateCell(pair.Key.X, pair.Key.Y, pair.Key.Z, pair.Value);
                }
            }
        }

        public Vector3 Subterrain2Terrain(Vector3 position) => Vector3.Transform(position, GlobalTransform);
        public Vector3 Subterrain2Terrain(Point3 position) => Subterrain2Terrain(new Vector3(position));
        public Point3 Subterrain2TerrainPoint(Vector3 position) => Terrain.ToCell(Subterrain2Terrain(position));
        public Point3 Subterrain2TerrainPoint(Point3 position) => Subterrain2TerrainPoint(new Vector3(position));

        public Vector3 Terrain2Subterrain(Vector3 position) => Vector3.Transform(position, InvertedGlobalTransform);
        public Vector3 Terrain2Subterrain(Point3 position) => Terrain2Subterrain(new Vector3(position));
        public Point3 Terrain2SubterrainPoint(Vector3 position) => Terrain.ToCell(Terrain2Subterrain(position));
        public Point3 Terrain2SubterrainPoint(Point3 position) => Terrain2SubterrainPoint(new Vector3(position));

        public void ArrangeChildrenTransform() {
            foreach (GVSubterrainSystem child in Children.Values) {
                child.GlobalTransform = child.OriginTransform * child.BaseTransform * child.AnchorTransform * GlobalTransform;
            }
        }

        public void Update() {
            if (m_hasDropped) {
                return;
            }
            if (UseParentLight) {
                Light = Parent?.Light ?? m_subsystemTerrain.Terrain.GetCellLight(Anchor.X, Anchor.Y, Anchor.Z);
            }
            TerrainUpdater.Update(m_lightChanged);
            m_lightChanged = false;
            ProcessModifiedCells();
            if (m_globalTransformChanged) {
                ArrangeChildrenTransform();
                m_globalTransformChanged = false;
            }
            foreach (GVSubterrainSystem child in Children.Values) {
                child.Update();
            }
        }

        public void Draw(Camera camera, int drawOrder) {
            if (m_hasDropped) {
                return;
            }
            switch (drawOrder) {
                case 0:
                    TerrainRenderer.PrepareForDrawing(camera);
                    TerrainRenderer.DrawOpaqueAndAlphaTested(camera);
                    break;
                case 100:
                    TerrainRenderer.DrawTransparent(camera);
                    break;
            }
            foreach (GVSubterrainSystem child in Children.Values) {
                child.Draw(camera, drawOrder);
            }
        }

        public void GenerateCell(int x, int y, int z, int value) {
            if (!Terrain.IsCellValid(x, y, z)) {
                return;
            }
            int chunkX = x >> 4;
            int chunkZ = z >> 4;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCoords(chunkX, chunkZ);
            if (chunkAtCell == null) {
                return;
            }
            value = Terrain.ReplaceLight(value, 0);
            Terrain.SetCellValueFast(x, y, z, value);
        }

        public void ChangeCell(int x, int y, int z, int value, bool updateModificationCounter = true) {
            if (!Terrain.IsCellValid(x, y, z)) {
                return;
            }
            int chunkX = x >> 4;
            int chunkZ = z >> 4;
            TerrainChunk chunkAtCell = Terrain.GetChunkAtCoords(chunkX, chunkZ);
            if (chunkAtCell == null) {
                TerrainUpdater.m_shouldUpdateChunks = true;
                chunkAtCell = Terrain.AllocateChunk(chunkX, chunkZ);
            }
            int cellValueFast = Terrain.GetCellValueFast(x, y, z);
            value = Terrain.ReplaceLight(value, 0);
            cellValueFast = Terrain.ReplaceLight(cellValueFast, 0);
            if (value == cellValueFast) {
                return;
            }
            Terrain.SetCellValueFast(x, y, z, value);
            if (updateModificationCounter) {
                chunkAtCell.ModificationCounter++;
            }
            TerrainUpdater.DowngradeChunkNeighborhoodState(chunkAtCell.Coords, 1, TerrainChunkState.InvalidLight, false);
            m_modifiedCells[new Point3(x, y, z)] = true;
            int num = Terrain.ExtractContents(cellValueFast);
            int num2 = Terrain.ExtractContents(value);
            if (num2 != num) {
                List<IGVBlockBehavior> blockBehaviors = m_subsystemGVBlockBehaviors.GetBlockBehaviors(num);
                foreach (IGVBlockBehavior behavior in blockBehaviors) {
                    behavior.OnBlockRemoved(
                        cellValueFast,
                        value,
                        x,
                        y,
                        z,
                        this
                    );
                }
                List<IGVBlockBehavior> blockBehaviors2 = m_subsystemGVBlockBehaviors.GetBlockBehaviors(num2);
                foreach (IGVBlockBehavior behavior in blockBehaviors2) {
                    behavior.OnBlockAdded(
                        value,
                        cellValueFast,
                        x,
                        y,
                        z,
                        this
                    );
                }
            }
            else {
                List<IGVBlockBehavior> blockBehaviors3 = m_subsystemGVBlockBehaviors.GetBlockBehaviors(num2);
                foreach (IGVBlockBehavior behavior in blockBehaviors3) {
                    behavior.OnBlockModified(
                        value,
                        cellValueFast,
                        x,
                        y,
                        z,
                        this
                    );
                }
            }
        }

        public void DestroyCell(int toolLevel, int x, int y, int z, int newValue, bool noDrop, bool noParticleSystem) {
            int cellValue = Terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            Block block = BlocksManager.Blocks[num];
            if (num != 0) {
                bool showDebris = true;
                Vector3 terrainPosition = Subterrain2Terrain(new Vector3(x, y, z));
                if (!noDrop) {
                    List<BlockDropValue> m_dropValues = [];
                    block.GetDropValues(
                        m_subsystemTerrain,
                        cellValue,
                        newValue,
                        toolLevel,
                        m_dropValues,
                        out showDebris
                    );
                    for (int i = 0; i < m_dropValues.Count; i++) {
                        BlockDropValue dropValue = m_dropValues[i];
                        if (dropValue.Count > 0) {
                            SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(dropValue.Value));
                            for (int j = 0; j < blockBehaviors.Length; j++) {
                                blockBehaviors[j]
                                .OnItemHarvested(
                                    (int)Math.Ceiling(terrainPosition.X),
                                    (int)Math.Ceiling(terrainPosition.Y),
                                    (int)Math.Ceiling(terrainPosition.Z),
                                    cellValue,
                                    ref dropValue,
                                    ref newValue
                                );
                            }
                            if (dropValue.Count > 0
                                && Terrain.ExtractContents(dropValue.Value) != 0) {
                                m_subsystemPickables.AddPickable(
                                    dropValue.Value,
                                    dropValue.Count,
                                    terrainPosition + new Vector3(0.5f),
                                    null,
                                    null
                                );
                            }
                        }
                    }
                }
                if (showDebris
                    && !noParticleSystem
                    && m_subsystemViews.CalculateDistanceFromNearestView(terrainPosition) < 16f) {
                    m_subsystemParticles.AddParticleSystem(block.CreateDebrisParticleSystem(m_subsystemTerrain, terrainPosition + new Vector3(0.5f), cellValue, 1f));
                }
            }
            ChangeCell(x, y, z, newValue);
        }

        public void ProcessModifiedCells() {
            m_modifiedList.Clear();
            foreach (Point3 key in m_modifiedCells.Keys) {
                m_modifiedList.Add(key);
            }
            m_modifiedCells.Clear();
            for (int i = 0; i < m_modifiedList.Count; i++) {
                Point3 point = m_modifiedList.Array[i];
                for (int j = 0; j < SubsystemTerrain.m_neighborOffsets.Length; j++) {
                    Point3 point2 = SubsystemTerrain.m_neighborOffsets[j];
                    int cellValue = Terrain.GetCellValue(point.X + point2.X, point.Y + point2.Y, point.Z + point2.Z);
                    List<IGVBlockBehavior> blockBehaviors = m_subsystemGVBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(cellValue));
                    foreach (IGVBlockBehavior behavior in blockBehaviors) {
                        behavior.OnNeighborBlockChanged(
                            point.X + point2.X,
                            point.Y + point2.Y,
                            point.Z + point2.Z,
                            point.X,
                            point.Y,
                            point.Z,
                            this
                        );
                    }
                }
            }
        }

        public void Dispose() => Dispose(true);

        public void Dispose(bool removeSelfFromParent) {
            foreach (TerrainChunk terrainChunk in Terrain.m_allocatedChunks) {
                for (int x = 0; x < 16; x++) {
                    for (int z = 0; z < 16; z++) {
                        for (int y = 0; y < 256; y++) {
                            int cellValue = terrainChunk.GetCellValueFast(x, y, z);
                            List<IGVBlockBehavior> blockBehaviors = m_subsystemGVBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(cellValue));
                            foreach (IGVBlockBehavior behavior in blockBehaviors) {
                                behavior.OnBlockRemoved(
                                    cellValue,
                                    0,
                                    x + (terrainChunk.Coords.X << 4),
                                    y,
                                    z + (terrainChunk.Coords.Y << 4),
                                    this
                                );
                            }
                        }
                    }
                }
            }
            GVStaticStorage.GVSubterrainSystemDictionary.Remove(ID);
            m_subsystemGVElectricity.RemoveSubterrain(ID);
            TerrainRenderer.Dispose();
            TerrainUpdater.Dispose();
            Terrain.Dispose();
            foreach (GVSubterrainSystem child in Children.Values) {
                child.Dispose(false);
            }
            if (removeSelfFromParent && Parent != null) {
                Parent.Children.Remove(Anchor);
            }
        }

        public static bool IsBlockAllowedForSubterrain(int contents) => contents switch {
            AirBlock.Index or BedrockBlock.Index or FireBlock.Index => false,
            _ => true
        };
    }
}