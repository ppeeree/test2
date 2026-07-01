using System.Collections.Generic;

namespace ACH.DataEntity.DevTree
{
    /// <summary>
    ///  DevTree数据库中DeviceInfo表 测点配置方案
    /// </summary>
    public class DeviceMeaslocPositopnPlan
    {
        /// <summary>
        /// 方案名称
        /// </summary>
        public string solutionName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime { get; set; }
        /// <summary>
        /// 方案详情
        /// </summary>
        public List<PlanCompModel> children { get; set; }

    }

    public class PlanCompModel
    {
        /// <summary>
        /// 是否展示特征值详情
        /// </summary>
        public string detailEv { get; set; }
        /// <summary>
        /// 方案名称
        /// </summary>
        public string solutionName { get; set; }
        /// <summary>
        /// 添加的机组模型方案ID
        /// </summary>
        public List<string> deviceModelList { get; set; }
        /// <summary>
        /// 方案名称
        /// </summary>
        public string planName { get; set; }
        /// <summary>
        /// 方案类型Code
        /// </summary>
        public string deviceCode { get; set; }
        /// <summary>
        /// 方案类型名称
        /// </summary>
        public string deviceModelType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> monitorDeviceProperty { get; set; }
        /// <summary>
        /// 监测的实体部件列表
        /// </summary>
        public List<string> monitorDevice { get; set; }
        /// <summary>
        /// 添加的测点和测点卡片位置
        /// </summary>
        public List<PlanMeaslocModel> measlocPositionList { get; set; }

    }

    public class PlanMeaslocModel
    {
        /// <summary>
        /// 测点Code
        /// </summary>
        public List<string> measlocCode { get; set; }
        /// <summary>
        /// 卡片位置
        /// </summary>
        public List<double> cardPosition { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 测点位置
        /// </summary>
        public List<double> spot { get; set; }
    }
}
