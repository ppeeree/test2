using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author: GuoKaile
    // create: 2012-08-27
    /// <summary>
    /// 转速单值数据实体 逐渐淘汰此类
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [Obsolete("no Use", false)]
    public class RotSpeedValue : WorkingConditionData
    {
        //-----------------------------------------------------------------------------------------------------------------------
        public RotSpeedValue()
        {
            this.Param_Type_Code = EnumWorkCondition_ParamType.WCPT_RotSpeed;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AveRotSpeed 输出转速
        /// </summary>
        [DataMember]
        public float AveRotSpeed
        {
            get
            {
                return (float)this.Param_Value;
            }
            set
            {
              this.Param_Value = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MaxRotSpeed 最大值
        /// </summary>
        [DataMember]
        public float MaxRotSpeed
        {
            get
            {
                return (float)this.MaxValue;
            }
            set
            {
                this.MaxValue = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MinRotSpeed 最小值
        /// </summary>
        [DataMember]
        public float MinRotSpeed
        {
            get
            {
                return (float)this.MinValue;
            }
            set
            {
                this.MinValue = value;
            }
        }
    }
}
