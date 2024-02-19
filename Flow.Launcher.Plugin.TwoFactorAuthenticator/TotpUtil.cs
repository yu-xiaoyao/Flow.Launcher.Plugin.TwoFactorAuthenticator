using System;
using System.Collections.Generic;
using System.Linq;
using Google.Authenticator;
using JetBrains.Annotations;
using GoogleTwoFactorAuthenticator = Google.Authenticator.TwoFactorAuthenticator;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class TotpUtil
{
    private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static long GetCurrentCounter(DateTime now, DateTime epoch, int timeStep)
    {
        return (long)(now - epoch).TotalSeconds / (long)timeStep;
    }


    public static string GenerateTOTPPinCode(OtpParam param)
    {
        return GenerateTOTPPinCode(param.Algorithm, param.Secret, true, param.Digits);
    }

    public static string GenerateTOTPPinCode(string algorithm, string secret, bool secretIsBase32 = true,
        int digits = 6)
    {
        var tfa = algorithm == null
            ? new GoogleTwoFactorAuthenticator()
            : new GoogleTwoFactorAuthenticator(ResolveHashTypeAlgorithm(algorithm));

        // if (digits == 6)
        // {
        // return tfa.GetCurrentPIN(param.Secret, DateTime.UtcNow, true);
        // return tfa.GeneratePINAtInterval(param.Secret, GetCurrentCounter(DateTime.UtcNow, _epoch, 30), param.Digits, true);
        // }

        // Digits == 8
        return tfa.GeneratePINAtInterval(secret, GetCurrentCounter(DateTime.UtcNow, _epoch, 30), digits,
            secretIsBase32);
    }


    public static HashType ResolveHashTypeAlgorithm([CanBeNull] string algorithm)
    {
        if (string.IsNullOrEmpty(algorithm))
            return HashType.SHA1;
        Enum.TryParse(typeof(HashType), algorithm, false, out var hashAlgorithm);
        if (hashAlgorithm is not HashType hashType)
        {
            throw new Exception("otpauth url invalid algorithm.");
        }

        return hashType;
    }


    public static List<string> GetAllAlgorithms()
    {
        var hashTypes = Enum.GetValues<HashType>();
        return hashTypes.Select(hashType => hashType.ToString()).ToList();
    }
}