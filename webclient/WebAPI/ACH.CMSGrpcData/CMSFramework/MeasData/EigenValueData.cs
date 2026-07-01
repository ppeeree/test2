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
    /// 特征值实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [ProtoBuf.ProtoInclude(20, typeof(EigenValueData_Vib))]
    [ProtoBuf.ProtoInclude(21, typeof(EigenValueData_SVM))]
    [ProtoBuf.ProtoContract]
    [KnownType(typeof(EigenValueData_Vib))]
    [KnownType(typeof(EigenValueData_SVM))]
    public class EigenValueData
    {
        public EigenValueData()
        {
            this.Data_Qual_Type = EnumDataQualityType.NotAcquired;
            this.AlarmDegree = EnumAlarmDegree.AlarmDeg_Unknown;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasDefinitionID 采集定义ID
        /// </summary>
        [DataMember(Order = 1)]
        [ProtoBuf.ProtoMember(1)]  
        public string MeasDefinitionID
        {
            get;
            set;
        }
        /// <summary>
        /// 波形定义ID
        /// </summary>
        [DataMember(Order = 2)]
        [ProtoBuf.ProtoMember(2)]  
        public string WaveDefinitionID
        {
            get;
            set;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasLocationID 测量位置ID
        /// </summary>
        [DataMember(Order = 3)]
        [ProtoBuf.ProtoMember(3)]  
        public string MeasLocationID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineID 机组ID
        /// </summary>
        [DataMember(Order = 4)]
        [ProtoBuf.ProtoMember(4)]  
        public string WindTurbineID
        {
            get;
            set;
        }   

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AcquisitionTime 采集时间
        /// </summary>
        [DataMember(Order = 5)]
        [ProtoBuf.ProtoMember(5)]  
        public DateTime AcquisitionTime
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // changed by steel @ 2011-07-21
        /// <summary>
        /// 特征值ID
        /// </summary>
        [DataMember(Order = 6)]
        [ProtoBuf.ProtoMember(6)]  
        public string EigenValueID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 工程单位名称 None EF Mapping
        /// </summary>
        [IgnoreDataMember]
        public string EngUnitName
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// EigenValue 特征值
        /// </summary>
        [DataMember(Order = 7)]
        [ProtoBuf.ProtoMember(7)]  
        public double Eigen_Value
        {
            get;
            set;
        }

        
        //-----------------------------------------------------------------------------------------------------------------------
        // author:steel
        // create:2011-07-21
        /// <summary>
        /// 数据质量
        /// </summary>
        [DataMember(Order = 8)]
        [ProtoBuf.ProtoMember(8)]  
        public EnumDataQualityType Data_Qual_Type
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // author: GuoKaile
        // time: 2011-08-18
        /// <summary>
        /// AlarmDegree 报警等级
        /// </summary>
        [DataMember(Order = 9)]
        [ProtoBuf.ProtoMember(9)]  
        public EnumAlarmDegree AlarmDegree
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///  add by steel @ 2015-11-6 没用的字段 for EF
        /// </summary>
        [IgnoreDataMember]
        [Obsolete("no Use", false)]
        public int WkConLevelCode
        {
            get;
            set;
        }

        /// <summary>
        /// EF Mapping
        /// </summary>
        [IgnoreDataMember]
        public MeasEvent_EigenValue MeasEvent_EV
        {
            get;
            set;
        }
    }
}
