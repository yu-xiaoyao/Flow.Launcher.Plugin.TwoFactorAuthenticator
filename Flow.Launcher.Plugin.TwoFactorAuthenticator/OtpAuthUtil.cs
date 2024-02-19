﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Flow.Launcher.Plugin.TwoFactorAuthenticator.Migration;
using Google.Authenticator;
using JetBrains.Annotations;
using GoogleTwoFactorAuthenticator = Google.Authenticator.TwoFactorAuthenticator;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class OtpAuthUtil
{
    [CanBeNull]
    public static string GeneratePinCode(OtpParam param)
    {
        if (param.OtpType == OtpParam.TotpType)
        {
            return TotpUtil.GenerateTOTPPinCode(param);
        }
        else if (param.OtpType == OtpParam.HotpType)
        {
            return HotpUtil.GenerateHOTPPinCode(param);
        }

        return null;
    }

    private static bool ValidOtpParam(OtpParam param)
    {
        if (string.IsNullOrWhiteSpace(param.Secret)) return false;
        if (string.IsNullOrWhiteSpace(param.Issuer)) return false;
        if (string.IsNullOrWhiteSpace(param.Name)) return false;
        if (param.Digits is not (6 or 8)) return false;
        return true;
    }

    public static bool ValidTotpParam(OtpParam param)
    {
        if (!ValidOtpParam(param))
            return false;

        if (param.OtpType != OtpParam.TotpType) return false;


        // Algorithm
        if (param.Algorithm != null)
        {
            Enum.TryParse(typeof(HashType), param.Algorithm, false, out var hashAlgorithm);
            if (hashAlgorithm is not HashType)
            {
                return false;
            }
        }


        return true;
    }

    public static bool ValidHotpParam(OtpParam param)
    {
        if (!ValidOtpParam(param))
            return false;

        if (param.OtpType != OtpParam.HotpType) return false;

        //TODO valid ...
        return false;
    }


    [CanBeNull]
    public static OtpParam ResolveOtpAuthUrl(string url)
    {
        if (url.StartsWith("otpauth://"))
        {
            var res = ParserOtpAuthUrl(url);
            return res;
        }

        return null;
    }

    [CanBeNull]
    public static OtpParam ParserOtpAuthUrl(string url)
    {
        var uri = new Uri(url);
        if (OtpParam.TotpType.Equals(uri.Host))
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            var issuer = query["issuer"];
            var name = uri.AbsolutePath[1..];

            var index = name.IndexOf(":", StringComparison.Ordinal);
            if (index != -1)
            {
                if (string.IsNullOrEmpty(issuer))
                    issuer = name[..index];

                name = name[(index + 1)..];
            }


            var secret = query["secret"];
            if (secret == null)
                throw new Exception("otpauth url invalid. miss secret.");

            var algorithm = query["algorithm"];

            // check valid algorithm
            TotpUtil.ResolveHashTypeAlgorithm(algorithm);

            var param = new OtpParam
            {
                OtpType = OtpParam.TotpType,
                Algorithm = algorithm,
                Secret = secret,
                Issuer = issuer,
                Name = name
            };

            return param;
        }

        // 其他 OTP URL 解析
        return null;
    }


    [CanBeNull]
    public static List<OtpParam> ResolveOtpAuthMigrationUrl(string url)
    {
        if (url.StartsWith("otpauth-migration://"))
        {
            return OtpMigrationUtil.ParseOtpMigration(url);
        }

        return null;
    }


    /// <summary>
    /// 生成分享二维码
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static byte[] CreateQRCode(OtpParam model)
    {
        // return CreateTotpQRCode(totp.Issuer, totp.AccountTitle, totp.Secret, totp.Algorithm);
        return null;
    }

    /// <summary>
    /// 创建TOTP二维码
    /// </summary>
    /// <param name="issuer"></param>
    /// <param name="accountTitle"></param>
    /// <param name="secret"></param>
    /// <param name="algorithm"></param>
    /// <param name="qrPixel"></param>
    /// <returns></returns>
    public static byte[] CreateTotpQRCode(string issuer, string accountTitle, string secret,
        [CanBeNull] string algorithm,
        int qrPixel = 20)
    {
        var tfa = algorithm == null
            ? new GoogleTwoFactorAuthenticator()
            : new GoogleTwoFactorAuthenticator(TotpUtil.ResolveHashTypeAlgorithm(algorithm));

        var setupCode = tfa.GenerateSetupCode(issuer, accountTitle, secret, true, qrPixel);

        var baseImageData = setupCode.QrCodeSetupImageUrl;
        var imageData = baseImageData["data:image/png;base64,".Length..];

        return Convert.FromBase64String(imageData);
    }
}

// public record OtpAuthModel
// {
//     public string Type { set; get; }
//     public string Name { set; get; }
// }
//
// public record TotpModel : OtpAuthModel
// {
//     public string Issuer { set; get; }
//     public string AccountTitle { set; get; }
//     public string Secret { set; get; }
//
//     /// <summary>
//     /// 支持: SHA1,SHA256,SHA512
//     /// default SHA1
//     /// </summary>
//     [CanBeNull]
//     public string Algorithm { set; get; }
// }