using EncryptedStorage.Data.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EncryptedStorage.Data
{
    public class EntityAccount : IEntityData<AccountModel>
    {
        private SQLiteConnection connection;

        public EntityAccount(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        public void Add(AccountModel entity)
        {
            connection.Insert(entity);
        }

        public void AddRange(List<AccountModel> entity)
        {
            entity.ForEach(a => connection.Insert(a));
        }

        public void Delete(AccountModel obj)
        {
            connection.Delete(obj);
        }

        public void Delete(Expression<Func<AccountModel, bool>> where)
        {
            connection.Table<AccountModel>().Delete(where);
        }

        public void DeleteAll()
        {
            connection.Table<AccountModel>().Where(a => true).Delete();
        }

        public AccountModel Get(Expression<Func<AccountModel, bool>> where)
        {
            var result = connection.Table<AccountModel>().Where(where).FirstOrDefault();
            return result;
        }

        public List<AccountModel> GetMany(Expression<Func<AccountModel, bool>> where)
        {
            var result = connection.Table<AccountModel>().Where(where).ToList();
            return result;
        }

        public List<AccountModel> GetAll()
        {
            var result = connection.Table<AccountModel>().ToList();
            return result;
        }

        public AccountModel GetById(long id)
        {
            var result = connection.Table<AccountModel>().Where(a => a.Id == id).FirstOrDefault();
            return result;
        }

        public AccountModel GetById(string id)
        {
            try
            {
                var convert = Convert.ToInt32(id);
                var result = connection.Table<AccountModel>().Where(a => a.Id == convert).FirstOrDefault();
                return result;
            }
            catch(Exception ex) { return null; }
        }

        public void Update(AccountModel entity)
        {
            connection.Update(entity);
        }
    }
}
