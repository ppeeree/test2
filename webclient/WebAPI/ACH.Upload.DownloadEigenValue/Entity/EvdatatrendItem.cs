using System;

namespace ACH.Upload.DownloadEigenValue.Entity
{
    public class EvdatatrendItem
    {
        public string WindTurbineID { get; set; }
        public string MeasLocationID { get; set; }
        public string EigenValueID { get; set; }
        public DateTime AcquisitionTime { get; set; }
        public string EigenValueCode { get; set; }
        public int SamplingTime { get; set; }
        public int WkConLevelCode { get; set; }
        public double EigenValue { get; set; }
        public int AlarmDegree { get; set; }
        public string MeasDefinitionID { get; set; }
        public string WaveDefinitionID { get; set; }
        public int DataQualType { get; set; }
    }
}
