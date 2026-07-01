namespace ACH.CMSWebClient.ControllerModel.Config
{
    public class AddHADUConfigParam
    {
        public string StationID { get; set; }
        public string MapStationID { get; set; }
        public List<string> DeviceIDList { get; set; }
        public List<string> MeasLocList { get; set; }

    }

    /*public class DeviceIDMap
    {
        public string DeviceID { get; set; }
        public string MapDeviceID { get; set; }
    }

    public class MeasLocIDMap
    {
        public string MeasLocCode { get; set; }
        public string MeasType { get; set; }
        public string Channel { get; set; }
    }*/
}
