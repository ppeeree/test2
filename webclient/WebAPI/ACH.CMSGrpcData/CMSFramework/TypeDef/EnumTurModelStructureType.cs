using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumTurModelStructureType")]
    public enum EnumTurModelStructureType
    {
        /// <summary>
        /// 直驱式
        /// </summary>
        [EnumMember]
        [Description("直驱式")]
        Structure_DirectDrive = 0,

        /// <summary>
        /// 双馈式
        /// </summary>
        [EnumMember]
        [Description("双馈式")]
        Structure_DoubleFed = 1

    }
}
