using System;
using SQLite;

namespace EncryptedStorage.Data.Models
{
    public class AccountModel
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public string StorageName { get; set; }
    }
}
