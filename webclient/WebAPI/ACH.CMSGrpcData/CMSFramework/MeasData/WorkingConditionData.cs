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
    /// 实时工况数据实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    //[KnownType(typeof(OutPowerData))]
    public class WorkingConditionData
    {
        // added by GuoKaile time 2011-08-15
        public WorkingConditionData()
        {
            this.Data_Qual_Type = EnumDataQualityType.NotAcquired;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineID 机组ID
        /// </summary>
        [DataMember(Order =1)]
        public string WindTurbineID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasDefinitionID 测量定义ID
        /// </summary>
        [DataMember(Order = 2)]
        public string MeasDefinitionID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasLocationID 测量位置ID
        /// </summary>
        [DataMember(Order = 3)]
        public string MeasLocationID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AcquisitionTime 采集时间
        /// </summary>
        [DataMember(Order = 4)]
        public DateTime AcquisitionTime
        {
            get;
            set;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Param_Type_Code 参数类型编号
        /// </summary>
        [DataMember(Order = 5)]
        public EnumWorkCondition_ParamType Param_Type_Code
        {
            get;
            set;
        }


        /* Comment by steel @ 2015-11-5
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Param_Type_Name 参数类型名称
        /// </summary>
        [DataMember]
        public string Param_Type_Name
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Eu_Type_Code 工程单位编号
        /// </summary>
        [DataMember]
        public string Eu_Type_Code
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Eu_Type_Name 工程单位名称
        /// </summary>
        [DataMember]
        public string Eu_Type_Name
        {
            get;
            set;
        }

        */

        /// <summary>
        /// 最大值
        /// </summary>
        [DataMember(Order = 6)]
        public double MaxValue
        {
            get;
            set;
        }

        /// <summary>
        /// 最小值
        /// </summary>
        [DataMember(Order = 7)]
        public double MinValue
        {
            get;
            set;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Param_Value 参数值
        /// </summary>
        [DataMember(Order = 8)]
        public double Param_Value
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
        [DataMember(Order = 9)]
        public EnumDataQualityType Data_Qual_Type
        {
            get;
            set;
        }
    }
}
