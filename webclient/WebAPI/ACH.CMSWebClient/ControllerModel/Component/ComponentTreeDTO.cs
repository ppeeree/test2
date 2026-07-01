using System.Collections.Generic;

namespace ACH.CMSWebClient.ControllerModel.Component
{
    public class ComponentTreeDTO
    {
        /// <summary>
        /// 聚合部件名称
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// 聚合部件类型
        /// </summary>
        public string EntityType { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// 实体部件列表
        /// </summary>
        public List<ComponentTreeItemDTO> Children { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ComponentTreeDTO()
        {
            Children = new List<ComponentTreeItemDTO>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entityType">聚合部件类型</param>
        /// <param name="entityName">聚合部件名称</param>
        /// <param name="sort">排序</param>
        public ComponentTreeDTO(string entityType, string entityName, string sort)
        {
            EntityType = entityType;
            EntityName = entityName;
            Sort = sort;
            Children = new List<ComponentTreeItemDTO>();
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="pageCompName">聚合部件名称</param>
        /// <param name="pageCompType">聚合部件类型</param>
        /// <param name="compName">实体部件名称</param>
        /// <param name="compType">实体部件类型</param>
        /// <param name="sort">排序</param>
        public void AddChild(string pageCompName, string pageCompType, string compName, string compType, int sort)
        {
            var child = new ComponentTreeItemDTO(pageCompName, pageCompType, compName, compType, sort);
            Children.Add(child);
        }
    }



    public class ComponentTreeItemDTO
    {
        /// <summary>
        /// 实体部件对应聚合部件名称
        /// </summary>
        public string FentityName { get; set; }

        /// <summary>
        /// 实体部件对应聚合部件类型
        /// </summary>
        public string FentityType { get; set; }

        /// <summary>
        /// 实体部件名称
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// 部件类型
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pageCompName">聚合部件名称</param>
        /// <param name="pageCompType">聚合部件类型</param>
        /// <param name="compName">实体部件名称</param>
        /// <param name="compType">实体部件类型</param>
        /// <param name="sort">排序</param>
        public ComponentTreeItemDTO(string pageCompName, string pageCompType, string compName, string compType, int sort)
        {
            FentityName = pageCompName;
            FentityType = pageCompType;
            EntityName = compName;
            EntityType = compType;
            Sort = sort.ToString();
        }
    }
}
