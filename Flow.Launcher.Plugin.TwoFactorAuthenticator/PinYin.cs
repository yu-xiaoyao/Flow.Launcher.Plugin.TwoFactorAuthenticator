﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

#region Common Pinyin Interface

public interface IPinyinMatch
{
    [CanBeNull]
    public List<string> FindPinyin(string key);

    public void SetKeywords(ICollection<string> keywords, bool force = false);

    [CanBeNull]
    public List<string> Find(string key);

    [CanBeNull]
    public List<int> FindIndex(string key);
}

public interface IWordsHelper
{
    [CanBeNull]
    public string GetPinyin(string text, bool tone = false);

    [CanBeNull]
    public string GetFirstPinyin(string text);

    [CanBeNull]
    public bool? HasChinese(string content);

    [CanBeNull]
    public bool? IsAllChinese(string content);
}

#endregion

public class PinYin
{
    // private static Type _pinyinMatchType;
    // private static Type _wordsHelperType;

    public static IPinyinMatch PinyinMatch { get; private set; }
    public static IWordsHelper WordsHelper { get; private set; }


    public static bool InitPinyinLib([CanBeNull] IPublicAPI publicApi)
    {
        return InitPinyinLib(publicApi, AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
    }


    /**
     * init or reload plugin
     * must be end with /
     */
    public static bool InitPinyinLib([CanBeNull] IPublicAPI publicApi, string flowBaseDir)
    {
        IPinyinMatch pinyinMatch = null;
        IWordsHelper wordsHelper = null;
        try
        {
            var dllPath = flowBaseDir + "ToolGood.Words.Pinyin.dll";
            if (File.Exists(dllPath))
            {
                var assembly = Assembly.LoadFile(dllPath);
                var pinyinMatchType = assembly.GetType("ToolGood.Words.Pinyin.PinyinMatch");
                var wordsHelperType = assembly.GetType("ToolGood.Words.Pinyin.WordsHelper");

                if (pinyinMatchType != null && wordsHelperType != null)
                {
                    pinyinMatch = NewPinyinMatch(pinyinMatchType);
                    wordsHelper = NewWordsHelper(wordsHelperType);
                }
            }
        }
        catch (Exception e)
        {
            publicApi?.LogException("InitPinyinLibError", e.Message, e);
        }

        if (pinyinMatch != null && wordsHelper != null)
        {
            publicApi?.LogInfo("InitPinyinLib", "Init PinyinLib Success");
            PinyinMatch = pinyinMatch;
            WordsHelper = wordsHelper;
            return true;
        }

        PinyinMatch = new NonePinyinMatch();
        WordsHelper = new NoneWordsHelper();

        return false;
    }

    public static void LoadPinyinResult(Settings settings, PluginInitContext context)
    {
        var keys = settings.OtpParams
            .Select(o => o.Remark)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        try
        {
            PinYin.PinyinMatch.SetKeywords(keys, true);
        }
        catch (Exception e)
        {
            context.API.LogException("LoadPinyinResult", e.Message, e);
        }
    }

    public static void Dispose()
    {
        // _pinyinMatchType = null;
        // _wordsHelperType = null;
        PinyinMatch = null;
        WordsHelper = null;
    }


    [CanBeNull]
    public static IPinyinMatch NewPinyinMatch(Type pinyinMatchType)
    {
        var setKeywordsMethod = pinyinMatchType.GetMethod("SetKeywords", new[] { typeof(ICollection<string>) });
        if (setKeywordsMethod == null) return null;

        var findIndexMethod = pinyinMatchType.GetMethod("FindIndex", new[] { typeof(string) });
        if (findIndexMethod == null) return null;
        if (findIndexMethod.ReturnType != typeof(List<int>)) return null;

        var findMethod = pinyinMatchType.GetMethod("Find", new[] { typeof(string) });
        if (findMethod == null) return null;
        if (findMethod.ReturnType != typeof(List<string>)) return null;

        var instance = Activator.CreateInstance(pinyinMatchType);
        if (instance == null) return null;

        return new FlowReflectionPinyinMatch
        {
            SetKeywordsMethod = setKeywordsMethod,
            FindIndexMethod = findIndexMethod,
            Instance = instance,
            FindMethod = findMethod
        };
    }

    [CanBeNull]
    public static IWordsHelper NewWordsHelper(Type wordsHelperType)
    {
        var hasChineseMethod = wordsHelperType.GetMethod("HasChinese", new[] { typeof(string) });
        if (hasChineseMethod == null) return null;

        return new FlowReflectionWordsHelper
        {
            HasChineseMethod = hasChineseMethod,
            GetPinyinMethod = wordsHelperType.GetMethod("GetPinyin", new[] { typeof(string), typeof(bool) }),
            GetFirstPinyinMethod = wordsHelperType.GetMethod("GetFirstPinyin", new[] { typeof(string) }),
            IsAllChineseMethod = wordsHelperType.GetMethod("IsAllChinese", new[] { typeof(string) }),
        };
    }
}

class NonePinyinMatch : IPinyinMatch
{
    public List<string> FindPinyin(string key)
    {
        return null;
    }

    public void SetKeywords(ICollection<string> keywords, bool force = false)
    {
    }


    public List<string> Find(string key)
    {
        return null;
    }

    public List<int> FindIndex(string key)
    {
        return null;
    }
}

public class FlowReflectionPinyinMatch : IPinyinMatch
{
    public MethodInfo SetKeywordsMethod { set; private get; }
    public MethodInfo FindIndexMethod { set; private get; }
    public object Instance { set; private get; }

    [CanBeNull] public MethodInfo FindMethod { set; private get; }

    private List<string> _keywords = new();


    public List<string> FindPinyin(string key)
    {
        if (!_keywords.Any()) return null;

        var indies = (List<int>)FindIndexMethod.Invoke(Instance, new object[] { key });
        if (indies == null) return null;
        return !indies.Any() ? new List<string>() : indies.Select(index => _keywords[index]).ToList();
    }

    public void SetKeywords(ICollection<string> keywords, bool force = false)
    {
        if (force)
        {
            _keywords = keywords.ToList();
            SetKeywordsMethod.Invoke(Instance, new object[] { _keywords });
        }
        else if (!_keywords.Any())
        {
            _keywords = keywords.ToList();
            SetKeywordsMethod.Invoke(Instance, new object[] { keywords });
        }
    }

    public List<string> Find(string key)
    {
        if (FindMethod == null) return null;
        return (List<string>)FindMethod.Invoke(Instance, new object[] { key });
    }

    public List<int> FindIndex(string key)
    {
        if (FindIndexMethod == null) return null;
        return (List<int>)FindIndexMethod.Invoke(Instance, new object[] { key });
    }
}

class NoneWordsHelper : IWordsHelper
{
    public string GetPinyin(string text, bool tone = false)
    {
        return null;
    }

    public string GetFirstPinyin(string text)
    {
        return null;
    }

    public bool? HasChinese(string content)
    {
        return null;
    }

    public bool? IsAllChinese(string content)
    {
        return null;
    }
}

public class FlowReflectionWordsHelper : IWordsHelper
{
    [CanBeNull] public MethodInfo GetPinyinMethod { set; private get; }
    [CanBeNull] public MethodInfo GetFirstPinyinMethod { set; private get; }
    [CanBeNull] public MethodInfo HasChineseMethod { set; private get; }
    [CanBeNull] public MethodInfo IsAllChineseMethod { set; private get; }

    /// <summary>
    /// 获取拼音全拼,支持多音,中文字符集为[0x4E00,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
    /// </summary>
    /// <param name="text">原文本</param>
    /// <param name="tone">是否带声调</param>
    /// <returns></returns>
    [CanBeNull]
    public string GetPinyin(string text, bool tone = false)
    {
        if (GetPinyinMethod == null) return null;
        return (string)GetPinyinMethod.Invoke(null, new object[] { text, tone });
    }

    /// <summary>
    /// 获取拼音首字母
    /// </summary>
    /// <param name="text">原文本</param>
    /// <returns></returns>
    [CanBeNull]
    public string GetFirstPinyin(string text)
    {
        if (GetFirstPinyinMethod == null) return null;
        return (string)GetFirstPinyinMethod.Invoke(null, new object[] { text });
    }

    /// <summary>
    /// 判断输入是否为中文  ,中文字符集为[0x4E00,0x9FA5][0x3400,0x4db5]
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public bool? HasChinese(string content)
    {
        if (HasChineseMethod == null) return null;
        return (bool?)HasChineseMethod.Invoke(null, new object[] { content });
    }

    /// <summary>
    /// 判断输入是否为中文  ,中文字符集为[0x4E00,0x9FA5][0x3400,0x4db5]
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public bool? IsAllChinese(string content)
    {
        if (IsAllChineseMethod == null) return null;
        return (bool?)IsAllChineseMethod.Invoke(null, new object[] { content });
    }
}