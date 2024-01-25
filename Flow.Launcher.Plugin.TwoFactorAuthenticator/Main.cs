using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class TwoFactorAuthenticator : IPlugin, IContextMenu, ISettingProvider
    {
        private static readonly string IconPath = "Images\\TwoFactorAuthenticatorIcon.png";

        private PluginInitContext _context;
        private Settings _settings;


        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
        }

        public List<Result> Query(Query query)
        {
            var result = new List<Result>();

            var search = query.Search.TrimStart();

            var isNullSearch = string.IsNullOrEmpty(search);
            var otpList = _settings.GetOtpList();

            foreach (var otp in otpList)
            {
                if (!isNullSearch)
                {
                    if (otp.Name == null || !otp.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                if (otp is TotpModel totp)
                {
                    if (!isNullSearch)
                    {
                        if (!totp.Issuer.Contains(search, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!totp.AccountTitle.Contains(search, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    result.Add(new Result
                    {
                        Title = totp.Issuer, SubTitle = totp.AccountTitle, IcoPath = IconPath, ContextData = totp,
                        Action = _ =>
                        {
                            var code = OtpAuthUtil.GetCurrentPIN(totp.Secret, totp.Algorithm);
                            _context.API.CopyToClipboard(code);
                            return true;
                        }
                    });
                }
            }

            return result;
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            return new List<Result>();
        }

        public Control CreateSettingPanel()
        {
            return new SettingsControlPanel(_context, _settings);
        }
    }
}