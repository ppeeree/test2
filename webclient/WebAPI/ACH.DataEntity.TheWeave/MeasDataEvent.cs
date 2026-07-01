using ACH.DataEntity.Enum;
using MessagePack;
using SqlSugar;
using System;

namespace ACH.DataEntity.TheWeave
{
    [SplitTable(SplitType.Month)]
    [SugarTable("MeasDataEvent_{year}{month}{day}")]
    public class MeasDataEvent
    {
        public MeasDataEvent()
        {
            StationID = string.Empty;
            DeviceID = string.Empty;
        }
        [SugarColumn(IsPrimaryKey = true)]
        [IgnoreMember]
        public long Id { get; set; }
        [Key(0)]
        public string StationID { get; set; } // 场站ID
        [Key(1)]
        public string DeviceID { get; set; } // 设备ID
        [SplitField]
        [Key(2)]
        public DateTime AcqTime { get; set; }
        [Key(3)]
        public EnumCompCategory CompType { get; set; } // 部件类型
        [Key(4)]
        public int Count { get; set; } // 数据个数
        [Key(5)]
        public double RotSpd { get; set; } // 转速值
        [Key(6)]
        public double Power { get; set; } // 功率值

    }
}
