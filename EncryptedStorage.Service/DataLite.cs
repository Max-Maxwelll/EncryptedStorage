using System;
using System.IO;
using System.Linq;
using System.Reflection;
using EncryptedStorage.Data;
using EncryptedStorage.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SQLite;

namespace EncryptedStorage.Service
{
    public class DataLite : IDataBase
    {
        private readonly HttpContext context;
        private readonly ILogger logger;
        private SQLiteConnection Connection;
        public EntityAccount Accounts { get; private set; }
        public EntityFile Files { get; private set; }

        public DataLite(IHttpContextAccessor context,
                        ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger("RequestInfoLogger");
            this.context = context.HttpContext;
            Initializing(this.context.Session.GetString("StorageName"));
            Accounts = new EntityAccount(Connection);
            Files = new EntityFile(Connection);
        }

        public void Initializing(string storage)
        {
            if (storage == null || storage.Length < 1)
                return;
            try
            {
                string nameSpace = "EncryptedStorage.Data.Models";
                string user = context.User?.Identity.Name;
                var dir = "wwwroot/Users/" + user + "/Storages/";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                dir += storage + ".db3";

                Connection = new SQLiteConnection(dir);
                Assembly assembly = typeof(AccountModel).Assembly;
                Type[] tablesList = assembly.GetExportedTypes().Where(t => t.Namespace == nameSpace).ToArray();
                foreach (Type type in tablesList)
                {
                    Connection.CreateTable(type);
                }
            }
            catch(Exception ex)
            {
                logger.LogInformation(ex.Message + " # " + ex.StackTrace);
            }
        }

        public void Delete(string name)
        {
            string user = context.User.Identity?.Name;
            if (user == null)
                return;
            string storage = "wwwroot/Users/" + user + "/Storages/" + name + ".db3";
            string files = "wwwroot/Users/" + user + "/Files/" + name;

            Close();
            if (File.Exists(storage))
                File.Delete(storage);

            if (Directory.Exists(files))
                Directory.Delete(files, true);
        }

        public void DeleteAll()
        {
            string user = context.User.Identity?.Name;
            if (user == null)
                return;
            var storages = "wwwroot/Users/" + user + "/Storages";
            var files = "wwwroot/Users/" + user + "/Files/";

            Close();
            if (Directory.Exists(storages))
                Directory.Delete(storages, true);
            if (Directory.Exists(files))
                Directory.Delete(files, true);
        }

        public FileResult Download(string name)
        {
            string user = context.User.Identity?.Name;
            if (user == null)
                return null;
            var path = "wwwroot/Users/" + user + "/Storages/" + name + ".db3";

            Close();
            using (FileStream file = new FileStream(path, FileMode.Open))
            {
                return new FileContentResult(new byte[file.ReadByte()], "application/octet")
                {
                    FileDownloadName = "storage.db3"
                };
            }
        }

        public void Close()
        {
            if(Connection != null)
            { 
                Connection.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}