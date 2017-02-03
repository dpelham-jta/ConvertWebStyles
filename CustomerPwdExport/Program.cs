using CustomerPwdExport.Models;
using MainStreetWrapper;
using MainStreetWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerPwdExport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Querying MainStreet for all existing Customer Records...");
            var site = args[0];
            var customerList = new List<CustomerModel>();
            var stateAbbreviations = Enum.GetNames(typeof(StateEnum));
            var pullRecent = (args.Length > 2 && args[2] == "recent");
            var writePasswordFile = (args.Length > 1 && !stateAbbreviations.Contains(args[1]));

            if (args.Length > 1 && args[1] != "TX" && stateAbbreviations.Contains(args[1]))
                stateAbbreviations = stateAbbreviations.Skip(Array.IndexOf(stateAbbreviations, args[1])).ToArray();

            drawTextProgressBar(0, stateAbbreviations.Length);

            if(pullRecent)
            {
                customerList = wsMainStreet.CustomerPointInTimeExport(DateTime.Today.AddDays(-11), site, true);
                //ProcessChunks(customerList, site, "Recent");
            }
            else if (site == "exofficio" && args.Length > 1 && args[1] == "TX")
            {
                customerList = wsMainStreet.CustomerListExport(TexasZips.ZipCodes, site, true);
                ProcessChunks(customerList, site, "TX");
            }
            else
            {
                for (var i = 0; i < stateAbbreviations.Length; i++)
                {
                    if (site == "exofficio" && stateAbbreviations[i] == "TX")
                        continue;

                    if (!writePasswordFile)
                    {
                        customerList = wsMainStreet.CustomerListExport(stateAbbreviations[i], site, true);
                        ProcessChunks(customerList, site, stateAbbreviations[i]);
                    }
                    else
                        customerList.AddRange(wsMainStreet.CustomerListExport(stateAbbreviations[i], site, true));
                    drawTextProgressBar(i + 1, stateAbbreviations.Length);
                    System.Threading.Thread.Sleep(500);
                }
            }

            if (writePasswordFile)
            {
                ReadInPasswordsFromFile(args[1], site, customerList);
            }

            Console.WriteLine();
            Console.WriteLine("Aaaaaand done.");
            Console.ReadKey();
        }

        private void ValidateArgs(string[] args)
        {
            if (args.Length == 0)
                throw new InvalidOperationException("Provide site name!");
            else if (args[0] != "marmot" && args[0] != "exofficio")
                throw new InvalidOperationException("first arg must either be 'marmot' or 'exofficio'");
        }

        private static void ProcessChunks(List<CustomerModel> customerList, string site, string state)
        {
            Console.WriteLine($"Querying individual Customer passwords ({state})");
            using (var fileWriter = new System.IO.StreamWriter(System.IO.File.Open($"customerExport_{site}.csv", System.IO.FileMode.Append)))
            {
                try
                {
                    for (var i = 0; i < customerList.Count; i += 100)
                    {
                        drawTextProgressBar(i, customerList.Count);
                        var customerListChunk = customerList.Skip(i).Take(100).ToList();
                        QueryCustomerPasswords(customerListChunk);
                        WriteResultsToCSV(customerListChunk, state, fileWriter);
                    }
                    drawTextProgressBar(customerList.Count, customerList.Count);
                }
                catch (Exception ex)
                {
                    if(ex != null)
                    { }
                }
                finally
                {
                    if (fileWriter != null)
                        fileWriter.Flush();
                }
            }
        }
        /*case "Pro":
                case "Super Pro":
                case "Pro Legacy":
                    return "ProDeal";
                case "VIP Executive Team":
                case "VIP Industry Pro":
                case "VIP Legacy":
                    return "VIPCustomer";*/
        private static List<CustomerModel> GetPricePointCustomers(string site)
        {
            var customers = new List<CustomerModel>();
            if (site == "marmot")
            {
                customers.AddRange(wsMainStreet.CustomerPricePointExport("Pro", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("Pro Legacy", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("Super Pro", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("VIP Executive Team", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("VIP Industry Pro", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("VIP Legacy", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("Employee", site, true));
            }
            else
            {
                customers.AddRange(wsMainStreet.CustomerPricePointExport("Pro Deal", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("Pro Deal Plus", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("20% Clearance", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("30% Clearance", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("50% Clearance", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("70% Clearance", site, true));
                customers.AddRange(wsMainStreet.CustomerPricePointExport("Employee", site, true));
            }
            return customers;
        }

        private static void ReadInPasswordsFromFile(string filename, string site, List<CustomerModel> customerList)
        {
            var pricePointCustomers = GetPricePointCustomers(site);
            Console.WriteLine("Mapping passwords to customers...");
            var passwordModels = (from line in System.IO.File.ReadAllLines(filename)
                                 let columns = line.Split(',')
                                 select BuildCustomerPassword(columns)).ToList();

            
            foreach (var customer in customerList)
            {
                if (pricePointCustomers.Any(c => c.Email == customer.Email))
                    customer.Price_Point = pricePointCustomers.First(c => c.Email == customer.Email).Price_Point;

                var match = passwordModels.FirstOrDefault(pm => pm.Name == customer.Name && pm.Email == customer.Email && !string.IsNullOrEmpty(pm.Password));
                if (match == null)
                    continue;

                customer.Password = match.Password;
                passwordModels.Remove(match);
            }

            /*Console.WriteLine("Chunking up some output");
            for (var i = 0; i < customerList.Count; i += ChunkSize)
            {
                drawTextProgressBar(i, customerList.Count);
                var customerListChunk = customerList.Skip(i).Take(ChunkSize);
                WriteResultsToFile(customerListChunk, site, i / ChunkSize);
            }
            drawTextProgressBar(customerList.Count, customerList.Count);*/
            Console.WriteLine("Writing results to file");
            WriteResultsToFile(customerList, site);
        }

        private static PasswordModel BuildCustomerPassword(string[] columns)
        {
            try
            {
                var data = columns.Where(c => !string.IsNullOrEmpty(c)).Select(c => c.Replace("&", "&amp;").Trim()).ToArray();
                if (data.Length > 4 && data[2].Contains("@"))
                    return new PasswordModel()
                    {
                        Name = string.Concat(data[0], " ", data[1]),
                        Email = data[2],
                        Password = data[3]
                    };
                else if (data.Length > 4)
                {
                    var password = string.Empty;
                    for (var i = 2; i < data.Length - 1; i++)
                    {
                        if (string.IsNullOrEmpty(data[i]))
                            break;
                        password += data[i] + ",";
                    }
                    password = password.TrimEnd(',');

                    return new PasswordModel()
                    {
                        Name = data[0],
                        Email = data[1],
                        Password = password
                    };
                }

                return new PasswordModel()
                {
                    Name = data[0],
                    Email = data[1],
                    Password = data[2]
                };
            }
            catch
            {
                Console.WriteLine("Bogus data: " + string.Join(", ", columns));
                return new PasswordModel();
            }
        }

        private static void QueryCustomerPasswords(List<CustomerModel> customerList)
        {
            //var progressInc = Math.Ceiling((double)customerList.Count / 100.0);
            //drawTextProgressBar(0, customerList.Count);

            foreach(var customer in customerList)
            {
                wsMainStreet.FetchPasswordForCustomer(customer);
                //drawTextProgressBar(++progress, customerList.Count);
                System.Threading.Thread.Sleep(250);
            }
        }

        private static void WriteResultsToCSV(List<CustomerModel> customerModels, string state, System.IO.StreamWriter writer)
        {
            customerModels.ToList().ForEach(cm => writer.WriteLine($"{cm.Name},{cm.Email},{cm.Password}, {state}"));
        }

        //private static void WriteResultsToFile(IEnumerable<CustomerModel> customerModels, int chunkID)
        private static void WriteResultsToFile(IEnumerable<CustomerModel> customerModels, string site)
        {
            using (var writer = new System.IO.StreamWriter(System.IO.File.OpenWrite($"customerExport_{site}.xml")))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<customers xmlns=\"http://www.demandware.com/xml/impex/customer/2006-10-31\">");
                customerModels.ToList().ForEach(cm => writer.WriteLine(cm.ToString()));
                writer.WriteLine("</customers>");
            }
        }

        private static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
    }
}
