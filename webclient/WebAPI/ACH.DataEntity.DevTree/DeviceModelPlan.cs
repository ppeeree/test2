using System.Collections.Generic;

namespace ACH.DataEntity.DevTree
{
    /// <summary>
    /// DevTree中DeviceInfo表 三维模型方案
    /// </summary>
    public class DeviceModelPlan
    {
        /// <summary>
        /// 聚合部件名称（表示该模型中有几个子页面） 
        /// </summary>
        public string compName { get; set; }
        public List<PageCompChildren> children { get; set; }
    }
    public class PageCompChildren
    {
        /// <summary>
        /// 厂商
        /// </summary>
        public string manufacturer { get; set; }
        public List<ModelInfo> children { get; set; }
    }

    public class ModelInfo
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public long createTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string[] defaultpropertiesBasic { get; set; }
        /// <summary>
        /// 该设备名称
        /// </summary>
        public string deviceModel { get; set; }
        /// <summary>
        /// 该设备编号（NAC）
        /// </summary>
        public string deviceModelCode { get; set; }
        /// <summary>
        /// 设备类型（明阳）
        /// </summary>
        public string deviceModelType { get; set; }
        /// <summary>
        /// 设备ID（唯一标识）
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 厂商
        /// </summary>
        public string manufacturer { get; set; }
        public List<ModelProperties> propertiesBasic { get; set; }
        public List<ModelShowInfo> propertiesView { get; set; }
    }

    /// <summary>
    /// 模型展示信息
    /// </summary>
    public class ModelShowInfo
    {
        /// <summary>
        /// 相机位置 [x, y, z]
        /// </summary>
        public double[] cameraPosition { get; set; }
        /// <summary>
        /// 是否展示
        /// </summary>
        public bool isShow { get; set; }
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool isVisible { get; set; }
        /// <summary>
        /// 模型名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 模型文件路径
        /// </summary>
        public string path { get; set; }
        /// <summary>
        /// 模型坐标 [x, y, z]
        /// </summary>
        public double[] position { get; set; }
        /// <summary>
        /// 旋转角度 [x, y, z]
        /// </summary>
        public double[] rotate { get; set; }
        /// <summary>
        /// 缩放比例 [x, y, z]
        /// </summary>
        public double[] scale { get; set; }
        /// <summary>
        /// 整体位置 [x, y, z]
        /// </summary>
        public double[] wholePosition { get; set; }
    }

    /// <summary>
    /// 模型属性
    /// </summary>
    public class ModelProperties
    {
        public string name { get; set; }
        public string value { get; set; }
        public string key { get; set; }
    }
}
