namespace ACH.CMSWebClient.ControllerModel.DownloadTask
{
    /// <summary>
    /// 数据下载页面设备树风场层
    /// </summary>
    public class DownloadDTStationDTO
    {
        public string StationName { get; set; }
        public string StationID { get; set; }
        public List<DownloadDTDeviceDTO> DeviceList { get; set; }
    }

    public class DownloadDTDeviceDTO
    {
        public string DeviceName { get; set; }
        public string DeviceID { get; set; }
        public List<DownloadDTMeaslocDTO> MeasLocList { get; set; }
    }

    public class DownloadDTMeaslocDTO
    {
        public string CompID { get; set; }
        public string CompName { get; set; }
        public string MeasLocID { get; set; }
        public string MeasLocName { get; set; }
    }
}
