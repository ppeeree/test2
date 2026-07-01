using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity
{
    [AttributeUsage(AttributeTargets.All)]
    internal class EngUnitAttibute : Attribute
    {
        public readonly string EngUnit;

        public EngUnitAttibute(string unit)  // url is a positional parameter
        {
            this.EngUnit = unit;
        }
    }


    public static class EnumWorkCondParamTypeHelper
    {
        /// <summary>
        /// 获取单位属性
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static string GetEngUnit(Enum en)
        {
            Type type = en.GetType();

            FieldInfo fi = en.GetType().GetField(en.ToString());

            EngUnitAttibute[] attributes =
                (EngUnitAttibute[])fi.GetCustomAttributes(
                typeof(EngUnitAttibute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].EngUnit;
            else
                return "未定义";
        }


        /// <summary>
        /// 获取单位属性
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        private static string GetDecscription(Enum en)
        {
            Type type = en.GetType();

            FieldInfo fi = en.GetType().GetField(en.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return "未定义";
        }


        /// <summary>
        /// 获取单位属性
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static List<KeyValuePair<int, string>> GetParamTypeList()
        {
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            foreach (EnumWorkCondition_ParamType e in Enum.GetValues(typeof(EnumWorkCondition_ParamType)))
            {
                list.Add(new KeyValuePair<int, string>((int)e, GetDecscription(e)));
            }

            return list;
        }
    }


    /// <summary>
    /// 工况参数定义，与原来版本一致
    /// </summary>
    public enum EnumWorkCondition_ParamType
    {
        /// <summary>
        /// 风速
        /// </summary>
        [EnumMember]
        [Description("风速")]
        [EngUnitAttibute("m/s")]
        WCPT_WindSpeed = 0,

        /// <summary>
        /// 输出功率
        /// </summary>
        [EnumMember]
        [Description("输出功率")]
        [EngUnitAttibute("KW")]
        WCPT_Power = 1,

        /// <summary>
        /// 发电机转速
        /// </summary>
        [EnumMember]
        [Description("发电机转速(CMS)")]
        [EngUnitAttibute("RPM")]
        WCPT_RotSpeed = 3,

        /// <summary>
        /// 主轴承温度
        /// </summary>
        [EnumMember]
        [Description("主轴承温度")]
        [EngUnitAttibute("℃")]
        WCPT_MBRTemp = 2,

        /// <summary>
        /// 偏航状态
        /// </summary>
        [EnumMember]
        [Description("偏航状态")]
        [EngUnitAttibute("")]
        WCPT_YAWState = 4,


        /// <summary>
        /// 齿轮箱高速轴温度
        /// </summary>
        [EnumMember]
        [Description("齿轮箱高速轴温度")]
        [EngUnitAttibute("℃")]
        WCPT_GB_HSSTemp = 5,

        /// <summary>
        /// 齿轮箱油温
        /// </summary>
        [EnumMember]
        [Description("齿轮箱油温")]
        [EngUnitAttibute("℃")]
        WCPT_GB_OilTemp = 6,

        /// <summary>
        /// 发电机驱动端轴承温度
        /// </summary>
        [EnumMember]
        [Description("发电机驱动端轴承温度")]
        [EngUnitAttibute("℃")]
        WCPT_GEN_DETemp = 7,

        /// <summary>
        /// 发电机非驱动端轴承温度
        /// </summary>
        [EnumMember]
        [Description("发电机非驱动端轴承温度")]
        [EngUnitAttibute("℃")]
        WCPT_GEN_NDETemp = 8,

        /// <summary>
        /// 发电机定子温度1
        /// </summary>
        [EnumMember]
        [Description("发电机定子温度1")]
        [EngUnitAttibute("℃")]
        WCPT_GEN_STR1Temp = 9,

        /// <summary>
        /// 发电机定子温度2
        /// </summary>
        [EnumMember]
        [Description("发电机定子温度2")]
        [EngUnitAttibute("℃")]
        WCPT_GEN_STR2Temp = 10,

        /// <summary>
        /// 发电机定子温度3
        /// </summary>
        [EnumMember]
        [Description("发电机定子温度3")]
        [EngUnitAttibute("℃")]
        WCPT_GEN_STR3Temp = 11,


        /// <summary>
        /// 机舱温度
        /// </summary>
        [EnumMember]
        [Description("机舱温度")]
        [EngUnitAttibute("℃")]
        WCPT_NAC_Temp = 12,

        /// <summary>
        /// 发电机转速
        /// </summary>
        [EnumMember]
        [Description("发电机转速(MCS)")]
        [EngUnitAttibute("RPM")]
        WCPT_RotSpeed_MCS = 13,

        ///// <summary>
        ///// 桨距角1，用于只配置一个参数情况
        ///// </summary>
        //[EnumMember]
        //[Description("桨距角1")]
        //[EngUnitAttibute("°")]
        //WCPT_BLD_PitchAngle = 13,

        /// <summary>
        /// 航向角, 机头方向与正北的夹角
        /// </summary>
        [EnumMember]
        [Description("航向角")]
        [EngUnitAttibute("°")]
        WCPT_NAC_Direction = 14,


        /// <summary>
        /// BVM 叶片变桨角度
        /// </summary>
        [EnumMember]
        [Description("桨距角1")]
        [EngUnitAttibute("°")]
        WCPT_BLD_Angle1 = 16,

        /// <summary>
        /// BVM 叶片变桨角度
        /// </summary>
        [EnumMember]
        [Description("桨距角2")]
        [EngUnitAttibute("°")]
        WCPT_BLD_Angle2 = 17,

        /// <summary>
        /// BVM 叶片变桨角度
        /// </summary>
        [EnumMember]
        [Description("桨距角3")]
        [EngUnitAttibute("°")]
        WCPT_BLD_Angle3 = 18,


        /// <summary>
        /// 沈阳平台，诊断要求
        /// </summary>
        [EnumMember]
        [Description("变桨电机1温度")]
        [EngUnitAttibute("℃")]
        WCPT_Pitch_Motor1Temp = 19,

        /// <summary>
        /// 沈阳平台，诊断要求
        /// </summary>
        [EnumMember]
        [Description("变桨电机2温度")]
        [EngUnitAttibute("℃")]
        WCPT_Pitch_Motor2Temp = 20,

        /// <summary>
        /// 沈阳平台，诊断要求
        /// </summary>
        [EnumMember]
        [Description("变桨电机3温度")]
        [EngUnitAttibute("℃")]
        WCPT_Pitch_Motor3Temp = 21,

        /// <summary>
        /// 沈阳平台，诊断要求
        /// </summary>
        [EnumMember]
        [Description("齿轮箱轴承温度1")]
        [EngUnitAttibute("℃")]
        WCPT_GBX_BRTemp1 = 22,

        /// <summary>
        /// 沈阳平台，诊断要求
        /// </summary>
        [EnumMember]
        [Description("齿轮箱轴承温度2")]
        [EngUnitAttibute("℃")]
        WCPT_GBX_BRTemp2 = 23,

        /// <summary>
        /// 沈阳平台，诊断要求
        /// </summary>
        [EnumMember]
        [Description("变频器机侧IGBT温度")]
        [EngUnitAttibute("℃")]
        WCPT_TD_MacTemp = 24,

        /// <summary>
        /// 沈阳平台，诊断要求
        /// </summary>
        [EnumMember]
        [Description("变频器网侧IGBT温度")]
        [EngUnitAttibute("℃")]
        WCPT_TD_NetTemp = 25,


        /// <summary>
        /// 沈阳平台，诊断要求, 含义？？
        /// </summary>
        [EnumMember]
        [Description("偏航功率")]
        [EngUnitAttibute("KW")]
        WCPT_YAW_Power = 26,


        /// <summary>
        ///  BVM，同偏航角度，数据采集中
        /// </summary>
        [EnumMember]
        [Description("机舱位置")]
        [EngUnitAttibute("")]
        WCPT_NAC_Location = 27,


        /// <summary>
        /// BVM，机舱方位与风向的夹角，未使用
        /// </summary>
        [EnumMember]
        [Description("机舱风向")]
        [EngUnitAttibute("")]
        WCPT_NAC_Wind = 28,


        [EnumMember]
        [Description("驱动方向塔筒振动幅度")]
        [EngUnitAttibute("")]
        WCPT_Tow_DEApe = 29,

        [EnumMember]
        [Description("非驱动方向塔筒振动幅度")]
        [EngUnitAttibute("")]
        WCPT_Tow_NDEApe = 30,

        /// <summary>
        /// BVM，轮毂内温度
        /// </summary>
        [EnumMember]
        [Description("轮毂温度")]
        [EngUnitAttibute("℃")]
        WCPT_HubTemp = 31,

        /// <summary>
        /// BVM，叶片温度
        /// </summary>
        [EnumMember]
        [Description("叶片1温度")]
        [EngUnitAttibute("℃")]
        WCPT_Blade_01Temp = 32,

        /// <summary>
        /// BVM，叶片温度
        /// </summary>
        [EnumMember]
        [Description("叶片2温度")]
        [EngUnitAttibute("℃")]
        WCPT_Blade_02Temp = 33,

        /// <summary>
        /// BVM，叶片温度
        /// </summary>
        [EnumMember]
        [Description("叶片3温度")]
        [EngUnitAttibute("℃")]
        WCPT_Blade_03Temp = 34,

        [EnumMember]
        [Description("油液磨粒")]
        [EngUnitAttibute("个")]
        WCPT_Oil_Debris = 35,

        /// <summary>
        /// BVM，偏航角度，用于判断是否偏航
        /// </summary>
        [EnumMember]
        [Description("偏航角度")]
        [EngUnitAttibute("°")]
        WCPT_YawAngle = 36,


        /// <summary>
        /// BVM 风向仪的输出，数据采集中
        /// </summary>
        [EnumMember]
        [Description("风向")]
        [EngUnitAttibute("")]
        WCPT_WindDirection = 37,


        /// <summary>
        /// BVM 数据采集中
        /// </summary>
        [EnumMember]
        [Description("环境温度")]
        [EngUnitAttibute("℃")]
        WCPT_Environment_Temp = 38,

        [EnumMember]
        [Description("基础倾斜X")]
        [EngUnitAttibute("°")]
        WCPT_Tower_TIMx = 39,

        [EnumMember]
        [Description("基础倾斜Y")]
        [EngUnitAttibute("°")]
        WCPT_Tower_TIMy = 40,
        [EnumMember]
        [Description("真实倾斜角度")]
        [EngUnitAttibute("°")]
        WCPT_Tower_TIMactrual = 41,

        [EnumMember]
        [Description("倾斜方向")]
        [EngUnitAttibute("rad")]
        WCPT_Tower_TIMoffset = 42,

        [EnumMember]
        [Description("TIM温度")]
        [EngUnitAttibute("rad")]
        WCPT_Tower_TIMTemperature = 43,

        [EnumMember]
        [Description("法兰1温度")]
        [EngUnitAttibute("℃")]
        WCPT_Tower_Flange_01Temp = 44,

        [EnumMember]
        [Description("法兰2温度")]
        [EngUnitAttibute("℃")]
        WCPT_Tower_Flange_02Temp = 45,

        [EnumMember]
        [Description("法兰3温度")]
        [EngUnitAttibute("℃")]
        WCPT_Tower_Flange_03Temp = 46,

        [EnumMember]
        [Description("法兰4温度")]
        [EngUnitAttibute("℃")]
        WCPT_Tower_Flange_04Temp = 47,

        [EnumMember]
        [Description("法兰5温度")]
        [EngUnitAttibute("℃")]
        WCPT_Tower_Flange_05Temp = 48,

        /// <summary>
        /// 安脉盛 机车监测 增加
        /// </summary>
        [EnumMember]
        [Description("轴承1温度")]
        AMS_WCPT_MBRTemp1 = 60,


        /// <summary>
        /// 安脉盛 机车监测 增加
        /// </summary>
        [EnumMember]
        [Description("轴承2温度")]
        AMS_WCPT_MBRTemp2 = 61,

        /// <summary>
        /// 安脉盛 机车监测 增加
        /// </summary>
        [EnumMember]
        [Description("轴承3温度")]
        AMS_WCPT_MBRTemp3 = 62,

#if NGC

        [EnumMember]
        [Description("油液颗粒级别2")]
        WCPT_Oil_DebrisL2 = 80,

        [EnumMember]
        [Description("油液颗粒级别3")]
        WCPT_Oil_DebrisL3 = 81,

        [EnumMember]
        [Description("油液颗粒级别4")]
        WCPT_Oil_DebrisL4 = 82,

        [EnumMember]
        [Description("油液颗粒级别5")]
        WCPT_Oil_DebrisL5 = 83,

        [EnumMember]
        [Description("油液颗粒级别6")]
        WCPT_Oil_DebrisL6 = 84,

        [EnumMember]
        [Description("油液颗粒级别7")]
        WCPT_Oil_DebrisL7 = 85,

        [EnumMember]
        [Description("油液颗粒级别8")]
        WCPT_Oil_DebrisL8 = 86,

        [EnumMember]
        [Description("油液颗粒级别9")]
        WCPT_Oil_DebrisL9 = 87,

        [EnumMember]
        [Description("油液颗粒级别10")]
        WCPT_Oil_DebrisL10 = 88,

        [EnumMember]
        [Description("油液颗粒级别11")]
        WCPT_Oil_DebrisL11 = 89,

        [EnumMember]
        [Description("油液颗粒级别12")]
        WCPT_Oil_DebrisL12 = 90,

        [EnumMember]
        [Description("油液颗粒级别13")]
        WCPT_Oil_DebrisL13 = 91,

        [EnumMember]
        [Description("油液颗粒级别14")]
        WCPT_Oil_DebrisL14 = 92,

        [EnumMember]
        [Description("油液颗粒级别15")]
        WCPT_Oil_DebrisL15 = 93,

        [EnumMember]
        [Description("油液颗粒级别16")]
        WCPT_Oil_DebrisL16 = 94,
#endif

        /// <summary>
        /// 无工况
        /// </summary>
        [EnumMember]
        [Description("无工况")]
        WCPT_NOWORKCONDTION = 99
    }
}
