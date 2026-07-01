using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ACH.Helper.ApiReponse
{
    public class CreateReponse
    {
        /// <summary>
        /// 辅助方法：创建成功的ApiResponse对象
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">数据</param>
        /// <param name="message">消息，默认为"操作成功"</param>
        /// <param name="code">请求code，默认为200</param>
        /// <returns>ApiResponse对象</returns>
        public ObjectResult CreateResponse<T>(T data, int code = 200, string message = "操作成功")
        {
            return new ObjectResult(new ApiResponse<T>
            {
                Code = code,
                Data = data,
                Message = message,
                Success = code == 200 ? true : false
            });
        }

    }
}
