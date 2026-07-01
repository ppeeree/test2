using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 采集单元实时状态实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public partial class RTAlarmStatus_DAU
    {
        public RTAlarmStatus_DAU()
        {
            DauID = "1";//默认为1
            sensorRTList = new List<RTAlarmStatus_Channel>();

            RSSensorRTList = new List<RTAlarmStatus_RSChannel>();
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Ds_asset_id 设备编号
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public string DauID
        {
            get;
            set;
        }

        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmState 报警状态
        /// </summary>
        [DataMember]
        public EnumDAUStatus AlarmState
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// StateUpdateTime 状态更新时间
        /// </summary>
        [DataMember]
        public DateTime StatusUpdateTime
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 运行记录
        /// </summary>
        [DataMember]
        public List<string> RunningLog
        {
            get;
            set;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// DAU传感器实时状态
        /// </summary>
        [DataMember]
        public List<RTAlarmStatus_Channel> sensorRTList
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// EF mapping
        /// </summary>
        [DataMember]
        public List<RTAlarmStatus_RSChannel> RSSensorRTList
        {
            get;
            set;
        }    
    
    }
}
