using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2011-06-09
    /// <summary>
    /// 转速波形数据实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public partial class RotSpeedWaveData
    {
        public RotSpeedWaveData()
        {
            this.Data_Qual_Type = EnumDataQualityType.NotAcquired;

            // 默认是1 
            //Remove By Maxy@2014-06-4 取消默认值
            //this.LineCounts = 1;
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
        /// MeasDefinitionID 测量定义ID
        /// </summary>
        [DataMember]
        public string MeasDefinitionID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasLocationID 采集位置ID
        /// </summary>
        [DataMember]
        public string MeasLocationID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AcquisitionTime 采集时间
        /// </summary>
        [DataMember]
        public DateTime AcquisitionTime
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SampleRate 采样频率
        /// </summary>
        //[DataMember]
        //[Obsolete("not need it at all", true)]
        //public double SampleRate
        //{
        //    get;
        //    set;
        //}

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WaveLength 数据长度 点数
        /// </summary>
        [DataMember]
        public int? WaveLength
        {
            get;
            set;
        }

        /// <summary>
        /// 波形路径
        /// </summary>
        [DataMember]
        public string WaveDataPath
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ConvertCoefficient 转换系数
        /// </summary>
        //[DataMember]
        //[Obsolete("not need it at all", true)]
        //public double ConvertCoefficient
        //{
        //    get;
        //    set;
        //}

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WaveData 波形数据，每个点64位double格式
        /// steel@2012-8-27修改为：每个点32位Float格式
        /// </summary>
        [DataMember]
        public byte[] WaveData
        {
            get;
            set;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 转速波形的线数
        /// </summary>
        [DataMember]
        public int? LineCounts
        {
            get;
            set;
        }

        /// <summary>
        /// 变速比
        /// </summary>
        [DataMember]
        public float? GearRatio
        {
            get;
            set;
        }        

        //-----------------------------------------------------------------------------------------------------------------------
        // author:lidan
        // create:2011-08-12
        /// <summary>
        /// Data_Qual_Type 数据质量
        /// </summary>
        [DataMember]
        public EnumDataQualityType Data_Qual_Type
        {
            get;
            set;
        }

        /// <summary>
        /// EF Mapping.
        /// </summary>
        [IgnoreDataMember]
        public MeasEvent_Wave WFDataMeasEvent
        {
            get;
            set;
        }
        /// <summary>
        /// DAU 时钟单位
        /// </summary>
        //public const double WINDDAUTICK = 1.0 / 150000000;
    }
}
