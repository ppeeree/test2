using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:dds
    // create:2019-12-4
    /// <summary>
    /// 油液水分数据 实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [Serializable]
    public class OilWaterData
    {
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 机组ID 
        /// </summary>
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }

        /// <summary>
        /// OilUnitID
        /// </summary>
        [DataMember]
        public string OilUnitID
        {
            get;
            set;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AcquisitionTime 采集时间
        /// </summary>
        [DataMember]
        public DateTime OilAcquisitionTime
        {
            get;
            set;
        }

        
        /// <summary>
        /// 水含量
        /// </summary>
        [DataMember]
        public float WaterContent
        {
            get;
            set;
        }

        /// <summary>
        /// 水活性
        /// </summary>
        [DataMember]
        public float WaterActivity
        {
            get;
            set;
        }

        /// <summary>
        /// 油温(From水分传感器)
        /// </summary>
        [DataMember]
        public float OilTempFromWaterSensor
        {
            get;
            set;
        }

    }
}
