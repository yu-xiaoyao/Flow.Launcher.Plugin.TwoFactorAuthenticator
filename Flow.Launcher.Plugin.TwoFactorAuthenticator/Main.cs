using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class TwoFactorAuthenticator : IPlugin, IContextMenu, ISettingProvider
    {
        private const string IconPath = "TwoFactorAuthenticatorIcon.png";

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
    }
}