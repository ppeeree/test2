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
    /// 油液数据 实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [Serializable]
    public class OilData
    {
        /// <summary>
        /// 磨粒数据
        /// </summary>
        [DataMember]
        public OilParticleData ParticleData
        {
            get;
            set;
        }

        /// <summary>
        /// 水分数据
        /// </summary>
        [DataMember]
        public OilWaterData WaterData
        {
            get;
            set;
        }

        /// <summary>
        /// 粘度数据
        /// </summary>
        [DataMember]
        public OilViscosityData ViscosityData
        {
            get;
            set;
        }
    }
}
