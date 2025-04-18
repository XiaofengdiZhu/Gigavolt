﻿using System.Collections.Generic;
using Engine.Graphics;

namespace Game {
    public static class GVStaticStorage {
        public static readonly Random random = new();
        public static readonly Dictionary<uint, GVArrayData> GVMBIDDataDictionary = new();

        public static uint GetUniqueGVMBID() {
            while (true) {
                uint num = random.UInt();
                if (num == 0u
                    || GVMBIDDataDictionary.ContainsKey(num)) {
                    continue;
                }
                return num;
            }
        }

        public static readonly Dictionary<uint, GVSubterrainSystem> GVSubterrainSystemDictionary = new();

        public static uint GetUniqueGVSubterrainID() {
            while (true) {
                uint num = random.UInt();
                if (num == 0u
                    || GVSubterrainSystemDictionary.ContainsKey(num)) {
                    continue;
                }
                return num;
            }
        }

        public static readonly List<SoundGeneratorGVElectricElement> GVSGCFEEList = new();

        public static bool DisplayVoltage = false;

        public static bool WheelPanelEnabled = true;

        public static HashSet<TerrainChunk> EditableItemBehaviorChangedChunks = [];

        public static Texture2D WhiteTexture;
    }
}