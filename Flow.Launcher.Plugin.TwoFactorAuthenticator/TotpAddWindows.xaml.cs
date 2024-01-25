using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Google.Authenticator;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public delegate void OnTotpAdd(TotpModel model);

/// <summary>
/// TotpAddWindows.xaml 的交互逻辑
/// </summary>
public partial class TotpAddWindows : Window
{
    [CanBeNull] private readonly OnTotpAdd _totpAdd;

    public TotpAddWindows([CanBeNull] OnTotpAdd totpAdd, [CanBeNull] TotpModel oldData = null)
    {
        _totpAdd = totpAdd;
        InitializeComponent();

        KeyDown += Esc_Exit_KeyDown;

        InitComboBoxData();
    }

    private void InitComboBoxData()
    {
        var allAlgorithms = OtpAuthUtil.GetAllAlgorithms();
        var result = new List<string>();
        result.AddRange(allAlgorithms);
        result.Add(""); // default

        foreach (var item in result)
            ComboBoxAlgorithm.Items.Add(item);

        ComboBoxAlgorithm.SelectedIndex = result.Count - 1;
    }

    private void Esc_Exit_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void ImportQrCodeBtn_Click(object sender, RoutedEventArgs e)
    {
        TextBlockTip.Text = "";

        var dialog = new OpenFileDialog
        {
            Filter = "Image Files |*.png;*.jpg;*.jpeg|All Files (*.*)|*.*",
            ShowReadOnly = true,
            Multiselect = false
        };
        var show = dialog.ShowDialog();
        if (show is not true) return;

        var file = dialog.FileName;
        ReadQrCodeFile(file);
    }

    private void ReadQrCodeFile(string filePath)
    {
        var result = QrCodeUtil.ResolveQrCodeFile(filePath);
        if (result == null)
        {
            TextBlockTip.Text = "Invalid QRCode File";
            return;
        }

        var otpUrl = OtpAuthUtil.ResolveOtpUrl(result);
        if (otpUrl == null)
        {
            TextBlockTip.Text = $"Invalid QRCode Data: {result}";
            return;
        }

        if (otpUrl is not TotpModel totp)
        {
            TextBlockTip.Text = $"Not Support QRCode Data: {result}";
            return;
        }

        if (totp.Algorithm != null)
        {
            HashType hashType;
            try
            {
                hashType = OtpAuthUtil.ResolveHashTypeAlgorithm(totp.Algorithm);
            }
            catch (Exception e)
            {
                TextBlockTip.Text = e.Message;
                return;
            }

            ComboBoxAlgorithm.SelectedIndex = (int)hashType;
        }

        TextBoxIssuer.Text = totp.Issuer;
        TextBoxAccount.Text = totp.AccountTitle;
        TextBoxSecret.Text = totp.Secret;
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

        var account = TextBoxAccount.Text.Trim();
        if (account.Length == 0)
        {
            TextBlockTip.Text = "account can not be null";
            TextBoxAccount.Focusable = true;
            return;
        }

        var alg = ComboBoxAlgorithm.SelectionBoxItem as string;
        var algorithm = string.IsNullOrEmpty(alg) ? null : alg;

        var model = new TotpModel
        {
            Type = OtpAuthUtil.TotpType,
            Secret = secret,
            Issuer = issuer,
            AccountTitle = account,
            Algorithm = algorithm
        };
        _totpAdd?.Invoke(model);
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}