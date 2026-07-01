using ACH.ACHLog.SeriLog;
using ACH.DevTree.Entity;
using ACH.MeasData.Entity;
using ACH.MeasData.FSDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ACH.Download.DataDownload
{
    /// <summary>
    /// 下载明阳格式的波形数据
    /// </summary>
    public class DownloadMYFormatWaveData
    {
        private CMSWaveSqliteDB _cmsWaveSqliteDB = new CMSWaveSqliteDB();
        private BMSWaveSqliteDB _bmsWaveSqliteDB = new BMSWaveSqliteDB();
        private TMSWaveSqliteDB _tmsWaveSqliteDB = new TMSWaveSqliteDB();


        /// <summary>
        /// 保存明阳格式的波形数据
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="wavePathList"></param>
        /// <param name="savePath"></param>
        public void SaveMYFormatWaveData(List<DevMeasLocation> measList, List<string> wavePathList, string savePath)
        {
            // 创建目录
            var spdPath = Path.Combine(savePath, "转速波形数据");
            var vibPath = Path.Combine(savePath, "时域波形数据");
            Directory.CreateDirectory(spdPath);
            Directory.CreateDirectory(vibPath);


            // 遍历已经筛选出的众芯格式的波形文件
            foreach (string wavePath in wavePathList)
            {
                if (!File.Exists(wavePath))
                {
                    ALog.Debug($"SaveMYFormatWaveData - {wavePath}文件不存在");
                    continue;
                }

                // 判断筛选出的文件后缀
                switch (Path.GetExtension(wavePath).ToLower())
                {
                    case ".cms":
                        CMSMeasEvent cmsMeasEvent = _cmsWaveSqliteDB.ConvertCMSData(wavePath);
                        SaveMYWorkingCondition(spdPath, measList, cmsMeasEvent.rotSpdData);

                        float speed = cmsMeasEvent.rotSpdData == null ? 0 : cmsMeasEvent.rotSpdData.AVG;
                        SaveMYVibData(vibPath, speed, measList, cmsMeasEvent.tdVibDatas);
                        break;
                    case ".bms":
                        BMSMeasEvent bmsMeasEvent = _bmsWaveSqliteDB.ConvertBMSData(wavePath);
                        SaveMYVibData(vibPath, 0, measList, bmsMeasEvent.tdVibDatas);
                        break;
                    case ".tms":
                        TMSMeasEvent tmsMeasEvent = _tmsWaveSqliteDB.ConvertTMSData(wavePath);
                        SaveMYVibData(vibPath, 0, measList, tmsMeasEvent.tdVibDatas);
                        break;
                    default:
                        ALog.Information($"不支持下载明阳格式的波形文件类型：{wavePath}");
                        break;
                }
            }
        }

        /// <summary>
        /// 保存转速波形
        /// </summary>
        /// <param name="savePath">保存地址</param>
        /// <param name="measList">测点列表</param>
        /// <param name="spddata">转速波形</param>
        private static void SaveMYWorkingCondition(string savePath, List<DevMeasLocation> measList, RotSpdWaveData? spddata)
        {
            try
            {
                if (spddata == null || spddata.WaveData == null)
                {
                    ALog.Debug($"SaveMYWorkingCondition - 读取出的转速数据为null");
                    return;
                }

                var meas = measList.FirstOrDefault();

                // 获取保存的文件名
                string fileName = $"{meas.StationName}_{meas.DeviceName}_转速_{spddata.AcqTime.ToString("yyyyMMddHHmmss")}.txt";

                // 保存数据
                File.WriteAllLines(Path.Combine(savePath, fileName), spddata.WaveData.Select(v => v.ToString()));
            }
            catch (Exception ex)
            {
                ALog.Error($"SaveMYWorkingCondition - {ex.Message}");
            }
        }

        /// <summary>
        /// 保存振动波形
        /// </summary>
        /// <param name="savePath">保存地址</param>
        /// <param name="measList">测点列表</param>
        /// <param name="data">振动波形</param>
        private static void SaveMYVibData(string savePath, float runningspeed, List<DevMeasLocation> measList, List<TWVibData> data)
        {
            try
            {
                // 遍历振动数据
                foreach (var item in data)
                {
                    // 获取对应的测点信息
                    var meas = measList.FirstOrDefault(x => x.MeasLoctionID == item.MeasLocID);
                    if (meas == null)
                    {
                        ALog.Debug($"SaveMYVibData - 找不到对应的测点信息：{item.MeasLocID}");
                        continue;
                    }

                    string speedString = runningspeed == 0 ? "无转速" : runningspeed.ToString("F2");

                    string fileName = $"{meas.StationName}_{meas.DeviceName}_{meas.ComponentName}_{meas.MeasLoctionName}_{item.SampleRate}Hz_加速度_{speedString}_{item.AcqTime.ToString("yyyyMMddHHmmss")}.txt";

                    File.WriteAllLines(Path.Combine(savePath, fileName), item.WaveData.Select(v => v.ToString()));
                }
            }
            catch (Exception ex)
            {
                ALog.Error($"SaveMYVibData - {ex.Message}");
            }
        }


        /// <summary>
        /// 字节数组转浮点数数组
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float[] BytesToFloats(byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
            {
                throw new ArgumentException("Byte array length must be a multiple of 4.");
            }

            int floatCount = bytes.Length / 4;
            float[] floats = new float[floatCount];
            Span<byte> byteSpan = bytes.AsSpan();
            Span<float> floatSpan = MemoryMarshal.Cast<byte, float>(byteSpan);
            floatSpan.CopyTo(floats);
            return floats;
        }

        /*public static float[] BytesToFloats(byte[] bytes, bool isBigEndian = false)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentException("Byte array is null or empty.");
            }

            if (bytes.Length % 4 != 0)
            {
                throw new ArgumentException("Byte array length must be a multiple of 4.");
            }

            int floatCount = bytes.Length / 4;
            float[] floats = new float[floatCount];

            // 如果需要转换字节顺序
            if (isBigEndian && !BitConverter.IsLittleEndian || !isBigEndian && BitConverter.IsLittleEndian)
            {
                // 小端序系统处理大端序数据，或大端序系统处理小端序数据
                for (int i = 0; i < floatCount; i++)
                {
                    // 反转每4个字节的顺序
                    byte[] floatBytes = new byte[4];
                    Array.Copy(bytes, i * 4, floatBytes, 0, 4);
                    Array.Reverse(floatBytes);  // 反转字节顺序
                    floats[i] = BitConverter.ToSingle(floatBytes, 0);
                }
            }
            else
            {
                // 字节顺序匹配，使用快速转换
                Span<byte> byteSpan = bytes.AsSpan();
                Span<float> floatSpan = MemoryMarshal.Cast<byte, float>(byteSpan);
                floatSpan.CopyTo(floats);
            }

            return floats;
        }*/
    }
}
