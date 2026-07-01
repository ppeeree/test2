using ACH.DataEntity.Enum;
using ACH.Helper.Others;
using ACH.MeasData.Entity;
using MessagePack;
using SqlSugar;
using System.Collections.Generic;

namespace ACH.DataEntity.TheWeave
{
    /// <summary>
    /// 振动波形
    /// </summary>
    [SplitTable(SplitType.Month)]
    [SugarTable("VibWaveData_{year}{month}{day}")]
    [MessagePackObject]
    public class VibWaveData : EIWaveData
    {
        /// <summary>
        /// 波形类型
        /// </summary>
        [Key(17)]
        public EnumWaveType WaveType;
        [Key(18)]
        public double RMS { get; set; }  // 通频RMS
        [Key(19)]
        public double KTS { get; set; }  // 峭度系数
        [Key(20)]
        public double IIF { get; set; } // 脉冲指标
        [Key(21)]
        public double LF { get; set; } // 裕度指标
        [Key(22)]
        public double SK { get; set; } // 倾斜度
        [Key(23)]
        public double CF { get; set; } // 峰值指标
        [Key(24)]
        public double SF { get; set; } // 波形指标

        //根据code 返回对应的辅助信息
        public List<EigenValueData> evCodeMaping()
        {
            string unit = EnumHelper.GetUnit(SingleType);
            List<EigenValueData> evData = new List<EigenValueData>();
            int _sampleRate = (int)(SampleRate / 2.56);
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_RMS", "通频RMS", unit, $"{0}-{_sampleRate}_RMS", RMS, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_KTS", "峭度系数", unit, $"{0}-{_sampleRate}_KTS", KTS, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_IIF", "脉冲指标", unit, $"{0}-{_sampleRate}_IIF", IIF, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_LF", "裕度指标", unit, $"{0}-{_sampleRate}_LF", LF, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_SK", "倾斜度", unit, $"{0}-{_sampleRate}_SK", SK, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_CF", "峰值指标", unit, $"{0}-{_sampleRate}_CF", CF, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_SF", "波形指标", unit, $"{0}-{_sampleRate}_SF", SF, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            return evData;
        }
    }
}
