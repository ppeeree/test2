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
    /// 油液粘度数据 实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [Serializable]
    public class OilViscosityData
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
        /// 动力粘度
        /// </summary>
        [DataMember]
        public float DynamicViscosity
        {
            get;
            set;
        }

        /// <summary>
        /// 运动粘度
        /// </summary>
        [DataMember]
        public float KinematicViscosity
        {
            get;
            set;
        }

        /// <summary>
        /// 密度
        /// </summary>
        [DataMember]
        public float Density
        {
            get;
            set;
        }

        /// <summary>
        /// 介电常数
        /// </summary>
        [DataMember]
        public float Permittivity
        {
            get;
            set;
        }

        /// <summary>
        /// 油温(from 粘度传感器)
        /// </summary>
        [DataMember]
        public float OilTempFromViscositySensor
        {
            get;
            set;
        }

    }
}
