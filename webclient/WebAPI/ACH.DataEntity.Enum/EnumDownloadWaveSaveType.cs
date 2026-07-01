using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ACH.DataEntity.Enum
{
    /// <summary>
    /// 数据下载保存类型枚举
    /// </summary>
    public enum EnumDownloadWaveSaveType
    {
        [Description("众芯")]
        ACH = 0,

        [Description("明阳")]
        MY = 1
    }
}
