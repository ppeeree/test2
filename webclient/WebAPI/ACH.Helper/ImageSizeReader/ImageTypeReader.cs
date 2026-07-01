using ACH.ACHLog.SeriLog;using ACH.DataEntity.Enum;using ACH.Helper.ImageSizeReader;using Svg.Skia;using System;using System.IO;using System.Text;using System.Text.RegularExpressions;namespace ACH.Helper.Image{    public class ImageTypeReader    {
        // 1 EMU = 1/914400 英寸
        private const int EMU_PER_INCH = 360000;






















        /// <summary>        /// 图片string -> byte[]        /// </summary>        /// <param name="imageUri"></param>        /// <returns></returns>                                                                                                                                                                                                                                                                                                                        public static ImageInfo GetByteTypeByImageUri(string imageUri)        {            ImageInfo res = new ImageInfo();            if (string.IsNullOrEmpty(imageUri))                return res;            try            {
                // 分割前缀和数据
                int commaIndex = imageUri.IndexOf(',');                if (commaIndex <= 0)                {                    throw new FormatException($"ConvertByImageUri-获取图片类型异常");                }                string prefix = imageUri.Substring(0, commaIndex);                string data = imageUri.Substring(commaIndex + 1);

                // 判断编码类型
                if (prefix.Contains("utf-8"))                {
                    // URL编码的文本（如SVG）
                    string decoded = Uri.UnescapeDataString(data);                    res.ImageByte = Encoding.UTF8.GetBytes(decoded);                    res.ImageType = EnumImageType.SVG_UTF;                    res.WidthEmu = 900;                    res.HeightEmu = 360;

                    // ALog.Debug($"获取到SVG");
                    return res;                }                else                {
                    // Base64编码（真正的二进制）
                    data = Uri.UnescapeDataString(data); // 以防URL转义
                    data = Regex.Replace(data, @"\s+", ""); // 移除空白
                    res.ImageByte = Convert.FromBase64String(data);                    res.ImageType = prefix.Contains("data:image/svg+xml") ? EnumImageType.SVG_BASE : EnumImageType.PNG_BASE;                    res.WidthEmu = 900;                    res.HeightEmu = 360;

                    // ALog.Debug($"获取到PNG");
                    return res;                }            }            catch (Exception ex)            {                ALog.Error(ex, $"转换失败。原始数据: {imageUri}");                return new ImageInfo();            }        }


































        /// <summary>        /// byte[] -> 图片string        /// </summary>        /// <param name="imageBytes"></param>        /// <param name="imageType"></param>        /// <returns></returns>        /// SVG返回的是Encoding.UTF8.GetString() + URL 编码        /// PNG/JPG返回的是Convert.ToBase64String()                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  public static string GetImageStringByImageByte(byte[] imageBytes, EnumImageType imageType)        {            if (imageBytes == null || imageBytes.Length == 0)                return string.Empty;

            // 处理 SVG 文本格式
            if (imageType == EnumImageType.SVG_UTF)            {                string svgContent = Encoding.UTF8.GetString(imageBytes);
                // URL 编码后拼接前缀
                return "data:image/svg+xml;charset=utf-8," + Uri.EscapeDataString(svgContent);            }            else if (imageType == EnumImageType.SVG_BASE)            {
                // 处理 PNG/JPG 等二进制格式
                string base64 = Convert.ToBase64String(imageBytes);                return $"data:image/svg+xml;base64,{base64}";            }            else            {
                // 处理 PNG/JPG 等二进制格式
                string base64 = Convert.ToBase64String(imageBytes);                return $"data:image/png;base64,{base64}";            }        }






















        /// <summary>        /// 根据图片类型判断前缀        /// </summary>        /// <param name="type"></param>        /// <returns></returns>                                                                                                                                                                                                                                                                                                public static string GetMimeType(EnumImageType imageType)        {            return imageType switch            {                EnumImageType.PNG_BASE => "image/png",                EnumImageType.SVG_BASE => "image/svg+xml",                EnumImageType.SVG_UTF => "image/svg+xml", // SVG 需先转 PNG，但这里返回原始 MIME
                _ => "image/png"            };        }






















        /// <summary>        /// 读取位图原始尺寸        /// </summary>        /// <param name="bytes"></param>        /// <returns></returns>                                                                                                                                                                                                                                                                                              private static void GetBitmapSize(ImageInfo imageInfo)        {            try            {                using var image = SixLabors.ImageSharp.Image.Load(imageInfo.ImageByte, out _);

                // 尝试获取DPI，缺省用96
                double dpi = image.Metadata?.HorizontalResolution > 0
? image.Metadata.HorizontalResolution
: 96.0;                imageInfo.WidthEmu = (long)(image.Width / dpi * EMU_PER_INCH);                imageInfo.HeightEmu = (long)(image.Height / dpi * EMU_PER_INCH);            }            catch (Exception ex)            {                ALog.Error(ex, $"GetBitmapSize-获取其余类型图片尺寸异常");            }        }





















        /// <summary>        /// 读取SVG尺寸（优先viewBox）        /// </summary>        /// <param name="bytes"></param>        /// <returns></returns>                                                                                                                                                                                                                                                                                                                  private static void GetSvgSize(ImageInfo imageInfo)        {            try            {                var svg = new SKSvg();                svg.Load(new MemoryStream(imageInfo.ImageByte));                if (svg.Picture?.CullRect is { } rect)                {
                    // 默认1用户单位=1px=9525 EMU(96 DPI)
                    imageInfo.WidthEmu = (long)(rect.Width * 9525);                    imageInfo.HeightEmu = (long)(rect.Height * 9525);                }            }            catch (Exception ex)            {                ALog.Error(ex, $"GetSvgSize-获取svg图片尺寸异常");            }        }






























        /// <summary>        /// 设置图片缩放        /// </summary>        /// <param name="imageInfo">图片信息对象</param>        /// <param name="maxWidthEmu">缩放宽度</param>        /// <param name="maxHeightEmu">缩放高度</param>        /// <returns></returns>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            public static ImageSize ScaleToFit(ImageInfo imageInfo, long? maxWidthEmu = null, long? maxHeightEmu = null)        {
            // 初始化赋值
            ImageSize res = new ImageSize();            res.WidthEmu = imageInfo.WidthEmu;            res.HeightEmu = imageInfo.HeightEmu;

            // 获取原始图片尺寸
            long OriginalWidthEmu = imageInfo.WidthEmu;            long OriginalHeightEmu = imageInfo.HeightEmu;            if (OriginalWidthEmu == 0 || OriginalHeightEmu == 0)            {                return res;            }            double ratio = (double)OriginalWidthEmu / OriginalHeightEmu;

            // 情况1：只限制宽度
            if (maxWidthEmu.HasValue && !maxHeightEmu.HasValue)            {                res.WidthEmu = maxWidthEmu.Value;                res.HeightEmu = (long)(res.WidthEmu / ratio);            }
            // 情况2：只限制高度
            else if (!maxWidthEmu.HasValue && maxHeightEmu.HasValue)            {                res.HeightEmu = maxHeightEmu.Value;                res.WidthEmu = (long)(res.HeightEmu * ratio);            }
            // 情况3：同时限制宽高（保持比例，不超出边界）
            else if (maxWidthEmu.HasValue && maxHeightEmu.HasValue)            {                double widthRatio = (double)maxWidthEmu.Value / OriginalWidthEmu;                double heightRatio = (double)maxHeightEmu.Value / OriginalHeightEmu;                double scale = Math.Min(widthRatio, heightRatio); // 取较小比例

                res.WidthEmu = (long)(OriginalWidthEmu * scale);                res.HeightEmu = (long)(OriginalHeightEmu * scale);            }            return res;        }    }}