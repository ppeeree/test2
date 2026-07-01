using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class VibWaveFormData : WaveFormData
    {       

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// DAUCode 采集单元编号 EF Ignore
        /// </summary>
        [DataMember(Order = 18)]
        public string DAUCode
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// DAUChannelID 采集通道ID
        /// </summary>
        [DataMember(Order = 19)]
        public int DAUChannelID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// eu_type_code 工程单位编号 EF Ignore
        /// </summary>
        [DataMember(Order = 20)]
        public string Eu_Type_Code
        {
            get;
            set;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SignalType 信号类型
        /// </summary>
        [DataMember(Order = 21)]
        public string SignalType
        {
            get;
            set;
        }
    }
}
