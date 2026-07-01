using ACH.DataEntity.Enum;

namespace ACH.DataEntity.TheWeave
{
    public class EvCalcConfig
    {
        public EvCalcConfig() { }
        public EvCalcConfig(EnumCompType compCode, string locCode, string evCode, string evType, double? startPara, double? endtPara, string eigenValueName, string unit
            , string signalTypeName, string signalTypeCode)
        {
            CompCode = compCode;
            LocCode = locCode;
            EigenValueCode = evCode;
            EigenType = evType;
            StartPara = startPara;
            EndtPara = endtPara;
            EigenValueName = eigenValueName;
            Unit = unit;
            SignalTypeName = signalTypeName;
            SignalTypeCode = signalTypeCode;
        }
        /// <summary>
        /// 部件Code
        /// </summary>
        public EnumCompType CompCode { get; set; }
        /// <summary>
        /// 测量位置Code
        /// </summary>
        public string LocCode { get; set; }
        /// <summary>
        /// 特征值Code
        /// </summary>
        public string EigenValueCode { get; set; }
        /// <summary>
        /// 特征值Type
        /// </summary>
        public string EigenType { get; set; }
        /// <summary>
        /// 起始参数
        /// </summary>
        public double? StartPara { get; set; }
        /// <summary>
        /// 结束参数
        /// </summary>
        public double? EndtPara { get; set; }
        /// <summary>
        /// 特征名称
        /// </summary>
        public string EigenValueName { get; set; }
        /// <summary>
        /// 特征单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 信号类型名称
        /// </summary>
        public string SignalTypeName { get; set; }
        /// <summary>
        /// 信号类型code
        /// </summary>
        public string SignalTypeCode { get; set; }
    }
}
