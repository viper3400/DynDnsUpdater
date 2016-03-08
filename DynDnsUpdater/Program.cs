using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynDnsUpdater.Properties;
using Microsoft.SqlServer.Server;

namespace DynDnsUpdater
{
    class Program
    {
        private const string CheckIpAddress = "http://checkip.dyndns.com/";
        private const string UpdateIpAddress = "https://dyndns.strato.com/nic/update?hostname={0}&myip={1}&wildcard=NOCHG";

        private static string _userName = "";
        private static string _password = "";
        private static string _hostName = "";
        private static bool _isForce = false;

        static void Main(string[] args)
        {
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
            Console.WriteLine("Last Recorded IP Address: " + Settings.Default.IpAddress);
            Console.WriteLine("Detected IP Address: " + ip);
            if (Settings.Default.IpAddress == ip && !_isForce)
            {
                Console.WriteLine("Update not necessary");
                return;
            }
            Settings.Default.IpAddress = ip;
            Settings.Default.Save();
            var finalUrl = string.Format(UpdateIpAddress, _hostName, ip);
            var resultText = SendUpdate(finalUrl);
            Console.WriteLine(resultText);
        }


        public static void DisplayHelpText()
        {
            Console.WriteLine("Updates Dyn.com hostname to current IP Address");
            Console.WriteLine("");
            Console.WriteLine("dyndnsupdater.exe /u:USERNAME /p:PASSWORD /h:HOSTNAME /f");
            Console.WriteLine("");
            Console.WriteLine("/u     Dyn.com username");
            Console.WriteLine("/p     Dyn.com password");
            Console.WriteLine("/h     Hostname to update.  Multiple hostnames can be included, separated by commas.");
            Console.WriteLine("       When multiple hostnames are used, it may be necessary to use the /f switch to force an update.");
            Console.WriteLine("       Once an update has been forced, the /f should not be necessary for subsequent updates.");
            Console.WriteLine("/f     Force the hostname(s) recorded to be updated.  Over use of this feature may lead to being throttling");
            Console.WriteLine("       or being completely blocked by dyn.com servers.  Only use when something is out of sync and does not");
            Console.WriteLine("       become fixed by further updates.");
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
                Console.WriteLine("GetData", ex);
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
