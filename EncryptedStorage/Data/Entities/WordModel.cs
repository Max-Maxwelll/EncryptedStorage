using SQLite;

namespace EncryptedStorage.Data.Entities.Storage
{
    public class WordModel
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public string User { get; set; }
        public string Storage { get; set; }
        public byte[] ControlWord { get; set; } 
    }
}
