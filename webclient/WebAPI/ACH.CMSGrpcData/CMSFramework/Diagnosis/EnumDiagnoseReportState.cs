using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity.Diagnosis
{
    /// <summary>
    /// 诊断报告完成状态的枚举
    /// </summary>
    public enum EnumDiagnoseReportState
    {
        [Description("待诊断")]
        Concluded = 1,
        [Description("未汇总")]
        UnSummary = 2,
        [Description("已汇总")]
        Summaried = 3,

    }
}
