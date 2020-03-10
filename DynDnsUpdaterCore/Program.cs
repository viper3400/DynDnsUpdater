using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

namespace DynDnsUpdaterCore
{
    class Program
    {
        private const string CheckIpAddress = "http://checkip.dyndns.com/";
        private const string UpdateIpAddress = "{2}/nic/update?hostname={0}&myip={1}&wildcard=NOCHG";

        private static ILogger log;
        private static CommandLineOptions options;
        private static LocalIpStore ipStore;
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Trace));
            services.AddSingleton<LocalIpStore>();
        }
        static void Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(o => options = o);
            if (parseResult.Tag != ParserResultType.Parsed) return;

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            log = serviceProvider.GetService<ILogger<Program>>();
            ipStore = serviceProvider.GetService<LocalIpStore>();

            var ipText = PullIpAddress();
            if (string.IsNullOrEmpty(ipText)) { return; }
            var ip = ParseIpAddressText(ipText);
            var lastRecordedIp = ipStore.Load();
            log.LogInformation("Last Recorded IP Address: " + lastRecordedIp);
            log.LogInformation("Detected IP Address: " + ip);
            if (lastRecordedIp == ip && !options.Force)
            {
                log.LogInformation("Update not necessary");
                serviceProvider.Dispose();
                return;
            }
            ipStore.Save(ip);
            log.LogInformation("New IP Address will be propagated to: " + options.ProviderUrl);
            var finalUrl = string.Format(UpdateIpAddress, options.Hostname, ip, options.ProviderUrl);
            var resultText = SendUpdate(finalUrl);
            log.LogInformation(resultText);

            serviceProvider.Dispose();
        }

        public static string SendUpdate(string url)
        {
            var client = new WebClient();
            client.Credentials = new NetworkCredential(options.Username, options.Password);
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
                log.LogError("GetData", ex);
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
