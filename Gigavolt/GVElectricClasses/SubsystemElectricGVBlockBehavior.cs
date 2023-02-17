using TemplatesDatabase;

namespace Game
{
    public class SubsystemGVElectricBlockBehavior : SubsystemBlockBehavior
    {
        public SubsystemGVElectricity m_subsystemGVElectric;

        public override int[] HandledBlocks => new int[]
        {
            788,
            833,
            840,
            837,
            843,
            856,
            834,
            835,
            845,
            924,
            846,
            857,
            880,
            881,
            883,
            838,
            839,
            841,
            842,
            884,
            887,
            886,
            888,
            844,
            851,
            879,
            852,
            954,
            953,
            882,
            885,
            756,
            757,
            758,
            783,
            784,
            866,
            894,
            786,
            763,
            797,
            798,
            910,
            911,
            805,
            806,
            807,
            934,
            935,
            936,
            847,
            853,
            854,
            923,
            855,
            943,
            820,
            821,
            899,
            916,
            927,
            937
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