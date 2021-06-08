using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Language_Dictionary.Services;
using Forms = System.Windows.Forms;
using Window = System.Windows.Window;

namespace Language_Dictionary
{
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOW = 5;

        public static Window ActivedWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);

        public static Window FocusedWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsFocused);

        private readonly Forms.NotifyIcon _notifyIcon;

        public App()
        {
            _notifyIcon = new Forms.NotifyIcon();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (CheckProcess()) Current.Shutdown();

            SettingsHelper.GetSettings();

            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindowOnClosing;
            MainWindow.Closed += MainWindowOnClosed;

            MainWindow.Show();

            CreateNotIcon();

            base.OnStartup(e);
        }

        private void CreateNotIcon()
        {
            _notifyIcon.Icon = new Icon("Resources/Img/dictionary.ico");
            var contextMenuIcon = _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.Text = "Language Dictionary";

            contextMenuIcon.Items.Add("Open Language Dictionary", new Bitmap("Resources/Img/dictionary.ico"), (obj, ev) => MainWindow.Show());
            contextMenuIcon.Items.Add("Quit", null, (obj, ev) => App.Current.Shutdown());
            _notifyIcon.Visible = true;
            _notifyIcon.MouseDoubleClick += NotifyIconOnMouseDoubleClick;
        }

        private bool CheckProcess()
        {
            var currentProcess = Process.GetCurrentProcess();
            var runningProcess = Process.GetProcesses().FirstOrDefault(process =>
                process.Id != currentProcess.Id &&
                process.ProcessName.Equals(currentProcess.ProcessName, StringComparison.Ordinal));

            if (runningProcess is null) return false;

            ShowWindow(runningProcess.MainWindowHandle, SW_SHOW);
            return true;
        }

        private void MainWindowOnClosed(object? sender, EventArgs e)
        {
            _notifyIcon.Dispose();
        }

        private void NotifyIconOnMouseDoubleClick(object sender, Forms.MouseEventArgs e)
        {
            MainWindow.Show();
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs e)
        {
            MainWindow.Hide();
            e.Cancel = true;
        }
    }
}
