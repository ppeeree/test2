using System.Collections.Generic;

namespace ACH.Helper.ApiReponse
{
    /// <summary>
    /// 请求下载任务列表返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TableResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
