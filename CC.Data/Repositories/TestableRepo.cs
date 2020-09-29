using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;

namespace CC.Data.Repositories
{

	// Summary:
	//     Represents a typed entity set that is used to perform create, read, update,
	//     and delete operations.
	//
	// Type parameters:
	//   TEntity:
	//     The entity type.
	public class TestableObjectSet<TEntity> : IDisposable, CC.Data.Repositories.ITestableObjectSet<TEntity> where TEntity : class
	{
		public TestableObjectSet(ccEntities context)
		{
			_context = context;
			_objectSet = context.CreateObjectSet<TEntity>();
		}
		private ccEntities _context;
		private ObjectSet<TEntity> _objectSet;
		public int SaveChanges()
		{
			return _context.SaveChanges();
		}

		public void AddObject(TEntity entity)
		{
			_objectSet.AddObject(entity);
		}
		//
		// Summary:
		//     Copies the scalar values from the supplied object into the object in the
		//     System.Data.Objects.ObjectContext that has the same key.
		//
		// Parameters:
		//   currentEntity:
		//     The detached object that has property updates to apply to the original object.
		//     The entity key of currentEntity must match the System.Data.Objects.ObjectStateEntry.EntityKey
		//     property of an entry in the System.Data.Objects.ObjectContext.
		//
		// Returns:
		//     The updated object.
		public TEntity ApplyCurrentValues(TEntity currentEntity)
		{
			return _objectSet.ApplyCurrentValues(currentEntity);
		}
		//
		// Summary:
		//     Sets the System.Data.Objects.ObjectStateEntry.OriginalValues property of
		//     an System.Data.Objects.ObjectStateEntry to match the property values of a
		//     supplied object.
		//
		// Parameters:
		//   originalEntity:
		//     The detached object that has property updates to apply to the original object.
		//     The entity key of originalEntity must match the System.Data.Objects.ObjectStateEntry.EntityKey
		//     property of an entry in the System.Data.Objects.ObjectContext.
		//
		// Returns:
		//     The updated object.
		public TEntity ApplyOriginalValues(TEntity originalEntity)
		{
			return _objectSet.ApplyOriginalValues(originalEntity);
		}
		//
		// Summary:
		//     Attaches an object or object graph to the object context in the current entity
		//     set.
		//
		// Parameters:
		//   entity:
		//     The object to attach.
		public void Attach(TEntity entity)
		{
			_objectSet.Attach(entity);
		}
		//
		// Summary:
		//     Creates an instance of the specified type.
		//
		// Type parameters:
		//   T:
		//     Type of object to be returned.
		//
		// Returns:
		//     An instance of the requested type T, or an instance of a proxy type that
		//     corresponds to the type T.
		public T CreateObject<T>() where T : class, TEntity
		{
			return _objectSet.CreateObject<T>();
		}

		//
		// Summary:
		//     Creates a new entity type object.
		//
		// Returns:
		//     The new entity type object, or an instance of a proxy type that corresponds
		//     to the entity type.
		public TEntity CreateObject()
		{
			throw new NotImplementedException();
		}

		//
		// Summary:
		//     Marks an object for deletion.
		//
		// Parameters:
		//   entity:
		//     An object that represents the entity to delete. The object can be in any
		//     state except System.Data.EntityState.Detached.
		public void DeleteObject(TEntity entity)
		{
			_objectSet.DeleteObject(entity);
		}
		//
		// Summary:
		//     Removes the object from the object context.
		//
		// Parameters:
		//   entity:
		//     Object to be detached. Only the entity is removed; if there are any related
		//     objects that are being tracked by the same System.Data.Objects.ObjectStateManager,
		//     those will not be detached automatically.
		public void Detach(TEntity entity)
		{
			_objectSet.Detach(entity);
		}

		public IEnumerator<TEntity> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return (_context as IQueryable).GetEnumerator();
		}

		public Type ElementType
		{

			get { return (_context as IQueryable).ElementType; }
		}

		public System.Linq.Expressions.Expression Expression
		{
			get { return (_context as IQueryable).Expression; }
		}

		public IQueryProvider Provider
		{
			get { return (_context as IQueryable).Provider; }
		}

		public void Dispose()
		{
			if (_context != null)
			{
				_context.Dispose();
			}
		}
	}

}
