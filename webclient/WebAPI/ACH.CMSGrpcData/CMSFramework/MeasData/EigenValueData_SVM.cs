using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class EigenValueData_SVM : EigenValueData
    {
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SVMRegister SVM寄存器地址
        /// </summary>
        [DataMember(Order = 10)]
        public int SVMRegister
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 特征值类型
        /// </summary>
        [DataMember(Order = 11)]
        public CMSFramework.BusinessEntity.SVM.EnumSVMEigenValueType EigenValueType
        {
            get;
            set;
        }
    }
}
