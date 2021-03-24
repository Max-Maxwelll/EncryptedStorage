using EncryptedStorage.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EncryptedStorage.Service
{
    public class RijndaelEncryptor : IEncryptor
    {
        private readonly HttpContext context;
        private readonly ILogger logger;
        private readonly RijndaelManaged manager;
        private readonly int[] sizes = { 16, 24, 32 };

        public RijndaelEncryptor(IHttpContextAccessor context, ILoggerFactory loggerFactory)
        {
            this.context = context.HttpContext;
            this.logger = loggerFactory.CreateLogger("RequestInfoLogger");
            this.manager = new RijndaelManaged();
            //manager.Padding = PaddingMode.PKCS7;
            logger.LogInformation(manager.Padding.ToString());
        }

        public byte[] Encrypt(byte[] message)
        {
            manager.Key = ToByte(context.Session.GetString("StorageKey"));
            manager.IV = ToByte(context.Session.GetString("StorageIV"));

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(ms, manager.CreateEncryptor(manager.Key, manager.IV), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(message, 0, message.Length);
                }

                return ms.ToArray();
            }
        }

        public byte[] Encrypt(byte[] key, byte[] IV, byte[] message)
        {
            manager.Key = key;
            manager.IV = IV;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(ms, manager.CreateEncryptor(manager.Key, manager.IV), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(message, 0, message.Length);
                }

                return ms.ToArray();
            }
        }

        public T Encrypt<T>(T obj)
        {
            manager.Key = ToByte(context.Session.GetString("StorageKey"));
            manager.IV = ToByte(context.Session.GetString("StorageIV"));

            Type type = obj.GetType();
            MemoryStream memoryStream;
            CryptoStream cryptoStream;

            foreach (PropertyInfo p in type.GetProperties())
            {
                if (p.PropertyType != typeof(string))
                    continue;
                if (p.GetValue(obj) == null)
                    continue;
                memoryStream = new MemoryStream();
                cryptoStream = new CryptoStream(memoryStream, manager.CreateEncryptor(manager.Key, manager.IV), CryptoStreamMode.Write);

                byte[] property = ToByte(p.GetValue(obj).ToString());
                cryptoStream.Write(property, 0, property.Length);
                cryptoStream.FlushFinalBlock();
                cryptoStream.Clear();
                p.SetValue(obj, ToString(memoryStream.ToArray()));
                memoryStream.Close();
            }
            
            return obj;
        }

        public byte[] Decrypt(byte[] message)
        {
            manager.Key = ToByte(context.Session.GetString("StorageKey"));
            manager.IV = ToByte(context.Session.GetString("StorageIV"));

            try
            {
                using (MemoryStream ms = new MemoryStream(message))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(ms, manager.CreateDecryptor(manager.Key, manager.IV), CryptoStreamMode.Read))
                    {
                        using (BinaryReader binaryReader = new BinaryReader(cryptoStream))
                        {
                            return binaryReader.ReadBytes(message.Length);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.LogInformation(ex.Message);
                logger.LogInformation(ex.StackTrace);
                return null;
            }
        }

        public byte[] Decrypt(byte[] key, byte[] IV, byte[] message)
        {
            manager.Key = key;
            manager.IV = IV;

            try
            {
                using (MemoryStream ms = new MemoryStream(message))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(ms, manager.CreateDecryptor(manager.Key, manager.IV), CryptoStreamMode.Read))
                    {
                        using (BinaryReader binaryReader = new BinaryReader(cryptoStream))
                        {
                            return binaryReader.ReadBytes(message.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.Message);
                logger.LogInformation(ex.StackTrace);
                return null;
            }
        }

        public T Decrypt<T>(T obj)
        {
            manager.Key = ToByte(context.Session.GetString("StorageKey"));
            manager.IV = ToByte(context.Session.GetString("StorageIV"));

            Type type = obj.GetType();
            MemoryStream memoryStream;
            CryptoStream cryptoStream;
            StreamReader streamReader;

            foreach (PropertyInfo p in type.GetProperties())
            {
                if (p.PropertyType != typeof(string))
                    continue;

                byte[] property = ToByte(p.GetValue(obj).ToString());
                memoryStream = new MemoryStream(property);
                cryptoStream = new CryptoStream(memoryStream, manager.CreateDecryptor(manager.Key, manager.IV), CryptoStreamMode.Read);
                streamReader = new StreamReader(cryptoStream);
                p.SetValue(obj, streamReader.ReadToEnd());
                streamReader.Close();
                cryptoStream.Clear();
                memoryStream.Close();
            }

            return obj;
        }

        public List<T> DecryptList<T>(List<T> objs)
        {
            manager.Key = ToByte(context.Session.GetString("StorageKey"));
            manager.IV = ToByte(context.Session.GetString("StorageIV"));

            MemoryStream memoryStream;
            CryptoStream cryptoStream;
            BinaryReader binaryReader;

            for (int i = 0; i < objs.Count(); i++)
            {
                try
                {
                    Type type = objs.ElementAt(i).GetType();

                    foreach (PropertyInfo p in type.GetProperties())
                    {
                        if (p.PropertyType != typeof(string))
                            continue;

                        byte[] property = ToByte(p.GetValue(objs.ElementAt(i)).ToString());
                        memoryStream = new MemoryStream(property);
                        cryptoStream = new CryptoStream(memoryStream, manager.CreateDecryptor(manager.Key, manager.IV), CryptoStreamMode.Read);
                        binaryReader = new BinaryReader(cryptoStream);
                        p.SetValue(objs.ElementAt(i), ToString(binaryReader.ReadBytes(property.Length)));
                        binaryReader.Close();
                        cryptoStream.Clear();
                        memoryStream.Close();
                    }
                }
                catch(Exception ex)
                {
                    logger.LogInformation(ex.Message + " # " + ex.StackTrace);
                    continue;
                }
            }

            return objs;
        }

        public async Task<bool> EncryptFile(IFormFile file, string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    var streamFile = file.OpenReadStream();
                    using (BinaryReader binaryReader = new BinaryReader(streamFile))
                    {
                        int length = (int)streamFile.Length;
                        var encrypted = Encrypt(binaryReader.ReadBytes(length));
                        await stream.WriteAsync(encrypted, 0, encrypted.Length);
                    }
                }
                return true;
            }
            catch (Exception ex) { return false; }
        }

        public bool CheckSizeKey(string key)
        {
            if (key == null) return false;
            var length = key.Length;

            var find = sizes.Where(s => s == length).FirstOrDefault();
            if (find == 0)
                return false;

            return true;
        }

        public byte[] ToByte(string value)
        {
            return Encoding.GetEncoding(1251).GetBytes(value);
        }
        public string ToString(byte[] value)
        {
            return Encoding.GetEncoding(1251).GetString(value);
        }

        public byte[] GenerateIV()
        {
            manager.GenerateIV();
            return manager.IV;
        }
    }
}
