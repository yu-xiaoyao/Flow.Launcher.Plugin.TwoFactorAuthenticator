using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using Point = System.Drawing.Point;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class QrCodeUtil
{
    [CanBeNull]
    public static string ResolveQrCodeFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        try
        {
            var bitmap = new Bitmap(filePath);
            return ResolveQrCode(bitmap);
        }
        catch (Exception e)
        {
            // error
            return null;
        }
    }

    /// <summary>
    /// 解析二维码
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    [CanBeNull]
    public static string ResolveQrCode(Bitmap bitmap)
    {
        var reader = new BarcodeReader
        {
            Options = new DecodingOptions
            {
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
            }
        };

        var result = reader.Decode(bitmap);
        return result?.Text;
    }

    [CanBeNull]
    public static Bitmap GetBitmap([CanBeNull] BitmapSource source)
    {
        if (source == null) return null;

        var bmp = new Bitmap(
            source.PixelWidth,
            source.PixelHeight,
            PixelFormat.Format32bppPArgb);

        var data = bmp.LockBits(
            new Rectangle(Point.Empty, bmp.Size),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppPArgb);
        source.CopyPixels(
            Int32Rect.Empty,
            data.Scan0,
            data.Height * data.Stride,
            data.Stride);
        bmp.UnlockBits(data);
        return bmp;
    }
}