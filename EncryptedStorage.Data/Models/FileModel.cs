using SQLite;

namespace EncryptedStorage.Data.Models
{
    public class FileModel
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public double Size { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
    }
}
