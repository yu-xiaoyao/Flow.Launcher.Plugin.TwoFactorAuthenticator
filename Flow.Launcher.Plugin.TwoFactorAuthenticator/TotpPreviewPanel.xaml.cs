using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public partial class TotpPreviewPanel : UserControl
{
    private readonly PluginInitContext _context;
    private readonly Settings _settings;
    private readonly OtpParam _param;

    private readonly DispatcherTimer _dispatcherTimer;

    public TotpPreviewPanel(OtpParam param, PluginInitContext context, Settings settings)
    {
        _param = param;
        _context = context;
        _settings = settings;
        InitializeComponent();

        // _context.API.LogInfo("Preview", $"{param.Remark} - {param.Name} - {param.Issuer}");
        // Initialized += MainWindow_Initialized;

        LabelType.Content = $"OTP Type: {param.OtpType}";
        LabelName.Content = $"Name: {param.Name}";
        LabelIssuer.Content = $"Issuer: {param.Issuer}";
        if (!string.IsNullOrWhiteSpace(param.Remark))
            LabelRemark.Content = $"Remark: {param.Remark}";


        Unloaded += MainWindow_Unloaded;
        Loaded += MainWindow_Loaded;
        _dispatcherTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _dispatcherTimer.Tick += (_, _) => _render();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        //_context.API.LogInfo("Loaded", $"{_param.Remark} - {_param.Name} - {_param.Issuer}");
        // render first
        _render();
        // start timer
        _dispatcherTimer.Start();
    }


    private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
    {
        //_context.API.LogInfo("Unloaded", $"{_param.Remark} - {_param.Name} - {_param.Issuer}");
        // stop timer
        _dispatcherTimer.Stop();
    }


    private void _render()
    {
        var remaining = TotpUtil.GetRemainingSeconds();
        var code = TotpUtil.GenerateTOTPPinCode(_param);

        // _context.API.LogInfo("Render", $"{_param.Remark} - {_param.Name} - {_param.Issuer}");

        PinCodeLabel.Content = code;
        RemainingTimeLabel.Content = string.Format(_context.API.GetTranslation("two_fa_remaining_time"), remaining);
    }

    private void MainWindow_Initialized(object sender, EventArgs e)
    {
        _context.API.LogInfo("Initialized", $"{_param.Remark} - {_param.Name} - {_param.Issuer}");
    }

    private void PinCode_Copy_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _context.API.CopyToClipboard(PinCodeLabel.Content as string,
            showDefaultNotification: _settings.CopyNotification);
    }
}