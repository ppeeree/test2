using System.Net.NetworkInformation;

namespace ACH.CMSWebClient.ControllerModel.Component
{
    public class EvCardStatusDTO
    {
        /// <summary>
        /// 特征值卡片ICON
        /// </summary>
        public string EvCardTitleIconCode { set; get; }
        /// <summary>
        /// 卡片标题
        /// </summary>
        public string EvCardTitle { set; get; }
        /// <summary>
        /// 卡片聚合状态
        /// </summary>
        public string EvSummaryStatus { set; get; }
        /// <summary>
        /// 卡片聚合状态时间
        /// </summary>
        public string EvSummaryStatusTime { set; get; }
        /// <summary>
        /// 特征值预警线
        /// </summary>
        public double EvThreshold { set; get; }
        /// <summary>
        /// 是否展示分布圆 false为不展示
        /// </summary>
        public Boolean IsShowCircle { set; get; }
        /// <summary>
        /// 是否展示雷达图 false为不展示
        /// </summary>
        public Boolean IsShowRadar { set; get; }
        /// <summary>
        /// 卡片三维坐标
        /// </summary>
        public List<double> EvCardPosition { set; get; }
        /// <summary>
        /// 卡片测点三维坐标
        /// </summary>
        public List<double> EvSpotPosition { set; get; }
        /// <summary>
        /// 特征值列表
        /// </summary>
        public List<EvStatusItemDTO> EvList { set; get; }


        // 构造函数
        public EvCardStatusDTO(string title, string iconCode, double threshold = 0, bool isShowCircle = false, bool isShowRadar = false, List<double> cardPosition = null, List<double> spotPosition = null)
        {
            EvCardTitle = title;
            EvCardTitleIconCode = iconCode;
            EvThreshold = threshold;
            IsShowCircle = isShowCircle;
            IsShowRadar = isShowRadar;
            EvCardPosition = cardPosition ?? new List<double>();
            EvSpotPosition = spotPosition ?? new List<double>();
            EvList = new List<EvStatusItemDTO>();
        }
        public EvCardStatusDTO()
        {

        }

        // 添加特征值方法
        public void AddEvStatusItem(EvStatusItemDTO item)
        {
            EvList.Add(item);
        }

        // 设置汇总状态
        public void SetSummaryStatus(string status, string statusTime)
        {
            EvSummaryStatus = status;
            EvSummaryStatusTime = statusTime;
        }
    }

    public class EvStatusItemDTO
    {
        /// <summary>
        /// 特征值名称
        /// </summary>
        public string EvName { set; get; }
        /// <summary>
        /// 分布圆名称
        /// </summary>
        public string CircleName { set; get; }
        /// <summary>
        /// 特征值Code
        /// </summary>
        public string EvCode { set; get; }
        /// <summary>
        /// 特征值ID
        /// </summary>
        public string EvID { set; get; }
        /// <summary>
        /// 特征值状态
        /// </summary>
        public string EvStatus { set; get; }
        /// <summary>
        /// 特征值状态时间
        /// </summary>
        public string EvStatusTime { set; get; }
        /// <summary>
        /// 特征值数值
        /// </summary>
        public double EvValue { set; get; }
        /// <summary>
        /// 单位
        /// </summary>
        public string EvUnit { set; get; }
        /// <summary>
        /// 分布圆角度
        /// </summary>
        public double AngleDegree { set; get; }

        public EvStatusItemDTO(string evName, string evCode, string evId, string evStatus, string evStatusTime, double evValue, string evUnit, double angleDegree = 0)
        {
            EvName = evName;
            EvCode = evCode;
            EvID = evId;
            EvStatus = evStatus;
            EvStatusTime = evStatusTime;
            EvValue = evValue;
            EvUnit = evUnit;
            AngleDegree = angleDegree;
        }
        public EvStatusItemDTO()
        {

        }
    }
}
