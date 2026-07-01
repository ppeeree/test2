using CMSFramework.BusinessEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    public class AlarmStatus_TowCom: AlarmStatus_BladeCom
    {
        /// <summary>
        /// 测量位置ID
        /// </summary>
        public string MeasLocID
        {
            get;
            set;
        }
        /// <summary>
        /// 塔基实际倾斜角度
        /// 正常：AlarmDeg_Normal  
        /// 一级表面损伤：AlarmDeg_Warning
        /// 二级表面损伤：AlarmDeg_Alarm
        /// </summary>
        public EnumAlarmDegree TowAlarmDegree
        {
            get;            
            set;
        }        
    }
}
