using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace URLTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = args[0];
            var testUrls = System.IO.File.ReadAllLines(fileName);
            var webClient = new WebClient();
            var bunkUrls = new List<string>();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            foreach(var url in testUrls)
            {
                try
                {
                    var response = webClient.DownloadString(url);
                    if (response.Contains("<h1>An Error Occurred!</h1>"))
                        bunkUrls.Add(url);
                }
                catch(Exception ex)
                {
                    bunkUrls.Add(url);
                }
            }

            using (var writer = new System.IO.StreamWriter(System.IO.File.Open("results.csv", System.IO.FileMode.CreateNew)))
            {
                foreach (var url in bunkUrls)
                    writer.WriteLine(url);
            }
        }
    }
}
