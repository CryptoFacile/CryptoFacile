using CryptoFacile.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace CryptoFacile
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public ApplicationConfig AppConf = new ApplicationConfig();
        public PoolConfig PoolConfig = new PoolConfig();
        public List<PoolConfig> PoolList = new List<PoolConfig>();
        public Process CPUProcess;
        public Process GPUProcess;
        public Process ServUO;
        public NotifyIcon nIcon = new NotifyIcon();
        public static event EventHandler<EventArgs> NIconDClick;
        public static event EventHandler<EventArgs> NIconClick;
        private static Mutex _mutex = null;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.ComponentModel.IContainer components;
        private static bool nIconClicked;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "CryptoFacile";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);
#if !DEBUG
            if (!createdNew)
            {
                const string message =
                "L'application semble déja en fonction veuillez regarder dans la barre systéme a côte de l'heure.";
                const string caption = "Application déja en fonction";
                var result = System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK);

                if (result == MessageBoxResult.OK)
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
#endif
                AppConf.Load();
                PoolList = PoolConfig.Load();
                base.OnStartup(e);
        }

        [STAThread]
        public static void Main()
        {
            App application = new App();
            application.components = new System.ComponentModel.Container();
            application.contextMenu1 = new System.Windows.Forms.ContextMenu();
            application.menuItem1 = new System.Windows.Forms.MenuItem();
            application.nIcon.Icon = new Icon(@"prog-ico.ico");
            application.nIcon.Visible = true;
            application.nIcon.Text = "Crypto Facile 2021";
            application.contextMenu1.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] { application.menuItem1 });
            application.menuItem1.Index = 0;
            application.menuItem1.Text = "Quiter Crypto Facile";
            application.menuItem1.Click += new System.EventHandler(application.quit_ContexMenu_Click);
            application.nIcon.ContextMenu = application.contextMenu1;

            application.nIcon.DoubleClick += NIconDoubleClick;
            application.nIcon.Click += notifyIcon_Click;
            application.InitializeComponent();
            application.Run();
        }

        private void quit_ContexMenu_Click(object Sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public async static void notifyIcon_Click(object sender, EventArgs e)
        {
            EventHandler<EventArgs> handler = NIconClick;
            if (nIconClicked) return;
            nIconClicked = true;
            await Task.Delay(SystemInformation.DoubleClickTime);
            if (!nIconClicked) return;
            nIconClicked = false;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private static void NIconDoubleClick(object sender, EventArgs e)
        {
            nIconClicked = false;
            EventHandler<EventArgs> handler = NIconDClick;
            if (handler != null)
            {
                handler(sender, e);
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppConf.Save();

            ((App)System.Windows.Application.Current).nIcon.Dispose();

            if (((App)System.Windows.Application.Current).CPUProcess != null)
                if (!((App)System.Windows.Application.Current).CPUProcess.HasExited)
                    ((App)System.Windows.Application.Current).CPUProcess.Kill();

            if (((App)System.Windows.Application.Current).GPUProcess != null)
                if (!((App)System.Windows.Application.Current).GPUProcess.HasExited)
                    ((App)System.Windows.Application.Current).GPUProcess.Kill();
        }
    }
}
