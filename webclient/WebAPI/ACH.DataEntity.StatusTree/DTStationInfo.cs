using System.Collections.Generic;

namespace ACH.DataEntity.StatusTree
{
    /// <summary>
    /// 构建设备树 - 风场层 （DT表示DeviceTree，设备树）
    /// </summary>
    public class DTStationInfo
    {
        /// <summary>
        /// 风场Code
        /// </summary>
        public string WindParkCode { get; set; } = "WindPark";

        /// <summary>
        /// 风场ID
        /// </summary>
        public string WindParkId { get; set; }

        /// <summary>
        /// 风场名称
        /// </summary>
        public string WindParkName { get; set; }

        /// <summary>
        /// 设备列表
        /// </summary>
        public List<DTDeviceStatus> DeviceList { get; set; }
    }
}
