using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace CMSFramework.BusinessEntity
{
    //报警定义类别
    [DataContract(Name = "EnumAlarmDefType")]
    public enum EnumMeasLocType
    {
        [EnumMember]
        [Description("振动")]
        VibAlarmDef = 1,

        [EnumMember]
        [Description("晃动")]
        SVMAlarmDef = 0,
    }
}
