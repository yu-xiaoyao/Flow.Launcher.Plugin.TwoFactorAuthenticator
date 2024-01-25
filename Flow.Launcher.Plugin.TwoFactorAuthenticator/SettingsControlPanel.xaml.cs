using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            AddTotpContextMenu();
        }

        private void Totp_Add_Click(object sender, RoutedEventArgs e)
        {
            var totpAdd = new TotpAddWindows(AddTotpItemToSettings)
            {
                Title = "Add TOTP",
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
            };
            totpAdd.ShowDialog();
        }

        private void Save_Settings(object sender, RoutedEventArgs e)
        {
            _context.API.SavePluginSettings();
        }


        private void DataGrid_Mouse_Double_Click(object sender, MouseButtonEventArgs e)
        {
            var dg = (sender as DataGrid)!;

            var index = dg.SelectedIndex;

            if (index < 0) return;

            var item = dg.SelectedItem;
            if (item is TotpModel totp)
            {
                var totpAdd = new TotpAddWindows(AddTotpItemToSettings, totp, index)
                {
                    Title = "Edit TOTP",
                    Topmost = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                };
                totpAdd.ShowDialog();
            }
        }

        private void AddTotpItemToSettings(TotpModel totp, int index)
        {
            if (index >= 0)
            {
                // UPDATE
                _settings.TotpList[index] = totp;
            }
            else
            {
                var findIndex = -1;
                for (var i = 0; i < _settings.TotpList.Count; i++)
                {
                    var item = _settings.TotpList[i];
                    if (!item.Secret.Equals(totp.Secret)) continue;
                    findIndex = i;
                    break;
                }

                if (findIndex != -1)
                {
                    var oldItem = _settings.TotpList[findIndex];

                    var result =
                        MessageBox.Show(
                            $"Two factor authenticator secret duplication old name = {oldItem.Name}, new  name = {totp.Name}",
                            "Confirm Replace?");
                    if (result is MessageBoxResult.OK or MessageBoxResult.Yes)
                    {
                        // replace update
                        _settings.TotpList[findIndex] = totp;
                    }
                }
                else
                {
                    _settings.TotpList.Add(totp);
                }
            }
        }


        private void AddTotpContextMenu()
        {
            var updateItem = new MenuItem
            {
                Header = "Update",
            };
            updateItem.Click += (o, args) =>
            {
                if (TotpDataGrid.SelectedIndex == -1) return;
                var item = TotpDataGrid.SelectedItem;
                if (item is TotpModel totp)
                {
                    var totpAdd = new TotpAddWindows(AddTotpItemToSettings, totp, TotpDataGrid.SelectedIndex)
                    {
                        Title = "Edit TOTP",
                        Topmost = true,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize,
                        ShowInTaskbar = false,
                    };
                    totpAdd.ShowDialog();
                }
            };
            var deleteItem = new MenuItem
            {
                Header = "Delete",
            };
            deleteItem.Click += (o, args) =>
            {
                if (TotpDataGrid.SelectedIndex == -1) return;
                var item = TotpDataGrid.SelectedItem;
                if (item is TotpModel totp)
                {
                    var result = MessageBox.Show(
                        $"confirm delete two factor authenticator , name = {totp.Name}, issuer = {totp.Issuer}",
                        "Confirm Delete?", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        _settings.TotpList.RemoveAt(TotpDataGrid.SelectedIndex);
                    }
                }
            };

            TotpDataGrid.ContextMenu = new ContextMenu
            {
                Items =
                {
                    updateItem,
                    deleteItem,
                },
                StaysOpen = true
            };
        }
    }
}