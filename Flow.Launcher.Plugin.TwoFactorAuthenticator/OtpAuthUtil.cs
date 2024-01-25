using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Authenticator;
using JetBrains.Annotations;
using GoogleTwoFactorAuthenticator = Google.Authenticator.TwoFactorAuthenticator;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class OtpAuthUtil
{
    public static readonly string TotpType = "totp";

    /// <summary>
    /// 生成当前PinCode
    /// </summary>
    /// <param name="secret"></param>
    /// <param name="algorithm"></param>
    /// <returns></returns>
    public static string GetCurrentPIN(string secret, [CanBeNull] string algorithm)
    {
        var tfa = algorithm == null
            ? new GoogleTwoFactorAuthenticator()
            : new GoogleTwoFactorAuthenticator(ResolveHashTypeAlgorithm(algorithm));
        return tfa.GetCurrentPIN(secret, DateTime.UtcNow, true);
    }

    /// <summary>
    /// 生成分享二维码
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static byte[] CreateQRCode(OtpAuthModel model)
    {
        if (model is TotpModel totp)
        {
            return CreateTotpQRCode(totp.Issuer, totp.AccountTitle, totp.Secret, totp.Algorithm);
        }

        //TODO more otp Algorithm
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
            : new GoogleTwoFactorAuthenticator(ResolveHashTypeAlgorithm(algorithm));

        var setupCode = tfa.GenerateSetupCode(issuer, accountTitle, secret, true, qrPixel);

        var baseImageData = setupCode.QrCodeSetupImageUrl;
        var imageData = baseImageData["data:image/png;base64,".Length..];

        return Convert.FromBase64String(imageData);
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


    [CanBeNull]
    public static OtpAuthModel ResolveOtpUrl(string url)
    {
        if (!url.StartsWith("otpauth://"))
            return null;

        var uri = new Uri(url);
        if (TotpType.Equals(uri.Host))
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            var issuer = query["issuer"];
            var account = uri.AbsolutePath[1..];

            var index = account.IndexOf(":", StringComparison.Ordinal);
            if (index != -1)
            {
                if (string.IsNullOrEmpty(issuer))
                    issuer = account[..index];

                account = account[(index + 1)..];
            }


            var secret = query["secret"];
            if (secret == null)
                throw new Exception("otpauth url invalid. miss secret.");

            var algorithm = query["algorithm"];

            // check valid algorithm
            ResolveHashTypeAlgorithm(algorithm);

            return new TotpModel
            {
                Type = TotpType,
                Algorithm = algorithm,
                Secret = secret,
                Issuer = issuer,
                AccountTitle = account
            };
        }

        //TODO 更多其他类型
        return null;
    }

    public static List<string> GetAllAlgorithms()
    {
        var hashTypes = Enum.GetValues<HashType>();
        return hashTypes.Select(hashType => hashType.ToString()).ToList();
    }
}

public record OtpAuthModel
{
    public string Type { set; get; }
    public string Name { set; get; }
}

public record TotpModel : OtpAuthModel
{
    public string Issuer { set; get; }
    public string AccountTitle { set; get; }
    public string Secret { set; get; }

    /// <summary>
    /// 支持: SHA1,SHA256,SHA512
    /// default SHA1
    /// </summary>
    [CanBeNull]
    public string Algorithm { set; get; }
}