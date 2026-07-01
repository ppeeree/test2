using ACH.Helper.Comparer;

namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    /// <summary>
    /// 诊断分析页面设备树接口返回值的基本类 - DT表示DeviceTree（设备树）
    /// </summary>
    public class DTStatusBasic : ISortable
    {
        public string? Name { get; set; }
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? Time { get; set; }

        public string GetSortableName() => Name ?? string.Empty;
    }


    /// <summary>
    /// 诊断分析左侧设备树接口返回值 - 风场层
    /// </summary>
    public class DTStationStatusDTO : ISortable
    {
        public string? Name { get; set; }
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Type { get; set; }
        public List<DTDeviceStatusDTO>? Children { get; set; }

        public string GetSortableName() => Name ?? string.Empty;
    }

    /// <summary>
    /// 诊断分析设备树接口 - 机组层
    /// </summary>
    public class DTDeviceStatusDTO : DTStatusBasic
    {
        public List<DTBigCompStatusDTO>? Children { get; set; }
    }
    /// <summary>
    /// 诊断分析设备树接口- 大部件层
    /// </summary>
    public class DTBigCompStatusDTO : DTStatusBasic
    {
        public List<DTCompStatusDTO>? Children { get; set; }
    }

    /// <summary>
    /// 诊断分析设备树接口- 实体部件层   
    /// </summary>
    public class DTCompStatusDTO : DTStatusBasic
    {
        public List<DTMeaslocStatusDTO> Children { get; set; }
    }

    /// <summary>
    /// 诊断分析设备树接口-测点层  
    /// </summary>
    public class DTMeaslocStatusDTO : DTStatusBasic
    {
        public bool IsDiagnosticResults { get; set; }
        public DateTime? DiagnosisTime { get; set; } // 诊断时间
        public string? DiagnosisStatus { get; set; } // 诊断状态
        public string? DiagnosisConclusion { get; set; } // 诊断结论

        public List<DTEvStatusDTO> Children { get; set; }
    }

    /// <summary>
    /// 诊断分析设备树接口- 特征值层
    /// </summary>
    public class DTEvStatusDTO : DTStatusBasic
    {
        public string? SignalTypeName { get; set; }
        public string? SignalTypeCode { get; set; }
        public double Value { get; set; }
        public string? Unit { get; set; }
    }
}
