using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EncryptedStorage.Service
{
    public interface IEncryptor
    {
        byte[] Encrypt(byte[] message);
        byte[] Encrypt(byte[] key, byte[] IV, byte[] message);
        byte[] Decrypt(byte[] message);
        byte[] Decrypt(byte[] key, byte[] IV, byte[] message);
        T Encrypt<T>(T obj);
        T Decrypt<T>(T obj);
        List<T> DecryptList<T>(List<T> objs);
        Task<bool> EncryptFile(IFormFile file, string path);
        bool CheckSizeKey(string key);
        byte[] ToByte(string value);
        string ToString(byte[] value);
        byte[] GenerateIV();
    }
}
