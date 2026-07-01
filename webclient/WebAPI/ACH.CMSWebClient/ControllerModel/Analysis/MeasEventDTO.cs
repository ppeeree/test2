using ACH.Helper.Comparer;

namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    public class MeasEventDTO : ISortable
    {
        public string AcqTime { get; set; }
        public string CompName { get; set; }
        public string MeaslocSummary { get; set; }
        public double RotSpeed { get; set; }
        public string WindParkId { get; set; }
        public string WindParkName { get; set; }
        public string WindturbineId { get; set; }
        public string WindturbineName { get; set; }
        public List<MeasEventWaveDTO> Children { get; set; }


        public string GetSortableName() => WindturbineId ?? string.Empty;
    }

    public class MeasEventWaveDTO : ISortable
    {
        public string AcqTime { get; set; }
        public string CompId { get; set; }
        public string CompName { get; set; }
        public string MeaslocId { get; set; }
        public string MeaslocName { get; set; }
        public string Rms { get; set; }
        public double SampleRate { get; set; }

        public int WaveLength { get; set; }
        public string WindturbineId { get; set; }
        public string WindturbineName { get; set; }
        public string WindParkName { get; set; }

        public string GetSortableName() => MeaslocName ?? string.Empty;
    }

}
