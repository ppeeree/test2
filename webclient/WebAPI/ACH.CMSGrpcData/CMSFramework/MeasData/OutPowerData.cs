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
    /// 输出功率数据实体  逐渐淘汰此类
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [Obsolete("no Use", false)]
    public partial class OutPowerData : WorkingConditionData
    {
        //-----------------------------------------------------------------------------------------------------------------------
        public OutPowerData()
        {
            this.Param_Type_Code = EnumWorkCondition_ParamType.WCPT_Power;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AvePowerValue 输出功率
        /// </summary>
        [DataMember]
        public double AvePowerValue
        {
            get
            {
                return this.Param_Value;
            }
            set
            {
                this.Param_Value = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MaxPowerValue 最大值
        /// </summary>
        [DataMember]
        public float MaxPowerValue
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MinPowerValue 最小值
        /// </summary>
        [DataMember]
        public float MinPowerValue
        {
            get;
            set;
        }

    }
}
