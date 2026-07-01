using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity
{
    public enum EnumSequenceType
    {
        [Description("测量定义ID")]
        MeasDefinitionID = 1,
        [Description("波形定义ID")]
        WaveDefinitionID = 2,
    }
}
