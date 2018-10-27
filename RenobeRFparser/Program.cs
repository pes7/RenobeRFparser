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
using IronPdf;
using System.Drawing;

namespace RenobeRFparser
{
    class Program
    {
        static int i = 0;
        static void Main(string[] args)
        {
            Thread th = new Thread(() =>
            {
                var _main = new Main();
                _main.ShowDialog();
            });
            th.Start();
        }
        static FileStream file;
        static List<string> glaves;

        public static void Parse()
        {
            glaves = new List<string>();
            string nextpart = "noindex-glava-1-katastrofa-iz-za-banki-koka-koly";
            preloaded_head = File.ReadAllText("preloaded.txt");

            var contenter = urlImage($"https://xn--80ac9aeh6f.xn--p1ai/v1/part/get/?bookAlias=u-menya-est-dom-v-mire-postapokalipsisa&partAlias={nextpart}");
            using (var client = new WebClient())
            {
                client.DownloadFile(contenter["result"]["book"]["image"]["mobile"]["image"].ToString(), "book.jpg");
            }

            //var f = File.Open("book.png", FileMode.Open);
            //var img = ReadFully(f);

            IronPdf.HtmlToPdf Renderer = new IronPdf.HtmlToPdf();

            //CREATE PDF
            //doc.AddAuthor("Nazar Ukolov conventor");
            //doc.AddImageFile("book.png","book.epub");
            //doc.AddTitle(contenter["result"]["book"]["title"].ToString());
            string _name = "";
            while (nextpart != null)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://xn--80ac9aeh6f.xn--p1ai/v1/part/get/?bookAlias=u-menya-est-dom-v-mire-postapokalipsisa&partAlias={nextpart}");
                JArray array = new JArray();
                using (var twitpicResponse = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(twitpicResponse.GetResponseStream()))
                {
                    var objText = reader.ReadToEnd();

                    JObject joResponse = JObject.Parse(objText);
                    var name = joResponse["result"]["book"]["title"];
                    if (_name.Length < 1)
                        _name = name.ToString();
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
                        if (title.ToString().Split('.').Length > 1)
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
                        //file = File.Open(path, FileMode.Create);
                        //d.Save(file);
                        //file.Close();

                        string _content = $"<head>{preloaded_head}<title>{title.ToString()}</title></head><body><h2>{title.ToString()}</h2><br> {content.ToString()}";
                        string dir = $"{name.ToString()}/{CleanFileName(title.ToString().Split('.').ElementAt(1))}.pdf";
                        Renderer.RenderHtmlAsPdf(_content).SaveAs(dir);
                        glaves.Add(dir);
                        Console.WriteLine($"{path} --- Loaded!");
                        Thread.Sleep(500);
                    }
                    catch { Console.WriteLine($"------------------------------------------ Error!"); }
                }
            }
            SavePDF(_name);
        }

        static List<string> glaves50;
        public static void SavePDF(string _name)
        {
            
            glaves50 = new List<string>();
            /*
            var files = Directory.GetFiles($"{_name}", "*.pdf").OrderBy(d => new FileInfo(d).CreationTime);
            foreach (var file in files)
            {
                PDFs.Add(PdfDocument.FromFile(file));
            }
            */
            int i = 0, k = 0;
            string path = $"Сводка/{_name}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            glaves50.AddRange(Directory.GetFiles($"{_name}", "*.pdf").OrderBy(d => new FileInfo(d).CreationTime));
            var PDFs = new List<PdfDocument>();
            foreach (var file in glaves50)
            {
                Console.WriteLine(file);
                PDFs.Add(PdfDocument.FromFile(file));
                if (i >= 50)
                {
                    PdfDocument PDFer = PdfDocument.Merge(PDFs);
                    PDFer.SaveAs($"{path}/{_name}_{k}.pdf");
                    k++;
                    i = 0;
                    PDFs.Clear();
                }
                i++;
            }
            if(PDFs.Count > 0)
            {
                PdfDocument PDFer = PdfDocument.Merge(PDFs);
                PDFer.SaveAs($"{path}/{_name}_{k}.pdf");
            }
            PDFs.Clear();
            var list = new List<string>();
            list.AddRange(Directory.GetFiles($"{Directory.GetCurrentDirectory()}/{path}","*.pdf").OrderBy(d => new FileInfo(d).CreationTime));
            foreach(var f in list)
            {
                PDFs.Add(PdfDocument.FromFile(f));
            }
            PdfDocument PDF = PdfDocument.Merge(PDFs);
            var image = Image.FromFile("book.jpg");
            var cover = ImageToPdfConvetrer.ImageToPdf(image);
            PDF.PrependPdf(cover);
            PDF.SaveAs($"{_name}.pdf");
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static JObject urlImage(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{url}");
            JArray array = new JArray();
            using (var twitpicResponse = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(twitpicResponse.GetResponseStream()))
            {
                var objText = reader.ReadToEnd();

                JObject joResponse = JObject.Parse(objText);
                return joResponse;
            }
        }

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
