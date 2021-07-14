using CryptoFacile.Models;
using LiveCharts;
using System.Collections.Generic;

namespace CryptoFacile
{
    public class MainViewModel
    {
        public MainViewModel()
        {

            Pool = new List<PoolConfig>();
        }
        public ConsoleContent ConsoleOutput = new ConsoleContent();
        public SeriesCollection HashRates { get; set; }
        public SeriesCollection CPUHash { get; set; }
        public List<PoolConfig> Pool { get; set; }
        public PoolConfig SelectedPoolCfg { get; set; }
    }
}
