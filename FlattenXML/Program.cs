using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FlattenXML
{
    class Program
    {
        static void Main(string[] args)
        {
            var catList = new List<string>();
            var root = XElement.Load("merch-categories.xml");
            var topLevels = from el in root.Elements("item") select el;
            foreach (var topLevel in topLevels)
            {
                catList.Add($"\"{topLevel.Attribute("text").Value}\"");
                var midLevels = from el in topLevel.Element("items").Elements("item") select el;
                foreach (var midLevel in midLevels)
                {
                    catList.Add($"\"{topLevel.Attribute("text").Value}\" \"{midLevel.Attribute("text").Value}\"");
                    var lowLevels = from el in midLevel.Element("items").Elements("item") select el;
                    catList.AddRange(lowLevels.Select(lowLevel => $"\"{topLevel.Attribute("text").Value}\" \"{midLevel.Attribute("text").Value}\" \"{lowLevel.Attribute("text").Value}\""));
                }
            }

            if (catList.Count > 0)
            {
                WriteResultsToFile(catList);
            }
        }

        private static void WriteResultsToFile(List<string> catList)
        {
            var resultBuilder = new StringBuilder();
            foreach (var resultLine in catList)
                resultBuilder.AppendLine(resultLine);

            File.WriteAllText("flattened_marmot_categories.txt", resultBuilder.ToString());
        }
    }
}
