using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 晃度仪波形数据
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class SVMWaveFormData : WaveFormData
    {
        /// <summary>
        /// 晃度仪ID EF Ignore
        /// </summary>
        [DataMember(Order = 18)]
        public string SvmID { get; set; }

      
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SVMRegister SVM寄存器地址
        /// </summary>
        [DataMember(Order = 19)]
        public int SVMRegister
        {
            get;
            set;
        }


        private EnumSVMParamType _paramType;
        /// <summary>
        /// SVM 数据类型
        /// </summary>
        [DataMember(Order = 20)]
        public EnumSVMParamType ParamType
        {
            get { return _paramType; }
            set { _paramType = value; 
                if(_paramType == EnumSVMParamType.Axisl ||
                    _paramType == EnumSVMParamType.Horizontal ||
                    _paramType == EnumSVMParamType.Vertical)
                {
                    this.WaveformType = EnumWaveFormType.WDF_SVM;
                }
                else
                {
                    this.WaveformType = EnumWaveFormType.WDF_SVMAGL;
                }
            }
        }

    }
}
