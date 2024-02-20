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
using JetBrains.Annotations;
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
            TotpDataGrid.ItemsSource = _settings.OtpParams;
            AddTotpContextMenu();
        }

        private void Otp_Add_Click(object sender, RoutedEventArgs e)
        {
            var totpAdd = new OtpAddWindows(AddTotpItemToSettings)
            {
                Title = "Add OTP",
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
            };
            totpAdd.ShowDialog();
        }

        private void Otp_Export_Json(object sender, RoutedEventArgs e)
        {
            ExportTotpJsonFile(_settings.OtpParams.ToList());
        }

        private void Import_OTP_Migration_From_QrFile(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image Files |*.png;*.jpg;*.jpeg|All Files (*.*)|*.*",
                ShowReadOnly = true,
                Multiselect = false
            };

            var showDialog = ofd.ShowDialog();
            if (showDialog is not true) return;

            var fileName = ofd.FileName;
            if (!File.Exists(fileName)) return;

            var otpMigration = QrCodeUtil.ResolveQrCodeFile(fileName);
            AddOtpAuthMigration(otpMigration);
        }

        private void Import_OTP_Migration_From_Clipboard(object sender, RoutedEventArgs e)
        {
            string otpMigration = null;
            if (Clipboard.ContainsImage())
            {
                var bitmap = QrCodeUtil.GetBitmap(Clipboard.GetImage());
                if (bitmap != null)
                {
                    otpMigration = QrCodeUtil.ResolveQrCode(bitmap);
                }
            }
            else if (Clipboard.ContainsFileDropList())
            {
                var fileDropList = Clipboard.GetFileDropList();

                if (fileDropList.Count > 0)
                {
                    var file = fileDropList[0];
                    if (file != null && (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg")))
                    {
                        otpMigration = QrCodeUtil.ResolveQrCodeFile(file);
                    }
                }
            }
            else if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                otpMigration = Clipboard.GetText(TextDataFormat.Text);
            }

            AddOtpAuthMigration(otpMigration);
        }

        private void AddOtpAuthMigration([CanBeNull] string otpMigration)
        {
            if (string.IsNullOrWhiteSpace(otpMigration)) return;

            var otpParams = OtpAuthUtil.ResolveOtpAuthMigrationUrl(otpMigration);
            if (otpParams is not { Count: > 0 }) return;
            foreach (var otpParam in otpParams)
            {
                _settings.OtpParams.Add(otpParam);
            }
        }

        private void Otp_Import_Json(object sender, RoutedEventArgs e)
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
                var totpList = JsonSerializer.Deserialize(content, typeof(List<OtpParam>)) as List<OtpParam>;
                if (totpList == null) return;
                foreach (var param in totpList)
                {
                    if (param.OtpType == OtpParam.TotpType)
                    {
                        //TODO valid json data valid
                        _settings.OtpParams.Add(param);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }


        private void ExportTotpJsonFile(List<OtpParam> otpList)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Json File |*.json"
            };
            var openResult = saveFileDialog.ShowDialog();
            if (openResult is not true) return;
            var filename = saveFileDialog.FileName;
            var result = JsonSerializer.Serialize(otpList, _jsonSerializerOptions);
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
            if (item is not OtpParam otpParam) return;

            var totpAdd = new OtpAddWindows(AddTotpItemToSettings, otpParam, index)
            {
                Title = "Edit OTP",
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
            };
            totpAdd.ShowDialog();
        }

        private void AddTotpItemToSettings(OtpParam otpParam, int index)
        {
            if (index >= 0)
            {
                // UPDATE
                _settings.OtpParams[index] = otpParam;
            }
            else
            {
                var findIndex = -1;
                for (var i = 0; i < _settings.OtpParams.Count; i++)
                {
                    var item = _settings.OtpParams[i];

                    if (item.OtpType == otpParam.OtpType && item.Secret == otpParam.Secret)
                    {
                        findIndex = i;
                        break;
                    }
                }

                if (findIndex != -1)
                {
                    var oldItem = _settings.OtpParams[findIndex];

                    var result =
                        MessageBox.Show(
                            $"duplication secret. old name = {oldItem.Name}, new name = {otpParam.Name}",
                            "Confirm Replace?", MessageBoxButton.YesNoCancel);
                    if (result is MessageBoxResult.Yes)
                    {
                        // replace update
                        _settings.OtpParams[findIndex] = otpParam;
                    }
                    else if (result is MessageBoxResult.No)
                    {
                        // add 
                        _settings.OtpParams.Add(otpParam);
                    }
                }
                else
                {
                    _settings.OtpParams.Add(otpParam);
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
                if (item is OtpParam param)
                {
                    var totpAdd = new OtpAddWindows(AddTotpItemToSettings, param, TotpDataGrid.SelectedIndex)
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
                if (item is OtpParam param)
                {
                    var result = MessageBox.Show(
                        $"confirm delete two factor authenticator , name = {param.Name}, issuer = {param.Issuer}",
                        "Confirm Delete?", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                    {
                        _settings.OtpParams.RemoveAt(TotpDataGrid.SelectedIndex);
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
                if (item is not OtpParam param) return;
                var otpAuthModels = new List<OtpParam> { param };
                ExportTotpJsonFile(otpAuthModels);
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