using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace RenobeRFparser
{
    class Program
    {
        static int i = 0;
        static void Main(string[] args)
        {
            string nextpart = "glava-1-nachinaya-s-segodnyashnego-dnya-ya-nasledniy-prints";
            preloaded_head = File.ReadAllText("preloaded.txt");
            while (nextpart != null)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://xn--80ac9aeh6f.xn--p1ai/v1/part/load/?bookAlias=releas-that-witch&partAlias={nextpart}");
                JArray array = new JArray();
                using (var twitpicResponse = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(twitpicResponse.GetResponseStream()))
                {
                    var objText = reader.ReadToEnd();

                    JObject joResponse = JObject.Parse(objText);
                    var name = joResponse["result"]["book"]["title"];
                    if (!Directory.Exists(name.ToString()))
                        Directory.CreateDirectory(name.ToString());
                    var title = joResponse["result"]["part"]["title"];
                    var content = joResponse["result"]["part"]["content"];
                    try
                    {
                        nextpart = joResponse["result"]["nextPart"]["url"].ToString();
                        var s = nextpart.Split('/');
                        nextpart = s.ElementAt(s.Length - 2);
                    }
                    catch { return; }
                    HtmlDocument d = new HtmlDocument();
                    d.LoadHtml($"<head>{preloaded_head}<title>{title.ToString()}</title></head><body><h2>{title.ToString()}</h2><br> {content.ToString()}");
                    try
                    {
                        string path;
                        if (title.ToString().Split('.').Length>1)
                            path = $"{ name.ToString() }/{CleanFileName(title.ToString().Split('.').First())} { CleanFileName(title.ToString().Split('.').ElementAt(1))}.html";
                        else
                            path = $"{ name.ToString() }/{CleanFileName(title.ToString().Split('.').First())}.html";
                        if (File.Exists(path))
                        {
                            try
                            {
                                path = $"{name.ToString()}/{CleanFileName(title.ToString().Split('.').First())} {CleanFileName(title.ToString().Split('.').ElementAt(1))} {CleanFileName(title.ToString().Split('.').ElementAt(2))}.html";
                            }
                            catch { }
                        }
                        file = File.Open(path, FileMode.Create);
                        d.Save(file);
                        file.Close();
                        Console.WriteLine($"{path} --- Loaded!");
                        char[] delimiters = new char[] { ' ', '\r', '\n' };
                        i += StripHTML(content.ToString()).Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
                    }
                    catch { Console.WriteLine($"------------------------------------------ Error!"); }
                }
            }
        }
        static FileStream file;

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        static string preloaded_head;
    }
}
