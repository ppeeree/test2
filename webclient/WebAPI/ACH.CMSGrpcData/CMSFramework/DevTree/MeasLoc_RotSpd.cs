using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2011-08-15
    /// <summary>
    /// 转速测量位置
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class MeasLoc_RotSpd : MeasLocation
    {
        //-----------------------------------------------------------------------------------------------------------------------
        // author: GuoKaile
        // create: 2013-04-3
        /// <summary>
        /// TurnRatio 变速比
        /// </summary>
        [DataMember]
        public float GearRatio
        {
            get;
            set;
        }

        /// <summary>
        /// 编码器线数
        /// </summary>
        [DataMember]
        public int LineCounts
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author:steel
        // create:2015-10-16
        /// <summary>
        /// EF Mapping
        /// </summary>
        [IgnoreDataMember]
        public WindTurbine DevWindTurbine
        {
            get;
            set;
        }
    }
}
