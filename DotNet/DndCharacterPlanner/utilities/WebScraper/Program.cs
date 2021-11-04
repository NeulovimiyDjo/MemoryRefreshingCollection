using Newtonsoft.Json;
using System;
using System.IO;
using WebScraper.Downloaders.Dnd5eWikidot;
using WebScraper.Models;
using WebScraper.Parsers.Dnd5eWikidot;
using WebScraper.Printers;

namespace WebScraper
{
    public static class Program
    {
        private static readonly string _downloadedPagesDir;
        private static readonly string _serverDataDir;

        static Program()
        {
            string webScraperProjectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            
            _downloadedPagesDir = webScraperProjectDir + "/../dnd5e_downloaded_pages";
            _serverDataDir = webScraperProjectDir + "/../../server/Data";

            Parser._downloadedPagesDir = _downloadedPagesDir;
        }


        static void Main(string[] args)
        {
            if (args.Length < 1) return;


            if (args[0] == "--download")
            {
                Downloader.DownloadAllWikidotPages();
            }
            else if(args[0] == "--save")
            {
                Database db = Parser.CreateDbFromDownloadedPages();
                SaveObject(db, "database.json");
            }
            else if (args[0] == "--print")
            {
                Database db = Parser.CreateDbFromDownloadedPages();
                Printer.Print(db);
            }
        }


        private static void SaveObject(dynamic obj, string fileName)
        {
            Directory.CreateDirectory(_serverDataDir);
            using var fs = File.CreateText(_serverDataDir + "/" + fileName);

            var serializer = new JsonSerializer();
            serializer.Serialize(fs, obj);
        }

    }
}
