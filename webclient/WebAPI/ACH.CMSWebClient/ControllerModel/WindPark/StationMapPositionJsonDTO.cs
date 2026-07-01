namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationMapPositionJsonDTO
    {
        /// <summary>
        /// 风场ID
        /// </summary>
        public string StationID { get; set; }
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
    }
}
