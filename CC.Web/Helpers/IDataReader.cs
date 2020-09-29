using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Helpers
{

	public static class idrf
	{
		public static System.Data.IDataReader GetReader<Ts>(IEnumerable<Ts> s)
		{
			return new IEnumerableDataReader<Ts>(s);
		}
	}

	/// <summary>
	/// Implements IDataReader on IEnumerable<typeparamref name="T"/> for use with SqlBulkCopy
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class IEnumerableDataReader<T> : System.Data.IDataReader
	{


		IEnumerable<T> source;
		IEnumerator<T> inumerator;
		List<System.Reflection.PropertyInfo> props;

		public IEnumerableDataReader(IEnumerable<T> s)
		{
			source = s;
			inumerator = source.GetEnumerator();
			props = typeof(T).GetProperties().ToList();
		}

		public void Close()
		{
			throw new NotImplementedException();
		}

		public int Depth
		{
			get { throw new NotImplementedException(); }
		}

		public System.Data.DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public bool IsClosed
		{
			get { throw new NotImplementedException(); }
		}

		public bool NextResult()
		{
			throw new NotImplementedException();
		}

		public bool Read()
		{
			return inumerator.MoveNext();
		}

		public int RecordsAffected
		{
			get { throw new NotImplementedException(); }
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public int FieldCount
		{
			get
			{
				return props.Count();
			}
		}

		#region get(int i)
		public bool GetBoolean(int i)
		{
			return (bool)props[i].GetValue(this.inumerator.Current,null);
		}

		public byte GetByte(int i)
		{
			return (byte)props[i].GetValue(this.inumerator.Current,null);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			return (char)props[i].GetValue(this.inumerator.Current,null);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public System.Data.IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName(int i)
		{
			return props[i].PropertyType.Name;
		}

		public DateTime GetDateTime(int i)
		{
			return (DateTime)props[i].GetValue(this.inumerator.Current, null);
		}

		public decimal GetDecimal(int i)
		{
			return (decimal)props[i].GetValue(this.inumerator.Current, null);
		}

		public double GetDouble(int i)
		{
			return (double)props[i].GetValue(this.inumerator.Current, null);
		}

		public Type GetFieldType(int i)
		{
			return props[i].PropertyType;
		}

		public float GetFloat(int i)
		{
			return (float)props[i].GetValue(this.inumerator.Current, null);
		}

		public Guid GetGuid(int i)
		{
			return (Guid)props[i].GetValue(this.inumerator.Current, null);
		}

		public short GetInt16(int i)
		{
			return (Int16)props[i].GetValue(this.inumerator.Current, null);
		}

		public int GetInt32(int i)
		{
			return (Int32)props[i].GetValue(this.inumerator.Current, null);
		}

		public long GetInt64(int i)
		{
			return (Int64)props[i].GetValue(this.inumerator.Current, null);
		}

		public string GetName(int i)
		{
			return props[i].Name;
		}
		#endregion

		public int GetOrdinal(string name)
		{
			return props.IndexOf(props.Single(f => f.Name == name));
		}

		public string GetString(int i)
		{
			return (string)props[i].GetValue(this.inumerator.Current, null);
		}

		public object GetValue(int i)
		{
			return props[i].GetValue(this.inumerator.Current, null);
		}

		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public bool IsDBNull(int i)
		{
			return this.GetValue(i) == null;
		}

		public object this[string name]
		{
			get { return props.Single(f => f.Name == name); }
		}

		public object this[int i]
		{
			get { return props[i]; }
		}
	}
}