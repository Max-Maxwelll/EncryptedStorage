using SQLite;
using System.ComponentModel.DataAnnotations;

namespace EncryptedStorage.Data.Entities
{
    public class StorageModel
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public string User { get; set; }
        public string Name { get; set; }
        public int CountAccaunts { get; set; }
        public int CountDocuments { get; set; }
        public int CountImages { get; set; }
        public int CountAudio { get; set; }
        public byte[] IV { get; set; }
    }
}
