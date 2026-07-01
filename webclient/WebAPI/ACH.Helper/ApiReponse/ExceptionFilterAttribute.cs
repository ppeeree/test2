using ACH.ACHLog.SeriLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;

namespace ACH.Helper.ApiReponse
{
    /// <summary>
    /// 异常过滤器，用于捕获控制器中抛出的所有异常。
    /// </summary>
    public class ExceptionFilterAttribute : Microsoft.AspNetCore.Mvc.Filters.ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            // 获取请求类型
            var httpMethod = context.HttpContext.Request.Method;
            // 获取请求方法名称
            var actionName = (context.HttpContext.GetRouteData().Values["action"] as string) ?? string.Empty;
            // 获取异常代码，默认500
            int code = 500;
            if (int.TryParse(context.HttpContext.Request.Query["code"], out int queryCode))
            {
                code = queryCode;
            }
            // 根据请求方法做不同的处理
            if (httpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                // GET 方法发生异常时，返回空集合或新对象
                // 由于无法直接获取原始方法的返回类型，我们使用一种更通用的方式
                context.Result = new OkObjectResult(new ApiResponse<object?>
                {
                    Code = code,
                    Data = new { }, // 返回空对象，客户端可以根据需要转换
                    Message = $"{actionName}接口调用失败！" + context.Exception.Message,
                    Success = false
                });
            }
            else
            {
                //POST 方法发生异常时，Data 统一返回 false
                context.Result = new OkObjectResult(new ApiResponse<bool>
                {
                    Code = code,
                    Data = false,
                    Message = $"{actionName}接口调用失败！" + context.Exception.Message,
                    Success = false
                });
            }
            context.ExceptionHandled = true;

            // 记录日志信息
            ALog.Error(context.Exception, $"接口调用异常：{actionName}，异常信息：" + context.Exception.Message);
        }
    }
}