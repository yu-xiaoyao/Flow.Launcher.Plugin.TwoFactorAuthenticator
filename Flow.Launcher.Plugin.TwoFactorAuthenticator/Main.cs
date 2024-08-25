using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class TwoFactorAuthenticator : IPlugin, IContextMenu, ISettingProvider, IAsyncReloadable, IPluginI18n
    {
        private static string IconPath = "Images\\TwoFactorAuthenticatorIcon.png";

        private PluginInitContext _context;
        private Settings _settings;


        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();

            _settings.PinyinSearch = PinYin.InitPinyinLib(context.API);
            if (_settings.PinyinSearch)
                PinYin.LoadPinyinResult(_settings, _context);
        }


        public List<Result> Query(Query query)
        {
            var search = query.Search.TrimStart();

            var result = _settings.OtpParams
                .Select(param => SelectOtpParam(param, search))
                .Where(r => r != null)
                .ToList();

            return result;
        }

        [CanBeNull]
        private Result SelectOtpParam(OtpParam param, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                // search is not empty, filter by input text

                var hasRemark = false;
                var pinyinMatch = false;
                var hasName = false;
                var hasIssuer = false;

                // 1. first filter by remark
                if (!string.IsNullOrWhiteSpace(param.Remark))
                {
                    hasRemark = param.Remark.Contains(search, StringComparison.OrdinalIgnoreCase);

                    if (_settings.PinyinSearch)
                    {
                        // mark text is match, no need to check pinyin
                        if (!hasRemark)
                        {
                            // _context.API.LogInfo("Pinyin", $"{param.Remark} - {search}. {pinyinMatch}");
                            pinyinMatch = _matchByPinyin(param.Remark, search);
                        }
                    }

                    if (_settings.SearchName)
                    {
                        hasName = param.Name.Contains(search, StringComparison.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    // no remark, force search name
                    hasName = param.Name.Contains(search, StringComparison.OrdinalIgnoreCase);
                }


                if (_settings.SearchIssuer)
                {
                    hasIssuer = param.Issuer.Contains(search, StringComparison.OrdinalIgnoreCase);
                }

                // only at least one match is ok
                if (!hasName && !hasIssuer && !hasRemark && !pinyinMatch)
                {
                    // all item is not match
                    return null;
                }
            }

            var title = string.IsNullOrWhiteSpace(param.Remark) ? param.Name : param.Remark;

            return new Result
            {
                Title = title,
                SubTitle = param.Name + ":" + param.Issuer,
                IcoPath = IconPath,
                ContextData = param,
                AutoCompleteText = title,
                Action = _ =>
                {
                    var code = OtpAuthUtil.GeneratePinCode(param);
                    _context.API.CopyToClipboard(code, showDefaultNotification: _settings.CopyNotification);
                    return true;
                },
                PreviewPanel = new Lazy<UserControl>(() =>
                    {
                        if (param.OtpType == OtpParam.TotpType)
                        {
                            return new TotpPreviewPanel(param, _context, _settings);
                        }

                        //TODO more type preview panel
                        return null;
                    }
                )
            };
        }

        /// <summary>
        /// 中文拼音搜索
        /// </summary>
        /// <param name="fromText"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        private bool _matchByPinyin(string fromText, string search)
        {
            try
            {
                var hasChinese = PinYin.WordsHelper.HasChinese(search);
                switch (hasChinese)
                {
                    case null:
                    case true:
                        return false;
                }

                // 输入 must english - pinyin

                // _context.API.LogInfo("MPYAAAAA", $"{fromText} - {search}");


                var result = PinYin.PinyinMatch.Find(search);

                // foreach (var se in result)
                // {
                //     _context.API.LogInfo("MPY", $"{fromText} - {search}....{se}");
                // }


                if (result == null || !result.Any())
                    return false;


                return result.Any(se => string.Equals(fromText, se, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception e)
            {
                _context.API.LogException("MatchByPinyin", e.Message, e);
            }

            return false;
        }


        #region Context Menu Event

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            var param = selectedResult.ContextData as OtpParam;
            var code = OtpAuthUtil.GeneratePinCode(param);
            var shareUrl = OtpAuthUtil.GenerateOtpAuthUrlShare(param);
            return new List<Result>
            {
                new()
                {
                    Title = code,
                    SubTitle = _context.API.GetTranslation("two_fa_current_code_copy_expire"),
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        _context.API.CopyToClipboard(code, showDefaultNotification: _settings.CopyNotification);
                        return true;
                    }
                },
                new()
                {
                    Title = _context.API.GetTranslation("two_fa_generate_share_qr_code"),
                    SubTitle = shareUrl,
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        new QRCodeForm(_context, shareUrl).Show();
                        return true;
                    }
                },
                new()
                {
                    Title = _context.API.GetTranslation("two_fa_generate_share_string"),
                    SubTitle = shareUrl,
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        _context.API.CopyToClipboard(shareUrl, showDefaultNotification: _settings.CopyNotification);
                        return true;
                    }
                },
                new()
                {
                    Title = _context.API.GetTranslation("two_fa_open_flow_settings"),
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        _context.API.OpenSettingDialog();
                        return true;
                    }
                },
                new()
                {
                    Title = _context.API.GetTranslation("two_fa_add_windows"),
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        var windows = new OtpAddWindows((model, index) => { })
                        {
                            WindowStartupLocation = WindowStartupLocation.CenterScreen,
                            Topmost = true
                        };
                        windows.ShowDialog();
                        return true;
                    }
                },
                new()
                {
                    Title = _context.API.GetTranslation("two_fa_open_settings"),
                    IcoPath = IconPath,
                    Action = _ =>
                    {
                        var windows = new Window
                        {
                            Content = new SettingsControlPanel(_context, _settings),
                            WindowStartupLocation = WindowStartupLocation.CenterScreen,
                            Topmost = true
                        };
                        windows.ShowDialog();
                        return true;
                    }
                }
            };
        }

        #endregion

        public Control CreateSettingPanel()
        {
            return new SettingsControlPanel(_context, _settings);
        }

        public Task ReloadDataAsync()
        {
            return Task.Run(() => { PinYin.LoadPinyinResult(_settings, _context); });
        }


        public string GetTranslatedPluginTitle()
        {
            return _context.API.GetTranslation("two_fa_plugin_title");
        }

        public string GetTranslatedPluginDescription()
        {
            return _context.API.GetTranslation("two_fa_plugin_description");
        }
    }
}