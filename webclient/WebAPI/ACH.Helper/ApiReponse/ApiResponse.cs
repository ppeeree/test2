namespace ACH.Helper.ApiReponse
{
    /// <summary>
    /// API响应模型
    /// </summary>
    public class ApiResponse<T>
    {
        public int? Code { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
