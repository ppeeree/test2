using ACH.DataEntity.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.DataEntity.StatusTree
{
    /// <summary>
    /// 构建设备树 - 测点层 （DT表示DeviceTree，设备树）
    /// </summary>
    public class DTMeaslocStatus
    {
        /// <summary>
        /// 测点Code
        /// </summary>
        public string MeaslocCode { get; set; }

        /// <summary>
        /// 测点ID
        /// </summary>
        public string MeaslocId { get; set; }

        /// <summary>
        /// 测点名称
        /// </summary>
        public string MeaslocName { get; set; }

        /// <summary>
        /// 测点状态
        /// </summary>
        public EnumAlarmStatus MeaslocStatus
        {
            get
            {
                if (EigenValueList == null || EigenValueList.Count == 0)
                {
                    return EnumAlarmStatus.Normal;
                }
                return EigenValueList.Max(x => x.EvStatus);
            }
        }

        /// <summary>
        /// 测点状态时间
        /// </summary>
        public DateTime MeaslocStatusTime
        {
            get
            {
                if (EigenValueList == null || EigenValueList.Count == 0)
                {
                    return DateTime.MinValue;
                }
                return EigenValueList.Max(x => x.EvStatusTime);
            }
        }

        /// <summary>
        /// 特征值列表
        /// </summary>
        public List<DTEvStatus> EigenValueList { get; set; }


        /// <summary>
        /// 是否有诊断结果
        /// </summary>
        public bool IsDiagnosticResults { get; set; }

        /// <summary>
        /// 诊断时间
        /// </summary>
        public DateTime? DiagnosisTime { get; set; }

        /// <summary>
        /// 诊断状态
        /// </summary>
        public string? DiagnosisStatus { get; set; }

        /// <summary>
        /// 诊断结论
        /// </summary>
        public string? DiagnosisConclusion { get; set; }


    }
}
