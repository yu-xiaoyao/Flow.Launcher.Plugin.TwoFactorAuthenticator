using Flow.Launcher.Plugin.TwoFactorAuthenticator.Migration;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class Main_Test
{
    // PS private info 
    public static void Main()
    {
        resolveUrl();
    }

    public static void resolveUrl()
    {
        var url =
            "otpauth-migration://offline?data=";

        var m1 = OtpMigrationUtil.ParseOtpMigration(url);

        var url2 =
            "otpauth-migration://offline?data=";

        var m2 = OtpMigrationUtil.ParseOtpMigration(url2);
    }
}