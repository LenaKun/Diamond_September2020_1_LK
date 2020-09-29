using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace CC.Data
{
	public interface IGenericRepository<T>
	 where T : class, new()
	{
		CC.Data.Services.IPermissionsBase Permissions { get; set; }
		void Add(T entity);
		void Attach(T entity);
		void Detach(T entity);
		System.Linq.IQueryable<T> GetAll();
		System.Linq.IQueryable<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>> expression);
		void Remove(T entity);
		int SaveChanges();
	}

	public class GenericRepository<T> : IDisposable, IGenericRepository<T>
	where T : class, new()
	{
		protected ccEntities context { get; set; }
		public CC.Data.Services.IPermissionsBase Permissions { get; set; }
		protected ObjectSet<T> dbset { get; set; }

		public GenericRepository() { }
		public GenericRepository(ccEntities context)
		{
			this.context = context;
			this.dbset = context.CreateObjectSet<T>();
		}
		public GenericRepository(ccEntities context, CC.Data.Services.IPermissionsBase permissions)
			: this(context)
		{


			this.Permissions = permissions;
		}



		public virtual void Add(T entity)
		{

			dbset.AddObject(entity);
		}

		public void Attach(T entity)
		{
			dbset.Attach(entity);
		}

		public void Remove(T entity)
		{
			dbset.DeleteObject(entity);
		}

		public void Detach(T entity)
		{
			dbset.Detach(entity);
		}

		public IQueryable<T> GetAll()
		{
			return dbset;
		}
		public IQueryable<T> GetAll(Expression<Func<T, bool>> expression)
		{
			return dbset.Where(expression);
		}

		public virtual int SaveChanges()
		{
			return context.SaveChanges();
		}


		public void Dispose()
		{
			if (this.context != null)
			{
				this.context.Dispose();
				this.context = null;
			}
		}



	}

	public class FakeRepository<T> : IGenericRepository<T>
	where T : class, new()
	{
		public FakeRepository() { }
		public FakeRepository(IEnumerable<T> data)
			: this()
		{
			this.data.AddRange(data);
		}
		private List<T> data = new List<T>();

		public Services.IPermissionsBase Permissions
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public void Add(T entity)
		{
			this.data.Add(entity);
		}

		public void Attach(T entity)
		{

		}

		public void Detach(T entity)
		{

		}

		public IQueryable<T> GetAll()
		{
			return this.data.AsQueryable<T>();
		}

		public IQueryable<T> GetAll(Expression<Func<T, bool>> expression)
		{
			return this.GetAll().Where(expression);
		}

		public void Remove(T entity)
		{
			this.data.Remove(entity);
		}

		public int SaveChanges()
		{
			return new Random().Next();
		}
	}
}
