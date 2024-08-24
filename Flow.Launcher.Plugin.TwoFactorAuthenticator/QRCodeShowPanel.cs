using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;


public class QRCodeShowPanel : UserControl
{
    private readonly PluginInitContext _context;
    private readonly string _content;
    

    /// <summary>
    /// init
    /// </summary>
    /// <param name="context"></param>
    /// <param name="content"></param>
    public QRCodeShowPanel(PluginInitContext context, string content)
    {
        _context = context;
        _content = content;

        AddQrCodeView();

        Loaded += MyLoadedRoutedEventHandler;
    }

    private void AddQrCodeView()
    {
        var imageView = new Image
        {
            Source = QrCodeUtil.CreateQrCode<BitmapImage>(_content)
        };
        AddChild(imageView);
    }

    private void MyLoadedRoutedEventHandler(object sender, RoutedEventArgs e)
    {
    }
}