using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace EO_Mod_Manager
{
    public class Utils
    {
        public const string MM_PROTOCOL = "eohdmm";
        public static void RegisterProtocol(string protocol, string title) {
            if(protocol == String.Empty)
                return;
            string exe_path = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
            RegistryKey key = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{protocol}");

            key.SetValue(String.Empty, $"URL: {title}");
            key.SetValue("URL Protocol", String.Empty);

            RegistryKey appTitle = key.CreateSubKey("Application");
            appTitle.SetValue("ApplicationName", title);

            key = key.CreateSubKey(@"shell\open\command");
            key.SetValue(String.Empty, $"\"{exe_path}\" %1");

            key.Close();
        }

        public static string GetTextFromURL(string url)
        {
            var options = new RestClientOptions(url)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Get);
            RestResponse response = client.Execute(request);
            return response.Content;
        }

        public static byte[] Download(string link, IProgress<string> progress = null)
        {
            if (progress == null)
                progress = new Progress<string>();
            var options = new RestClientOptions(link)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Get);
            progress.Report($"Downloading {link}");
            byte[] response = client.DownloadData(request);
            return response == null ? null : response;
        }
    }
}
