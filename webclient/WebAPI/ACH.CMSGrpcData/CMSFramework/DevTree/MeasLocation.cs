using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2011-06-18
    /// <summary>
    /// 采集位置实体基类
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [KnownType(typeof(MeasLoc_Process))]
    [KnownType(typeof(MeasLoc_RotSpd))]
    [KnownType(typeof(MeasLoc_Vib))]
    public abstract class MeasLocation
    {
        //-----------------------------------------------------------------------------------------------------------------------
        // removed by GuoKaile time 2012-02-16
        /// <summary>
        /// 构造器
        /// </summary>
        //public MeasLocation()
        //{            
        //    // add by lidan @2011-12-14 （初始化，欲移除属性）
        //    ChannelNumber = "1";
        //}

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasLocationID 采集位置ID
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public string MeasLocationID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineID 机组ID
        /// </summary>
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasLocName 测量位置名称
        /// </summary>
        [DataMember]
        public string MeasLocName
        {
            get;
            set;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        // author: steel
        // create: 2011-07-21
        /// <summary>
        /// 采集位置顺序
        /// </summary>
        [DataMember]
        public int OrderSeq
        {
            get;
            set;
        }

        /*
        //-----------------------------------------------------------------------------------------------------------------------
        // author: yangming
        // create: 2012-03-28
        /// <summary>
        /// 工程单位
        /// </summary>
        [DataMember]
        public string Unit
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // modified by steel @ 2011-07-21去除该字段
        /// <summary>
        /// ds_asset_id 设备编号
        /// </summary>
        [Obsolete("not need it anymore.", true)]
        [DataMember]
        public string Ds_asset_id
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// DAUName 设备名称
        /// </summary>
        [DataMember]
        public string DAUName
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ChannelNumber 通道编号
        /// </summary>
        [DataMember]
        public string ChannelNumber
        {
            get;
            set;
        }

        */

        //-----------------------------------------------------------------------------------------------------------------------
        // author: GuoKaile 
        // time: 2011-08-24
        public override string ToString()
        {
            return this.MeasLocName;
        }

    }
}
