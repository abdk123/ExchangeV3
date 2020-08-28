using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BWR.ShareKernel.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(object id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(T obj);
        void RefershEntity(T obj);
        object Save();
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate);
    }
}
