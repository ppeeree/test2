
using System.ComponentModel;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumDAUType")]
    public enum EnumDAUType
    {
        [EnumMember]
        [Description("振动监测")]
        Vibration = 0,

        [EnumMember]
        [Description("超声波监测")]
        Ultrasonic = 1,

        [EnumMember]
        [Description("光纤应变")]
        FiberStrain = 2,
    }
}
