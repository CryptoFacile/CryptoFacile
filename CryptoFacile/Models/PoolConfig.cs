using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoFacile.Models
{
    public class PoolConfig
    {
        private IniFile PoolConf;
        private const string poolConfigPath = "poolconfig";
        public string Name { get; set; }
        public string Type { get; set; }
        public string Adress { get; set; }
        public string Worker { get; set; }
        public string Port { get; set; }
        public string Custom { get; set; }

        public PoolConfig()
        {

        }
        public List<PoolConfig> Load()
        {
            List<PoolConfig> poolList = new List<PoolConfig>();

            if (Directory.Exists("poolconfig"))
            {
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(poolConfigPath);
                foreach (string fileName in fileEntries)
                {
                    poolList.Add(ProcessFile(fileName));
                }
                return poolList;
            }
            else
            {
                Directory.CreateDirectory("poolconfig");
                var defautPool = new Models.PoolConfig
                {
                    Name = "Default",
                    Type = "GPU",
                    Custom = "-pool ethash.poolbinance.com:8888 -wal nirad -worker top_up -epsw x -asm 2 -dbg -1 -allpools 1 -mode 1 -log 0"
                };
                //set default gpu pool
                poolList.Add(defautPool);
                defautPool.Save();
            }
            return poolList;
        }
        public void Save()
        {
            PoolConf = new IniFile($".\\{poolConfigPath}\\{Name.Trim().ToLower()}-{Type}.conf");
            PoolConf.Write("Name", Name, "PoolConfig");
            PoolConf.Write("Adress", Adress, "PoolConfig");
            PoolConf.Write("Worker", Worker, "PoolConfig");
            PoolConf.Write("Port", Port, "PoolConfig");
            PoolConf.Write("Custom", Custom, "PoolConfig");
        }

        // Insert logic for processing found files here.
        public PoolConfig ProcessFile(string path)
        {
            //Console.WriteLine("Processed file '{0}'.", path);
            PoolConf = new IniFile(path);
            PoolConfig poolConfig = new PoolConfig();
            poolConfig.Name = PoolConf.Read("Name","PoolConfig");
            poolConfig.Adress = PoolConf.Read("Adress", "PoolConfig");
            poolConfig.Worker = PoolConf.Read("Worker", "PoolConfig");
            poolConfig.Port = PoolConf.Read("Port", "PoolConfig");
            poolConfig.Custom = PoolConf.Read("Custom", "PoolConfig");
            return poolConfig;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
