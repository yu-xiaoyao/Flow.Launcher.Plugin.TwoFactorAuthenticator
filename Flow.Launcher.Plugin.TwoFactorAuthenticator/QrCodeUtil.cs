using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class QrCodeUtil
{
    [CanBeNull]
    public static string ResolveQrCodeFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var bitmap = new Bitmap(filePath);
        return ResolveQrCode(bitmap);
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
}