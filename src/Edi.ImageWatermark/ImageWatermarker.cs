﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Edi.ImageWatermark
{
    public class ImageWatermarker : IDisposable
    {
        public bool SkipWatermarkForSmallImages { get; set; }

        public int SmallImagePixelsThreshold { get; set; }

        private readonly Stream _originImageStream;

        private readonly string _imgExtensionName;

        public ImageWatermarker(Stream originImageStream, string imgExtensionName)
        {
            _originImageStream = originImageStream;
            _imgExtensionName = imgExtensionName;
        }

        public MemoryStream AddWatermark(string watermarkText, Color color,
            WatermarkPosition watermarkPosition = WatermarkPosition.BottomRight,
            int textPadding = 10,
            int fontSize = 20,
            Font font = null)
        {
            using (var watermarkedStream = new MemoryStream())
            using (var img = Image.FromStream(_originImageStream))
            {
                if (SkipWatermarkForSmallImages && img.Height * img.Width < SmallImagePixelsThreshold)
                {
                    return null;
                }

                using (var graphic = Graphics.FromImage(img))
                {
                    var brush = new SolidBrush(color);

                    var f = font ?? new Font(FontFamily.GenericSansSerif, fontSize,
                                FontStyle.Bold, GraphicsUnit.Pixel);

                    var textSize = graphic.MeasureString(watermarkText, f);
                    int x = textPadding, y = textPadding;

                    switch (watermarkPosition)
                    {
                        case WatermarkPosition.TopLeft:
                            x = textPadding; y = textPadding;
                            break;
                        case WatermarkPosition.TopRight:
                            x = img.Width - (int)textSize.Width - textPadding;
                            y = textPadding;
                            break;
                        case WatermarkPosition.BottomLeft:
                            x = textPadding;
                            y = img.Height - (int)textSize.Height - textPadding;
                            break;
                        case WatermarkPosition.BottomRight:
                            x = img.Width - (int)textSize.Width - textPadding;
                            y = img.Height - (int)textSize.Height - textPadding;
                            break;
                        default:
                            x = textPadding; y = textPadding;
                            break;
                    }

                    graphic.DrawString(watermarkText, f, brush, new Point(x, y));

                    ImageFormat fmt = null;
                    switch (_imgExtensionName)
                    {
                        case ".png":
                            fmt = ImageFormat.Png;
                            break;
                        case ".jpg":
                        case ".jpeg":
                            fmt = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            fmt = ImageFormat.Bmp;
                            break;
                    }
                    img.Save(watermarkedStream, fmt);
                    return watermarkedStream;
                }
            }
        }

        public void Dispose()
        {
            _originImageStream?.Dispose();
        }
    }
}
