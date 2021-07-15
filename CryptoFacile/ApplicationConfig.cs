using System.IO;


namespace CryptoFacile
{
    public class ApplicationConfig
    {
        public int HashGive = 10;
        public int CpuSpeed = 75;
        public bool CpuActive = true;
        public bool GpuActive = true;
        public bool IsLess4GB = false;
        private IniFile ConfFile = new IniFile("Reglages.ini");

        public ApplicationConfig()
        {
            if (!File.Exists("Reglages.ini"))
                Save();
        }

        public void Load()
        {
            HashGive = int.TryParse(ConfFile.Read("HashGive"), out HashGive) ? HashGive : 10;
            CpuSpeed = int.TryParse(ConfFile.Read("CpuSpeed"), out CpuSpeed) ? CpuSpeed : 70;
            CpuActive = bool.TryParse(ConfFile.Read("CpuActive"), out CpuActive) ? CpuActive : true;
            GpuActive = bool.TryParse(ConfFile.Read("GpuActive"), out GpuActive) ? GpuActive : true;
            IsLess4GB = bool.TryParse(ConfFile.Read("IsLess4GB"), out IsLess4GB) ? IsLess4GB : false;
        }

        public void Save()
        {
            ConfFile.Write("HashGive", HashGive.ToString());
            ConfFile.Write("CpuSpeed", CpuSpeed.ToString());
            ConfFile.Write("CpuActive", CpuActive.ToString());
            ConfFile.Write("GpuActive", GpuActive.ToString());
            ConfFile.Write("IsLess4GB", IsLess4GB.ToString());
        }
    }
}
