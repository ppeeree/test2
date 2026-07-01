using ACH.Helper.Others;
using MessagePack;
using SqlSugar;
using System.Collections.Generic;

namespace ACH.DataEntity.TheWeave
{
    /// <summary>
    /// 等间隔时间序列  equal intervals
    /// </summary>
    [SplitTable(SplitType.Month)]
    [SugarTable("EIWaveData_{year}{month}{day}")]
    [MessagePackObject]
    public class EIWaveData : WaveDataBase
    {
        /// <summary>
        /// 采样频率
        /// </summary>
        [Key(11)]
        public double SampleRate { get; set; } // 采样频率
        /// <summary>
        /// 峰峰值
        /// </summary>
        [Key(12)]
        public double PPK { get; set; }  // 峰峰值
        /// <summary>
        /// 最大值
        /// </summary>
        [Key(13)]
        public double MAX { get; set; }  // 最大值
        /// <summary>
        /// 最小值
        /// </summary>
        [Key(14)]
        public double MIN { get; set; }  // 最小值
        /// <summary>
        /// 平均值
        /// </summary>
        [Key(15)]
        public double AVG { get; set; }  // 平均值
        /// <summary>
        /// 算术平均值
        /// </summary>
        [Key(16)]
        public double AbsAVG { get; set; }  // 算术平均值


        //根据code 返回对应的辅助信息
        public List<EigenValueData> evCodeMaping()
        {
            string unit = EnumHelper.GetUnit(SingleType);
            List<EigenValueData> evData = new List<EigenValueData>();
            int _sampleRate = (int)(SampleRate / 2.56);
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_PPK", "峰峰值", unit, $"{0}-{_sampleRate}_PPK", PPK, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_MAX", "最大值", unit, $"{0}-{_sampleRate}_MAX", MAX, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_MIN", "最小值", unit, $"{0}-{_sampleRate}_MIN", MIN, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_AVG", "平均值", unit, $"{0}-{_sampleRate}_AVG", AVG, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&{0}-{_sampleRate}_AbsAVG", "算术平均值", unit, $"{0}-{_sampleRate}_AbsAVG", AbsAVG, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), SampleRate));
            return evData;
        }

    }
}
