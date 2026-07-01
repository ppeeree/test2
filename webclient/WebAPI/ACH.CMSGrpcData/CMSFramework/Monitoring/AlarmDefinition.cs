using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:wangyan
    // create:2015年11月18日
    /// <summary>
    /// 报警设置实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class AlarmDefinition
    {
        public AlarmDefinition()
        {
            AlarmDefThresholdGroup = new List<AlarmDefThreshold>();
        }
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
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasLocationID 测量位置ID
        /// </summary>
        [DataMember]
        public string MeasLocationID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // create by steel @ 2011-07-21
        /// <summary>
        /// 特征值ID
        /// </summary>
        [DataMember]
        public string EigenValueID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // create by whr @ 2012-6-15
        /// <summary>
        /// 工况参数
        /// </summary>
        [DataMember]
        public short WorkConParameter
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 报警阈值组ID
        /// </summary>
        [DataMember]
        public string ThresholdGroup
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 测量位置类别
        /// </summary>
        [DataMember]
        public EnumMeasLocType MeasLocType
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 工况上线
        /// </summary>
        [DataMember]
        public double UpperLimitValue
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 工况下线
        /// </summary>
        [DataMember]
        public double LowerLimitValue
        {
            get;
            set;
        }

        public List<AlarmDefThreshold> AlarmDefThresholdGroup
        {
            get;
            set;
        }
    }
}
