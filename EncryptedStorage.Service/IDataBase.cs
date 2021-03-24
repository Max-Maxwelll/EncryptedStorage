using Microsoft.AspNetCore.Mvc;

namespace EncryptedStorage.Service
{
    public interface IDataBase
    {
        void Initializing(string storage);
        void Delete(string name);
        void DeleteAll();
        FileResult Download(string name);
        void Close();
    }
}
