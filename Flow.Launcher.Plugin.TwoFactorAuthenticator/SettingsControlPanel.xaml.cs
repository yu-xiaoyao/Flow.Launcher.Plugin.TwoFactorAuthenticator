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
            TotpDataGrid.IsReadOnly = true;
            TotpDataGrid.ItemsSource = _settings.TotpList;
        }

        private void Totp_Add_Click(object sender, RoutedEventArgs e)
        {
            var totpAdd = new TotpAddWindows(AddTotpItemToSettings)
            {
                Name = "Add TOTP",
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
            };
            totpAdd.ShowDialog();
        }

        private void AddTotpItemToSettings(TotpModel totp)
        {
            var findIndex = -1;
            for (var i = 0; i < _settings.TotpList.Count; i++)
            {
                var item = _settings.TotpList[i];
                if (!item.Secret.Equals(totp.Secret)) continue;
                findIndex = i;
                break;
            }

            _settings.TotpList.Add(totp);

            //TODO valid dep
            // if (findIndex != -1)
            // {
            //     var oldItem = _settings.TotpList[findIndex];
            //
            //     var result = MessageBox.Show("Secret duplication");
            //     if (result == MessageBoxResult.OK)
            //     {
            //         // replace 
            //         _settings.TotpList[findIndex] = totp;
            //     }
            // }
            // else
            // {
            //     _settings.TotpList.Add(totp);
            // }
        }
    }
}