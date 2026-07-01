namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class ControlStationInfoDTO
    {
        // 风场个数
        public int StationNum { get; set; }
        // 机组总数
        public int DeviceNum { get; set; }
        // 监控率 （1表示全部监控）
        public double StationControlRate { get; set; }
    }
}
