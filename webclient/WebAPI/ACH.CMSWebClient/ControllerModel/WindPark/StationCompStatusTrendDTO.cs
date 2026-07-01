namespace ACH.CMSWebClient.ControllerModel.WindPark
{
    public class StationCompStatusTrendDTO
    {
        public List<string> Time { get; set; }
        public List<CompTrendList> FaultStatusList { get; set; }
    }

    public class CompTrendList
    {
        public string? CompCode { get; set; }
        public string? CompName { get; set; }
        public List<List<int>> FaultCount { get; set; }
    }
}
