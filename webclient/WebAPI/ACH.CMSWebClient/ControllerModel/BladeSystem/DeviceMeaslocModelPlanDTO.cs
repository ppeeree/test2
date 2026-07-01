using ACH.DataEntity.DevTree;

namespace ACH.CMSWebClient.ControllerModel.BladeSystem
{
    /// <summary>
    /// 机组模型方案和测点配置方案
    /// </summary>
    public class DeviceMeaslocModelPlanDTO
    {
        /// <summary>
        /// 三维模型信息（和表中存储的json文件解析类是一致的）
        /// </summary>
        public List<DeviceModelPlan> model { get; set; }

        /// <summary>
        /// 测点配置信息（和表中存储的json文件解析类是一致的）
        /// </summary>
        public List<PlanCompModel> measloc { get; set; }
    }

}
