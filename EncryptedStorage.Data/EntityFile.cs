using EncryptedStorage.Data.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EncryptedStorage.Data
{
    public class EntityFile : IEntityData<FileModel>
    {
        private SQLiteConnection connection;

        public EntityFile(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        public void Add(FileModel entity)
        {
            connection.Insert(entity);
        }

        public void AddRange(List<FileModel> entity)
        {
            entity.ForEach(a => connection.Insert(a));
        }

        public void Delete(FileModel obj)
        {
            connection.Delete(obj);
        }

        public void Delete(Expression<Func<FileModel, bool>> where)
        {
            connection.Table<FileModel>().Delete(where);
        }

        public void DeleteAll()
        {
            connection.Table<FileModel>().Where(a => true).Delete();
        }

        public FileModel Get(Expression<Func<FileModel, bool>> where)
        {
            var result = connection.Table<FileModel>().Where(where).FirstOrDefault();
            return result;
        }

        public List<FileModel> GetMany(Expression<Func<FileModel, bool>> where)
        {
            var result = connection.Table<FileModel>().Where(where).ToList();
            return result;
        }

        public List<FileModel> GetAll()
        {
            var result = connection.Table<FileModel>().ToList();
            return result;
        }

        public FileModel GetById(long id)
        {
            var result = connection.Table<FileModel>().Where(a => a.Id == id).FirstOrDefault();
            return result;
        }

        public FileModel GetById(string id)
        {
            try
            {
                var convert = Convert.ToInt32(id);
                var result = connection.Table<FileModel>().Where(a => a.Id == convert).FirstOrDefault();
                return result;
            }
            catch (Exception ex) { return null; }
        }

        public void Update(FileModel entity)
        {
            connection.Update(entity);
        }
    }
}
