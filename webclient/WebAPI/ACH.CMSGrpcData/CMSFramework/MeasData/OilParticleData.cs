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
    /// 油液磨粒数据 实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [Serializable]
    public class OilParticleData
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
        /// 铁磁区间1磨粒数量
        /// </summary>
        [DataMember]
        public int Ferromagnetic1
        {
            get;
            set;
        }

        [DataMember]
        public int Ferromagnetic2
        {
            get;
            set;
        }

        [DataMember]
        public int Ferromagnetic3
        {
            get;
            set;
        }

        [DataMember]
        public int Ferromagnetic4
        {
            get;
            set;
        }

        [DataMember]
        public int Ferromagnetic5
        {
            get;
            set;
        }

        /// <summary>
        /// 非铁磁区间1磨粒数量
        /// </summary>
        [DataMember]
        public int Nonferromagnetic1
        {
            get;
            set;
        }

        [DataMember]
        public int Nonferromagnetic2
        {
            get;
            set;
        }

        [DataMember]
        public int Nonferromagnetic3
        {
            get;
            set;
        }

        [DataMember]
        public int Nonferromagnetic4
        {
            get;
            set;
        }

        [DataMember]
        public int Nonferromagnetic5
        {
            get;
            set;
        }

    }
}
