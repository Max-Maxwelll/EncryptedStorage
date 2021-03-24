using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cors;
using EncryptedStorage.Data;
using EncryptedStorage.Data.Entities.Storage;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;
using EncryptedStorage.Service;
using EncryptedStorage.Data.Models;

namespace EncryptedStorage.Controllers
{
    [EnableCors("AllowMyOrigin")]
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly ILogger logger;
        private readonly IEncryptor encryptor;
        private readonly DataLite dataLite;
        private readonly StorageDbContext storageContext;
        //private LiteBase storageDB;

        public AccountController(
            ILogger<AccountController> logger,
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
        public IActionResult Create([FromBody]AccountModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var storageName = HttpContext.Session?.GetString("StorageName");
                    if (storageName == null)
                        return new BadRequestObjectResult("Нужно зайти в хранилище");

                    //LoadStorages(ref storageDB, storageName);
                    var storage = storageContext.Storages
                        .Where(s => s.Name == storageName)
                        .FirstOrDefault();

                    model = encryptor.Encrypt(model);
                    dataLite.Accounts.Add(model);
                    dataLite.Close();

                    logger.LogInformation("Аккаунт добавлен в хранилище");
                    return new OkObjectResult("Аккаунт добавлен в хранилище");
                }
                catch (Exception ex)
                {
                    logger.LogInformation("##### " + ex.StackTrace);
                    return new BadRequestObjectResult(ex.Message);
                }
            }

            return new BadRequestObjectResult("Модель не корректна");
        }

        [HttpGet]
        public IActionResult GetAccounts()
        {
            try
            {
                var storageName = HttpContext.Session?.GetString("StorageName");
                //List<AccountModel> accounts;
                if (storageName == null)
                    return new BadRequestObjectResult("Нужно зайти в хранилище");

                //LoadStorages(ref storageDB, storageName);
                var storage = storageContext.Storages
                        .Where(s => s.Name == storageName)
                        .FirstOrDefault();

                if (storage == null)
                    return new BadRequestObjectResult("Хранилище не найдено");

                if (HttpContext.Session?.GetString("StorageKey") == null)
                    return new BadRequestObjectResult("Ключ отсутствует");

                var accounts = encryptor.DecryptList(dataLite.Accounts.GetAll());

                dataLite.Close();

                return new OkObjectResult(accounts);
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.Message + "######## " + ex.StackTrace);
                return new BadRequestObjectResult(ex.Message + "\n" + ex.StackTrace);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var name = HttpContext.Session?.GetString("StorageName");
                if (name == null)
                    return new BadRequestObjectResult("Нужно зайти в хранилище");

                //LoadStorages(ref storageDB, name);

                var account = dataLite.Accounts.GetById(id);

                if (account == null)
                    return new BadRequestObjectResult("Аккаунт не найден");

                dataLite.Accounts.Delete(account);
                dataLite.Close();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkObjectResult("Аккаунт удален");
        }

        [HttpGet]
        public IActionResult DeleteAll()
        {
            var name = HttpContext.Session?.GetString("StorageName");
            if (name == null)
                return new BadRequestObjectResult("Нужно зайти в хранилище");

            //LoadStorages(ref storageDB, name);

            try
            {
                var accounts = dataLite.Accounts.GetAll();

                if (accounts == null)
                    return new BadRequestObjectResult("Список аккаунтов пуст");

                dataLite.Accounts.DeleteAll();
                dataLite.Close();
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.Message);
                logger.LogInformation(ex.StackTrace);
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkObjectResult("Все аккаунты удалины");
        }

        [HttpPost]
        public IActionResult Change([FromBody]AccountModel model)
        {
            if (ModelState.IsValid)
            {
                var storageName = HttpContext.Session?.GetString("StorageName");
                if (storageName == null)
                    return new BadRequestObjectResult("Нужно зайти в хранилище");

                //LoadStorages(ref storageDB, storageName);

                var account = dataLite.Accounts.Get(a => a.Id == model.Id);

                var storage = storageContext.Storages
                    .Where(s => s.Name == storageName)
                    .FirstOrDefault();

                if (account == null)
                    return new BadRequestObjectResult("Аккаунт не найден");

                model = encryptor.Encrypt(model);
                dataLite.Accounts.Update(model);

                dataLite.Close();

                return new OkObjectResult("Аккаунт обновлен");
            }

            return new BadRequestObjectResult("Набор данных не корректен");
        }

        [HttpGet("{id}")]
        public IActionResult GetPassword(int id)
        {
            try
            {
                string password;
                var storageName = HttpContext.Session?.GetString("StorageName");
                if (storageName == null)
                    return new BadRequestObjectResult("Нужно зайти в хранилище");

                if (storageName == null)
                    return new BadRequestObjectResult("Нужно зайти в хранилище");

                //LoadStorages(ref storageDB, storageName);

                var account = dataLite.Accounts.Get(a => a.Id == id);
                dataLite.Close();
                var storage = storageContext.Storages
                        .Where(s => s.Name == storageName)
                        .FirstOrDefault();

                if (storage == null)
                    return new BadRequestObjectResult("Хранилище не найдено");

                if (account == null)
                    return new BadRequestObjectResult("Аккаунт не найден");

                logger.LogInformation(" # " + account.Password.ToString());

                password = encryptor.ToString(encryptor.Decrypt(encryptor.ToByte(account.Password)));
                
                return new OkObjectResult(password);
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
        //[HttpGet]
        //public IActionResult DisplayTables()
        //{
        //    if (!CheckTempData())
        //        return new BadRequestObjectResult("Нужно зайти в хранилище");

        //    LoadStorages(ref storageDB, TempData["StorageName"].ToString());
        //    var tables = storageDB.Connection.TableMappings;
        //    return new OkObjectResult(JsonConvert.SerializeObject(storageDB.Connection.TableMappings));
        //}

        #region Helpers

        private bool CheckSession(string name)
        {
            byte[] value;
            var result = HttpContext.Session.TryGetValue(name, out value);
            if (result)
                return true;
            return false;
        }

        //private void LoadStorages(ref LiteBase db, string name)
        //{
        //    var dirStorages = "wwwroot/Users/" + User?.Identity.Name + "/Storages/";
        //    db.InitTables(dirStorages, name);
        //}

        #endregion

    }
}
