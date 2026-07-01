using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // 注册编码提供程序
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string[] files = {
            "E:\\15-theweave\\webclient\\WebAPI\\ACH.Helper\\ImageSizeReader\\ImageSize.cs",
            "E:\\15-theweave\\webclient\\WebAPI\\ACH.Helper\\Others\\ZipCryptoHelper.cs",
            "E:\\15-theweave\\webclient\\WebAPI\\ACH.Helper\\Others\\EnumHelper.cs",
            "E:\\15-theweave\\webclient\\WebAPI\\ACH.Helper\\Others\\LogStore.cs",
            "E:\\15-theweave\\webclient\\WebAPI\\ACH.Helper\\Others\\HttpHelper.cs",
            "E:\\15-theweave\\webclient\\WebAPI\\ACH.Helper\\Others\\CommonConstantData.cs",
            "E:\\15-theweave\\webclient\\WebAPI\\ACH.Helper\\Others\\Entity\\LogsInfo.cs",
            "E:\\15-theweave\\webclient\\WebAPI\\ACH.Helper\\ImageSizeReader\\ImageTypeReader.cs"
        };

        foreach (string file in files)
        {
            Console.WriteLine($"处理文件: {file}");
            
            try
            {
                // 直接替换所有乱码字符（不依赖编码转换）
                string content = File.ReadAllText(file, Encoding.UTF8);
                
                // 使用正则表达式替换所有可能的乱码字符
                content = System.Text.RegularExpressions.Regex.Replace(
                    content, 
                    "[\\x80-\\xff][\\x40-\\xff]{2,}|", 
                    "乱码已清理"
                );
                
                // 保存为UTF-8
                File.WriteAllText(file, content, Encoding.UTF8);
                Console.WriteLine($"文件 {file} 处理完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理文件 {file} 时出错: {ex.Message}");
            }
        }

        Console.WriteLine("所有文件处理完成");
    }
}
