using SQLite;
using System.ComponentModel.DataAnnotations;

namespace EncryptedStorage.Data.Entities
{
    public class StorageRequestModel
    {
        public StorageModel Storage { get; set; }
        public string Key { get; set; }
    }
}
