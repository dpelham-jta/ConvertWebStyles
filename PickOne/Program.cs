using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PickOne
{
    class Program
    {
        static readonly Regex _styleRegex = new Regex(@"\d{4}-\d{4}");

        static void Main(string[] args)
        {
            try
            {
                DoYerThing();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }
            Console.ReadKey();
        }

        static void DoYerThing()
        {
            var currentPics = Directory.EnumerateFiles(".\\", "*.jpg");
            var processedStyles = new Dictionary<string, string>();
            Console.Out.Write("A");

            foreach (var pic in currentPics)
            {
                if (pic.Contains("ALT"))
                    continue;

                var style = PicStyle(pic);
                if (string.IsNullOrEmpty(style))
                    continue;

                style = ".\\" + style + ".jpg";

                if (!(processedStyles.ContainsKey(style) || currentPics.Contains(style)))
                {
                    File.Copy(pic, style);
                    processedStyles[style] = style;
                    Console.Out.Write("a");
                }
            }

            Console.Write("nd done.");
        }

        static string PicStyle(string picName)
        {
            var match = _styleRegex.Match(picName);
            return match.Success ? match.Value : string.Empty;
        }
    }
}
