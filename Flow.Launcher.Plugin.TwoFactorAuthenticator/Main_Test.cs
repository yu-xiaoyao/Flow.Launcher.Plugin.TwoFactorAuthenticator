using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web;
using Flow.Launcher.Plugin.TwoFactorAuthenticator.Migration;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class Main_Test
{
    private static readonly string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    // PS private info 
    public static void Main()
    {
        // var resultList = ToolGood.Words.Pinyin.WordsHelper.GetPinyinList(content);
        pinyin();
        // pinyinMatch();
        // reflectionTest();
        // resolveUrl1();
        // resolveUrl2();
    }

    private static void pinyin()
    {
        var appData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        PinYin.InitPinyinLib($@"{appData}\FlowLauncher\app-1.18.0\");


        Console.WriteLine(PinYin.WordsHelper?.HasChinese("-asdfajadf-"));
        Console.WriteLine(PinYin.WordsHelper?.HasChinese("-asdfn这ajadf-"));


        PinYin.PinyinMatch?.SetKeywords(new List<string> { "东 涌堡垒机", "南基" });

        var matchResult1 = PinYin.PinyinMatch?.Find("dong y");
        Console.WriteLine(matchResult1);
        if (matchResult1 != null)
        {
            foreach (var s in matchResult1)
            {
                Console.WriteLine(s);
                Console.WriteLine(s.Length);
            }
        }
    }


    private static void pinyinMatch()
    {
        var appData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        var dllPath = appData + @"\FlowLauncher\app-1.18.0\ToolGood.Words.Pinyin.dll";
        var assembly = Assembly.LoadFile(dllPath);
        var pinyinMatchType = assembly.GetType("ToolGood.Words.Pinyin.PinyinMatch");
        Console.WriteLine("pinyinMatchType = " + pinyinMatchType);
        if (pinyinMatchType != null)
        {
            var methodInfos = pinyinMatchType.GetMethods();
            foreach (var methodInfo in methodInfos)
            {
                Console.WriteLine(methodInfo);
            }

            var setKeywordsMethod = pinyinMatchType.GetMethod("SetKeywords", new[] { typeof(ICollection<string>) });
            Console.WriteLine("setKeywordsMethod = " + setKeywordsMethod);
            var instance = Activator.CreateInstance(pinyinMatchType);
            setKeywordsMethod.Invoke(instance, new object[] { new List<string> { "爱", "你" } });
        }
    }

    private static void reflectionTest()
    {
        var appData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        var dllPath = appData + @"\FlowLauncher\app-1.18.0\ToolGood.Words.Pinyin.dll";
        var assembly = Assembly.LoadFile(dllPath);
        var wordsHelperType = assembly.GetType("ToolGood.Words.Pinyin.WordsHelper");
        Console.WriteLine("wordsHelperType: " + wordsHelperType);
        if (wordsHelperType != null)
        {
            // var methodInfos = wordsHelperType.GetMethods();
            // foreach (var methodInfo in methodInfos)
            // {
            //     Console.WriteLine(methodInfo);
            // }


            var method = wordsHelperType.GetMethod("HasChinese", new[] { typeof(string) });
            Console.WriteLine("method: " + method);
            if (method != null)
            {
                var result = method.Invoke(null, new object[] { "爱你" });
                Console.WriteLine(result);
            }


            Console.WriteLine("---------------");
            var getPinyin = wordsHelperType.GetMethod("GetPinyin", new[] { typeof(string), typeof(bool) });
            Console.WriteLine("GetPinyin: method is null = " + (getPinyin == null));
            if (getPinyin != null)
            {
                var result = getPinyin.Invoke(null, new object[] { "爱你", false });
                Console.WriteLine("GetAllPinyin Result:" + result);
                Console.WriteLine("GetAllPinyin Result:" + (result == null));
            }
        }
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