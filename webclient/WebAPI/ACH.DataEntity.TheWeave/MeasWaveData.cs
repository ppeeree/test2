using System;

namespace ACH.DataEntity.TheWeave
{
    public class MeasWaveData
    {
        public DateTime AcqTime { get; set; }
        public double[] WaveData { get; set; }
        public string? WavePath { get; set; }

        public string CompId { get; set; }
        public string CompName { get; set; }
        public string MeaslocCode { get; set; }
        public string MeaslocId { get; set; }
        public string MeaslocName { get; set; }
        public double? RMS { get; set; }

        public double? RoteSpd { get; set; }
        public int SampleRate { get; set; }
        public int SamplePoint { get; set; }

        public string WaveType { get; set; }
        public int WaveLength { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string? WindParkName { get; set; }
    }
}
