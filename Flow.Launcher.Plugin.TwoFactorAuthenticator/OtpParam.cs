using System;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public record OtpParam
{
    public static readonly string TotpType = "totp";
    public static readonly string HotpType = "hotp";


    /// <summary>
    /// 自定义备注名
    /// </summary>
    [CanBeNull]
    public string Remark { set; get; }

    /// <summary>
    /// TOTP,HOTP
    /// </summary>
    public string OtpType { set; get; }

    /**
     * base 32 string
     */
    public string Secret { set; get; }

    public string Issuer { set; get; }
    public string Name { set; get; }

    /// <summary>
    /// 支持: SHA1,SHA256,SHA512
    /// default SHA1
    /// </summary>

    [CanBeNull]
    public string Algorithm { set; get; }

    public int Digits { set; get; } = 6;

    public ulong Counter { set; get; }
}