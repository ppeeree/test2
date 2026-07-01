using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2011-06-08
    /// <summary>
    /// 报警类型实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class AlarmType
    {
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmDegree 报警等级
        /// </summary>
        [DataMember]
        public EnumAlarmDegree AlarmDegree
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmTypeName 报警类型名称
        /// </summary>
        [DataMember]
        public string AlarmTypeName
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmTypeColor 报警颜色定义
        /// </summary>
        [DataMember]
        public string AlarmTypeColor
        {
            get;
            set;
        }
 
        /// <summary>
        /// 未知
        /// </summary>
        public static AlarmType AlarmType_Unknown = new AlarmType { 
            AlarmDegree = EnumAlarmDegree.AlarmDeg_Unknown, AlarmTypeColor = "#A0A0A0", AlarmTypeName = "未知" };//696969
        
        /// <summary>
        /// 正常
        /// </summary>
        public static AlarmType AlarmType_Normal = new AlarmType {
            AlarmDegree = EnumAlarmDegree.AlarmDeg_Normal, AlarmTypeColor = "#00FF00", AlarmTypeName = "正常" };//008000
         
        ///// <summary>
        ///// 系统异常
        ///// </summary>
        //public static AlarmType AlarmType_CommunicationError = new AlarmType { 
        //    AlarmDegree = EnumAlarmDegree.AlarmDeg_SystemError, AlarmTypeColor = "#4169E1", AlarmTypeName = "系统异常" };
        
        /// <summary>
        /// 注意
        /// </summary>
        public static AlarmType AlarmType_Warning = new AlarmType { 
            AlarmDegree = EnumAlarmDegree.AlarmDeg_Warning, AlarmTypeColor = "#FFFF00", AlarmTypeName = "注意" };
         
        /// <summary>
        /// 危险
        /// </summary>
        public static AlarmType AlarmType_Alarm = new AlarmType
        {
            AlarmDegree = EnumAlarmDegree.AlarmDeg_Alarm,
            AlarmTypeColor = "#FF0000",
            AlarmTypeName = "危险"
        };

        ///// <summary>
        ///// 严重
        ///// </summary>
        //public static AlarmType AlarmType_AlarmSerious = new AlarmType
        //{
        //    AlarmDegree = EnumAlarmDegree.AlarmDeg_AlarmSerious,
        //    AlarmTypeColor = "#FF0000",
        //    AlarmTypeName = "严重报警"
        //};

        ///// <summary>
        ///// 轻微覆冰
        ///// </summary>
        //public static AlarmType AlarmType_ICECommon = new AlarmType
        //{
        //    AlarmDegree = EnumAlarmDegree.AlarmDeg_ICECommon,
        //    AlarmTypeColor = "#A6C8F3",
        //    AlarmTypeName = "轻微覆冰"
        //};

        ///// <summary>
        ///// 轻微覆冰
        ///// </summary>
        //public static AlarmType AlarmType_ICESerious = new AlarmType
        //{
        //    AlarmDegree = EnumAlarmDegree.AlarmDeg_ICESerious,
        //    AlarmTypeColor = "#A6C8F3",
        //    AlarmTypeName = "严重覆冰"
        //};

        ///// <summary>
        ///// 损伤
        ///// </summary>
        //public static AlarmType AlarmType_Harm = new AlarmType
        //{
        //    AlarmDegree = EnumAlarmDegree.AlarmDeg_Harm,
        //    AlarmTypeColor = "#34AADC",
        //    AlarmTypeName = "损伤"
        //};

        //------------------------------------------------------------------------------------------------------------------
        // Author: steel
        // Create: 2011-09-28
        /// <summary>
        /// 获取报警类型
        /// </summary>
        /// <param name="_alarmDeg"></param>
        /// <returns></returns>
        public static AlarmType GetAlarmTypeByDegree(EnumAlarmDegree _alarmDeg)
        {
            switch (_alarmDeg)
            {
                case EnumAlarmDegree.AlarmDeg_Unknown:
                    return AlarmType_Unknown;

                case EnumAlarmDegree.AlarmDeg_Normal:
                    return AlarmType_Normal;

                //case EnumAlarmDegree.AlarmDeg_SystemError:
                //    return AlarmType_CommunicationError;

                case EnumAlarmDegree.AlarmDeg_Warning:
                    return AlarmType_Warning;

                //case EnumAlarmDegree.AlarmDeg_Alarm:
                //    return AlarmType_Alarm;

                //case EnumAlarmDegree.AlarmDeg_ICECommon:
                //    return AlarmType_ICECommon;

                //case EnumAlarmDegree.AlarmDeg_ICESerious:
                //    return AlarmType_ICESerious;

                //case EnumAlarmDegree.AlarmDeg_Harm:
                //    return AlarmType_Harm;

                //case EnumAlarmDegree.AlarmDeg_AlarmSerious:
                //    return AlarmType_AlarmSerious;
                default:
                    return AlarmType_Unknown;
            }
        }

    }
}
