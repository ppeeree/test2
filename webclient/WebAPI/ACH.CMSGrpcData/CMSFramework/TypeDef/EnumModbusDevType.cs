using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumModbusDevType")]
    public enum EnumModbusDevType
    {
        [EnumMember]
        [Description("静态倾角")]
        StaticTIM = 0,

        [EnumMember]
        [Description("动态晃度仪")]
        DynamicSVM = 1,

        [EnumMember]
        [Description("静态晃度仪")]
        StaticSVM = 2,

        [EnumMember]
        [Description("鑫世达油液传感器")]
        SenseStarOil = 3,

        [EnumMember]
        [Description("威锐达油液传感器")]
        WeridaOil = 4,
    }
}
