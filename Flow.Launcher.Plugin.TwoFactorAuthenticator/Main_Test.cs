using System;
using Flow.Launcher.Plugin.TwoFactorAuthenticator.Migration;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class Main_Test
{
    // PS private info 
    public static void Main()
    {
        resolveUrl1();
        resolveUrl2();
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