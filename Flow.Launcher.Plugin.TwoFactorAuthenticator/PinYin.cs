using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public class PinYin
{
    private static Type _pinyinMatchType;
    private static Type _wordsHelperType;

    [CanBeNull] public static PinyinMatch PinyinMatch { get; private set; }
    [CanBeNull] public static WordsHelper WordsHelper { get; private set; }

    public static void InitPinyinLib()
    {
        InitPinyinLib(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
    }


    /**
     * init or reload plugin
     * must be end with /
     */
    public static void InitPinyinLib(string flowBaseDir)
    {
        var dllPath = flowBaseDir + "ToolGood.Words.Pinyin.dll";
        if (File.Exists(dllPath))
        {
            var assembly = Assembly.LoadFile(dllPath);
            _pinyinMatchType = assembly.GetType("ToolGood.Words.Pinyin.PinyinMatch");
            NewPinyinMatch();

            _wordsHelperType = assembly.GetType("ToolGood.Words.Pinyin.WordsHelper");
            NewWordsHelper();
        }
    }

    public static void Dispose()
    {
        _pinyinMatchType = null;
        _wordsHelperType = null;
        PinyinMatch = null;
        WordsHelper = null;
    }


    public static void NewPinyinMatch()
    {
        if (_pinyinMatchType == null) return;

        var setKeywordsMethod = _pinyinMatchType.GetMethod("SetKeywords", new[] { typeof(ICollection<string>) });
        if (setKeywordsMethod == null) return;

        var findIndexMethod = _pinyinMatchType.GetMethod("FindIndex", new[] { typeof(string) });
        if (findIndexMethod == null) return;
        if (findIndexMethod.ReturnType != typeof(List<int>)) return;

        var instance = Activator.CreateInstance(_pinyinMatchType);
        if (instance == null) return;

        PinyinMatch = new PinyinMatch
        {
            SetKeywordsMethod = setKeywordsMethod,
            FindIndexMethod = findIndexMethod,
            Instance = instance,
            FindMethod = _pinyinMatchType.GetMethod("Find", new[] { typeof(string) })
        };
    }

    public static void NewWordsHelper()
    {
        if (_wordsHelperType == null) return;

        WordsHelper = new WordsHelper
        {
            GetPinyinMethod = _wordsHelperType.GetMethod("GetPinyin", new[] { typeof(string), typeof(bool) }),
            GetFirstPinyinMethod = _wordsHelperType.GetMethod("GetFirstPinyin", new[] { typeof(string) }),
            HasChineseMethod = _wordsHelperType.GetMethod("HasChinese", new[] { typeof(string) }),
            IsAllChineseMethod = _wordsHelperType.GetMethod("IsAllChinese", new[] { typeof(string) }),
        };
    }
}

public class PinyinMatch
{
    public MethodInfo SetKeywordsMethod { set; private get; }
    public MethodInfo FindIndexMethod { set; private get; }
    public object Instance { set; private get; }

    [CanBeNull] public MethodInfo FindMethod { set; private get; }

    [CanBeNull] private List<string> _keywords = null;


    [CanBeNull]
    public List<string> FindPinyin(string key)
    {
        if (_keywords == null || !_keywords.Any()) return null;

        var indies = (List<int>)FindIndexMethod.Invoke(Instance, new object[] { key });
        if (indies == null) return null;

        return !indies.Any() ? new List<string>() : indies.Select(index => _keywords[index]).ToList();
    }

    public void SetKeywords(ICollection<string> keywords, bool force = false)
    {
        if (force)
        {
            _keywords = keywords.ToList();

            if (keywords == null || keywords.Any())
            {
                SetKeywordsMethod.Invoke(Instance, new object[] { new List<string>() });
            }
            else
            {
            }
        }
        else
        {
            if (_keywords == null || !_keywords.Any())
            {
                _keywords = keywords?.ToList();
                SetKeywordsMethod.Invoke(Instance, new object[] { keywords });
            }
        }
    }

    [CanBeNull]
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

public class WordsHelper
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