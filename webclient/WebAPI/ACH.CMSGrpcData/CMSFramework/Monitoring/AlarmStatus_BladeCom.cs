using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    public class AlarmStatus_BladeCom
    {
        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindTurbineID
        {
            get;
            set;
        }

        /// <summary>
        /// 部件ID
        /// </summary>
        public string ComponentID
        {
            get;
            set;
        }

        /// <summary>
        /// AlarmUpdateTime 状态更新时间
        /// </summary>
        public DateTime AlarmUpdateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 表面损伤等级  StatusCode个位
        /// 正常：AlarmDeg_Normal  
        /// 一级表面损伤：AlarmDeg_Warning
        /// 二级表面损伤：AlarmDeg_Alarm
        /// </summary>
        public EnumAlarmDegree SurfaceDmgAlarmDegree
        {
            get
            {
                return GetAlarmDegree(1);
            }
            set
            {
                SetUnitNumber(value, 1);
            }
        }

        /// <summary>
        /// 结构损伤等级  StatusCode十位
        /// 正常：AlarmDeg_Normal  
        /// 结构损伤：AlarmDeg_Warning
        /// </summary>
        public EnumAlarmDegree StructDmgAlarmDegree
        {
            get
            {
                return GetAlarmDegree(2);
            }
            set
            {
                SetUnitNumber(value, 2);
            }
        }

        /// <summary>
        /// 覆冰等级  StatusCode百位
        /// 正常：AlarmDeg_Normal  
        /// 一般覆冰：AlarmDeg_Warning  
        /// 严重覆冰：AlarmDeg_Alarm
        /// </summary>
        public EnumAlarmDegree IceAlarmDegree
        {
            get
            {
                return GetAlarmDegree(3);
            }
            set
            {
                SetUnitNumber(value, 3);
            }
        }

        /// <summary>
        /// 桨距角偏差等级 StatusCode千位
        /// 正常：AlarmDeg_Normal  
        /// 桨距角偏差：AlarmDeg_Warning
        /// </summary>
        public EnumAlarmDegree PbAlarmDegree
        {
            get
            {
                return GetAlarmDegree(4);
            }
            set
            {
                SetUnitNumber(value, 4);
            }
        }


        /// <summary>
        /// 状态码，存储数据库改用此信息
        /// </summary>
        internal Int16 StatusCode
        {
            get;
            private set;
        }


        private EnumAlarmDegree GetAlarmDegree(int position)
        {
            int unitNumber = GetUnitNumber(StatusCode, position);

            return (EnumAlarmDegree)unitNumber;
        }

        /// <summary>获取某一位上的数字
        /// </summary>
        /// <param name="number"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private int GetUnitNumber(int number, int position)
        {
            //权重
            int weight = (int)Math.Pow(10, position);

            return (number - number / weight * weight) * 10 / weight;
        }


        /// <summary> 设置某一位的值      
        /// </summary>
        /// <param name="degree"></param>
        /// <param name="position"></param>
        private void SetUnitNumber(EnumAlarmDegree degree,int position)
        {
            int weight= (int)Math.Pow(10, position-1);
            int oldUnitNumber = GetUnitNumber(StatusCode, position) * weight;
            int newUnitNumber = ((int)degree) * weight;

            StatusCode=(short)(StatusCode - oldUnitNumber + newUnitNumber);
        }
    }
}
