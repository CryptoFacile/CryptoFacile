using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using CryptoFacile.Models;

namespace CryptoFacile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        App _app = ((App)System.Windows.Application.Current);
        private bool IsRunning = false;
        private WindowState m_storedWindowState;
        private Queue<string> ConsoleOutputQueue = new Queue<string>();
        private MainViewModel ViewDataContex = new MainViewModel();
        DispatcherTimer timer1;
        private ChartValues<double> GpuValues = new ChartValues<double>();
        private ChartValues<double> CpuValues = new ChartValues<double>();

        double fvitesse_CPU = 0; //kH/S
        Int32 SpeedWatchdogCpu = 120;
        double fvitesse_GPU = 0; //MH/s
        Int32 SpeedWatchdogGpu = 120;
        string Sdata_GPU = "-";
        string SsharesCPU = "0/0";
        string SsharesGPU = "0/0/0";
        int pendingtick = 60 * 2;//update pending each 2 min
        int clientUpdate = 60 * 10;
        private Wallet _Wallet;

        public MainWindow()
        {
            App.NIconDClick += NIconDClick;
            App.NIconClick += nIconClick;
            InitializeComponent();
            chkBox_cpu_active.IsChecked = _app.AppConf.CpuActive;
            chkBox_gpu_active.IsChecked = _app.AppConf.GpuActive;
            sld_cpu.Value = _app.AppConf.CpuSpeed;

            ViewDataContex.HashRates = new SeriesCollection
            {
                new StackedAreaSeries
                {
                    Values = GpuValues,
                    Name = "gpu"
                }
            };
            ViewDataContex.CPUHash = new SeriesCollection
            {
                new StackedAreaSeries
                {
                    Values = CpuValues,
                    Name = "cpu"
                }
            };
            con.ItemsSource = ViewDataContex.ConsoleOutput.ConsoleOutput;
            timer1 = new DispatcherTimer();
            DataContext = ViewDataContex;
            timer1.Tick += Update_Timer;
            timer1.Interval = TimeSpan.FromSeconds(1);
            timer1.Start();
            
            foreach (PoolConfig cfg in _app.PoolList)
            {
                ViewDataContex.Pool.Add(cfg);
            }
            ViewDataContex.SelectedPoolCfg = _app.PoolList[0];
        }

        private void NIconDClick(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Show();
                WindowState = m_storedWindowState;
            }
        }

        private void nIconClick(object sender, EventArgs e)
        {
            _app.nIcon.ShowBalloonTip(5000, "Vitesse GPU", $"{fvitesse_GPU.ToString("#0.00")}", ToolTipIcon.Info);
        }

        private void Update_Timer(object sender, EventArgs e)
        {
            pendingtick--;
            clientUpdate--;
            if (clientUpdate < 1)
            {
                clientUpdate = 60 * 10;
            }
            else if (clientUpdate > (60 * 10) - 5)
            {
                lbl_ClientStat.Foreground = System.Windows.Media.Brushes.Yellow;
                lbl_ClientStat.Content = "Vérification de mise à jour.";
            }
            else
            {
                lbl_ClientStat.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00B300"));
                lbl_ClientStat.Content = "Clients a jour avec le seveur.";
            }

            txt_speedcpu.Content = fvitesse_CPU.ToString("0.000 kH/s");
            txt_speedgpu.Content = fvitesse_GPU.ToString("0.000 MH/s");
            txt_infoGPU.Content = Sdata_GPU;
            txt_ShareGPU.Content = SsharesGPU;
            txt_ShareCPU.Content = SsharesCPU;
            while (ConsoleOutputQueue.Count > 0)
            {
                string queueValue = ConsoleOutputQueue.Dequeue();
                if (!string.IsNullOrEmpty(queueValue))
                    ViewDataContex.ConsoleOutput.ConsoleOutput.Add(queueValue);
                if (ViewDataContex.ConsoleOutput.ConsoleOutput.Count > 200)
                    ViewDataContex.ConsoleOutput.ConsoleOutput.RemoveAt(0);
            }

            if (Scroller.VerticalOffset == Scroller.ScrollableHeight)
            {
                Scroller.ScrollToEnd();
            }

            SpeedWatchdogCpu -= 1;
            if (SpeedWatchdogCpu == 0)
                fvitesse_CPU = 0;
            SpeedWatchdogGpu -= 1;
            if (SpeedWatchdogGpu == 0)
                fvitesse_GPU = 0;
        }
        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning == true)
            {
                if (_app.AppConf.CpuActive)
                    ExecXmRigStop();
                if (_app.AppConf.GpuActive)
                    ExecPhoenixMinerStop();
                bnt_connect.Content = "Start";
                ViewDataContex.ConsoleOutput.ConsoleOutput.Add("Aucune action en cours.");
                IsRunning = false;
                sld_cpu.IsEnabled = true;
                Cpu_Data.Visibility = Visibility.Hidden;
                Gpu_Data.Visibility = Visibility.Hidden;
                GridStart.Visibility = Visibility.Visible;
            }
            else
            {

                _app.AppConf.CpuActive = chkBox_cpu_active.IsChecked ?? false;
                _app.AppConf.GpuActive = chkBox_gpu_active.IsChecked ?? false;
                _app.AppConf.CpuSpeed = int.TryParse(sld_cpu.Value.ToString("00"), out _app.AppConf.CpuSpeed) ? _app.AppConf.CpuSpeed : 75;

                if (_app.AppConf.CpuActive || _app.AppConf.GpuActive)
                {
                    GridStart.Visibility = Visibility.Hidden;
                    Cpu_Data.Visibility = _app.AppConf.CpuActive ? Visibility.Visible : Visibility.Hidden;
                    Gpu_Data.Visibility = _app.AppConf.GpuActive ? Visibility.Visible : Visibility.Hidden;
                    IsRunning = true;
                    sld_cpu.IsEnabled = false;
                    ViewDataContex.ConsoleOutput.ConsoleOutput.Add("Le minage de cryptomonaie a commencé");
                    bnt_connect.Content = "Stop";

                    if (_app.AppConf.CpuActive)
                    {
                        StartCpuMining();
                        ViewDataContex.ConsoleOutput.ConsoleOutput.Add($"Info : puissance CPU réglé à {(int)sld_cpu.Value}%");
                    }

                    if (_app.AppConf.GpuActive)
                        StartGpuMining();
                }
                else
                    ViewDataContex.ConsoleOutput.ConsoleOutput.Add("Sélectionner une méthode d'extraction");
            }
        }
        private void ExecXmRigStop()
        {
            if (!((App)System.Windows.Application.Current).CPUProcess.HasExited)
                ((App)System.Windows.Application.Current).CPUProcess.Kill();
        }
        private void ExecPhoenixMinerStop()
        {
            if (!((App)System.Windows.Application.Current).GPUProcess.HasExited)
                ((App)System.Windows.Application.Current).GPUProcess.Kill();
        }
        private void StartCpuMining()  //MINING CPU
        {
            string S_pause_active = "";
            //string argumetns = $"-o gulf.moneroocean.stream:10128 -u 85wYEY7y6NJS2aAVWVC6g1c88jbbk7mJuEk2jeMpfxZWEB149Xe5A3ebpzen8Z5Nn76QxR4C6PrpRW1sqfYJns3yLfM4EvF -p Nirad~cn-heavy/xhv --cpu-priority 1 --cpu-max-threads-hint={_app._app.AppConf.CpuSpeed} {S_pause_active} --donate-level=0 --donate-over-proxy=0 ";
            string argumetns = " --donate-level 0 -o ca.haven.herominers.com:10452 -u hvxy8BKUGwgFQA4dW3KfGXVGLgKRBvnPv2QN7FQwjNDNiTX2W8mo6qN785BBC2AkrsK1hLFuam55iQ5a3HyME7MA2Zd7EUrAzY -p Nirad -a cn-heavy/xhv -k";
            int process_pourcent = (int)sld_cpu.Value;

            if ((bool)chkBox_actif.IsChecked)
            {
                S_pause_active = "--pause-on-active 10"; //temps en secondes
            }
            ((App)System.Windows.Application.Current).CPUProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @".\lib\cpu-m.dll",
                    Arguments = argumetns,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            ((App)System.Windows.Application.Current).CPUProcess.Start();
            ((App)System.Windows.Application.Current).CPUProcess.OutputDataReceived += Proc_OutputDataReceived;
            ((App)System.Windows.Application.Current).CPUProcess.BeginOutputReadLine();
        }
        private void StartGpuMining()  //MINING GPU
        {
            ((App)System.Windows.Application.Current).GPUProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @".\lib\gpu-m.dll",
                    Arguments = ViewDataContex.SelectedPoolCfg.Custom,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            ((App)System.Windows.Application.Current).GPUProcess.Start();
            ((App)System.Windows.Application.Current).GPUProcess.OutputDataReceived += GPU_OutputDataReceived;
            ((App)System.Windows.Application.Current).GPUProcess.BeginOutputReadLine();
        }

        private void GPU_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

            if (!string.IsNullOrEmpty(e.Data))
            {
                ConsoleOutputQueue.Enqueue(e.Data);
                if (e.Data.Contains("GPU0") || e.Data.Contains("GPU1")) //GPU1: 58C 38% 87W
                {
                    if (e.Data.Contains("%"))
                    {
                        Sdata_GPU = $"{ e.Data.Substring(5, e.Data.Length - 5)}";
                    }
                    else if (e.Data.Contains("found!") || e.Data.Contains("job"))
                        return;
                    //else
                    //output.Enqueue(e.Data);
                }
                else if (e.Data.Contains("Eth speed"))
                {
                    //ConsoleOutputQueue.Enqueue(e.Data);
                    SpeedWatchdogGpu = 120;
                    string[] Svitesse_GPU = $"{ e.Data.Substring(11, e.Data.Length - 11)}".Split(' ');
                    SsharesGPU = Svitesse_GPU[3].Substring(0, Svitesse_GPU[3].Length - 1);
                    double gpu_speed = 0;
                    if (double.TryParse(Svitesse_GPU[0], NumberStyles.Any, CultureInfo.InvariantCulture, out gpu_speed))
                        fvitesse_GPU = gpu_speed;
                    GpuValues.Add(fvitesse_GPU);
                    if (GpuValues.Count > 200)
                        GpuValues.RemoveAt(0);
                    //output.Enqueue($"Vitesse GPU: {fvitesse_GPU.ToString("#####.00")} MH/s");            
                }

                else if (e.Data.Contains("accepted"))
                {
                   /* if (fvitesse_GPU != 0)
                    {
                        ConsoleOutputQueue.Enqueue("[GPU] Fragment trouvé!");
                    }
                    else
                        ConsoleOutputQueue.Enqueue($"[GPU] Évaluation de la vitesse"); //Message non vrai. En fait un fragment a été trouvé mais la vitesse est encore à 0*/
                }
            }
        }
        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

            if (!string.IsNullOrEmpty(e.Data))
            {

                //ConsoleOutputQueue.Enqueue(e.Data.ToString());

                if (e.Data.Contains("accepted"))
                {
                    string[] a = e.Data.Split(' ');
                    SsharesCPU = a[10].Substring(1, a[10].Length - 2);
                   // ConsoleOutputQueue.Enqueue("[CPU] Fragment trouvé!");
                }
                else if (e.Data.Contains("cpu"))
                {
                    //ConsoleOutputQueue.Enqueue($"Info : {e.Data.Substring(42, e.Data.Length - 42)}");
                }
                else if (e.Data.Contains("miner"))
                {
                    if (e.Data.Contains("speed"))
                    {
                        SpeedWatchdogCpu = 120;
                        string[] Svitesse_CPU = e.Data.ToString().Split(' ');
                        double cpu_speed = 0;
                        if (double.TryParse(Svitesse_CPU[9],NumberStyles.Any, CultureInfo.InvariantCulture,out cpu_speed))
                            fvitesse_CPU = cpu_speed / 1000;
                        CpuValues.Add(fvitesse_CPU);
                        if (CpuValues.Count > 200)
                            CpuValues.RemoveAt(0);
                        //output.Enqueue($"Vitesse CPU: {fvitesse_CPU.ToString("#####.00")} kH/s");
                    }
                   /* else if (e.Data.Contains("paused"))
                        //ConsoleOutputQueue.Enqueue("Sur pause en raison que l'ordinateur est utilisé");

                    else if (e.Data.Contains("resumed"))
                        ConsoleOutputQueue.Enqueue("Calcul recommencé");*/
                }

            }
        }
        private void AboutClick(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Show();
        }
        private void bnt_ShowConsole_Click(object sender, RoutedEventArgs e)
        {
            Scroller.Visibility = Scroller.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            if (Scroller.Visibility == Visibility.Visible)
            {
                MainWindows_View.MinWidth = 875;
                MainWindows_View.Width = 875;
                bnt_ShowConsole.Content = "Masquer console";
            }
            else
            {
                bnt_ShowConsole.Content = "Afficher console";
                MainWindows_View.MinWidth = 380;
                MainWindows_View.Width = 380;
            }
        }
        private void Wallet_Click(object sender, RoutedEventArgs e)
        {
            _Wallet = new Wallet(_app.AppConf,IsRunning);
            _Wallet.Show();
            _Wallet.WindowState = WindowState.Normal;
            _Wallet.Focus();
        }
        private void sd_Cpu_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int set = (int)e.NewValue;
            sld_cpu_text.Content = $"{set} %";
        }
        private void MainWindows_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            else
                m_storedWindowState = WindowState;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
    public class ConsoleContent : INotifyPropertyChanged
    {
        string consoleInput = string.Empty;
        ObservableCollection<string> consoleOutput = new ObservableCollection<string>() { "Aucun minage en cours." };

        public string ConsoleInput
        {
            get
            {
                return consoleInput;
            }
            set
            {
                consoleInput = value;
                OnPropertyChanged("ConsoleInput");
            }
        }

        public ObservableCollection<string> ConsoleOutput
        {
            get
            {
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                OnPropertyChanged("ConsoleOutput");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
