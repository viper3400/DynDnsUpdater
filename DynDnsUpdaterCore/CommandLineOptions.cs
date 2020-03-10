using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynDnsUpdaterCore
{
    public class CommandLineOptions
    {
        [Option('u', "username", Required = true, HelpText = "Dyn.com (or other provider) username")]
        public string Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "Dyn.com (or other provider) password")]
        public string Password { get; set; }

        [Option('h', "hostname", Required = true, HelpText = "Hostname to update.  Multiple hostnames can be included, separated by commas. When multiple hostnames are used, it may be necessary to use the /f switch to force an update. Once an update has been forced, the /f should not be necessary for subsequent updates.")]
        public string Hostname { get; set; }

        [Option('f', "force", Required = false, HelpText = "Force the hostname(s) recorded to be updated.  Over use of this feature may lead to being throttling or being completely blocked by dyn.com servers.  Only use when something is out of sync and does not become fixed by further updates.")]
        public bool Force { get; set; }

        [Option("provider", Required = false, HelpText = "If you like to use another ISP with support for the Remote Access Update API (i.e. https://dyndns.strato.com)")]
        public string ProviderUrl { get; set; }

        [Usage(ApplicationAlias = "DnyDnsUpdaterCore")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Updates Dyn.com (or other provider) hostname to current IP Address", new CommandLineOptions { Username = "username", Password = "sosecret", Hostname="www.mysite.com"})
                };
            }
        }
    }
}
