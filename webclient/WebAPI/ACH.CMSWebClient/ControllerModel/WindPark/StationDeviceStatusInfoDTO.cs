namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationDeviceStatusInfoDTO
    {
        /// <summary>
        /// 机组ID
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 机组名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 机组型号名称
        /// </summary>
        public string DeviceModel { get; set; }

        /// <summary>
        /// 机组型号类型
        /// </summary>
        public string DeviceModelType { get; set; }

        /// <summary>
        /// 机组厂商
        /// </summary>
        public string DeviceVindor { get; set; }

        /// <summary>
        /// 机组状态
        /// </summary>
        public string DeviceStatus { get; set; }

        /// <summary>
        /// 投运日期
        /// </summary>
        public string OperationalDate { get; set; }

        /// <summary>
        /// 机组经度
        /// </summary>
        public string Longitude { get; set; }

        /// <summary>
        /// 风机纬度
        /// </summary>
        public string Latitude { get; set; }

        /// <summary>
        /// 风机海拔
        /// </summary>
        public string Elevation { get; set; }

        /// <summary>
        /// 风机运行率
        /// </summary>
        public string PoweringRate { get; set; }
    }
}
