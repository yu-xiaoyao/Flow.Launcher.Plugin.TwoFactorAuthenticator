using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Google.Authenticator;
using JetBrains.Annotations;
using Microsoft.Win32;
using ZXing;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public delegate void OnTotpAdd(OtpParam model, int index);

/// <summary>
/// TotpAddWindows.xaml 的交互逻辑
/// </summary>
public partial class OtpAddWindows : Window
{
    [CanBeNull] private readonly OnTotpAdd _totpAdd;
    private readonly int _index;
    [CanBeNull] private readonly OtpParam _oldData;

    public OtpAddWindows([CanBeNull] OnTotpAdd totpAdd, [CanBeNull] OtpParam oldData = null, int index = -1)
    {
        _totpAdd = totpAdd;
        _oldData = oldData;
        _index = index;
        InitializeComponent();

        KeyDown += Esc_Exit_KeyDown;

        InitData();
    }

    private void InitData()
    {
        // type
        ComboBoxOtpType.Items.Add(OtpParam.TotpType);

        //TODO HOTP not support
        // ComboBoxOtpType.Items.Add(OtpParam.HotpType);

        ComboBoxDigits.Items.Add("6");
        ComboBoxDigits.Items.Add("8");

        var allAlgorithms = TotpUtil.GetAllAlgorithms();
        var algorithmList = new List<string>();
        algorithmList.AddRange(allAlgorithms);
        algorithmList.Add(""); // default

        foreach (var item in algorithmList)
            ComboBoxAlgorithm.Items.Add(item);

        var cbIndex = algorithmList.Count - 1;

        if (_oldData != null)
        {
            ImportQrCodeBtn.IsEnabled = false;
            ImportClipboard.IsEnabled = false;

            if (_oldData.OtpType == OtpParam.HotpType)
                ComboBoxOtpType.SelectedIndex = 1;
            else
                ComboBoxOtpType.SelectedIndex = 0; // default TOTP

            if (_oldData.Digits == 8) ComboBoxDigits.SelectedIndex = 1;
            else ComboBoxDigits.SelectedIndex = 0;


            // update 
            TextBoxName.Text = _oldData.Name ?? "";
            TextBoxSecret.Text = _oldData.Secret;
            TextBoxIssuer.Text = _oldData.Issuer;
            TextBoxRemark.Text = _oldData.Remark ?? "";
            TextBoxCounter.Text = $"{_oldData.Counter}";

            if (!string.IsNullOrEmpty(_oldData.Algorithm))
            {
                cbIndex = (int)TotpUtil.ResolveHashTypeAlgorithm(_oldData.Algorithm);
            }
        }
        else
        {
            ComboBoxOtpType.SelectedIndex = 0;
            ComboBoxDigits.SelectedIndex = 0;
            TextBoxCounter.Text = "0";
        }

        ComboBoxAlgorithm.SelectedIndex = cbIndex;
    }

    private void Esc_Exit_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }


    private void ImportQrCodeFile_Click(object sender, RoutedEventArgs e)
    {
        TextBlockTip.Text = "";

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

        var bitmap = new Bitmap(fileName);

        string result;
        try
        {
            result = QrCodeUtil.ResolveQrCode(bitmap);
        }
        catch (Exception exception)
        {
            TextBlockTip.Text = $"Invalid QRCode File. exception: {exception.Message}";
            return;
        }

        var param = OtpAuthUtil.AnalyzeOtpAuthUrl(result);
        if (param == null || !param.Any())
        {
            TextBlockTip.Text = $"Invalid QRCode Data: {result}";
            return;
        }

        SetDataView(param);
    }

    private void ImportClipboard_Click(object sender, RoutedEventArgs e)
    {
        string result = null;
        if (Clipboard.ContainsImage())
        {
            var bitmap = QrCodeUtil.GetBitmap(Clipboard.GetImage());
            if (bitmap != null)
            {
                result = QrCodeUtil.ResolveQrCode(bitmap);
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
                    result = QrCodeUtil.ResolveQrCodeFile(file);
                }
            }
        }
        else if (Clipboard.ContainsText(TextDataFormat.Text))
        {
            result = Clipboard.GetText(TextDataFormat.Text);
        }

        if (result == null) return;

        var param = OtpAuthUtil.AnalyzeOtpAuthUrl(result);
        if (param == null || !param.Any())
        {
            TextBlockTip.Text = $"Invalid QRCode Data: {result}";
            return;
        }

        SetDataView(param);
    }

    private void SetDataView(List<OtpParam> paramList)
    {
        if (paramList.Count > 1)
        {
            TextBlockTip.Text = $"Current Data Only Support One. Count: {paramList.Count}";
            return;
        }

        var param = paramList[0];
        if (param.Algorithm != null)
        {
            HashType hashType;
            try
            {
                hashType = TotpUtil.ResolveHashTypeAlgorithm(param.Algorithm);
            }
            catch (Exception e)
            {
                TextBlockTip.Text = e.Message;
                return;
            }

            ComboBoxAlgorithm.SelectedIndex = (int)hashType;
        }

        TextBoxIssuer.Text = param.Issuer;
        TextBoxName.Text = param.Name;
        TextBoxSecret.Text = param.Secret;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        TextBlockTip.Text = "";
        var secret = TextBoxSecret.Text.Trim();
        if (secret.Length == 0)
        {
            TextBlockTip.Text = "secret can not be null";
            TextBoxSecret.Focusable = true;
            return;
        }

        var issuer = TextBoxIssuer.Text.Trim();
        if (issuer.Length == 0)
        {
            TextBlockTip.Text = "issuer can not be null";
            TextBoxIssuer.Focusable = true;
            return;
        }

        var name = TextBoxName.Text.Trim();
        if (name.Length == 0)
        {
            TextBlockTip.Text = "name can not be null";
            TextBoxName.Focusable = true;
            return;
        }

        ulong counter = 0;
        if (!string.IsNullOrEmpty(TextBoxCounter.Text.Trim()))
        {
            if (!ulong.TryParse(TextBoxCounter.Text.Trim(), out counter))
            {
                TextBlockTip.Text = "counter must be number";
                TextBoxCounter.Focusable = true;
                return;
            }
        }

        var otpType = ComboBoxOtpType.SelectedIndex switch
        {
            1 => OtpParam.HotpType,
            _ => OtpParam.TotpType
        };

        var alg = ComboBoxAlgorithm.SelectionBoxItem as string;
        var algorithm = string.IsNullOrEmpty(alg) ? null : alg;

        var model = new OtpParam
        {
            OtpType = otpType,
            Name = name,
            Secret = secret,
            Issuer = issuer,
            Algorithm = algorithm,
            Remark = TextBoxRemark.Text.Trim(),
            Digits = ComboBoxDigits.SelectedIndex == 0 ? 6 : 8,
            Counter = counter
        };
        _totpAdd?.Invoke(model, _index);
        Close();
    }

    [GeneratedRegex("[^0-9]+")]
    private static partial Regex MyRegex();

    private void TextBoxCounter_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = MyRegex().IsMatch(e.Text);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}