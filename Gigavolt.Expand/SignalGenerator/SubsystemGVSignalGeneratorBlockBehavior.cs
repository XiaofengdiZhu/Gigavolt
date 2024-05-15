#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVSignalGeneratorBlockBehavior : SubsystemBlockBehavior {
        public class Data(int step, int nowAmplitude) {
            public int Step = step;
            public int NowAmplitude = nowAmplitude;
        }

        public Dictionary<Point3, Data> m_datas = new();
        public override int[] HandledBlocks => new[] { GVSignalGeneratorBlock.Index };

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            foreach (ValuesDictionary value3 in valuesDictionary.GetValue<ValuesDictionary>("Blocks").Values) {
                try {
                    m_datas.Add(value3.GetValue<Point3>("Point"), new Data(value3.GetValue<int>("Step"), value3.GetValue<int>("NowAmplitude")));
                }
                catch (Exception e) {
                    // ignored
                }
            }
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            base.Save(valuesDictionary);
            int num = 0;
            ValuesDictionary valuesDictionary2 = new();
            valuesDictionary.SetValue("Blocks", valuesDictionary2);
            foreach (KeyValuePair<Point3, Data> pair in m_datas) {
                ValuesDictionary valuesDictionary3 = new();
                valuesDictionary2.SetValue(num++.ToString(CultureInfo.InvariantCulture), valuesDictionary3);
                valuesDictionary3.SetValue("Point", pair.Key);
                valuesDictionary3.SetValue("Step", pair.Value.Step);
                valuesDictionary3.SetValue("NowAmplitude", pair.Value.NowAmplitude);
            }
        }

        public Data? GetData(Point3 position) => m_datas.GetValueOrDefault(position);
        public int? GetStep(Point3 position) => m_datas.TryGetValue(position, out Data? result) ? result.Step : null;

        public void SetStep(Point3 position, int step) {
            if (m_datas.TryGetValue(position, out Data? data)) {
                data.Step = step;
            }
            else {
                m_datas.Add(position, new Data(step, 0));
            }
        }

        public int? GetNowAmplitude(Point3 position) => m_datas.TryGetValue(position, out Data? result) ? result.NowAmplitude : null;

        public void SetNowAmplitude(Point3 position, int nowAmplitude) {
            if (m_datas.TryGetValue(position, out Data? data)) {
                data.NowAmplitude = nowAmplitude;
            }
            else {
                m_datas.Add(position, new Data(0, nowAmplitude));
            }
        }

        public bool Remove(Point3 position) => m_datas.Remove(position);

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) {
            int data = Terrain.ExtractData(SubsystemTerrain.Terrain.GetCellValue(x, y, z));
            if (!GVSignalGeneratorBlock.GetIsTopPart(data)) {
                int face = RotateableMountedGVElectricElementBlock.GetFaceFromDataStatic(data);
                Point3 up = GVSignalGeneratorBlock.m_upPoint3[face * 4 + RotateableMountedGVElectricElementBlock.GetRotation(data)] + new Point3(x, y, z);
                if (Terrain.ExtractContents(SubsystemTerrain.Terrain.GetCellValue(up.X, up.Y, up.Z)) == 0) {
                    Point3 faceDirection = -CellFace.FaceToPoint3(face);
                    int faceValue = SubsystemTerrain.Terrain.GetCellValue(up.X + faceDirection.X, up.Y + faceDirection.Y, up.Z + faceDirection.Z);
                    Block block = BlocksManager.Blocks[Terrain.ExtractContents(faceValue)];
                    if ((block.IsCollidable_(faceValue) && !block.IsFaceTransparent(SubsystemTerrain, face, faceValue))
                        || (face == 4 && block is FenceBlock)) {
                        SubsystemTerrain.ChangeCell(up.X, up.Y, up.Z, Terrain.MakeBlockValue(GVSignalGeneratorBlock.Index, 0, GVSignalGeneratorBlock.SetIsTopPart(data, true)));
                        return;
                    }
                }
                SubsystemTerrain.DestroyCell(
                    int.MaxValue,
                    x,
                    y,
                    z,
                    0,
                    false,
                    false
                );
            }
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int face = RotateableMountedGVElectricElementBlock.GetFaceFromDataStatic(data);
            int rotation = RotateableMountedGVElectricElementBlock.GetRotation(data);
            Point3 upDirection = GVSignalGeneratorBlock.m_upPoint3[face * 4 + rotation];
            bool isUp = GVSignalGeneratorBlock.GetIsTopPart(data);
            Point3 origin = new(x, y, z);
            Point3 another = origin + upDirection * (isUp ? -1 : 1);
            int anotherData = Terrain.ExtractData(SubsystemTerrain.Terrain.GetCellValue(another.X, another.Y, another.Z));
            if (GVSignalGeneratorBlock.GetIsTopPart(anotherData) != isUp
                && RotateableMountedGVElectricElementBlock.GetFaceFromDataStatic(anotherData) == face
                && RotateableMountedGVElectricElementBlock.GetRotation(anotherData) == rotation) {
                SubsystemTerrain.ChangeCell(another.X, another.Y, another.Z, 0);
            }
            Remove(isUp ? another : origin);
        }
    }
}