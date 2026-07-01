using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public enum EnumSignalType
    {
        [EnumMember]
        [Description("加速度")]
        Acceleration = 0,
        [EnumMember]
        [Description("速度")]//是否需要和速率区别开？？
        Velocity = 1,
        [EnumMember]
        [Description("位移")]
        Displacement = 2,
        [EnumMember]
        [Description("电压")]
        Voltage = 3,
        [EnumMember]
        [Description("电流")]
        Current = 4,
    }

    //信号类型 旧数据都是字符串 "加速度" 考虑兼容问题以及后续会增加，还是使用字符串而不是枚举。防止需要底层代码修改
     [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class WFSignalType
    {
        public const string Acceleration = "加速度";
        public const string Velocity = "速度";
        public const string Displacement = "位移";
        public const string Voltage = "电压";
        public const string Current = "电流";
    }
}
