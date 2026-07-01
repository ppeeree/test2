using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumTransStyleType")]
    public enum EnumTransStyleType
    {
        [EnumMember]
        [Description("水平")]
        Horizontal = 0,
        [EnumMember]
        [Description("垂直")]
        Vertical = 1,
        [EnumMember]
        [Description("轴向向左")]
        AxisToLeft = 2,
        [EnumMember]
        [Description("轴向向右")]
        AxisToRight = 3,
    }
}
