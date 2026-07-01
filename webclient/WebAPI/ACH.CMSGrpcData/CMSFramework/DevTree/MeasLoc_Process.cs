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
    // create:2011-06-18
    /// <summary>
    /// 过程量采集位置实体
    /// </summary>    
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [KnownType(typeof(EnumWorkConDataSource))]
    public class MeasLoc_Process : MeasLocation
    {
        //-----------------------------------------------------------------------------------------------------------------------
        // removed by GuoKaile time 2012-02-16
        /// <summary>
        /// 构造器
        /// </summary>
        //public MeasLoc_Process()
        //{            
        //    // add by lidan @2011-12-14 （初始化，欲移除属性）
        //    FieldBusType = "ModbusOnTcp";
        //    ServerAddress = "192.168.20.51";
        //    ParmaChannelNumber = "10912";
        //}

        private EnumWorkCondition_ParamType _Param_Type_Code;

        //-----------------------------------------------------------------------------------------------------------------------
        // added by GuoKaile time 2011-08-15
        // modified by GuoKaile time 2012-03-31
        /// <summary>
        /// Param_Type_Code 参数类型编号
        /// </summary>
        [DataMember]
        public EnumWorkCondition_ParamType Param_Type_Code
        {
            get
            {
                return _Param_Type_Code;
            }
            set
            {
                _Param_Type_Code = value;

                if (value == EnumWorkCondition_ParamType.WCPT_Power)
                {
                    Eu_type_code = EngUnit.EngUnit_OutPower;
                }
                else if (value == EnumWorkCondition_ParamType.WCPT_RotSpeed)
                {
                    Eu_type_code = EngUnit.EngUnit_RotSpeed;
                }
                else if (value == EnumWorkCondition_ParamType.WCPT_WindSpeed)
                {
                    Eu_type_code = EngUnit.EngUnit_WindSpeed;
                }
                else if (value == EnumWorkCondition_ParamType.WCPT_NAC_Temp)
                {
                    Eu_type_code = EngUnit.EngUnit_Temperature;
                }
                else if (value == EnumWorkCondition_ParamType.WCPT_YAWState)
                {
                    // added by GuoKaile time 2012-03-31
                    Eu_type_code = string.Empty;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // added by ym time 2012-08-21 
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
        // added by GuoKaile time 2011-08-15
        /// <summary>
        /// Eu_type_code 工程单位编号
        /// </summary>
        [DataMember]
        public string Eu_type_code
        {
            get;

            // modifed by lidan @2011-9-27:"private" to "public"
            // private set;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// FieldBusType 工况数据来源
        /// </summary>
        [DataMember]
        public EnumWorkConDataSource FieldBusType
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ServerAddress 主控系统主机地址
        /// </summary>
        [DataMember]
        public string ServerAddress
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ParmaChannelNumber 变量寄存地址
        /// </summary>
        [DataMember]
        public string ParmaChannelNumber
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
        public WindTurbine DevWindTurbine
        {
            get;
            set;
        }
    }
}
