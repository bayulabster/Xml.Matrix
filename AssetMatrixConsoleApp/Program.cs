using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AssetMatrixConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Asset matrix console app v 0.1\n");
            
            string[] xmlPath = System.IO.File.ReadAllLines("XmlPath.txt");
            string path = xmlPath[0];
            System.Console.WriteLine("Contents are retrieve from " + path);

            string[] list = System.IO.File.ReadAllLines("EngineXmlList.txt");
            List<string> labs = new List<string>();
            foreach (string lab in list)
                labs.Add(path + lab);

            string[] XPathQuery = System.IO.File.ReadAllLines("Query.txt");
            string query = XPathQuery[0];            
            
            List<String> results = new List<string>();
            results.Add("\nQuery: " + query + "\n");

            foreach (String labUrl in labs)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(labUrl);
                XmlNode root = doc.DocumentElement;
                
                XmlNodeList itemNodes = doc.SelectNodes(query);

                if (itemNodes.Count > 0)
                {
                    results.Add("\n" + labUrl);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(labUrl);

                    foreach (XmlNode itemNode in itemNodes)
                        if (itemNode != null)
                        {
                            string result = "\t" + itemNode.Name.ToString() + " : " + itemNode.Value.ToString();
                            Console.WriteLine(result);
                            results.Add("\n" + result);
                        }
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
            }

            string[] printresult = results.ToArray();
            System.IO.File.WriteAllLines("SearchResult.txt", printresult);

            Console.WriteLine("End of result.");
            Console.ReadKey();
        }
    }
}
