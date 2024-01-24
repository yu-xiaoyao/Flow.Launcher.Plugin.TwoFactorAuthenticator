using System;
using System.Collections.Generic;
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

            result.Add(new Result
            {
                Title = "Test",
                SubTitle = "SubTitle",
                IcoPath = IconPath
            });

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