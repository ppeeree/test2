using System.ComponentModel;
namespace ACH.Diagnosis.BearingDamageModel
{
    public enum EnumBeaingFaultFrequency
    {
        [Description("内圈故障频率")]
        BPFI,
        [Description("外圈故障频率")]
        BPFO,
        [Description("滚动体故障频率")]
        BSF,
        [Description("保持架故障频率")]
        FTF
      
    }
}
