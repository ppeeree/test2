using ACH.Helper.Others;
using MessagePack;
using SqlSugar;
using System.Collections.Generic;

namespace ACH.DataEntity.TheWeave
{
    [SplitTable(SplitType.Month)]
    [SugarTable("PulseWaveData_{year}{month}{day}")]
    [MessagePackObject]
    public class PulseWaveData : WaveDataBase
    {
        [Key(11)]
        public double MAX { get; set; }
        [Key(12)]
        public double MIN { get; set; }
        [Key(13)]
        public double AVG { get; set; }
        [Key(14)]
        public int MImplus { get; set; } // 每周期脉冲数
        [Key(15)]
        public int Type { get; set; } // 转速类型（转速序列值：type=0  转速脉冲时刻值:    type=1   脉冲电压波形: type=2）


        //根据code 返回对应的辅助信息
        public List<EigenValueData> evCodeMaping()
        {
            List<EigenValueData> evData = new List<EigenValueData>();
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&MAX", "最大值", "r/min", $"MAX", MAX, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), 0));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&MIN", "最小值", "r/min", $"MIN", MIN, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), 0));
            evData.Add(new EigenValueData(DeviceID, ComponentID, MeasLocID, $"{MeasLocID}&&AVG", "平均值", "r/min", $"AVG", AVG, AcqTime, SingleType.ToString(), EnumHelper.GetDescription(SingleType), 0));

            return evData;
        }
    }
}
