using System;
using System.Collections.Generic;
using System.Linq;
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
            var search = query.Search.TrimStart();

            var result = _settings.OtpParams
                .Select(param => SelectOtpParam(param, search))
                .Where(r => r != null)
                .ToList();

            #region old impl

            // old impl
            // var otpList = _settings.GetOtpList();
            // foreach (var otp in otpList)
            // {
            //     if (otp is TotpModel totp)
            //     {
            //         var item = SelectOtpModel(totp, search);
            //         if (item != null)
            //             result.Add(item);
            //     }
            // }

            #endregion

            return result;
        }

        [CanBeNull]
        private Result SelectOtpParam(OtpParam param, string search)
        {
            var searchKeyEmpty = string.IsNullOrEmpty(search);
            if (!searchKeyEmpty)
            {
                // search is not empty
                var hasRemark = param.Remark != null &&
                                param.Remark.Contains(search, StringComparison.OrdinalIgnoreCase);

                var hasIssuer = param.Issuer.Contains(search, StringComparison.OrdinalIgnoreCase);

                var hasName = param.Name.Contains(search, StringComparison.OrdinalIgnoreCase);

                if (!hasName && !hasIssuer && !hasRemark)
                {
                    return null;
                }
            }

            return new Result
            {
                Title = string.IsNullOrWhiteSpace(param.Remark) ? param.Name : $"{param.Name}({param.Remark})",
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


        #region old data struct

        /*[CanBeNull]
        private Result SelectOtpModel(TotpModel totp, string search)
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
                    var code = TotpUtil.GenerateTOTPPinCode(totp.Algorithm, totp.Secret);
                    _context.API.CopyToClipboard(code);
                    return true;
                }
            };
        }*/

        #endregion

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