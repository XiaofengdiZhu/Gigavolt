using TemplatesDatabase;

namespace Game
{
    public class SubsystemGVElectricBlockBehavior : SubsystemBlockBehavior
    {
        public SubsystemGVElectricity m_subsystemGVElectric;

        public override int[] HandledBlocks => new int[]
        {
            644,
            646,
            647,
            653,
            654,
            684,
            686,
            688,
            699,
            756,
            757,
            758,
            763,
            783,
            784,
            786,
            797,
            798,
            805,
            806,
            807,
            820,
            821,
            833,
            834,
            835,
            837,
            838,
            839,
            840,
            841,
            842,
            843,
            844,
            845,
            846,
            847,
            851,
            852,
            853,
            854,
            855,
            856,
            857,
            866,
            879,
            880,
            881,
            882,
            883,
            884,
            885,
            886,
            887,
            888,
            894,
            899,
            910,
            911,
            916,
            923,
            924,
            927,
            934,
            935,
            936,
            937,
            943,
            953,
            954
        };

        public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
        {
            m_subsystemGVElectric.OnGVElectricElementBlockGenerated(x, y, z);
        }

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
        {
            m_subsystemGVElectric.OnGVElectricElementBlockAdded(x, y, z);
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
        {
            m_subsystemGVElectric.OnGVElectricElementBlockRemoved(x, y, z);
        }

        public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
        {
            m_subsystemGVElectric.OnGVElectricElementBlockModified(x, y, z);
        }

        public override void OnChunkDiscarding(TerrainChunk chunk)
        {
            m_subsystemGVElectric.OnChunkDiscarding(chunk);
        }

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
        {
            for (int i = 0; i < 6; i++)
            {
                m_subsystemGVElectric.GetGVElectricElement(x, y, z, i)?.OnNeighborBlockChanged(new CellFace(x, y, z, i), neighborX, neighborY, neighborZ);
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
        {
            int x = raycastResult.CellFace.X;
            int y = raycastResult.CellFace.Y;
            int z = raycastResult.CellFace.Z;
            for (int i = 0; i < 6; i++)
            {
                GVElectricElement GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(x, y, z, i);
                if (GVElectricElement != null)
                {
                    return GVElectricElement.OnInteract(raycastResult, componentMiner);
                }
            }
            return false;
        }

        public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
        {
            int x = cellFace.X;
            int y = cellFace.Y;
            int z = cellFace.Z;
            int num = 0;
            GVElectricElement GVElectricElement;
            while (true)
            {
                if (num < 6)
                {
                    GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(x, y, z, num);
                    if (GVElectricElement != null)
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return;
            }
            GVElectricElement.OnCollide(cellFace, velocity, componentBody);
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
        {
            int x = cellFace.X;
            int y = cellFace.Y;
            int z = cellFace.Z;
            int num = 0;
            GVElectricElement GVElectricElement;
            while (true)
            {
                if (num < 6)
                {
                    GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(x, y, z, num);
                    if (GVElectricElement != null)
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return;
            }
            GVElectricElement.OnHitByProjectile(cellFace, worldItem);
        }

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            m_subsystemGVElectric = Project.FindSubsystem<SubsystemGVElectricity>(throwOnError: true);
        }
    }
}