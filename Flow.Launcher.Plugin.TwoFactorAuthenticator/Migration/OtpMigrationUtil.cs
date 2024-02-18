using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator.Migration;

public class OtpMigrationUtil
{
    // same migration.proto
    private static readonly string DIGIT_COUNT_SIX = "DIGIT_COUNT_SIX";
    private static readonly string DIGIT_COUNT_EIGHT = "DIGIT_COUNT_EIGHT";


    private static readonly string ALGORITHM_SHA1 = "ALGORITHM_SHA1";
    private static readonly string ALGORITHM_SHA256 = "ALGORITHM_SHA256";
    private static readonly string ALGORITHM_SHA512 = "ALGORITHM_SHA512";
    private static readonly string ALGORITHM_MD5 = "ALGORITHM_MD5";

    [CanBeNull]
    public static List<OtpAuthModel> ParseOtpMigration(string url)
    {
        var uri = new Uri(url);

        if ("otpauth-migration" != uri.Scheme || "offline" != uri.Host)
        {
            return null;
        }

        var queryString = HttpUtility.ParseQueryString(uri.Query);
        var data = queryString["data"];
        if (string.IsNullOrEmpty(data))
            return null;
        // Console.WriteLine($"raw data = {data}");
        var decodeData = HttpUtility.UrlDecode(data, Encoding.UTF8);

        var fromBase64String = Convert.FromBase64String(decodeData);

        // Console.WriteLine($"base64 data size = {fromBase64String.Length}");

        try
        {
            var payload = Payload.Parser.ParseFrom(fromBase64String);
            // Console.WriteLine($"payload = {payload}");

            var otpParameters = payload.OtpParameters;
            foreach (var parameter in otpParameters)
            {
                Console.WriteLine($"parameter = {parameter}");

                var otpType = (int)parameter.Type;
                if (otpType == 2)
                {
                    // TOTP
                }
                else if (otpType == 1)
                {
                    // HOTP
                }
            }
        }
        catch (Exception e)
        {
        }

        return null;
    }
}