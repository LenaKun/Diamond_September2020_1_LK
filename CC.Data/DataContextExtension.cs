using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CC.Data
{
	#region
	public class ValueChangedEventArgs<T> : EventArgs
	{
		public ValueChangedEventArgs() { }
		public ValueChangedEventArgs(T oldValue, T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		public T OldValue { get; set; }
		public T NewValue { get; set; }
	}

	public class ValueChangingEventArgs<T> : ValueChangedEventArgs<T>
	{
		public ValueChangingEventArgs() : base() { }

		private bool _cancel = false;
		public bool Cancel
		{
			get { return _cancel; }
			set { _cancel = value; }
		}

	}
	#endregion

	public partial class ccEntities
	{


		public static string Serialize<T>(T value)
		{

			if (value == null)
			{
				return null;
			}

			XmlSerializer serializer = new XmlSerializer(typeof(T));

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
			settings.Indent = false;
			settings.OmitXmlDeclaration = false;

			using (StringWriter textWriter = new StringWriter())
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
				{
					serializer.Serialize(xmlWriter, value);
				}
				return textWriter.ToString();
			}
		}

		public static T Deserialize<T>(string xml)
		{

			if (string.IsNullOrEmpty(xml))
			{
				return default(T);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(T));

			XmlReaderSettings settings = new XmlReaderSettings();
			// No settings need modifying here

			using (StringReader textReader = new StringReader(xml))
			{
				using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
				{
					return (T)serializer.Deserialize(xmlReader);
				}
			}
		}


		[EdmFunction("Edm", "DiffDays")]
		public static int? DiffDays(DateTime? dateValue1, DateTime? dateValue2)
		{
			int? res = null;
			if (dateValue1 != null && dateValue2 != null)
			{
				res = (int)(dateValue2.Value - dateValue1.Value).TotalDays;
			}
			System.Diagnostics.Trace.WriteLine(string.Format("DiffDays ({0}, {1}) = {2}", dateValue1, dateValue2, res));
			return res;
		}
		[EdmFunction("Edm", "DiffMonths")]
		public static int? DiffMonths(DateTime? dateValue1, DateTime? dateValue2)
		{
			if (dateValue1 == null || dateValue2 == null)
			{
				return null;
			}
			else
			{
				return (dateValue2.Value.Year - dateValue1.Value.Year) * 12 + dateValue2.Value.Month - dateValue1.Value.Month;
			}
		}

		[EdmFunction("Edm", "AddMonths")]
		public static DateTime? AddMonths(DateTime? dateValue, int? addValue)
		{
			return dateValue == null || addValue == null ? (DateTime?)null : dateValue.Value.AddMonths(addValue.Value);
		}

		public User ConetxtUser { get; set; }

		#region Constructors

		public ccEntities(bool LazyLoadingEnabled = false, bool ProxyCreationEnabled = false)
			: this()
		{

		}

		public ccEntities(User user)
			: this()
		{
			this.ConetxtUser = user;
		}
		void CcDataContext_ObjectMaterialized(object sender, System.Data.Objects.ObjectMaterializedEventArgs e)
		{
			var objectcontext = sender as ObjectContext;
		}

		#endregion

		void CcDataContext_SavingChanges(object sender, EventArgs e)
		{
			var objectcontext = sender as ObjectContext;
		}

		public override int SaveChanges(System.Data.Objects.SaveOptions options)
		{

			//detect changes
			if (options.HasFlag(SaveOptions.DetectChangesBeforeSave))
			{
				this.DetectChanges();
			}

			if (this.ConetxtUser != null)
			{
				foreach (var entry in ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Modified | System.Data.EntityState.Added))
				{

					System.Reflection.PropertyInfo prop = null;
					if (!entry.IsRelationship && entry.Entity != null)
					{
						prop = entry.Entity.GetType().GetProperty("UpdatedAt");
						if (prop != null)
						{
							prop.SetValue(entry.Entity, DateTime.Now, null);
						}

						prop = entry.Entity.GetType().GetProperty("UpdatedById") ?? entry.Entity.GetType().GetProperty("UpdatedBy");
						if (prop != null)
						{
							prop.SetValue(entry.Entity, this.ConetxtUser.Id, null);
						}
					}
				}

			}

			foreach (var entry in ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Modified | System.Data.EntityState.Added | System.Data.EntityState.Deleted))
			{

				if (entry.Entity is EmergencyCap)
				{
					var ec = entry.Entity as EmergencyCap;
					foreach (var name in entry.GetModifiedProperties())
					{
						if (new string[] { "updatedat", "updatedby" }.Contains(name.ToLower())) continue;

						var h = new History()
						{
							FieldName = name,
							OldValue = string.Concat(new object[] { entry.OriginalValues[name] }),
							NewValue = string.Concat(new object[] { entry.CurrentValues[name] }),

							UpdatedBy = ec.UpdatedBy,
							UpdateDate = ec.UpdatedAt,
							ReferenceId = ec.Id,
							TableName = entry.EntitySet.Name
						};
						this.Histories.AddObject(h);
					}
				}
			}

			if (this.ConetxtUser != null)
			{
				//call PropertyChanging event trigger on objects that support it before sending field updates to the db
				var clientsUpdating = ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Modified);
				var permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.ConetxtUser);
				foreach (var entry in clientsUpdating)
				{
					foreach (var propertyName in entry.GetModifiedProperties())
					{
						var propChangingNotifier = entry.Entity as INotifyPropertySaving;
						if (propChangingNotifier != null)
						{

							ValidationContext validationContext = new ValidationContext(entry, null, new Dictionary<object, object>() { { typeof(User), this.ConetxtUser } });

							try
							{
								//notifiy an object of a property update will be set to a database
								propChangingNotifier.OnPropertySaving(propertyName, permissions);

							}
							catch (PropertyChangeDeniedException)
							{
								//the property change is invalid
							}
						}
					}

				}
			}

			try
			{
				return base.SaveChanges(options);
			}
			catch
			{
				
				throw;
			}


		}

	}
}
