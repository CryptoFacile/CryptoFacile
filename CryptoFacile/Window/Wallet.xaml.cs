using CryptoFacile.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CryptoFacile
{
    /// <summary>
    /// Logique d'interaction pour Wallet.xaml
    /// </summary>
    public partial class Wallet : Window
    {
        private int _Progress;
        private bool IsRunning = false;
        App _app = ((App)System.Windows.Application.Current);

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
            SL2.Value = _app.AppConf.HashGive;
            listPools.ItemsSource = _app.PoolList;

            if (IsRunning)
                DisableControl();
        }
        private void DisableControl()
        {
            SL2.IsEnabled = false;
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
        }


        private void Window_Closed(object sender, System.EventArgs e)
        {
            if(!IsRunning && ((Window)sender).IsLoaded)
            {
                _app.AppConf.HashGive = (int)SL2.Value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new Uri("https://accounts.binance.com/fr/register?ref=75547280").AbsoluteUri);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void EditClick(object sender, RoutedEventArgs e)
        {
            var baseobj = sender as FrameworkElement;
            var myObject = baseobj.DataContext as PoolConfig;
            Console.WriteLine(myObject.Name);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var baseobj = sender as FrameworkElement;
            var myObject = baseobj.DataContext as PoolConfig;
            lblCustom.Text =
                $"Nom : {myObject.Name}\n" +
                $"Custom String :  {myObject.Custom}\n";
        }
    }
}
