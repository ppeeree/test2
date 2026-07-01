namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class ControlMapInfoDTO
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }
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
        /// JSON文件内容
        /// </summary>
        public string JsonFile { get; set; }
    }
}

