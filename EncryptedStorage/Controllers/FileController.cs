using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cors;
using EncryptedStorage.Data;
using System.IO;
using EncryptedStorage.Data.Entities.Storage;
using System.Security.Cryptography;
using EncryptedStorage.Service;
using EncryptedStorage.Data.Models;

namespace EncryptedStorage.Controllers
{
    [EnableCors("AllowMyOrigin")]
    [Authorize]
    [Route("[controller]/[action]")]
    public class FileController : Controller
    {
        private readonly ILogger logger;
        private readonly IEncryptor encryptor;
        private readonly DataLite dataLite;
        private readonly StorageDbContext storageContext;
        //private LiteBase storageDB;

        public FileController(
            ILogger<UserController> logger,
            IEncryptor encryptor,
            DataLite dataLite,
            StorageDbContext storageContext)
        {
            this.logger = logger;
            this.encryptor = encryptor;
            this.dataLite = dataLite;
            this.storageContext = storageContext;
            //storageDB = new LiteBase();
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var files = Request.Form.Files;
                    var storageName = HttpContext.Session?.GetString("StorageName");

                    if (storageName == null)
                        return new BadRequestObjectResult("Хранилище не выбрано");

                    if (files == null)
                        return new BadRequestObjectResult("Файл отсутствует");

                    //LoadStorages(ref storageDB, storageName);
                    
                    var storage = storageContext.Storages
                        .Where(s => s.Name == storageName && s.User == User.Identity.Name)
                        .FirstOrDefault();

                    if (storage == null)
                        new BadRequestObjectResult("Хранилище не найдено");

                    var fileName = (Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 10)).Replace(@"\", "0").Replace(@"/", "0");

                    foreach (var file in files)
                    {
                        var findFile = dataLite.Files.Get(f => f.Name == file.Name);

                        if (findFile != null)
                            return new BadRequestObjectResult("Файл с таким именем существует");

                        var type = file.FileName.Split('.').Last();
                        fileName += "." + type;
                        var group = file.ContentType.Split('/')[0];

                        //storageDB.Connection.Insert(
                        //    new CryptoFile()
                        //    {
                        //        Name = file.Name,
                        //        Size = Math.Round(file.Length / (double)1000000, 2),
                        //        Group = group,
                        //        Type = type,
                        //        Path = fileName
                        //    }
                        //);
                        //storageDB.Connection.Close();
                        dataLite.Files.Add(
                            new FileModel()
                            {
                                Name = file.Name,
                                Size = Math.Round(file.Length / (double)1000000, 2),
                                Group = group,
                                Type = type,
                                Path = fileName
                            }
                        );
                        dataLite.Close();

                        await encryptor.EncryptFile(file, GetDirFile() + fileName);
                    }

                    return new OkObjectResult(files[0].Name);
;               }
                catch (Exception ex)
                {
                    logger.LogInformation("#####" + ex.StackTrace);
                    return new BadRequestObjectResult(ex.Message);
                }
            }

            return new BadRequestObjectResult("Модель данных не корректна");
        }

        [HttpGet]
        public IActionResult GetFiles()
        {
            var storageName = HttpContext.Session?.GetString("StorageName");

            if (storageName == null)
                return new BadRequestObjectResult("Нужно зайти в хранилище");

            //LoadStorages(ref storageDB, storageName);

            var files = dataLite.Files.GetAll();
            dataLite.Close();
            if (files == null)
                return new BadRequestObjectResult("Файлы отсутствуют");

            return new OkObjectResult(files);
        }

        [HttpGet("{name}")]
        public IActionResult Delete(string name)
        {
            try
            {
                var storageName = HttpContext.Session?.GetString("StorageName");
                if (storageName == null)
                    return new BadRequestObjectResult("Нужно зайти в хранилище");

                //LoadStorages(ref storageDB, storageName);

                var file = dataLite.Files.Get(f => f.Name == name);
                if (file == null)
                    return new BadRequestObjectResult("Файл не найден");

                dataLite.Files.Delete(file);
                dataLite.Close();
                System.IO.File
                    .Delete(GetDirFile() + name);

                return new OkObjectResult("Файл удален");
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [HttpGet("{name}")]
        public FileResult GetEncryptFile(string name)
        {
            try
            {
                var storage = HttpContext.Session.GetString("StorageName");
                if (storage == null)
                    return null;
                //LoadStorages(ref storageDB, storage);
                var file = dataLite.Files.Get(f => f.Name == name);
                dataLite.Close();
                if (file == null)
                    return null;
                var path = GetDirFile() + file.Path;

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                string fileType = file.Group + "/" + file.Type;
                string fileName = name + "." + file.Type;

                return File(fs, fileType, fileName);
            }
            catch(Exception ex)
            {
                return null;
            }
            
        }

        [HttpGet("{name}")]
        public FileResult GetDecryptFile(string name)
        {
            try
            {
                var storageName = HttpContext.Session.GetString("StorageName");
                if (storageName == null)
                    return null;
                //LoadStorages(ref storageDB, storageName);
                var file = dataLite.Files.Get(f => f.Name == name);
                var storage = storageContext.Storages
                    .Where(s => s.Name == storageName)
                    .FirstOrDefault();
                dataLite.Close();
                if (file == null)
                    return null;
                var path = GetDirFile() + file.Path;

                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                var decrypted = encryptor.Decrypt(br.ReadBytes((int)fs.Length));
                MemoryStream ms = new MemoryStream(decrypted);
                string fileType = file.Group + "/" + file.Type;
                string fileName = name + "." + file.Type;

                return File(ms, fileType, fileName);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        #region Helpers

        //private void LoadData(LiteBase db)
        //{
        //    var dirStorages = "wwwroot/Users/" + User?.Identity.Name + "/";
        //    db.InitTables(dirStorages, User?.Identity.Name + ".db3", "Main");
        //}

        //private void LoadStorages(ref LiteBase db, string name)
        //{
        //    var dirStorages = "wwwroot/Users/" + User?.Identity.Name + "/Storages/";
        //    db.InitTables(dirStorages, name);
        //}

        private byte[] GenerateIV()
        {
            using(RijndaelManaged manager = new RijndaelManaged())
            {
                return manager.IV;
            }
        }

        private string GetDirFile()
        {
            var storageName = HttpContext.Session?.GetString("StorageName");
            var path = "wwwroot/Users/" + User?.Identity.Name + "/Files/" + storageName + "/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        #endregion

    }
}
