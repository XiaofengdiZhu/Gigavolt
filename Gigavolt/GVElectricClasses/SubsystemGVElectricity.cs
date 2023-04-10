using Engine;
using Engine.Input;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TemplatesDatabase;

namespace Game
{
    public class SubsystemGVElectricity : Subsystem, IUpdateable
    {
        public static GVElectricConnectionPath[] m_connectionPathsTable = new GVElectricConnectionPath[120]
        {
            new GVElectricConnectionPath(0, 1, -1, 4, 4, 0),
            new GVElectricConnectionPath(0, 1, 0, 0, 4, 5),
            new GVElectricConnectionPath(0, 1, -1, 2, 4, 5),
            new GVElectricConnectionPath(0, 0, 0, 5, 4, 2),
            new GVElectricConnectionPath(-1, 0, -1, 3, 3, 0),
            new GVElectricConnectionPath(-1, 0, 0, 0, 3, 1),
            new GVElectricConnectionPath(-1, 0, -1, 2, 3, 1),
            new GVElectricConnectionPath(0, 0, 0, 1, 3, 2),
            new GVElectricConnectionPath(0, -1, -1, 5, 5, 0),
            new GVElectricConnectionPath(0, -1, 0, 0, 5, 4),
            new GVElectricConnectionPath(0, -1, -1, 2, 5, 4),
            new GVElectricConnectionPath(0, 0, 0, 4, 5, 2),
            new GVElectricConnectionPath(1, 0, -1, 1, 1, 0),
            new GVElectricConnectionPath(1, 0, 0, 0, 1, 3),
            new GVElectricConnectionPath(1, 0, -1, 2, 1, 3),
            new GVElectricConnectionPath(0, 0, 0, 3, 1, 2),
            new GVElectricConnectionPath(0, 0, -1, 2, 2, 0),
            null,
            null,
            null,
            new GVElectricConnectionPath(-1, 1, 0, 4, 4, 1),
            new GVElectricConnectionPath(0, 1, 0, 1, 4, 5),
            new GVElectricConnectionPath(-1, 1, 0, 3, 4, 5),
            new GVElectricConnectionPath(0, 0, 0, 5, 4, 3),
            new GVElectricConnectionPath(-1, 0, 1, 0, 0, 1),
            new GVElectricConnectionPath(0, 0, 1, 1, 0, 2),
            new GVElectricConnectionPath(-1, 0, 1, 3, 0, 2),
            new GVElectricConnectionPath(0, 0, 0, 2, 0, 3),
            new GVElectricConnectionPath(-1, -1, 0, 5, 5, 1),
            new GVElectricConnectionPath(0, -1, 0, 1, 5, 4),
            new GVElectricConnectionPath(-1, -1, 0, 3, 5, 4),
            new GVElectricConnectionPath(0, 0, 0, 4, 5, 3),
            new GVElectricConnectionPath(-1, 0, -1, 2, 2, 1),
            new GVElectricConnectionPath(0, 0, -1, 1, 2, 0),
            new GVElectricConnectionPath(-1, 0, -1, 3, 2, 0),
            new GVElectricConnectionPath(0, 0, 0, 0, 2, 3),
            new GVElectricConnectionPath(-1, 0, 0, 3, 3, 1),
            null,
            null,
            null,
            new GVElectricConnectionPath(0, 1, 1, 4, 4, 2),
            new GVElectricConnectionPath(0, 1, 0, 2, 4, 5),
            new GVElectricConnectionPath(0, 1, 1, 0, 4, 5),
            new GVElectricConnectionPath(0, 0, 0, 5, 4, 0),
            new GVElectricConnectionPath(1, 0, 1, 1, 1, 2),
            new GVElectricConnectionPath(1, 0, 0, 2, 1, 3),
            new GVElectricConnectionPath(1, 0, 1, 0, 1, 3),
            new GVElectricConnectionPath(0, 0, 0, 3, 1, 0),
            new GVElectricConnectionPath(0, -1, 1, 5, 5, 2),
            new GVElectricConnectionPath(0, -1, 0, 2, 5, 4),
            new GVElectricConnectionPath(0, -1, 1, 0, 5, 4),
            new GVElectricConnectionPath(0, 0, 0, 4, 5, 0),
            new GVElectricConnectionPath(-1, 0, 1, 3, 3, 2),
            new GVElectricConnectionPath(-1, 0, 0, 2, 3, 1),
            new GVElectricConnectionPath(-1, 0, 1, 0, 3, 1),
            new GVElectricConnectionPath(0, 0, 0, 1, 3, 0),
            new GVElectricConnectionPath(0, 0, 1, 0, 0, 2),
            null,
            null,
            null,
            new GVElectricConnectionPath(1, 1, 0, 4, 4, 3),
            new GVElectricConnectionPath(0, 1, 0, 3, 4, 5),
            new GVElectricConnectionPath(1, 1, 0, 1, 4, 5),
            new GVElectricConnectionPath(0, 0, 0, 5, 4, 1),
            new GVElectricConnectionPath(1, 0, -1, 2, 2, 3),
            new GVElectricConnectionPath(0, 0, -1, 3, 2, 0),
            new GVElectricConnectionPath(1, 0, -1, 1, 2, 0),
            new GVElectricConnectionPath(0, 0, 0, 0, 2, 1),
            new GVElectricConnectionPath(1, -1, 0, 5, 5, 3),
            new GVElectricConnectionPath(0, -1, 0, 3, 5, 4),
            new GVElectricConnectionPath(1, -1, 0, 1, 5, 4),
            new GVElectricConnectionPath(0, 0, 0, 4, 5, 1),
            new GVElectricConnectionPath(1, 0, 1, 0, 0, 3),
            new GVElectricConnectionPath(0, 0, 1, 3, 0, 2),
            new GVElectricConnectionPath(1, 0, 1, 1, 0, 2),
            new GVElectricConnectionPath(0, 0, 0, 2, 0, 1),
            new GVElectricConnectionPath(1, 0, 0, 1, 1, 3),
            null,
            null,
            null,
            new GVElectricConnectionPath(0, -1, -1, 2, 2, 4),
            new GVElectricConnectionPath(0, 0, -1, 4, 2, 0),
            new GVElectricConnectionPath(0, -1, -1, 5, 2, 0),
            new GVElectricConnectionPath(0, 0, 0, 0, 2, 5),
            new GVElectricConnectionPath(-1, -1, 0, 3, 3, 4),
            new GVElectricConnectionPath(-1, 0, 0, 4, 3, 1),
            new GVElectricConnectionPath(-1, -1, 0, 5, 3, 1),
            new GVElectricConnectionPath(0, 0, 0, 1, 3, 5),
            new GVElectricConnectionPath(0, -1, 1, 0, 0, 4),
            new GVElectricConnectionPath(0, 0, 1, 4, 0, 2),
            new GVElectricConnectionPath(0, -1, 1, 5, 0, 2),
            new GVElectricConnectionPath(0, 0, 0, 2, 0, 5),
            new GVElectricConnectionPath(1, -1, 0, 1, 1, 4),
            new GVElectricConnectionPath(1, 0, 0, 4, 1, 3),
            new GVElectricConnectionPath(1, -1, 0, 5, 1, 3),
            new GVElectricConnectionPath(0, 0, 0, 3, 1, 5),
            new GVElectricConnectionPath(0, -1, 0, 5, 5, 4),
            null,
            null,
            null,
            new GVElectricConnectionPath(0, 1, -1, 2, 2, 5),
            new GVElectricConnectionPath(0, 0, -1, 5, 2, 0),
            new GVElectricConnectionPath(0, 1, -1, 4, 2, 0),
            new GVElectricConnectionPath(0, 0, 0, 0, 2, 4),
            new GVElectricConnectionPath(1, 1, 0, 1, 1, 5),
            new GVElectricConnectionPath(1, 0, 0, 5, 1, 3),
            new GVElectricConnectionPath(1, 1, 0, 4, 1, 3),
            new GVElectricConnectionPath(0, 0, 0, 3, 1, 4),
            new GVElectricConnectionPath(0, 1, 1, 0, 0, 5),
            new GVElectricConnectionPath(0, 0, 1, 5, 0, 2),
            new GVElectricConnectionPath(0, 1, 1, 4, 0, 2),
            new GVElectricConnectionPath(0, 0, 0, 2, 0, 4),
            new GVElectricConnectionPath(-1, 1, 0, 3, 3, 5),
            new GVElectricConnectionPath(-1, 0, 0, 5, 3, 1),
            new GVElectricConnectionPath(-1, 1, 0, 4, 3, 1),
            new GVElectricConnectionPath(0, 0, 0, 1, 3, 4),
            new GVElectricConnectionPath(0, 1, 0, 4, 4, 5),
            null,
            null,
            null
        };

        public static GVElectricConnectorDirection?[] m_connectorDirectionsTable = new GVElectricConnectorDirection?[36]
        {
            null,
            GVElectricConnectorDirection.Right,
            GVElectricConnectorDirection.In,
            GVElectricConnectorDirection.Left,
            GVElectricConnectorDirection.Top,
            GVElectricConnectorDirection.Bottom,
            GVElectricConnectorDirection.Left,
            null,
            GVElectricConnectorDirection.Right,
            GVElectricConnectorDirection.In,
            GVElectricConnectorDirection.Top,
            GVElectricConnectorDirection.Bottom,
            GVElectricConnectorDirection.In,
            GVElectricConnectorDirection.Left,
            null,
            GVElectricConnectorDirection.Right,
            GVElectricConnectorDirection.Top,
            GVElectricConnectorDirection.Bottom,
            GVElectricConnectorDirection.Right,
            GVElectricConnectorDirection.In,
            GVElectricConnectorDirection.Left,
            null,
            GVElectricConnectorDirection.Top,
            GVElectricConnectorDirection.Bottom,
            GVElectricConnectorDirection.Bottom,
            GVElectricConnectorDirection.Right,
            GVElectricConnectorDirection.Top,
            GVElectricConnectorDirection.Left,
            null,
            GVElectricConnectorDirection.In,
            GVElectricConnectorDirection.Top,
            GVElectricConnectorDirection.Right,
            GVElectricConnectorDirection.Bottom,
            GVElectricConnectorDirection.Left,
            GVElectricConnectorDirection.In,
            null
        };

        public static int[] m_connectorFacesTable = new int[30]
        {
            4,
            3,
            5,
            1,
            2,
            4,
            0,
            5,
            2,
            3,
            4,
            1,
            5,
            3,
            0,
            4,
            2,
            5,
            0,
            1,
            2,
            1,
            0,
            3,
            5,
            0,
            1,
            2,
            3,
            4
        };

        public float m_remainingSimulationTime;

        public Dictionary<Point3, uint> m_persistentElementsVoltages = new Dictionary<Point3, uint>();

        public Dictionary<GVElectricElement, bool> m_GVElectricElements = new Dictionary<GVElectricElement, bool>();

        public Dictionary<CellFace, GVElectricElement> m_GVElectricElementsByCellFace = new Dictionary<CellFace, GVElectricElement>();

        public Dictionary<Point3, bool> m_pointsToUpdate = new Dictionary<Point3, bool>();

        public Dictionary<Point3, GVElectricElement> m_GVElectricElementsToAdd = new Dictionary<Point3, GVElectricElement>();

        public Dictionary<GVElectricElement, bool> m_GVElectricElementsToRemove = new Dictionary<GVElectricElement, bool>();

        public Dictionary<Point3, bool> m_wiresToUpdate = new Dictionary<Point3, bool>();

        public List<Dictionary<GVElectricElement, bool>> m_listsCache = new List<Dictionary<GVElectricElement, bool>>();

        public Dictionary<int, Dictionary<GVElectricElement, bool>> m_futureSimulateLists = new Dictionary<int, Dictionary<GVElectricElement, bool>>();

        public Dictionary<GVElectricElement, bool> m_nextStepSimulateList;

        public DynamicArray<GVElectricConnectionPath> m_tmpConnectionPaths = new DynamicArray<GVElectricConnectionPath>();

        public Dictionary<CellFace, bool> m_tmpVisited = new Dictionary<CellFace, bool>();

        public Dictionary<CellFace, bool> m_tmpResult = new Dictionary<CellFace, bool>();

        public static bool DebugDrawGVElectrics = false;

        public static int SimulatedGVElectricElements;

        public float CircuitStepDuration = 0.01f;

        public bool debugMode = false;
        public bool keyboardDebug = false;
        public Dictionary<ComponentPlayer, GVStepFloatingButtons> m_debugButtonsDictionary = new Dictionary<ComponentPlayer, GVStepFloatingButtons>();
        public Queue<DateTime> last1000Updates = new Queue<DateTime>(1002);

        public SubsystemTime SubsystemTime
        {
            get;
            set;
        }

        public SubsystemTerrain SubsystemTerrain
        {
            get;
            set;
        }

        public SubsystemAudio SubsystemAudio
        {
            get;
            set;
        }

        public int FrameStartCircuitStep
        {
            get;
            set;
        }

        public int CircuitStep
        {
            get;
            set;
        }

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public void OnGVElectricElementBlockGenerated(int x, int y, int z)
        {
            m_pointsToUpdate[new Point3(x, y, z)] = false;
        }

        public void OnGVElectricElementBlockAdded(int x, int y, int z)
        {
            m_pointsToUpdate[new Point3(x, y, z)] = true;
        }

        public void OnGVElectricElementBlockRemoved(int x, int y, int z)
        {
            m_pointsToUpdate[new Point3(x, y, z)] = true;
        }

        public void OnGVElectricElementBlockModified(int x, int y, int z)
        {
            m_pointsToUpdate[new Point3(x, y, z)] = true;
        }

        public void OnChunkDiscarding(TerrainChunk chunk)
        {
            foreach (CellFace key in m_GVElectricElementsByCellFace.Keys)
            {
                if (key.X >= chunk.Origin.X && key.X < chunk.Origin.X + 16 && key.Z >= chunk.Origin.Y && key.Z < chunk.Origin.Y + 16)
                {
                    m_pointsToUpdate[new Point3(key.X, key.Y, key.Z)] = false;
                }
            }
        }

        public static GVElectricConnectorDirection? GetConnectorDirection(int mountingFace, int rotation, int connectorFace)
        {
            GVElectricConnectorDirection? result = m_connectorDirectionsTable[6 * mountingFace + connectorFace];
            if (result.HasValue)
            {
                if (result.Value < GVElectricConnectorDirection.In)
                {
                    return (GVElectricConnectorDirection)((int)(result.Value + rotation) % 4);
                }
                return result;
            }
            return null;
        }

        public static int GetConnectorFace(int mountingFace, GVElectricConnectorDirection connectionDirection)
        {
            return m_connectorFacesTable[(int)(5 * mountingFace + connectionDirection)];
        }

        public void GetAllConnectedNeighbors(int x, int y, int z, int mountingFace, DynamicArray<GVElectricConnectionPath> list)
        {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            var GVElectricElementBlock = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)] as IGVElectricElementBlock;
            if (GVElectricElementBlock == null)
            {
                return;
            }
            for (GVElectricConnectorDirection GVElectricConnectorDirection = GVElectricConnectorDirection.Top; GVElectricConnectorDirection < (GVElectricConnectorDirection)5; GVElectricConnectorDirection++)
            {
                for (int i = 0; i < 4; i++)
                {
                    GVElectricConnectionPath GVElectricConnectionPath = m_connectionPathsTable[20 * mountingFace + 4 * (int)GVElectricConnectorDirection + i];
                    if (GVElectricConnectionPath == null)
                    {
                        break;
                    }
                    GVElectricConnectorType? connectorType = GVElectricElementBlock.GetConnectorType(SubsystemTerrain, cellValue, mountingFace, GVElectricConnectionPath.ConnectorFace, x, y, z);
                    if (!connectorType.HasValue)
                    {
                        break;
                    }
                    int x2 = x + GVElectricConnectionPath.NeighborOffsetX;
                    int y2 = y + GVElectricConnectionPath.NeighborOffsetY;
                    int z2 = z + GVElectricConnectionPath.NeighborOffsetZ;
                    int cellValue2 = SubsystemTerrain.Terrain.GetCellValue(x2, y2, z2);
                    var GVElectricElementBlock2 = BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)] as IGVElectricElementBlock;
                    if (GVElectricElementBlock2 == null)
                    {
                        continue;
                    }
                    GVElectricConnectorType? connectorType2 = GVElectricElementBlock2.GetConnectorType(SubsystemTerrain, cellValue2, GVElectricConnectionPath.NeighborFace, GVElectricConnectionPath.NeighborConnectorFace, x2, y2, z2);
                    if (connectorType2.HasValue && ((connectorType.Value != 0 && connectorType2.Value != GVElectricConnectorType.Output) || (connectorType.Value != GVElectricConnectorType.Output && connectorType2.Value != 0)))
                    {
                        int connectionMask = GVElectricElementBlock.GetConnectionMask(cellValue);
                        int connectionMask2 = GVElectricElementBlock2.GetConnectionMask(cellValue2);
                        if ((connectionMask & connectionMask2) != 0)
                        {
                            list.Add(GVElectricConnectionPath);
                        }
                    }
                }
            }
        }

        public GVElectricElement GetGVElectricElement(int x, int y, int z, int mountingFace)
        {
            m_GVElectricElementsByCellFace.TryGetValue(new CellFace(x, y, z, mountingFace), out GVElectricElement value);
            return value;
        }

        public void QueueGVElectricElementForSimulation(GVElectricElement GVElectricElement, int circuitStep)
        {
            if (circuitStep == CircuitStep + 1)
            {
                if (m_nextStepSimulateList == null && !m_futureSimulateLists.TryGetValue(CircuitStep + 1, out m_nextStepSimulateList))
                {
                    m_nextStepSimulateList = GetListFromCache();
                    m_futureSimulateLists.Add(CircuitStep + 1, m_nextStepSimulateList);
                }
                m_nextStepSimulateList[GVElectricElement] = true;
            }
            else if (circuitStep > CircuitStep + 1)
            {
                if (!m_futureSimulateLists.TryGetValue(circuitStep, out Dictionary<GVElectricElement, bool> value))
                {
                    value = GetListFromCache();
                    m_futureSimulateLists.Add(circuitStep, value);
                }
                value[GVElectricElement] = true;
            }
        }

        public void QueueGVElectricElementConnectionsForSimulation(GVElectricElement GVElectricElement, int circuitStep)
        {
            foreach (GVElectricConnection connection in GVElectricElement.Connections)
            {
                if (connection.ConnectorType != 0 && connection.NeighborConnectorType != GVElectricConnectorType.Output)
                {
                    QueueGVElectricElementForSimulation(connection.NeighborGVElectricElement, circuitStep);
                }
            }
        }

        public uint? ReadPersistentVoltage(Point3 point)
        {
            if (m_persistentElementsVoltages.TryGetValue(point, out uint value))
            {
                return value;
            }
            return null;
        }

        public void WritePersistentVoltage(Point3 point, uint voltage)
        {
            m_persistentElementsVoltages[point] = voltage;
        }

        public void Update(float dt)
        {
            if (keyboardDebug)
            {
                if (Keyboard.IsKeyDownOnce(Key.F5))
                {
                    debugMode = !debugMode;
                }
                if(debugMode)
                {
                    if (Keyboard.IsKeyDownOnce(Key.F7))
                    {
                        JumpUpdate();
                    }
                    else if (Keyboard.IsKeyDownOnce(Key.F6))
                    {
                        StepUpdate();
                    }
                }
            }
            if (!debugMode)
            {
                StepUpdateSkip();
                FrameStartCircuitStep = CircuitStep;
                SimulatedGVElectricElements = 0;
                m_remainingSimulationTime = MathUtils.Min(m_remainingSimulationTime + dt, 0.1f);
                while (m_remainingSimulationTime >= CircuitStepDuration)
                {
                    UpdateGVElectricElements();
                    m_remainingSimulationTime -= CircuitStepDuration;
                    m_nextStepSimulateList = null;
                    if (m_futureSimulateLists.TryGetValue(++CircuitStep, out Dictionary<GVElectricElement, bool> value))
                    {
                        m_futureSimulateLists.Remove(CircuitStep);
                        SimulatedGVElectricElements += value.Count;
                        foreach (GVElectricElement key in value.Keys)
                        {
                            if (m_GVElectricElements.ContainsKey(key))
                            {
                                SimulateGVElectricElement(key);
                            }
                        }
                        ReturnListToCache(value);
                    }
                    while (last1000Updates.Count >= 1000)
                    {
                        last1000Updates.Dequeue();
                    }
                    last1000Updates.Enqueue(DateTime.Now);
                }
            }
            if (DebugDrawGVElectrics)
            {
                DebugDraw();
            }
        }

        public void JumpUpdate()
        {
            if (debugMode)
            {
                StepUpdateSkip();
                FrameStartCircuitStep = CircuitStep;
                SimulatedGVElectricElements = 0;
                UpdateGVElectricElements();
                m_nextStepSimulateList = null;
                if (m_futureSimulateLists.TryGetValue(++CircuitStep, out Dictionary<GVElectricElement, bool> value))
                {
                    m_futureSimulateLists.Remove(CircuitStep);
                    SimulatedGVElectricElements += value.Count;
                    foreach (GVElectricElement key in value.Keys)
                    {
                        if (m_GVElectricElements.ContainsKey(key))
                        {
                            SimulateGVElectricElement(key);
                        }
                    }
                    ReturnListToCache(value);
                }
                while (last1000Updates.Count >= 1000)
                {
                    last1000Updates.Dequeue();
                }
                last1000Updates.Enqueue(DateTime.Now);
            }
        }
        public bool inStepping;
        public List<GVElectricElement> steppingElements;
        public int steppingIndex;
        Dictionary<GVElectricElement, bool> lastWhat;
        public void StepUpdate()
        {
            if(debugMode)
            {
                if (inStepping)
                {
                    StepUpdateRun();
                }
                else
                {
                    StepUpdateInitiate();
                }
            }
        }
        public void StepUpdateInitiate()
        {
            FrameStartCircuitStep = CircuitStep;
            SimulatedGVElectricElements = 0;
            UpdateGVElectricElements();
            m_nextStepSimulateList = null;
            if (m_futureSimulateLists.TryGetValue(++CircuitStep, out Dictionary<GVElectricElement, bool> value))
            {
                if (lastWhat != null) {
                    ReturnListToCache(lastWhat); 
                }
                m_futureSimulateLists.Remove(CircuitStep);
                SimulatedGVElectricElements += value.Count;
                steppingElements = value.Keys.ToList();
                steppingIndex = 0;
                if (steppingElements.Count > 0)
                {
                    inStepping = true;
                    StepUpdateRun();
                }
                lastWhat = value;
            }
        }
        public void StepUpdateRun()
        {
            GVElectricElement nowElement = steppingElements[steppingIndex++];
            if (m_GVElectricElements.ContainsKey(nowElement))
            {
                SimulateGVElectricElement(nowElement);
            }
            if (steppingElements.Count <= steppingIndex)
            {
                inStepping = false;
            }
        }
        public void StepUpdateSkip()
        {
            while (inStepping)
            {
                StepUpdate();
            }
        }
        public override void Load(ValuesDictionary valuesDictionary)
        {
            SubsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
            SubsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
            SubsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
            string[] array = valuesDictionary.GetValue<string>("GigaVoltagesByCell").Split(new char[1]
            {
                ';'
            }, StringSplitOptions.RemoveEmptyEntries);
            int num = 0;
            while (true)
            {
                if (num < array.Length)
                {
                    string[] array2 = array[num].Split(new string[] { "," }, StringSplitOptions.None);
                    if (array2.Length != 4)
                    {
                        break;
                    }
                    int x = int.Parse(array2[0], System.Globalization.NumberStyles.HexNumber, null);
                    int y = int.Parse(array2[1], System.Globalization.NumberStyles.HexNumber, null);
                    int z = int.Parse(array2[2], System.Globalization.NumberStyles.HexNumber, null);
                    uint value = uint.Parse(array2[3], System.Globalization.NumberStyles.HexNumber, null);
                    m_persistentElementsVoltages[new Point3(x, y, z)] = value;
                    num++;
                    continue;
                }
                return;
            }
            throw new InvalidOperationException("Invalid number of tokens.");
        }

        public override void Save(ValuesDictionary valuesDictionary)
        {
            int num = 0;
            var stringBuilder = new StringBuilder();
            foreach (KeyValuePair<Point3, uint> persistentElementsVoltage in m_persistentElementsVoltages)
            {
                if (num > 500)
                {
                    break;
                }
                stringBuilder.Append(persistentElementsVoltage.Key.X.ToString("X", null));
                stringBuilder.Append(',');
                stringBuilder.Append(persistentElementsVoltage.Key.Y.ToString("X", null));
                stringBuilder.Append(',');
                stringBuilder.Append(persistentElementsVoltage.Key.Z.ToString("X", null));
                stringBuilder.Append(',');
                stringBuilder.Append(persistentElementsVoltage.Value.ToString("X", null));
                stringBuilder.Append(';');
                num++;
            }
            valuesDictionary.SetValue("GigaVoltagesByCell", stringBuilder.ToString());
        }

        public static GVElectricConnectionPath GetConnectionPath(int mountingFace, GVElectricConnectorDirection localConnector, int neighborIndex)
        {
            return m_connectionPathsTable[16 * mountingFace + 4 * (int)localConnector + neighborIndex];
        }

        public void SimulateGVElectricElement(GVElectricElement GVElectricElement)
        {
            if (GVElectricElement.Simulate())
            {
                QueueGVElectricElementConnectionsForSimulation(GVElectricElement, CircuitStep + 1);
            }
        }

        public void AddGVElectricElement(GVElectricElement GVElectricElement)
        {
            m_GVElectricElements.Add(GVElectricElement, value: true);
            foreach (CellFace cellFace2 in GVElectricElement.CellFaces)
            {
                m_GVElectricElementsByCellFace.Add(cellFace2, GVElectricElement);
                m_tmpConnectionPaths.Clear();
                GetAllConnectedNeighbors(cellFace2.X, cellFace2.Y, cellFace2.Z, cellFace2.Face, m_tmpConnectionPaths);
                foreach (GVElectricConnectionPath tmpConnectionPath in m_tmpConnectionPaths)
                {
                    var cellFace = new CellFace(cellFace2.X + tmpConnectionPath.NeighborOffsetX, cellFace2.Y + tmpConnectionPath.NeighborOffsetY, cellFace2.Z + tmpConnectionPath.NeighborOffsetZ, tmpConnectionPath.NeighborFace);
                    if (m_GVElectricElementsByCellFace.TryGetValue(cellFace, out GVElectricElement value) && value != GVElectricElement)
                    {
                        int cellValue = SubsystemTerrain.Terrain.GetCellValue(cellFace2.X, cellFace2.Y, cellFace2.Z);
                        int num = Terrain.ExtractContents(cellValue);
                        GVElectricConnectorType value2 = ((IGVElectricElementBlock)BlocksManager.Blocks[num]).GetConnectorType(SubsystemTerrain, cellValue, cellFace2.Face, tmpConnectionPath.ConnectorFace, cellFace2.X, cellFace2.Y, cellFace2.Z).Value;
                        int cellValue2 = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
                        int num2 = Terrain.ExtractContents(cellValue2);
                        GVElectricConnectorType value3 = ((IGVElectricElementBlock)BlocksManager.Blocks[num2]).GetConnectorType(SubsystemTerrain, cellValue2, cellFace.Face, tmpConnectionPath.NeighborConnectorFace, cellFace.X, cellFace.Y, cellFace.Z).Value;
                        GVElectricElement.Connections.Add(new GVElectricConnection
                        {
                            CellFace = cellFace2,
                            ConnectorFace = tmpConnectionPath.ConnectorFace,
                            ConnectorType = value2,
                            NeighborGVElectricElement = value,
                            NeighborCellFace = cellFace,
                            NeighborConnectorFace = tmpConnectionPath.NeighborConnectorFace,
                            NeighborConnectorType = value3
                        });
                        value.Connections.Add(new GVElectricConnection
                        {
                            CellFace = cellFace,
                            ConnectorFace = tmpConnectionPath.NeighborConnectorFace,
                            ConnectorType = value3,
                            NeighborGVElectricElement = GVElectricElement,
                            NeighborCellFace = cellFace2,
                            NeighborConnectorFace = tmpConnectionPath.ConnectorFace,
                            NeighborConnectorType = value2
                        });
                    }
                }
            }
            QueueGVElectricElementForSimulation(GVElectricElement, CircuitStep + 1);
            QueueGVElectricElementConnectionsForSimulation(GVElectricElement, CircuitStep + 2);
            GVElectricElement.OnAdded();
        }

        public void RemoveGVElectricElement(GVElectricElement GVElectricElement)
        {
            GVElectricElement.OnRemoved();
            QueueGVElectricElementConnectionsForSimulation(GVElectricElement, CircuitStep + 1);
            m_GVElectricElements.Remove(GVElectricElement);
            foreach (CellFace cellFace in GVElectricElement.CellFaces)
            {
                m_GVElectricElementsByCellFace.Remove(cellFace);
            }
            foreach (GVElectricConnection connection in GVElectricElement.Connections)
            {
                int num = connection.NeighborGVElectricElement.Connections.FirstIndex((GVElectricConnection c) => c.NeighborGVElectricElement == GVElectricElement);
                if (num >= 0)
                {
                    connection.NeighborGVElectricElement.Connections.RemoveAt(num);
                }
            }
        }

        public void UpdateGVElectricElements()
        {
            foreach (KeyValuePair<Point3, bool> item in m_pointsToUpdate)
            {
                Point3 key = item.Key;
                int cellValue = SubsystemTerrain.Terrain.GetCellValue(key.X, key.Y, key.Z);
                for (int i = 0; i < 6; i++)
                {
                    GVElectricElement GVElectricElement = GetGVElectricElement(key.X, key.Y, key.Z, i);
                    if (GVElectricElement != null)
                    {
                        if (GVElectricElement is WireDomainGVElectricElement)
                        {
                            m_wiresToUpdate[key] = true;
                        }
                        else
                        {
                            m_GVElectricElementsToRemove[GVElectricElement] = true;
                        }
                    }
                }
                if (item.Value)
                {
                    m_persistentElementsVoltages.Remove(key);
                }
                int num = Terrain.ExtractContents(cellValue);
                if (BlocksManager.Blocks[num] is IGVElectricWireElementBlock)
                {
                    m_wiresToUpdate[key] = true;
                }
                else
                {
                    var GVElectricElementBlock = BlocksManager.Blocks[num] as IGVElectricElementBlock;
                    if (GVElectricElementBlock != null)
                    {
                        GVElectricElement GVElectricElement2 = GVElectricElementBlock.CreateGVElectricElement(this, cellValue, key.X, key.Y, key.Z);
                        if (GVElectricElement2 != null)
                        {
                            m_GVElectricElementsToAdd[key] = GVElectricElement2;
                        }
                    }
                }
            }
            RemoveWireDomains();
            foreach (KeyValuePair<GVElectricElement, bool> item2 in m_GVElectricElementsToRemove)
            {
                RemoveGVElectricElement(item2.Key);
            }
            AddWireDomains();
            foreach (GVElectricElement value in m_GVElectricElementsToAdd.Values)
            {
                AddGVElectricElement(value);
            }
            m_pointsToUpdate.Clear();
            m_wiresToUpdate.Clear();
            m_GVElectricElementsToAdd.Clear();
            m_GVElectricElementsToRemove.Clear();
        }

        public void AddWireDomains()
        {
            m_tmpVisited.Clear();
            foreach (Point3 key in m_wiresToUpdate.Keys)
            {
                for (int i = key.X - 1; i <= key.X + 1; i++)
                {
                    for (int j = key.Y - 1; j <= key.Y + 1; j++)
                    {
                        for (int k = key.Z - 1; k <= key.Z + 1; k++)
                        {
                            for (int l = 0; l < 6; l++)
                            {
                                m_tmpResult.Clear();
                                ScanWireDomain(new CellFace(i, j, k, l), m_tmpVisited, m_tmpResult);
                                if (m_tmpResult.Count > 0)
                                {
                                    var GVElectricElement = new WireDomainGVElectricElement(this, m_tmpResult.Keys);
                                    AddGVElectricElement(GVElectricElement);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RemoveWireDomains()
        {
            foreach (Point3 key in m_wiresToUpdate.Keys)
            {
                for (int i = key.X - 1; i <= key.X + 1; i++)
                {
                    for (int j = key.Y - 1; j <= key.Y + 1; j++)
                    {
                        for (int k = key.Z - 1; k <= key.Z + 1; k++)
                        {
                            for (int l = 0; l < 6; l++)
                            {
                                if (m_GVElectricElementsByCellFace.TryGetValue(new CellFace(i, j, k, l), out GVElectricElement value) && value is WireDomainGVElectricElement)
                                {
                                    RemoveGVElectricElement(value);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ScanWireDomain(CellFace startCellFace, Dictionary<CellFace, bool> visited, Dictionary<CellFace, bool> result)
        {
            var dynamicArray = new DynamicArray<CellFace>();
            dynamicArray.Add(startCellFace);
            while (dynamicArray.Count > 0)
            {
                CellFace key = dynamicArray.Array[--dynamicArray.Count];
                if (visited.ContainsKey(key))
                {
                    continue;
                }
                TerrainChunk chunkAtCell = SubsystemTerrain.Terrain.GetChunkAtCell(key.X, key.Z);
                if (chunkAtCell == null || !chunkAtCell.AreBehaviorsNotified)
                {
                    continue;
                }
                int cellValue = SubsystemTerrain.Terrain.GetCellValue(key.X, key.Y, key.Z);
                int num = Terrain.ExtractContents(cellValue);
                var GVElectricWireElementBlock = BlocksManager.Blocks[num] as IGVElectricWireElementBlock;
                if (GVElectricWireElementBlock == null)
                {
                    continue;
                }
                int connectedWireFacesMask = GVElectricWireElementBlock.GetConnectedWireFacesMask(cellValue, key.Face);
                if (connectedWireFacesMask == 0)
                {
                    continue;
                }
                for (int i = 0; i < 6; i++)
                {
                    if ((connectedWireFacesMask & (1 << i)) != 0)
                    {
                        var key2 = new CellFace(key.X, key.Y, key.Z, i);
                        visited.Add(key2, value: true);
                        result.Add(key2, value: true);
                        m_tmpConnectionPaths.Clear();
                        GetAllConnectedNeighbors(key2.X, key2.Y, key2.Z, key2.Face, m_tmpConnectionPaths);
                        foreach (GVElectricConnectionPath tmpConnectionPath in m_tmpConnectionPaths)
                        {
                            int x = key2.X + tmpConnectionPath.NeighborOffsetX;
                            int y = key2.Y + tmpConnectionPath.NeighborOffsetY;
                            int z = key2.Z + tmpConnectionPath.NeighborOffsetZ;
                            dynamicArray.Add(new CellFace(x, y, z, tmpConnectionPath.NeighborFace));
                        }
                    }
                }
            }
        }

        public Dictionary<GVElectricElement, bool> GetListFromCache()
        {
            if (m_listsCache.Count > 0)
            {
                Dictionary<GVElectricElement, bool> result = m_listsCache[m_listsCache.Count - 1];
                m_listsCache.RemoveAt(m_listsCache.Count - 1);
                return result;
            }
            return new Dictionary<GVElectricElement, bool>();
        }

        public void ReturnListToCache(Dictionary<GVElectricElement, bool> list)
        {
            list.Clear();
            m_listsCache.Add(list);
        }

        public void DebugDraw()
        {
        }

        public void SetSpeed(float speed)
        {
            CircuitStepDuration = 0.01f / speed;
        }
    }
}