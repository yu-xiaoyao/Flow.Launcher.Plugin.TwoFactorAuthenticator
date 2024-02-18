using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    /// <summary>
    /// SettingsControl.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsControlPanel : UserControl
    {
        private readonly PluginInitContext _context;
        private readonly Settings _settings;

        private JsonSerializerOptions _jsonSerializerOptions;


        public SettingsControlPanel(PluginInitContext context, Settings settings)
        {
            _context = context;
            _settings = settings;

            InitializeComponent();

            InitSettingData();
        }

        private void InitSettingData()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 中文字不編碼
                WriteIndented = true // 換行與縮排
            };

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

        private void Totp_Export_Json(object sender, RoutedEventArgs e)
        {
            ExportTotpJsonFile(_settings.TotpList.ToList());
        }

        private void Totp_Import_Json(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Json File |*.json",
                Multiselect = false
            };
            var openResult = ofd.ShowDialog();
            if (openResult is not true) return;
            var fileName = ofd.FileName;
            if (!File.Exists(fileName)) return;
            try
            {
                var content = File.ReadAllText(fileName);
                var totpList = JsonSerializer.Deserialize(content, typeof(List<TotpModel>)) as List<TotpModel>;
                foreach (var totpModel in totpList)
                {
                    if (OtpAuthUtil.TotpType == totpModel.Type)
                    {
                        //TODO valid json data valid
                        _settings.TotpList.Add(totpModel);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }


        private void ExportTotpJsonFile(List<TotpModel> totpList)
        {
            var result = JsonSerializer.Serialize(totpList, _jsonSerializerOptions);
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Json File |*.json"
            };
            var openResult = saveFileDialog.ShowDialog();
            if (openResult is not true) return;
            var filename = saveFileDialog.FileName;
            File.WriteAllText(filename, result, Encoding.UTF8);
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
            if (item is not TotpModel totp) return;

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

            var exportItem = new MenuItem
            {
                Header = "Export",
            };
            exportItem.Click += (o, args) =>
            {
                if (TotpDataGrid.SelectedIndex == -1) return;
                var item = TotpDataGrid.SelectedItem;
                if (item is TotpModel totp)
                {
                    var otpAuthModels = new List<TotpModel> { totp };
                    ExportTotpJsonFile(otpAuthModels);
                }
            };


            TotpDataGrid.ContextMenu = new ContextMenu
            {
                Items =
                {
                    updateItem,
                    deleteItem,
                    exportItem,
                },
                StaysOpen = true
            };
        }
    }
}