using System;
using System.Collections.Generic;
using System.Windows.Controls;
using JetBrains.Annotations;

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

            var otpList = _settings.GetOtpList();
            foreach (var otp in otpList)
            {
                if (otp is TotpModel totp)
                {
                    var item = SelectTotpModel(totp, search);
                    if (item != null)
                        result.Add(item);
                }
            }

            return result;
        }

        [CanBeNull]
        private Result SelectTotpModel(TotpModel totp, string search)
        {
            var searchKeyEmpty = string.IsNullOrEmpty(search);
            if (!searchKeyEmpty)
            {
                // search is not empty
                var hasName = false;
                var hasIssuer = false;
                var hasAccount = false;

                if (totp.Name != null && totp.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    hasName = true;

                if (totp.Issuer.Contains(search, StringComparison.OrdinalIgnoreCase))
                    hasIssuer = true;

                if (totp.AccountTitle.Contains(search, StringComparison.OrdinalIgnoreCase))
                    hasAccount = true;

                if (!hasName && !hasIssuer && !hasAccount)
                {
                    return null;
                }
            }

            return new Result
            {
                Title = string.IsNullOrEmpty(totp.Name) ? totp.Issuer : totp.Name,
                SubTitle = totp.Issuer + ":" + totp.AccountTitle,
                IcoPath = IconPath,
                ContextData = totp,
                Action = _ =>
                {
                    var code = OtpAuthUtil.GetCurrentPIN(totp.Secret, totp.Algorithm);
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
    }
}