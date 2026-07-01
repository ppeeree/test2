using ACH.DataEntity.Enum;
using MessagePack;
using SqlSugar;
using System;

namespace ACH.DataEntity.TheWeave
{
    /// <summary>
    /// 时序波形-基础类
    /// </summary>
    public class WaveDataBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [IgnoreMember]
        public long Id { get; set; }
        [Key(0)]
        public string DeviceID { get; set; } // 设备ID
        [Key(1)]

        public string ComponentID { get; set; } // 部件ID
        [Key(2)]
        public string MeasLocID { get; set; } // 测量位置(测点/测点+方向)
        [Key(3)]
        public int SamplePoint { get; set; } // 采样点数
        [Key(4)]
        public EnumSignalType SingleType { get; set; } // 信号类型
        [SplitField]
        [Key(5)]
        public DateTime AcqTime { get; set; } // 采集时间
        [SugarColumn(IsIgnore = true)]
        [Key(6)]
        public double[]? WaveData { get; set; } // 波形数据（与WavePath二选一）
        [Key(7)]
        public string? WavePath { get; set; } // 数据路径
        [Key(8)]
        public double SensorVoltage { get; set; } // 偏置电压
        [Key(9)]
        public int SensorAlarm { get; set; } // 传感器状态
        [Key(10)]
        public int DataQuality { get; set; } // 数据质量
    }
}
