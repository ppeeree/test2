using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:steel
    // create:2011-06-08
    /// <summary>
    /// 工程单位
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public abstract class EngUnit
    {
        // --Begin added by GuoKaile time 2011-08-15         
        /// <summary>
        ///  风速(m/s)
        /// </summary>
        public const string EngUnit_WindSpeed = "3";
         
        /// <summary>
        /// 电压(V)
        /// </summary>
        public const string EngUnit_Voltage = "4";
         
        /// <summary>
        /// 加速度单位编号(m/s^2)
        /// </summary>
        public const string EngUnit_Accelerate = "11";
       
        /// <summary>
        /// 功率(KW)
        /// </summary>
        public const string EngUnit_OutPower = "12";
          
        /// <summary>
        /// 温度(℃)
        /// </summary>
        public const string EngUnit_Temperature = "13";
          
        /// <summary>
        /// 转速(RPM)
        /// </summary>
        public const string EngUnit_RotSpeed = "14";
 
        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public const string EngUnit_Hz = "15";

        // --End added by GuoKaile time 2011-08-15



        //-----------------------------------------------------------------------------------------------------------------------
        // add by steel @ 2016-5-5 
        /// <summary>
        /// 获取工况参数的单位
        /// </summary>
        /// <param name="_paramType"></param>
        /// <returns></returns>
        public static string GetWCEngUnitName(EnumWorkCondition_ParamType _paramType)
        {
            switch (_paramType)
            {
                case EnumWorkCondition_ParamType.WCPT_GB_HSSTemp:
                case EnumWorkCondition_ParamType.WCPT_GB_OilTemp:
                case EnumWorkCondition_ParamType.WCPT_GEN_DETemp:
                case EnumWorkCondition_ParamType.WCPT_GEN_NDETemp:
                case EnumWorkCondition_ParamType.WCPT_GEN_STR1Temp:
                case EnumWorkCondition_ParamType.WCPT_GEN_STR2Temp:
                case EnumWorkCondition_ParamType.WCPT_GEN_STR3Temp:
                case EnumWorkCondition_ParamType.WCPT_MBRTemp:
                case EnumWorkCondition_ParamType.WCPT_NAC_Temp:
                    return "℃";

                case EnumWorkCondition_ParamType.WCPT_Power:
                    return "KW";

                case EnumWorkCondition_ParamType.WCPT_BLD_Angle1:
                case EnumWorkCondition_ParamType.WCPT_BLD_Angle2:
                case EnumWorkCondition_ParamType.WCPT_BLD_Angle3:
                case EnumWorkCondition_ParamType.WCPT_NAC_Direction:
                    return "°";

                case EnumWorkCondition_ParamType.WCPT_RotSpeed:
                    return "RPM";

                case EnumWorkCondition_ParamType.WCPT_WindSpeed:
                    return "m/s";

                default:
                    return " ";
            }
        }
    }
}
