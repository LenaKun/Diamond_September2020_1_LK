using System;
namespace CC.Data.Repositories
{
	interface ITestableObjectSet<TEntity>: System.Linq.IQueryable<TEntity>
	 where TEntity : class
	{
		void AddObject(TEntity entity);
		void Attach(TEntity entity);
		void Detach(TEntity entity);
		void DeleteObject(TEntity entity);
		int SaveChanges();		
	}
}
