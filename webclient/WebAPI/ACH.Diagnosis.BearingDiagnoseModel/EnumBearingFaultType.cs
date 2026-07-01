using System.ComponentModel;
namespace ACH.Diagnosis.BearingDiagnoseModel
{
    public enum EnumBearingFaultType
    {
        [Description("内圈")]
        INNERCIRCLE,
        [Description("外圈")]
        OUTSIDECIRCLE,
        [Description("滚动体")]
        ROLLINGELEMENT,
        [Description("保持架")]
        CAGE
    }
}
