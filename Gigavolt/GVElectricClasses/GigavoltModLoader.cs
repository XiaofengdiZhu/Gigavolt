using Engine;
using Engine.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class GigavoltModLoader:ModLoader
    {
        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("OnProjectDisposed", this);
        }
        public override void OnProjectDisposed()
        {
            foreach(SoundGeneratorGVElectricElement element in GVStaticStorage.GVSGCFEEList)
            {
                if (element.m_sound != null)
                {
                    if (element.m_playing)
                    {
                        element.m_sound.Stop();
                    };
                    element.m_sound.Dispose();
                }
            }
            GVStaticStorage.GVSGCFEEList.Clear();
        }
    }
}
