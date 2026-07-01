using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:ZhangMai
    // create:2013-04-26
    /// <summary>
    /// 晃度仪寄存器晃度测量位置关联实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class MeasLoc_SVM : MeasLocation
    {
        /// <summary>
        /// 参数类型
        /// </summary>
        [DataMember]
        public EnumSVMParamType ParamType
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ComponentID 部件ID
        /// </summary>
        [DataMember]
        public string ComponentID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SectionName 截面
        /// </summary>
        [DataMember]
        public string SectionName
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
        public WindTurbineComponent DevTurComponent
        {
            get;
            set;
        }
    }
}
