using System;
using System.Windows;


namespace CryptoFacile
{
    /// <summary>
    /// Logique d'interaction pour Wallet.xaml
    /// </summary>
    public partial class Wallet : Window
    {
        private int _Progress;
        private ApplicationConfig _AppConf = new ApplicationConfig();
        private bool IsRunning = false;
        string pwallet = "";

        public int Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
            }
        }
        public Wallet(ApplicationConfig appConfig,bool isRunning)
        {
            InitializeComponent();
            IsRunning = isRunning;
            _AppConf = appConfig;
            SL2.Value = _AppConf.HashGive;
            pwallet = _AppConf.XHVWalet;
            if (_AppConf.XHVWalet.Length > 24)
                txt_XMR.Text = _AppConf.XHVWalet.Substring(0, 24);
            //txtETH.Text = _AppConf.ETHWalet;
           //txtETC.Text = _AppConf.ETCWalet;

            Dev();

            if (IsRunning)
                DisableControl();
        }

        private void DisableControl()
        {
            SL2.IsEnabled = false;
            txt_XMR.IsEnabled = false;
        }

        private void SL2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue <= 3)
            {
                SL2.Value = 3;
                return;
            }
            int hashGive = (int)e.NewValue;
            txtslider2.Content = $"{hashGive} %";
            Dev();
        }

        private void Dev()
        {
            if (_AppConf.XHVWalet.ToLower() == "nirad")
            {
                SL2.Value = 0;
                SL2.Minimum = 0;
                _Progress = 0;
                SL2.IsEnabled = false;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if(!IsRunning && ((Window)sender).IsLoaded)
            {
                ((App)System.Windows.Application.Current).AppConf.HashGive = (int)SL2.Value;
                if(string.IsNullOrEmpty(_AppConf.XHVWalet))
                    if (txt_XMR.Text.Length > 24)
                        ((App)System.Windows.Application.Current).AppConf.XHVWalet = txt_XMR.Text;
                if (_AppConf.XHVWalet.Length > 24)
                    if (!_AppConf.XHVWalet.Substring(0, 24).Equals(txt_XMR.Text))
                        ((App)System.Windows.Application.Current).AppConf.XHVWalet = txt_XMR.Text;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new Uri("https://accounts.binance.com/fr/register?ref=75547280").AbsoluteUri);
        }
    }
}
