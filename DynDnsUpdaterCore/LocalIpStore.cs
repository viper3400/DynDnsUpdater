using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DynDnsUpdaterCore
{
    public class LocalIpStore
    {
        private readonly ILogger logger;
        private readonly string ipStoreDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DynDnsUpdater");
        private readonly string ipStore;
        public LocalIpStore(ILogger<LocalIpStore> logger)
        {
            this.logger = logger;
            ipStore = Path.Combine(ipStoreDir, "DnyDnsUpdater.store");
            Directory.CreateDirectory(ipStoreDir);
        }
        public string Load()
        {
            logger.LogDebug("Try to load local ip store: {0}", ipStore);
            if (File.Exists(ipStore))
            {
                var storedIp = File.ReadAllText(ipStore);
                logger.LogInformation("Local ip is: {0}", storedIp);
                return storedIp;
            }
            else
            {
                logger.LogInformation("No local ip store found.");
                return "";
            }
        }

        public void Save(string ipAddress)
        {
            logger.LogInformation("Write ip {0} to local ip store: {1}", ipAddress, ipStore);
            File.WriteAllText(ipStore, ipAddress);
        }
    }
}
