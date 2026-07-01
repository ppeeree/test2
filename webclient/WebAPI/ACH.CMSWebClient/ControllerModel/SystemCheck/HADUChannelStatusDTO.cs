using ACH.Alarm.Entity;
using NetTaste;
using Renci.SshNet.Messages;
using System.Security.Claims;
using System.Threading;

namespace ACH.CMSWebClient.ControllerModel.SystemCheck
{
    /// <summary>
    /// HADU通道状态
    /// </summary>
    public class HADUChannelStatusDTO
    {
        /// <summary>
        /// 表格唯一ID
        /// </summary>
        public string Guid { set; get; }

        /// <summary>
        /// 通道号
        /// </summary>
        public int ChannelNum { set; get; }

        /// <summary>
        /// 测点ID
        /// </summary>
        public string MeaslocID { set; get; }

        /// <summary>
        /// 测点名称
        /// </summary>
        public string MeaslocName { set; get; }

        /// <summary>
        /// 通道状态
        /// </summary>
        public string ChannelStatus { set; get; }

        /// <summary>
        /// 通道状态时间
        /// </summary>
        public string ChannelStatusTime { set; get; }

        /// <summary>
        /// 判定数值
        /// </summary>
        public string Value { set; get; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { set; get; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="guid">表格唯一ID</param>
        /// <param name="channel">通道号</param>
        /// <param name="measLoctionName">测点名称</param>
        /// <param name="measlocID">测点ID</param>
        /// <param name="unit">单位</param>
        /// <param name="alarms">该测点的通道状态</param>
        public HADUChannelStatusDTO(string guid, int channel, string measLoctionName, string measlocID, string unit, ChannelStatusAlarm? alarm)
        {
            string status = "--";
            if (alarm != null)
            {
                status = alarm.ChannelStatus == EnumChannelStatus.Normal ? "正常" : "故障";
            }

            Guid = guid;
            ChannelNum = channel;
            MeaslocName = measLoctionName;
            MeaslocID = measlocID;
            ChannelStatus = status;
            ChannelStatusTime = alarm != null ? alarm.AcqTime.ToString("yyyy-MM-dd HH:mm:ss") : "--";
            Value = alarm != null && alarm.Voltage != null ? Math.Round(alarm.Voltage.Value, 2).ToString("0.00") : "--";
            Unit = unit;
        }
    }
}
