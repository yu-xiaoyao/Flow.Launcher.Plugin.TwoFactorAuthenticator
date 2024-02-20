using System;
using System.Text;
using System.Web;
using Flow.Launcher.Plugin.TwoFactorAuthenticator.Migration;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class Main_Test
{
    private static readonly string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    // PS private info 
    public static void Main()
    {
        resolveUrl1();
        resolveUrl2();
    }


    public static void testResolveUrl()
    {
        // var uri = new Uri("http://localhost/abc?key=%E7%88%B1%E4%BD%A0");
        var uri = new Uri("http://localhost/abc?key=爱你");
        var query = HttpUtility.ParseQueryString(uri.Query);
        Console.WriteLine($"RAW = {query}");

        // 这一步会自动URL decode...
        var key = query["key"];

        Console.WriteLine($"RAW key = {key}");

        var dKey = HttpUtility.UrlDecode(key, Encoding.UTF8);
        Console.WriteLine($"RAW key = {dKey}");
    }


    public static void resolveQrCodeFile1()
    {
        var filePath = $"{BasePath}/g-1-account.jpg";
        var qrCodeContent = QrCodeUtil.ResolveQrCodeFile(filePath);
        Console.WriteLine($"QRCODE = {qrCodeContent}");
        var otpMigration = OtpMigrationUtil.ParseOtpMigration(qrCodeContent);
        foreach (var otpParam in otpMigration)
        {
            Console.WriteLine(otpParam);
        }
    }

    public static void resolveQrCodeFile2()
    {
        var filePath = $"{BasePath}/g-2-account.jpg";
        var qrCodeContent = QrCodeUtil.ResolveQrCodeFile(filePath);
        Console.WriteLine($"QRCODE = {qrCodeContent}");
        var otpMigration = OtpMigrationUtil.ParseOtpMigration(qrCodeContent);
        foreach (var otpParam in otpMigration)
        {
            Console.WriteLine(otpParam);
        }
    }


    public static void resolveUrl1()
    {
        var url =
            "otpauth-migration://offline?data=";

        var m1List = OtpMigrationUtil.ParseOtpMigration(url);

        if (m1List != null)
        {
            foreach (var otpParam in m1List)
            {
                Console.WriteLine(otpParam);
            }

            var test = m1List[0];
            Console.WriteLine($"test = {test}");
            var code = TotpUtil.GenerateTOTPPinCode(test);
            Console.WriteLine($"code = {code}");
        }
    }

    public static void resolveUrl2()
    {
        var url =
            "otpauth-migration://offline?data=";

        var m1List = OtpMigrationUtil.ParseOtpMigration(url);

        if (m1List != null)
        {
            foreach (var otpParam in m1List)
            {
                Console.WriteLine(otpParam);
            }

            var test = m1List[0];
            Console.WriteLine($"test = {test}");
            var code = TotpUtil.GenerateTOTPPinCode(test);
            Console.WriteLine($"code = {code}");
        }
    }
}