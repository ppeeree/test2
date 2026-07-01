using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:lidan
    // create:2011-12-08
    /// <summary>
    /// 传感器直流分量数据 实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class SensorDCData
    {
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Ds_asset_id 设备编号
        /// </summary>
        [DataMember(Order = 1)]
//#if NET40
//        [System.ComponentModel.DataAnnotations.Key]
//#endif
        public string WindTurbineID
        {
            get;
            set;
        }


        [DataMember(Order = 6)]
        public string DAUId
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SensorID 通道号
        /// </summary>
        [DataMember(Order = 2)]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public int ChannelNumber
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AcquisitionTime 采集时间
        /// </summary>
        [DataMember(Order = 3)]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public DateTime DCAcquisitionTime
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SensorID 直流分量数据值
        /// </summary>
        [DataMember(Order = 4)]
        public double DCDataValue
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// eu_type_code 工程单位编号 EF No mapping
        /// </summary>
        [DataMember(Order = 5)]
        public string Eu_Type_Code
        {
            get;
            set;
        }        
    }
}
