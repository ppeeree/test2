using MessagePack;
using SqlSugar;
using System;

namespace ACH.DataEntity.TheWeave
{
    [SplitTable(SplitType.Month)]
    [SugarTable("EigenValueData_{year}{month}{day}")]
    [MessagePackObject]
    public class EigenValueData
    {
        [SugarColumn(IsPrimaryKey = true)][IgnoreMember] public long Id { get; set; }
        [Key(0)] public string DeviceID { get; set; } // 设备ID
        [Key(1)] public string ComponentID { get; set; } // 部件ID
        [Key(2)] public string MeasLocID { get; set; } // 测量位置(测点/测点+方向)

        [Key(3)] public string EigenValueID { get; set; }
        [Key(4)] public string EigenValueCode { get; set; }
        [SugarColumn(IsIgnore = true)][Key(5)] public string EigenValueName { get; set; }
        [SugarColumn(IsIgnore = true)][Key(6)] public string Unit { get; set; }

        [SugarColumn(IsNullable = true)][Key(7)] public string SingleType { get; set; }
        [SugarColumn(IsNullable = true)][Key(8)] public string SingleTypeName { get; set; }
        [Key(9)] public double SampleRate { get; set; } // 采样频率
        [SplitField][Key(10)] public DateTime AcqTime { get; set; } // 采集时间
        [Key(11)] public double EigenValue { get; set; } // 特征值
        [Key(12)] public int DataQuality { get; set; } // 数据质量


        public EigenValueData() { }
        public EigenValueData(string deviceID, string compId, string measLocId, string eigenValueID,
            string eigenValueName, string unit, string eigenValueCode, double eigenValue, DateTime time,
            string singleType, string singleTypeName, double sampleRate)
        {
            DeviceID = deviceID;
            ComponentID = compId;
            MeasLocID = measLocId;
            EigenValueID = eigenValueID;
            EigenValueName = eigenValueName;
            Unit = unit;
            EigenValueCode = eigenValueCode;
            EigenValue = eigenValue;
            AcqTime = time;
            SingleType = singleType;
            SingleTypeName = singleTypeName;
            SampleRate = sampleRate;

        }
    }
}
