using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;


namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2011-06-09
    /// <summary>
    /// 设备实时状态实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public abstract class DeviceRTAlarmStatus
    {
        //-----------------------------------------------------------------------------------------------------------------------
        // author:steel
        // create:2011-07-27
        /// <summary>
        /// 构造器
        /// </summary>
        public DeviceRTAlarmStatus()
        {
            AlarmDegree = AlarmType.AlarmType_Unknown.AlarmDegree;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 设备节点 ID
        /// </summary>
        [DataMember]
        public abstract string DevSegmentID
        {
            get;
            set;
        }

        
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmDegree 报警等级
        /// </summary>
        [DataMember]
        public virtual EnumAlarmDegree AlarmDegree
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmUpdateTime 状态更新时间
        /// </summary>
        [DataMember]
        public DateTime AlarmUpdateTime
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // Author: GuoKaile
        // Time: 2011-06-29
        /// <summary>
        /// AlarmType 报警类型
        /// </summary>
        //[IgnoreDataMember]
        //public AlarmType AlarmType
        //{
        //    get
        //    {
        //        return AlarmType.GetAlarmTypeByDegree(AlarmDegree);
        //    }
        //}


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 父级设备节点 ID
        /// </summary>
        [DataMember]
        public virtual string ParentSegmentID
        {
            get;
            set;
        }

        ////-----------------------------------------------------------------------------------------------------------------------
        //// Author: GuoKaile
        //// Time: 2011-07-25
        ///// <summary>
        ///// 设备节点名称 
        ///// </summary>
        [IgnoreDataMember]
        public string DevSegmentName
        {
            get;
            set;
        }

    }
}
