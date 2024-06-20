using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class TwoFactorAuthenticator : IPlugin, IContextMenu, ISettingProvider, IAsyncReloadable
    {
        private static string IconPath = "Images\\TwoFactorAuthenticatorIcon.png";

        private PluginInitContext _context;
        private Settings _settings;


        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();

            PinYin.InitPinyinLib();
            LoadPinyinResult();
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
            var searchKeyEmpty = string.IsNullOrEmpty(search);
            if (!searchKeyEmpty)
            {
                // search is not empty

                var hasRemark = false;
                var pinyinMatch = false;
                if (!string.IsNullOrWhiteSpace(param.Remark))
                {
                    hasRemark = param.Remark.Contains(search, StringComparison.OrdinalIgnoreCase);

                    if (!hasRemark)
                    {
                        pinyinMatch = _matchByPinyin(param.Remark, search);
                    }
                }


                var hasIssuer = param.Issuer.Contains(search, StringComparison.OrdinalIgnoreCase);

                var hasName = param.Name.Contains(search, StringComparison.OrdinalIgnoreCase);


                if (!hasName && !hasIssuer && !hasRemark && !pinyinMatch)
                {
                    return null;
                }
            }

            return new Result
            {
                Title = string.IsNullOrWhiteSpace(param.Remark) ? param.Name : param.Remark,
                SubTitle = param.Name + "(" + param.Issuer + ")",
                IcoPath = IconPath,
                ContextData = param,
                Action = _ =>
                {
                    var code = OtpAuthUtil.GeneratePinCode(param);
                    _context.API.CopyToClipboard(code);
                    return true;
                }
            };
        }


        public List<Result> LoadContextMenus(Result selectedResult)
        {
            return new List<Result>();
        }

        public Control CreateSettingPanel()
        {
            return new SettingsControlPanel(_context, _settings);
        }

        public Task ReloadDataAsync()
        {
            return Task.Run(() => { LoadPinyinResult(); });
        }

        private void LoadPinyinResult()
        {
            if (PinYin.PinyinMatch == null) return;

            var keys = _settings.OtpParams
                .Select(o => o.Remark)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            PinYin.PinyinMatch.SetKeywords(keys);
        }

        private bool _matchByPinyin(string fromText, string search)
        {
            if (PinYin.WordsHelper == null) return false;
            if (PinYin.PinyinMatch == null) return false;

            var hasChinese = PinYin.WordsHelper.HasChinese(search);
            if (hasChinese is not true) return false;
            
            var result = PinYin.PinyinMatch.Find(search);
            if (result == null || !result.Any())
                return false;
            return result.Any(se => string.Equals(fromText, se, StringComparison.OrdinalIgnoreCase));
        }
    }
}