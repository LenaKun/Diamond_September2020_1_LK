using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Entity;
using System.Data.Objects;
using System.Data;

namespace CC.Data.Repositories
{
	public class Repository<T> : IRepository<T>, IDisposable
		where T : class
	{



		protected User RepositoryUser
		{
			get { return _objectContext.ConetxtUser; }
			set { RepositoryUser = value; }
		}
		protected ObjectSet<T> _objectSet;

		public ObjectSet<T> ObjectSet
		{
			get { return _objectSet; }
			set { _objectSet = value; }
		}
		protected ccEntities _objectContext;

		public ccEntities ObjectContext
		{
			get { return _objectContext; }
			set { _objectContext = value; }
		}
		protected CcRepository _parent;

		public Repository() { }
		public Repository(ccEntities objectContext)
		{

			_objectContext = objectContext as ccEntities;
			_objectSet = objectContext.CreateObjectSet<T>();
			_objectContext.SavingChanges += new EventHandler(_objectContext_SavingChanges);
			_objectContext.ObjectMaterialized += new ObjectMaterializedEventHandler(_objectContext_ObjectMaterialized);
		}
		private void _objectContext_ObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
		{

		}
		public Repository(ccEntities objectContext, CcRepository parent)
			: this(objectContext)
		{
			this._parent = parent;
		}


		public virtual void Add(T entity)
		{
			_objectSet.AddObject(entity);
		}

		public virtual void Attach(T entity)
		{
			_objectSet.Attach(entity);
		}

		public virtual void Remove(T entity)
		{
			_objectSet.DeleteObject(entity);
		}

		public virtual void Detach(T entity)
		{
			_objectSet.Detach(entity);
		}

		public virtual IQueryable<T> Select
		{
			get
			{
				return _objectSet.AsQueryable();
			}
		}

		public int SaveChanges()
		{
			return _objectContext.SaveChanges();
		}

		public void ApplyChanges(T entity)
		{
			{
				var entry = _objectContext.ObjectStateManager.GetObjectStateEntry(entity);
				if (entry != null)
				{
					entry.ApplyCurrentValues(entity);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="NotSupportedException"></exception>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual T GetById(int id)
		{

			if (!typeof(T).GetInterfaces().Any(f => f == typeof(IIntIdRecord)))
			{
				throw new NotSupportedException("GetById(int id) is supported only if the Reposytory's type implements IIntIdRecord interface");
			}

			string containerName = _objectContext.DefaultContainerName;
			string setName = _objectContext.CreateObjectSet<T>().EntitySet.Name;
			// Build entity key
			var entityKey = new EntityKey(containerName + "." + setName, "Id", id);

			return (T)_objectContext.GetObjectByKey(entityKey);
		}

		public EntityKey asdf(T entity)
		{
			using (ccEntities context = new ccEntities())
			{
				var tempset = context.CreateObjectSet<T>();
				tempset.Attach(entity);
				var key = context.ObjectStateManager.GetObjectStateEntry(entity).EntityKey;
				tempset.Detach(entity);

			}
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="refreshMode"></param>
		/// <exception cref="ArgumentException">An object is not attached to the context.</exception>
		/// <exception cref="InvalidOperationException">Record does not exists at the store.</exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <param name="entity"></param>
		public void Refresh(RefreshMode refreshMode, T entity)
		{
			_objectContext.Refresh(refreshMode, entity);
		}
		public void Refresh(RefreshMode refreshMode, IEnumerable<T> entities)
		{
			_objectContext.Refresh(refreshMode, entities);
		}
		public void TryRefresh(RefreshMode refreshMode, IEnumerable<T> entities)
		{
			try
			{
				_objectContext.Refresh(refreshMode, entities);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.TraceWarning(ex.Message);
			}
		}










		void _objectContext_SavingChanges(object sender, EventArgs e)
		{
			var itemsUpdating = _objectContext.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Added).Where(f => f.Entity.GetType() == typeof(T));
			OnUpdating(itemsUpdating.Select(f => f.Entity).Cast<T>());

			this.OnInserting(itemsUpdating);
			this.OnUpdating(_objectContext.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Added).Where(f => f.Entity.GetType() == typeof(T)));
			this.OnDeleting(_objectContext.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Added).Where(f => f.Entity.GetType() == typeof(T)));
		}

		protected virtual void OnInserting(IEnumerable<ObjectStateEntry> entries)
		{
		}
		protected virtual void OnUpdating(IEnumerable<ObjectStateEntry> entries)
		{
		}
		protected virtual void OnDeleting(IEnumerable<ObjectStateEntry> entries)
		{
		}

		protected event EventHandler<GenEventArgs<IEnumerable<T>>> updating;
		protected virtual void OnUpdating(IEnumerable<T> entries)
		{
			if (updating != null)
			{
				updating(this, new GenEventArgs<IEnumerable<T>>() { EvantData = entries });
			}
		}

		public void Dispose()
		{
			if (_objectContext != null) _objectContext.Dispose();
		}

	}
	public class GenEventArgs<T> : EventArgs
	{
		public T EvantData { get; set; }
	}

	public static class RepoFactory
	{
		public static IRepository<T> GetRepository<T>(ccEntities context) where T : class
		{
			IRepository<T> result = null;
			if (typeof(T) == typeof(Client))
			{
				result = new ClientsRepository(context) as IRepository<T>;
			}
			else if (typeof(T) == typeof(Agency))
			{
				result = new AgenciesRepository(context) as IRepository<T>;
			}
			else
			{
				result = new Repository<T>(context);
			}
			return result;
		}
	}


}
