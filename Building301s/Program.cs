using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Building301s
{
    class Program
    {
        private static string URL_MASK = "https://exofficio.com";
        private static System.IO.StreamWriter _mappingStream;
        private static System.IO.StreamWriter _manualStream;
        private static System.Text.RegularExpressions.Regex _categoryRegex = new System.Text.RegularExpressions.Regex("\\/(men|women|kids|equipment|sale)(-.*)?\\/$");

        static void Main(string[] args)
        {
            OpenFileStreams();
            try
            {
                var fileName = Get301FileName(args);
                var data = ParseFile(fileName);
                CreateRegexRedirects(data);
                Console.WriteLine("All done, boss");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("GAME OVER MAN, GAME OVER!!");
            }
            finally
            {
                CloseFileStreams();
            }
            Console.ReadKey();
        }

        static void OpenFileStreams()
        {
            _manualStream = new System.IO.StreamWriter(System.IO.File.OpenWrite("diy_301s.csv"));
            _mappingStream = new System.IO.StreamWriter(System.IO.File.OpenWrite("heres_ur_301s.txt"));
        }

        static void CloseFileStreams()
        {
            _manualStream.Flush();
            _manualStream.Dispose();

            _mappingStream.Flush();
            _mappingStream.Dispose();
        }

        static void CreateRegexRedirects(Dictionary<string, string> data)
        {
            var productIdRegex = new System.Text.RegularExpressions.Regex("\\d{4}-\\d{4}");
            var htmlRegex = new System.Text.RegularExpressions.Regex("\\w+(?=\\.html)");
            foreach (var urlSet in data)
            {
                var matches = productIdRegex.Match(urlSet.Value);
                if(matches.Success)
                {
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Product-Show,,pid,{matches.Value}");
                    continue;
                }
                matches = htmlRegex.Match(urlSet.Value);
                if(matches.Success)
                {
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Page-Show,,cid,{matches.Value}");
                    continue;
                }
                AddToShitList(urlSet);
            }
        }

        static void CreateMarmot301s(Dictionary<string, string> data)
        {
            foreach (var urlSet in data)
            {
                if (urlSet.Value.Equals(URL_MASK) || urlSet.Value.Equals(URL_MASK + "/"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Home-Show");
                else if (urlSet.Value.Contains("/search?q="))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Search-Show,,q," + urlSet.Value.Substring(urlSet.Value.IndexOf('=') + 1));
                else if (urlSet.Value.Contains("/about-us/our-story.html"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Page-Show,,cid,our-story");
                else if (urlSet.Value.EndsWith("/articles"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,RootsRated-ShowLandingPage");
                else if (urlSet.Value.EndsWith("/stores"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Stores-Find");
                else if (_categoryRegex.IsMatch(urlSet.Value))
                {
                    var match = _categoryRegex.Match(urlSet.Value);
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Search-Show,,cgid,{match.Value.TrimStart('/').TrimEnd('/')}");
                }
                else
                    AddToShitList(urlSet);
            }
        }

        static void CreateExo301s(Dictionary<string, string> data)
        {
            foreach(var urlSet in data)
            {
                if (urlSet.Value.Equals(URL_MASK))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Home-Show");
                else if (urlSet.Value.Contains("/search?q="))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Search-Show,,q," + urlSet.Value.Substring(urlSet.Value.IndexOf('=') + 1));
                else if (urlSet.Value.Contains("/about/about.html"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Search-ShowContent,,fdid,about");
                else if (urlSet.Value.EndsWith("/underwear/"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Search-Show,,cgid,underwear");
                else if (urlSet.Value.EndsWith("/catalog.html"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Page-Show,,cid,catalog");
                else if (urlSet.Value.EndsWith("/articles"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,RootsRated-ShowLandingPage");
                else if (urlSet.Value.EndsWith("/stores"))
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Stores-Find");
                else if(_categoryRegex.IsMatch(urlSet.Value))
                {
                    var match = _categoryRegex.Match(urlSet.Value);
                    WriteToMappingStream($"**{urlSet.Key}** p,,,Search-Show,,cgid,{match.Value.TrimStart('/').TrimEnd('/')}");
                }
                else
                    AddToShitList(urlSet);
            }
        }

        static void WriteToMappingStream(string mapping)
        {
            _mappingStream.WriteLine(mapping);
        }

        static void AddToShitList(KeyValuePair<string, string> urls)
        {
            _manualStream.WriteLine($"{urls.Key},{urls.Value}");
        }

        // parse file
        // strip origin url and format
        // parse destination url for known pipes
        // write known pipes to text file
        // write unknown urls to csv
        static Dictionary<string, string> ParseFile(string fileName)
        {
            var rows = System.IO.File.ReadAllLines(fileName);
            Console.WriteLine($"File input received, parsing {rows.Count()} rows");
            var rv = new Dictionary<string, string>();

            foreach(var row in rows)
            {
                var cells = row.Split(',');
                rv[cells[0].Trim().Replace(URL_MASK, string.Empty)] = cells[1].Trim().Replace("http:", "https:");
            }

            return rv;
        }

        static string Get301FileName(string[] args)
        {
            if (args.Length > 0 && args[0].EndsWith("csv", StringComparison.InvariantCultureIgnoreCase))
                return args[0];

            Console.WriteLine("Please enter the data file name:");
            var filePath = Console.ReadLine();
            return filePath;
        }
    }
}
