using System;
using System.Collections.Generic;
using System.Web;
using Google.Authenticator;
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
    public static List<OtpParam> ParseOtpMigration(string url)
    {
        var uri = new Uri(url);

        if ("otpauth-migration" != uri.Scheme || "offline" != uri.Host)
        {
            return null;
        }

        var queryString = HttpUtility.ParseQueryString(uri.Query);

        // 这一步会自动URL decode...
        var data = queryString["data"];
        if (string.IsNullOrEmpty(data))
            return null;

        // Console.WriteLine($"raw    data = {data}");
        // data = HttpUtility.UrlDecode(data, Encoding.UTF8);
        // Console.WriteLine($"http decode = {data}");


        var fromBase64String = Convert.FromBase64String(data);

        // Console.WriteLine($"base64 data size = {fromBase64String.Length}");

        var resultList = new List<OtpParam>();

        try
        {
            var payload = Payload.Parser.ParseFrom(fromBase64String);
            // Console.WriteLine($"payload = {payload}");

            var otpParameters = payload.OtpParameters;
            foreach (var parameter in otpParameters)
            {
                // Console.WriteLine($"parameter = {parameter}");

                var otpType = (int)parameter.Type;
                if (otpType == 2)
                {
                    // TOTP
                    var item = BuildTotp(parameter);
                    if (item != null) resultList.Add(item);
                }
                else if (otpType == 1)
                {
                    // HOTP
                }
            }

            return resultList;
        }
        catch (Exception e)
        {
        }

        return null;
    }

    [CanBeNull]
    private static OtpParam BuildTotp(Payload.Types.OtpParameters parameters)
    {
        var digits = parameters.Digits switch
        {
            Payload.Types.OtpParameters.Types.DigitCount.Six => 6,
            Payload.Types.OtpParameters.Types.DigitCount.Eight => 8,
            _ => -1
        };

        // same with HashType
        var algorithm = parameters.Algorithm switch
        {
            Payload.Types.OtpParameters.Types.Algorithm.Sha1 => "SHA1",
            Payload.Types.OtpParameters.Types.Algorithm.Sha256 => "SHA256",
            Payload.Types.OtpParameters.Types.Algorithm.Sha512 => "SHA512",
            _ => "SHA1"
        };

        // var key = BitConverter.ToString(parameters.Secret.ToByteArray().Replace("-", "");
        var key = Base32Encoding.ToString(parameters.Secret.ToByteArray());

        // Console.WriteLine(key);

        var param = new OtpParam
        {
            OtpType = OtpParam.TotpType,
            Secret = key,
            Issuer = parameters.Issuer,
            Name = parameters.Name,
            Algorithm = algorithm,
            Digits = digits,
            Counter = parameters.Counter
        };
        return OtpAuthUtil.ValidTotpParam(param) ? param : null;
    }
}