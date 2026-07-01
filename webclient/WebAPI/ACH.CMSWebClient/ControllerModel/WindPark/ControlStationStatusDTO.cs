using ACH.Helper.Comparer;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class ControlStationStatusDTO : ISortable
    {
        /// <summary>
        /// 风场ID
        /// </summary>
        public string StationID { get; set; }
        /// <summary>
        /// 风场名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 风场经度
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// 风场纬度
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// 风场海拔
        /// </summary>
        public double Elevation { get; set; }
        /// <summary>
        /// 视角经度
        /// </summary>
        public double ViewLongitude { get; set; }
        /// <summary>
        /// 视角纬度
        /// </summary>
        public double ViewLatitude { get; set; }
        /// <summary>
        /// 视角海拔
        /// </summary>
        public double ViewElevation { get; set; }
        /// <summary>
        /// 机组个数
        /// </summary>
        public int DeviceNum { get; set; }
        /// <summary>
        /// 正常机组个数
        /// </summary>
        public int NormalDeviceNum { get; set; }
        /// <summary>
        /// 注意机组个数
        /// </summary>
        public int AttentionDeviceNum { get; set; }
        /// <summary>
        /// 警告机组个数
        /// </summary>
        public int WarningDeviceNum { get; set; }
        /// <summary>
        /// 危险机组个数
        /// </summary>
        public int DangerDeviceNum { get; set; }

        public string GetSortableName() => StationName ?? string.Empty;
    }
}
