using System;

namespace server.Services
{
    public class DndDatabase
    {
        public string Data { get; }


        public DndDatabase()
        {
            string appDir = System.IO.Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
            string dataDir = appDir + "/Data";

            Data = System.IO.File.ReadAllText(dataDir + "/database.json");
        }
    }
}
