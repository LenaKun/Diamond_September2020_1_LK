using System;
using System.Data.Objects;

namespace CC.Data.Repositories
{
    public interface IRepository<T>:IDisposable
     where T : class
    {
        void Add(T entity);
        void Attach(T entity);
        void Remove(T entity);
        void Detach(T entity);
        System.Linq.IQueryable<T> Select { get; }
        int SaveChanges();
    }
}


