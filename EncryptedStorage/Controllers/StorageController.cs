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
using EncryptedStorage.Data.Entities;
using EncryptedStorage.Models.StorageViewModels;
using EncryptedStorage.Data.Entities.Storage;
using System.Security.Cryptography;
using EncryptedStorage.Service;

namespace EncryptedStorage.Controllers
{
    [EnableCors("AllowMyOrigin")]
    [Authorize]
    [Route("[controller]/[action]")]
    public class StorageController : Controller
    {
        private readonly ILogger logger;
        private readonly IEncryptor encryptor;
        private readonly DataLite dataLite;
        private readonly StorageDbContext storageContext;
        //private LiteBase storageDB;

        public StorageController(
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

        [HttpPost]
        public async Task<IActionResult> CreateStorage([FromBody]StorageRequestModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Storage.User = User.Identity.Name;
                    model.Storage.IV = encryptor.GenerateIV();

                    var result = storageContext.Storages
                        .Where(s => s.Name == model.Storage.Name && s.User == User.Identity.Name);

                    if (model.Storage.Name == null)
                        return new BadRequestObjectResult("Отсутствует имя хранилища");

                    if (result.Count() != 0)
                        return new BadRequestObjectResult("Хранилище с таким именем существует");

                    if (!encryptor.CheckSizeKey(model.Key))
                        return new BadRequestObjectResult("Размер ключа [16, 24, 32]");

                    await storageContext.Storages.AddAsync(model.Storage);

                    HttpContext.Session.SetString("StorageIV", encryptor.ToString(model.Storage.IV));
                    HttpContext.Session.SetString("StorageKey", model.Key);

                    await storageContext.Words.AddAsync(
                        new WordModel() {
                            User = User.Identity.Name,
                            Storage = model.Storage.Name,
                            ControlWord = encryptor.Encrypt(encryptor.ToByte("ControlWord"))
                        });
                
                    await storageContext.SaveChangesAsync();
                    //LoadStorages(storageDB, model.Storage.Name);
                    //storageDB.Connection.Close();
                    dataLite.Initializing(model.Storage.Name);
                    dataLite.Close();

                    logger.LogInformation("Хранилище создано");
                    return new OkObjectResult("Хранилище создано");
                }
                catch (Exception ex)
                {
                    logger.LogInformation("##### " + ex.StackTrace);
                    return new BadRequestObjectResult(ex.Message);
                }
            }

            return new BadRequestObjectResult("Модель данных не корректна");
        }

        [HttpGet]
        public IActionResult GetStorages()
        {
            try
            {
                return new OkObjectResult(storageContext.Storages
                    .Where(s => s.User == User.Identity.Name)
                    .ToList());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public async Task<IActionResult> ChangeKeyStorage([FromBody]ChangeKeyStorageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var storage = storageContext.Storages
                    .Where(s => s.Name == model.Name && s.User == User.Identity.Name)
                    .FirstOrDefault();

                var word = storageContext.Words
                    .Where(w => w.Storage == model.Name && w.User == User.Identity.Name)
                    .FirstOrDefault();

                if (storage == null)
                    return new BadRequestObjectResult("Хранилище не найдено");

                if(word == null)
                    return new BadRequestObjectResult("Контрольное слово отсутствует");

                if (!model.NewKey.Equals(model.ConfirmKey))
                    return new BadRequestObjectResult("Пароли не совпадают");

                if (model.OldKey.Equals(model.NewKey))
                    return new BadRequestObjectResult("Старый и новый пароли совпадают");

                if (!encryptor.CheckSizeKey(model.NewKey) || !encryptor.CheckSizeKey(model.OldKey))
                    return new BadRequestObjectResult("Размер ключа [16, 24, 32]");

                var controlWord = encryptor.Decrypt(encryptor.ToByte(model.OldKey), storage.IV, word.ControlWord);

                if (controlWord == null)
                    return new BadRequestObjectResult("Ключ неверный");

                storage.IV = encryptor.GenerateIV();

                word.ControlWord = encryptor.Encrypt(encryptor.ToByte(model.NewKey), storage.IV, controlWord);

                storageContext.Storages.Update(storage);
                storageContext.Words.Update(word);

                storageContext.Storages.Update(storage);
                await storageContext.SaveChangesAsync();

                return new OkObjectResult("Пароль хранилища обновлен");
            }

            return new BadRequestObjectResult("Модель данных не корректна");
        }

        [HttpGet]
        public IActionResult DownloadStorage(string name)
        {
            return dataLite.Download(name);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            try
            {
                string storageName = HttpContext.Session?.GetString("StorageName");
                if (storageName == name)
                    return new BadRequestObjectResult("Вы находитеся в этом хранилище");

                var storage = storageContext.Storages
                .Where(s => s.Name == name && s.User == User.Identity.Name)
                .FirstOrDefault();
                var word = storageContext.Words
                    .Where(w => w.Storage == name && w.User == User.Identity.Name)
                    .FirstOrDefault();
                if (storage == null)
                    return new BadRequestObjectResult("Хранилище не найдено");

                storageContext.Remove(storage);
                if (word != null)
                    storageContext.Remove(word);

                //if (storageDB.Connection != null)
                //    storageDB = new LiteBase();
                //var path = "wwwroot/Users/" + User?.Identity.Name + "/Storages/" + name + ".db3";
                //if (System.IO.File.Exists(path))
                //    System.IO.File.Delete(path);

                //if(System.IO.Directory.Exists(GetDirFile(false) + name))
                //    System.IO.Directory.Delete(GetDirFile(false) + name, true);
                dataLite.Close();
                dataLite.Delete(name);
                
                await storageContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.StackTrace);
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkObjectResult("Хранилище удалено");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                var storage = HttpContext.Session?.GetString("StorageName");
                if (storage != null)
                    return new BadRequestObjectResult("Вы должны выйти из хранилища");
                var storages = storageContext.Storages
                .Where(s => s.User == User.Identity.Name);
                if (storages == null)
                    return new BadRequestObjectResult("Список хранилищ пуст");

                dataLite.Close();
                dataLite.DeleteAll();

                storageContext.RemoveRange(storages);
                await storageContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.StackTrace);
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkObjectResult("Все хранилища удалины");
        }

        [HttpPost]
        public IActionResult EnterKey([FromBody]KeyViewModel model)
        {
            var storage = storageContext.Storages
                .Where(s => s.Name == model.StorageName && s.User == User.Identity.Name)
                .FirstOrDefault();
            var word = storageContext.Words
                .Where(w => w.Storage == model.StorageName && w.User == User.Identity.Name)
                .FirstOrDefault();

            if (storage == null)
                return new BadRequestObjectResult("Хранилище не найдено");

            if (word == null)
                return new BadRequestObjectResult("Контрольное слово не найдено");

            if (!encryptor.CheckSizeKey(model.Key))
                return new BadRequestObjectResult("Размер ключа [16, 24, 32]");

            //HttpContext.Session.SetString("StorageKey", );
            // Возсожно нужно хранить в STRING
            //HttpContext.Session.SetString("StorageIV", );

            if (encryptor.Decrypt(encryptor.ToByte(model.Key), storage.IV, word.ControlWord) == null)
                return new BadRequestObjectResult("Ключ неверный");

            //dataLite.Close();
            dataLite.Initializing(model.StorageName);
            dataLite.Close();
            HttpContext.Session.SetString("StorageName", model.StorageName);
            HttpContext.Session.SetString("StorageKey", model.Key);
            // Возсожно нужно хранить в STRING
            HttpContext.Session.SetString("StorageIV", encryptor.ToString(storage.IV));

            return new OkObjectResult("Ключ верный");
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> UploadKey()
        {
            try
            {
                var files = Request.Form.Files;
                if (files == null)
                {
                    return new BadRequestObjectResult("Файл отсутствует");
                }

                using (StreamReader streamReader = new StreamReader(files[0].OpenReadStream()))
                {
                    return new OkObjectResult(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.StackTrace);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetCurrent()
        {
            try
            {
                var name = HttpContext.Session?.GetString("StorageName");
                if (name == null)
                    return new BadRequestObjectResult("Нужно зайти в хранилище");

                var user = User.Identity?.Name;

                var storage = storageContext.Storages.Where(s => s.Name == name && s.User == user).FirstOrDefault();

                if (storage == null)
                    return new BadRequestObjectResult("Хранилище не найдено");

                return new OkObjectResult(storage);
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex.Message + " # " + ex.StackTrace);
            }
        }

        #region Helpers

        #endregion

    }
}
