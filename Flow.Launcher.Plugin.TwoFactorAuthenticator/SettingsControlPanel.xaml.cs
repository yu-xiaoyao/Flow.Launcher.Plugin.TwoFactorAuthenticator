using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    /// <summary>
    /// SettingsControl.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsControlPanel : UserControl
    {
        private readonly PluginInitContext _context;
        private readonly Settings _settings;

        public SettingsControlPanel(PluginInitContext context, Settings settings)
        {
            _context = context;
            _settings = settings;

            InitializeComponent();

            InitSettingData();
        }

        private void InitSettingData()
        {
            TestLabel.Content = $"COUNT is = {_settings.AuthenticatorItems.Count}";
        }
    }
}