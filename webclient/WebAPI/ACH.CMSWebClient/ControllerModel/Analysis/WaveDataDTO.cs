namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    public class WaveDataDTO
    {
        public string BandWidth { get; set; } = "";
        public string CompName { get; set; } = "";
        public object dataKv { get; set; } = "";
        public List<List<double>> DataVOS { get; set; } = new();

        public string MeaslocId { get; set; } = "";
        public string MeaslocName { get; set; } = "";

        public int Peak { get; set; } = 0;
        public string power { get; set; } = "";
        public double Rms { get; set; } = 0;
        public double Rotatespeed { get; set; } = 0;
        public double SampleRate { get; set; } = 0;

        public string Samplingtimelength { get; set; } = "";
        public string Temp { get; set; } = "";
        public string Time { get; set; } = "";

        public string UnitX { get; set; } = "";
        public string UnitY { get; set; } = "";
        public string WaveCategory { get; set; } = "";
        public int WaveLength { get; set; } = 0;
        public string WindParkName { get; set; } = "";

        public string WindSpd { get; set; } = "";
        public string WindturbineId { get; set; } = "";
        public string WindturbineName { get; set; } = "";
    }
}
