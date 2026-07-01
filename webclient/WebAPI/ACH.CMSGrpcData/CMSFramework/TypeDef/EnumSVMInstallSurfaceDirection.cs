using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity.SVM
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:Maxy
    // create:2013-06-15
    /// <summary>
    /// 晃度仪安装面方向
    /// </summary>
    [DataContract(Name = "EnumSVMInstallSurfaceDirection")]
    public enum EnumSVMInstallSurfaceDirection
    {
        /// <summary>
        ///上
        /// </summary>
        [EnumMember]
        [Description("上")]
        Up=0,

        /// <summary>
        ///下
        /// </summary>
        [EnumMember]
        [Description("下")]
        Down=1,

        /// <summary>
        ///左
        /// </summary>
        [EnumMember]
        [Description("左")]
        Left=2,

        /// <summary>
        ///右
        /// </summary>
        [EnumMember]
        [Description("右")]
        Right=3,

        /// <summary>
        ///前
        /// </summary>
        [EnumMember]
        [Description("前")]
        Ahead=4,

        /// <summary>
        ///后
        /// </summary>
        [EnumMember]
        [Description("后")]
        Back=5
    }
}
