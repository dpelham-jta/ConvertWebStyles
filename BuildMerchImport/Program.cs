using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BuildMerchImport
{
    class Program
    {
        private static string CATALOG_NAME = "masterCatalog_Marmot_full.xml";
        static void Main(string[] args)
        {
            var fileName = args[0];
            var merchList = File.ReadAllLines(fileName);
            var catalog = new XmlDocument();
            using (var catFile = new StreamReader(File.Open(CATALOG_NAME, FileMode.Open)))
            {
                catalog.Load(catFile);
            }

            MakeSomeXML(merchList, catalog);
        }

        static void MakeSomeXML(string[] merchList, XmlDocument catalog)
        {
            
            var products = catalog.GetElementsByTagName("product");
            var productDictionary = new Dictionary<string, XmlNode>();
            foreach (XmlNode product in products)
                productDictionary.Add(product.Attributes["product-id"].Value, product);

            using (var output = new StreamWriter(File.Open("output.xml", FileMode.Create)))
            {
                output.WriteLine("<?xml version=\"1.0\" encoding=\"UTF - 8\"?>");
                output.WriteLine("<catalog xmlns=\"http://www.demandware.com/xml/impex/catalog/2006-10-31\" catalog-id=\"masterCatalog_Marmot\">");

                foreach (var merchItem in merchList)
                {
                    var merchComponents = merchItem.Split(',');
                    if (!productDictionary.ContainsKey(merchComponents[0]))
                        continue;
                    var product = productDictionary[merchComponents[0]];
                    WriteProductNode(output, merchComponents[0], merchComponents[1], product["variations"]["variants"].ChildNodes);
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
