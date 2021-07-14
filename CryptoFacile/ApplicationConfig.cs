using System.IO;


namespace CryptoFacile
{
    public class ApplicationConfig
    {
        public string XHVWalet = "";
        public int HashGive = 75;
        public int CpuSpeed = 70;
        public bool CpuActive = true;
        public bool GpuActive = false;
        public bool IsLess4GB = false;
        private IniFile ConfFile = new IniFile("Reglages.ini");

        public ApplicationConfig()
        {
            if (!File.Exists("Reglages.ini"))
                Save();
        }

        public void Load()
        {
            XHVWalet = ConfFile.Read("XHVWalet");
            HashGive = int.TryParse(ConfFile.Read("HashGive"), out HashGive) ? HashGive : 100;
            CpuSpeed = int.TryParse(ConfFile.Read("CpuSpeed"), out CpuSpeed) ? CpuSpeed : 70;
            CpuActive = bool.TryParse(ConfFile.Read("CpuActive"), out CpuActive) ? CpuActive : true;
            GpuActive = bool.TryParse(ConfFile.Read("GpuActive"), out GpuActive) ? GpuActive : true;
            IsLess4GB = bool.TryParse(ConfFile.Read("IsLess4GB"), out IsLess4GB) ? IsLess4GB : false;
            if (HashGive < 15)
                HashGive = 100;
        }

        public void Save()
        {
            ConfFile.Write("XHVWalet", XHVWalet);
            ConfFile.Write("HashGive", HashGive.ToString());
            ConfFile.Write("CpuSpeed", CpuSpeed.ToString());
            ConfFile.Write("CpuActive", CpuActive.ToString());
            ConfFile.Write("GpuActive", GpuActive.ToString());
            ConfFile.Write("IsLess4GB", IsLess4GB.ToString());
        }
    }
}
