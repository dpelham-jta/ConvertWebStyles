using MainStreetWrapper;
using MainStreetWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductImageExport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Querying product images...");
            var productImages = wsMainStreet.QueryProductImages();

            if (args.Length > 0 && args[0] == "-l")     // -l switch for exporting styles with letter suffixes
            {
                var alphaSuffixRegex = new System.Text.RegularExpressions.Regex("\\w+[a-zA-Z]");
                productImages = productImages.Where(pi => alphaSuffixRegex.IsMatch(pi.ItemID));
            }
            
            Console.WriteLine($"{productImages.Count()} products found, writing results to file...");
            WriteResultsToFile(productImages.OrderBy(p => p.ItemID));
        }

        private static void WriteResultsToFile(IEnumerable<ProductImageModel> productImages)
        {
            var resultBuilder = new StringBuilder();
            productImages.ToList().ForEach(cm => resultBuilder.Append(cm.ToString()));

            System.IO.File.WriteAllText("marmot_image_export.csv", resultBuilder.ToString());
        }
    }
}
