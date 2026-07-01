using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.TypeDef
{
    /// <summary>
    /// BVM诊断状态数据类型
    /// </summary>
    [DataContract(Name = "BVMEnumBladeMonSource")]
    public enum BVMEnumBladeMonType
    {
        /// <summary>
        /// 损伤诊断状态库
        /// </summary>
        [EnumMember]
        [Description("HarmDB")]
        HarmDB = 1,
        /// <summary>
        /// 覆冰诊断状态库
        /// </summary>
        [EnumMember]
        [Description("FreezeDB")]
        FreezeDB = 2,        
    }
}
