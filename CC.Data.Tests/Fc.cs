using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data.Repositories;

namespace CC.Data.Tests
{
    class Fc<T> : IRepository<T> where T : class
    {
        private List<T> data = new List<T>();

        public void Add(T entity)
        {
            data.Add(entity);
        }

        public void Attach(T entity)
        {
            throw new NotImplementedException();
        }

        public void Remove(T entity)
        {
            data.Remove(entity);
        }

        public void Detach(T entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Select
        {
            get
            {
                return data.AsQueryable();
            }
        }

        public int SaveChanges()
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
        }
    }
}
