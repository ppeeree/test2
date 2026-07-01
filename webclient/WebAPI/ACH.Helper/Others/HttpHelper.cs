using ACH.ACHLog.SeriLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ACH.Helper.Others
{
    public class HttpHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };


        /// <summary>
        /// 发送GET请求（同步）
        /// </summary>
        public static (bool Success, string ResponseBody) Get(string url, Dictionary<string, string>? query = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(url);
                if (query?.Count > 0)
                {
                    var queryParams = query.Select(kvp =>
                        $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}");
                    var queryString = string.Join("&", queryParams);

                    if (string.IsNullOrEmpty(uriBuilder.Query))
                    {
                        uriBuilder.Query = queryString;
                    }
                    else
                    {
                        uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + queryString;
                    }
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 添加超时控制
                var task = _httpClient.GetAsync(uriBuilder.ToString(), cts.Token);
                var resp = task.Result; // 保持同步调用
                var body = resp.Content.ReadAsStringAsync().Result;

                return (resp.IsSuccessStatusCode, body);
            }
            catch (AggregateException ex) // 处理Task中的异常
            {
                var innerEx = ex.InnerException;
                if (innerEx is HttpRequestException || innerEx is TaskCanceledException)
                {
                    ALog.Error(innerEx, $"GET 请求网络异常 → {url}{query}");
                }
                else if (innerEx is UriFormatException)
                {
                    ALog.Error(innerEx, $"GET 请求URL格式错误 → {url}{query}");
                }
                else
                {
                    ALog.Error(innerEx, $"GET 请求发生异常 → {url}{query}");
                }
                return (false, null!);
            }
            catch (HttpRequestException ex)
            {
                ALog.Error(ex, $"GET 请求网络异常 → {url}{query}");
                return (false, null!);
            }
            catch (TaskCanceledException ex)
            {
                ALog.Error(ex, $"GET 请求超时 → {url}{query}");
                return (false, null!);
            }
            catch (UriFormatException ex)
            {
                ALog.Error(ex, $"GET 请求URL格式错误 → {url}{query}");
                return (false, null!);
            }
            catch (ArgumentNullException ex)
            {
                ALog.Error(ex, $"GET 请求参数为空 → {url}{query}");
                return (false, null!);
            }
        }


        /// <summary>
        /// 发送POST请求（同步）
        /// </summary>
        public static (bool Success, string ResponseBody) Post<T>(string url, T data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = _httpClient.PostAsync(url, content).Result;
                var body = resp.Content.ReadAsStringAsync().Result;
                return (resp.IsSuccessStatusCode, body);
            }
            catch (HttpRequestException ex)
            {
                ALog.Error(ex, $"POST 请求网络异常 → {url}");
                return (false, null!);
            }
            catch (TaskCanceledException ex)
            {
                ALog.Error(ex, $"POST 请求超时 → {url}");
                return (false, null!);
            }
        }
    }
}
