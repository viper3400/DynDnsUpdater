using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using DynDnsUpdater.Properties;

namespace DynDnsUpdater
{
    class Program
    {
        private const string CheckIpAddress = "http://checkip.dyndns.com/";
        private const string UpdateIpAddress = "{2}/nic/update?hostname={0}&myip={1}&wildcard=NOCHG";

        private static string _userName = "";
        private static string _password = "";
        private static string _hostName = "";
        private static bool _isForce = false;

        private static ILogger log;

        static void Main(string[] args)
        {
            // Init logger
            var assemblyPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            log = new Log4NetLogger(typeof(Program), Path.Combine(assemblyPath, "log4net.config"));

            if (!args.Any())
            {
                DisplayHelpText();
                return;
            }
            ParseArgs(args);

            if (string.IsNullOrEmpty(_userName) || string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_hostName))
            {
                DisplayHelpText();
                return;
            }

            var ipText = PullIpAddress();
            if (string.IsNullOrEmpty(ipText)) { return; }
            var ip = ParseIpAddressText(ipText);
            log.Info("Last Recorded IP Address: " + Settings.Default.IpAddress);
            log.Info("Detected IP Address: " + ip);
            if (Settings.Default.IpAddress == ip && !_isForce)
            {
                log.Info("Update not necessary");
                return;
            }
            Settings.Default.IpAddress = ip;
            Settings.Default.Save();
            log.Info("New IP Address will be propagated to: " + Settings.Default.UpdateServerName);
            var finalUrl = string.Format(UpdateIpAddress, _hostName, ip, Settings.Default.UpdateServerName);
            var resultText = SendUpdate(finalUrl);
            log.Info(resultText);
        }


        public static void DisplayHelpText()
        {
            log.Error("Updates Dyn.com hostname to current IP Address");
            log.Error("");
            log.Error("dyndnsupdater.exe /u:USERNAME /p:PASSWORD /h:HOSTNAME /f");
            log.Error("");
            log.Error("/u     Dyn.com username");
            log.Error("/p     Dyn.com password");
            log.Error("/h     Hostname to update.  Multiple hostnames can be included, separated by commas.");
            log.Error("       When multiple hostnames are used, it may be necessary to use the /f switch to force an update.");
            log.Error("       Once an update has been forced, the /f should not be necessary for subsequent updates.");
            log.Error("/f     Force the hostname(s) recorded to be updated.  Over use of this feature may lead to being throttling");
            log.Error("       or being completely blocked by dyn.com servers.  Only use when something is out of sync and does not");
            log.Error("       become fixed by further updates.");
        }

        public static void ParseArgs(string[] args)
        {
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg)) { continue;}
                if (arg.ToLower() == "/f")
                {
                    _isForce = true;
                }

                var splitText = arg.Split(':');
                if (splitText.Count() != 2)
                {
                    continue;
                }
                var option = splitText[0].Replace("/", "").ToLower();
                var text = splitText[1];
                switch (option)
                {
                    case "u":
                        _userName = text;
                        break;
                    case "p":
                        _password = text;
                        break;
                    case "h":
                        _hostName = text;
                        break;
                }
            }
        }

        public static string SendUpdate(string url)
        {
            var client = new WebClient();
            client.Credentials = new NetworkCredential(_userName, _password);
            client.Headers.Add("User-Agent:DNS Update Client - 1.0" + GetFileVersion());
            return client.DownloadString(url);
        }

        public static string PullIpAddress()
        {
            string results = "";
            try
            {
                var client = new WebClient();
                results = client.DownloadString(CheckIpAddress);
            }
            catch (Exception ex)
            {
                log.Error("GetData", ex);
            }
            return results;
        }

        public static string ParseIpAddressText(string text)
        {
            text = text.Replace("</body></html>", "").Replace("\r\n", "");
            return text.Substring(text.LastIndexOf(" ")).Trim();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string GetFileVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}
