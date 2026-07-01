using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 风电场设备树的状态
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class DevTreeAlarmStatus
    {
        public DevTreeAlarmStatus()
        {
            TurbineRTState = new List<AlarmStatus_Turbine>();
            CompontRTState = new List<AlarmStatus_Component>();
            MeasLocRTState = new List<AlarmStatus_MeasLocVib>();
            SVMMeasLocRTState = new List<AlarmStatus_MeasLocSVM>();
            AlarmeventList = new List<AlarmEvent>();
        }
        /// <summary>
        /// 机组实时状态
        /// </summary>
        [DataMember]
        public List<AlarmStatus_Turbine> TurbineRTState
        {
            get;
            set;
        }

        /// <summary>
        /// 部件实时状态
        /// </summary>
        [DataMember]
        public List<AlarmStatus_Component> CompontRTState
        {
            get;
            set;
        }

        /// <summary>
        /// 测量位置实时状态
        /// </summary>
        [DataMember]
        public List<AlarmStatus_MeasLocVib> MeasLocRTState
        {
            get;
            set;
        }


        /// <summary>
        /// SVM测量位置报警状态
        /// </summary>
        [DataMember]
        public List<AlarmStatus_MeasLocSVM> SVMMeasLocRTState
        {
            get;
            set;
        }

        ///// <summary>
        ///// 特征值实时状态
        ///// </summary>
        //[DataMember]
        //public List<EigenValueData> EigenValueDataState
        //{
        //    get;
        //    set;
        //}


        /// <summary>
        /// 报警事件
        /// </summary>
        [DataMember]
        public List<AlarmEvent> AlarmeventList
        {
            get;
            set;
        }


        public string WindParkID
        {
            get;
            set;
        }
    }
}
