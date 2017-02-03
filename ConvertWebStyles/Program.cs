using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConvertWebStyles
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var images = ParseFile();
                var delimitedList = ProcessAltImages(images);
                WriteResultsToFile(delimitedList);
                Console.WriteLine("All done");
                Console.ReadKey();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
                Console.ReadKey();
            }
        }

        private static List<string> ParseFile()
        {
            var altImages = new List<string>();
            using (var fileStream = new StreamReader(File.Open("webspecials.txt", FileMode.Open)))
            {
                while (!fileStream.EndOfStream)
                    altImages.Add(fileStream.ReadLine());
            }

            return altImages;
        }

        private static Dictionary<string, string> ProcessAltImages(List<string> altImages)
        {
            var delimitedList = new Dictionary<string, string>();

            foreach(var img in altImages)
            {
                var imgName = Path.GetFileNameWithoutExtension(img);
                var styleName = imgName.Substring(0, imgName.IndexOf('_'));
                if (delimitedList.ContainsKey(styleName))
                    delimitedList[styleName] = string.Concat(delimitedList[styleName], "|", imgName);
                else
                    delimitedList.Add(styleName, imgName);
            }

            return delimitedList;
        }

        private static void WriteResultsToFile(Dictionary<string, string> formattedList)
        {
            var resultBuilder = new StringBuilder();
            foreach (var resultLine in formattedList.Keys)
                resultBuilder.AppendLine(string.Concat(resultLine, ",", formattedList[resultLine]));

            File.WriteAllText("formattedspecials.csv", resultBuilder.ToString());
        }
    }
}
