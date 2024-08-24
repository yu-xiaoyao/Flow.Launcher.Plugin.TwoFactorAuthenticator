﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator;

public partial class QRCodeForm : Window
{
    private readonly PluginInitContext _context;
    private readonly string _content;

    private bool _fixWindows = true;

    /// <summary>
    /// init
    /// </summary>
    /// <param name="content"></param>
    public QRCodeForm(PluginInitContext context, string content)
    {
        _context = context;
        _content = content;

        Title = "QRCode";
        Width = 640.0;
        Height = 640.0;

        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Topmost = true;
        ShowInTaskbar = false;

        // Opacity = 0.9;
        // AllowsTransparency = true;
        // Background = new SolidColorBrush(Colors.White);

        WindowStyle = WindowStyle.None;

        Activated += Window_Activated;
        Deactivated += Window_Deactivated;
        KeyDown += Esc_Exit_KeyDown;

        AddContextMenu();

        AddQrCodePanel();
    }

    private void AddContextMenu()
    {
        var copyAsFile = new MenuItem
        {
            Header = "Copy As File",
        };
        copyAsFile.Click += (o, args) =>
        {
            var filePath = QrCodeUtil.CreateQrCode<string>(_content);
            try
            {
                if (File.Exists(filePath))
                {
                    CopyFileToClipboard(filePath);
                    _context.API.ShowMsg("copy success");
                }
            }
            catch (Exception e)
            {
                var message = $"{filePath} -- {e.Message}";
                _context.API.LogException("QrCodeGenerator", message, e);
            }
        };
        var fixWindows = new MenuItem
        {
            Header = "Fix Windows",
        };
        fixWindows.Click += (o, args) => _fixWindows = !_fixWindows;

        ContextMenu = new ContextMenu
        {
            Items =
            {
                copyAsFile,
                fixWindows
            }
        };
    }

    private void Window_Activated(object sender, EventArgs e)
    {
    }

    private void AddQrCodePanel()
    {
        var ww = Width;
        var wh = Height;
        var panel = new QRCodeShowPanel(_context, _content)
        {
            Width = ww,
            Height = wh,
        };
        AddChild(panel);
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (_fixWindows)
        {
            Close();
        }
    }

    private void Esc_Exit_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true; // cancels the window close    
        Hide(); // Programmatically hides the window
    }

    private static void CopyFileToClipboard(string filePath)
    {
        var stC = new StringCollection { filePath };
        var t = new Thread(() => { Clipboard.SetFileDropList(stC); });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
    }
}