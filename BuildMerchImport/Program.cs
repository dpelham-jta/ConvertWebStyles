using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace BuildMerchImport
{
    class Program
    {
        private static string CATALOG_NAME = "exo_catalog_full.xml";
        private static bool STRIP_COLOR = true;
        private static bool REHYPHENATE_PRODUCT = true;

        static void Main(string[] args)
        {
            var catalog = GetCatalog();
            var merchLookup = new Dictionary<string, string>();

            foreach (var fileName in args)
            {
                var merchList = File.ReadAllLines(fileName);
                foreach(var row in merchList)
                {
                    var cols = row.Split(',');
                    if (cols[0].Contains("-") && STRIP_COLOR)
                        cols[0] = cols[0].Substring(0, cols[0].IndexOf('-'));
                    if (REHYPHENATE_PRODUCT)
                        cols[0] = cols[0].Insert(4, "-");

                    merchLookup[cols[0]] = cols[1];
                }
            }

            MakeSomeXML(merchLookup, catalog);
        }

        static XmlDocument GetCatalog()
        {
            var catalog = new XmlDocument();
            using (var catFile = new StreamReader(File.Open(CATALOG_NAME, FileMode.Open)))
            {
                catalog.Load(catFile);
            }
            return catalog;
        }

        static void MakeSomeXML(Dictionary<string, string> merchLookup, XmlDocument catalog)
        {
            
            var products = catalog.GetElementsByTagName("product");
            var productDictionary = new Dictionary<string, XmlNode>();
            foreach (XmlNode product in products)
                productDictionary.Add(product.Attributes["product-id"].Value, product);

            using (var output = new StreamWriter(File.Open("output.xml", FileMode.Create)))
            {
                output.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                output.WriteLine("<catalog xmlns=\"http://www.demandware.com/xml/impex/catalog/2006-10-31\" catalog-id=\"masterCatalog_Marmot\">");

                foreach (var productId in merchLookup.Keys)
                {
                    if (!productDictionary.ContainsKey(productId))
                        continue;
                    var product = productDictionary[productId];
                    WriteProductNode(output, productId, merchLookup[productId], product["variations"]["variants"].ChildNodes);
                }

                output.WriteLine("</catalog>");
            }
        }

        private static void WriteProductNode(StreamWriter output, string productId, string defaultUpc, XmlNodeList variants)
        {
            output.WriteLine($"\t<product product-id=\"{productId}\">");
            output.WriteLine("\t\t<variations>");
            output.WriteLine("\t\t\t<variants>");

            foreach(XmlNode node in variants)
            {
                var upc = node.Attributes["product-id"].Value;
                if (upc == defaultUpc)
                    output.WriteLine($"\t\t\t\t<variant product-id=\"{upc}\" default=\"true\"/>");
                else
                    output.WriteLine($"\t\t\t\t<variant product-id=\"{upc}\"/>");
            }
            
            output.WriteLine("\t\t\t</variants>");
            output.WriteLine("\t\t</variations>");
            output.WriteLine("\t</product>");
        }
    }
}
