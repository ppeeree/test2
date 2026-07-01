using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ACH.Common.Tools
{
    public static class ConfigMapping
    {
        private static Dictionary<string, string>? keyValues { get; set; }

        public static string GetValuesByKey(string key)
        {
            if (keyValues == null)
            {
                // 读取配置文件
                keyValues = InitConfig();
            }
            if (keyValues != null)
            {
                if (keyValues.ContainsKey(key))
                {
                    return keyValues[key];
                }
                else
                {
                    // 不存在的配置, 记录Key
                    return key;
                }
            }

            // 记录错误日志，缺少配置
            return key;
        }

        private static Dictionary<string, string>? InitConfig()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigMap.json");
                return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                // 加载映射配置出现异常
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
