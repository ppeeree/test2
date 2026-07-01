using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Enum;
using ACH.Helper.ImageSizeReader;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ACH.Helper.Image
{
    public class HighQualityImage
    {
        #region 旧版生成高清图
        /// <summary>
        /// 准备高质量图片字节
        /// </summary>
        public static byte[] PrepareHighQualityImageBytes(ImageInfo imageInfo, ImageSize size)
        {
            if (imageInfo.ImageType == EnumImageType.SVG_BASE || imageInfo.ImageType == EnumImageType.SVG_UTF)
            {
                // SVG -> 高分辨率PNG（关键：放大2倍渲染）
                return RenderSvgToHighResPng(imageInfo.ImageByte);
            }

            // 位图直接返回原始数据，避免二次压缩
            return ConvertToHighResolution(imageInfo.ImageByte, 300);
        }

        /// <summary>
        /// PNG高质量渲染
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="targetDpi"></param>
        /// <returns></returns>
        /// <summary>
        /// 使用SkiaSharp修改PNG图片DPI和清晰度（跨平台兼容）
        /// </summary>
        private static byte[] ConvertToHighResolution(byte[] originalImage, int targetDpi = 300)
        {
            try
            {
                // 1. 解码原始图片
                using var originalBitmap = SKBitmap.Decode(originalImage);
                if (originalBitmap == null)
                {
                    throw new InvalidDataException("PNG图片解码失败");
                }

                // 2. 创建高分辨率位图
                var info = new SKImageInfo(
                    originalBitmap.Width,
                    originalBitmap.Height,
                    SKColorType.Rgba8888,
                    SKAlphaType.Premul
                );

                using var surface = SKSurface.Create(info);
                var canvas = surface.Canvas;

                // 3. 高质量绘制
                canvas.Clear(SKColors.Transparent);

                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                    IsDither = true
                };

                canvas.DrawBitmap(originalBitmap, 0, 0, paint);

                // 4. 编码为高分辨率PNG
                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                return data.ToArray();
            }
            catch (Exception ex)
            {
                ALog.Error(ex, "ConvertToHighResolution-提高图片清晰度异常");
                throw;
            }
        }

        /// <summary>
        /// SVG 高质量渲染
        /// </summary>
        private static byte[] RenderSvgToHighResPng(byte[] svgBytes, float scale = 2.0f)
        {
            var svg = new SKSvg();
            using var stream = new MemoryStream(svgBytes);
            svg.Load(stream);

            if (svg.Picture == null)
                throw new InvalidDataException("SVG 加载失败");

            var rect = svg.Picture.CullRect;
            int width = (int)Math.Ceiling(rect.Width * scale);
            int height = (int)Math.Ceiling(rect.Height * scale);

            // 高分辨率渲染
            using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;

            // 防锯齿设置
            canvas.Clear(SKColors.Transparent);
            canvas.Scale(scale);
            canvas.DrawPicture(svg.Picture);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        #endregion


        /*#region
        public static byte[] PrepareHighQualityImageBytes(ImageInfo imageInfo, int targetDpi = 300)
        {
            if (imageInfo.ImageType == EnumImageType.SVG_BASE || imageInfo.ImageType == EnumImageType.SVG_UTF)
            {
                // SVG -> 高分辨率PNG（关键：放大2倍渲染）
                return RenderSvgToHighResPng(imageInfo.ImageByte, targetDpi);
            }

            // 位图直接返回原始数据，避免二次压缩
            return ConvertToHighResolution(imageInfo.ImageByte, targetDpi);
        }

        /// <summary>
        /// SVG → PNG，按 targetDpi 渲染
        /// </summary>
        private static byte[] RenderSvgToHighResPng(byte[] svgBytes, int dpi)
        {
            *//* 1. Skia 加载 SVG 并渲染到高像素表面 *//*
            using var stream = new MemoryStream(svgBytes);
            var svg = new SKSvg();
            svg.Load(stream);
            if (svg.Picture == null) throw new InvalidDataException("SVG 加载失败");

            var rect = svg.Picture.CullRect;
            int pixW = (int)Math.Ceiling(rect.Width * dpi / 96f);
            int pixH = (int)Math.Ceiling(rect.Height * dpi / 96f);

            using var surface = SKSurface.Create(new SKImageInfo(pixW, pixH,
                                                 SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.Scale(dpi / 96f);
            canvas.DrawPicture(svg.Picture);

            *//* 2. Skia 编码成 PNG 内存流 *//*
            using var skImg = surface.Snapshot();
            using var skData = skImg.Encode(SKEncodedImageFormat.Png, 100);
            var skPng = skData.ToArray();

            *//* 3. ImageSharp 重新打开 → 写 dpi → 再编码 *//*
            using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(skPng);
            img.Metadata.HorizontalResolution = dpi;
            img.Metadata.VerticalResolution = dpi;

            using var ms = new MemoryStream();
            img.Save(ms, new PngEncoder());
            return ms.ToArray();
        }

        /// <summary>
        /// 位图：像素够就仅改 dpi，不够就放大
        /// </summary>
        private static byte[] ConvertToHighResolution(byte[] originalImage, int dpi)
        {
            using var src = SKBitmap.Decode(originalImage);
            if (src == null) throw new InvalidDataException("PNG 解码失败");

            // 如果原图像素已经 >= 300 dpi 对应的尺寸，就直接复用像素
            int minW = (int)(src.Width * 300d / dpi);
            int minH = (int)(src.Height * 300d / dpi);

            SKBitmap dst = (src.Width >= minW && src.Height >= minH)
                ? src    // 像素够用
                : src.Resize(new SKImageInfo(minW, minH), SKFilterQuality.High);

            using var surface = SKSurface.Create(new SKImageInfo(dst.Width, dst.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(dst, 0, 0,
                new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High });

            using var img = surface.Snapshot();
            using var data = img.Encode(SKEncodedImageFormat.Png, 100);
            return AddDpiToPng(data.ToArray(), dpi);
        }

        /// <summary>
        /// 把 dpi 写进 PNG 的 pHYs 块
        /// </summary>
        /// <summary>
        /// 把 PNG 的 pHYs 块删掉，避免 WPS 缩放
        /// </summary>
        private static byte[] AddDpiToPng(byte[] pngData, int dpi)
        {
            // 我们不再写 pHYs，而是把已有的 pHYs 删掉
            using var ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            // 签名 8 字节
            bw.Write(pngData, 0, 8);

            int pos = 8;
            while (pos < pngData.Length - 4)
            {
                uint len = ReadUint32(pngData, pos);   // 块长度
                string type = Encoding.ASCII.GetString(pngData, pos + 4, 4);

                if (type != "pHYs")   // 只保留非 pHYs 块
                {
                    bw.Write(pngData, pos, 12 + (int)len); // 写 长度+类型+数据+crc
                }
                pos += 12 + (int)len;
            }
            return ms.ToArray();
        }

        private static uint ReadUint32(byte[] buf, int off)
        {
            return (uint)((buf[off] << 24) | (buf[off + 1] << 16) |
                          (buf[off + 2] << 8) | buf[off + 3]);
        }


        #endregion*/

    }
}
