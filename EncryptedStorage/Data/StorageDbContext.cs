using EncryptedStorage.Data.Entities;
using EncryptedStorage.Data.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace EncryptedStorage.Data
{
    public class StorageDbContext : DbContext
    {
        public StorageDbContext(DbContextOptions<StorageDbContext> options)
            :base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<StorageModel> Storages { get; set; }
        public DbSet<WordModel> Words { get; set; }
    }
}
