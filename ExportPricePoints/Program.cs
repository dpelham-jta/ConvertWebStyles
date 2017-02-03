using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExportPricePoints
{
    class Program
    {
        static Dictionary<string, int> ProDealTiers = new Dictionary<string, int> {
            { "Original Price", 0 },
            { "Pro", 40 },
            { "Super Pro", 50 },
            { "VIP", 60 },
            { "Employee", 70 }
        };

        static string CurrentStyle;

        static void Main(string[] args)
        {
            try
            {
                var styles = ParseFile();
                var delimitedList = ProcessAllStyles(styles);
                WriteResultsToFile(delimitedList);
                Console.WriteLine("All done");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
                Console.ReadKey();
            }
        }

        private static Dictionary<string, List<string>> ParseFile()
        {
            var styles = new Dictionary<string, List<string>>();
            using (var fileStream = new StreamReader(File.Open("prodealstyles.txt", FileMode.Open)))
            {
                while (!fileStream.EndOfStream)
                {
                    var styleStr = fileStream.ReadLine().Split(',');
                    if (!styles.ContainsKey(styleStr[1]))
                        styles[styleStr[1]] = new List<string>();

                    styles[styleStr[1]].Add(styleStr[0]);
                }
            }

            return styles;
        }

        private static List<PricePoint> ProcessAllStyles(Dictionary<string, List<string>> styles)
        {
            var formattedList = new List<PricePoint>();

            foreach (var style in styles.Keys)
            {
                AddPricePointsPerStyle(formattedList, style);

                foreach(var upc in styles[style])
                {
                    AddPricePointsPerStyle(formattedList, upc);
                }
            }

            return formattedList;
        }

        private static void AddPricePointsPerStyle(List<PricePoint> formattedList, string style)
        {
            foreach (var pricePoint in ProDealTiers)
            {
                formattedList.Add(new PricePoint(style, pricePoint.Key, pricePoint.Value));
            }
        }

        private static void WriteResultsToFile(List<PricePoint> formattedList)
        {
            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine("ItemCd,Item Price Level Code,Item Price Calc Method Id,Cost Basis Id,Item Price Point Quantity,Item Price Point Value");
            foreach (var resultLine in formattedList)
                resultBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", resultLine.Style, resultLine.PricePointName, resultLine.PriceCalcMethod, resultLine.CostBasisId, resultLine.PricePointQuantity, resultLine.PricePointValue));

            File.WriteAllText("pricepoints.csv", resultBuilder.ToString());
        }
    }

    struct PricePoint
    {
        public PricePoint(string style, string name, int value)
        {
            Style = style;
            PricePointName = name;
            PricePointValue = value;
        }

        public string Style { get; set; }
        public string PricePointName { get; set; }
        public int PriceCalcMethod { get { return 2; } }
        public int CostBasisId { get { return PricePointName.Contains("Original") ? 1 : 3; } }
        public int PricePointQuantity { get { return 1; } }
        public int PricePointValue { get; set; }
    }
}
