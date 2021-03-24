using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EncryptedStorage.Data
{
    public interface IEntityData<TEntity>
    {
        void Add(TEntity entity);
        void AddRange(List<TEntity> entity);
        void Update(TEntity entity);
        TEntity Get(Expression<Func<TEntity, bool>> where);
        TEntity GetById(long id);
        TEntity GetById(string id);
        List<TEntity> GetAll();
        List<TEntity> GetMany(Expression<Func<TEntity, bool>> where);
        void Delete(TEntity obj);
        void Delete(Expression<Func<TEntity, bool>> where);
        void DeleteAll();
    }
}
