using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 转速通道实时状态实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class RTAlarmStatus_RSChannel
    {
        public RTAlarmStatus_RSChannel()
        {
            DauID = "1";//默认为1
        }
        /// <summary>
        /// 机组ID
        /// </summary>
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }

        [DataMember]
        public string DauID
        {
            get;
            set;
        }


        /// <summary>
        /// 通道编号
        /// </summary>
        [DataMember]
        public int ChannelNumber
        {
            get;
            set;
        }
        /// <summary>
        /// 通道状态
        /// </summary>
        [DataMember]
        public EnumRotSpeedChannelStatus AlarmState
        {
            get;
            set;
        }

        /// <summary>
        /// 状态更新时间
        /// </summary>
        [DataMember]
        public DateTime StatusUpdateTime
        {
            get;
            set;
        }

    }
}
