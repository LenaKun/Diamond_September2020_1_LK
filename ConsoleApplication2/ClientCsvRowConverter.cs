using System;
using System.Collections.Generic;
using System.Linq;
using CC.Data;
using System.Text;

namespace ConsoleApplication2
{
	class ClientCsvRowConverter : System.ComponentModel.TypeConverter
	{
		public List<Country> coutnries = null;
		public List<State> states = null;
		public List<NationalIdType> nationalIdTypes = null;

		public List<Agency> agencies = null;

		public ClientCsvRowConverter()
		{
			using (var db = new ccEntities())
			{
				db.ContextOptions.ProxyCreationEnabled = false;
				db.ContextOptions.LazyLoadingEnabled = false;

				coutnries = db.Countries.ToList();
				states = db.States.ToList();
				nationalIdTypes = db.NationalIdTypes.ToList();
				agencies = db.Agencies.ToList();
			}
		}

		public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType.Equals(typeof(Client)))
			{
				return true;
			}
			else
			{
				return base.CanConvertTo(context, destinationType);
			}
		}

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			var r = value as zz;

			if (destinationType.Equals(typeof(Client)))
			{
				var c = new Client();
				c.Id = r.CLIENT_ID;
				c.Agency = agencies.Single(f => f.Id == r.ORG_ID);
				c.LastName = r.LAST_NAME;
				c.FirstName = r.FIRST_NAME;
				c.MiddleName = r.MIDDLE_NAME;
				c.BirthDate = r.DOB;

				c.Address = r.ADDRESS;
				c.City = r.CITY;
				if (!string.IsNullOrEmpty(r.STATE_CODE))
				{
					try { c.State = states.Single(f => f.Code == r.STATE_CODE); }
					catch (InvalidOperationException) { /*ignore states*/ }
				}
				if (!string.IsNullOrEmpty(r.TYPE_OF_ID))
				{
					try { c.NationalIdType = nationalIdTypes.Single(f => f.Name.Equals(r.TYPE_OF_ID, StringComparison.InvariantCultureIgnoreCase)); }
					catch (InvalidOperationException)
					{
						var n = new NationalIdType()
						{
							Name = r.TYPE_OF_ID
						};
						using (var db = new ccEntities())
						{
							db.NationalIdTypes.AddObject(n);
							db.SaveChanges();
							c.NationalIdType = n;
							nationalIdTypes.Add(n);
						}
						//throw new InvalidOperationException("NationalIdType \"" + r.TYPE_OF_ID + "\" is invalid.");
					}
				}
				c.NationalId = r.Gov_ID;
				c.Phone = r.PHONE;

				c.IsCeefRecipient = r.CLIENT_COMP_PROGRAM;
				c.CeefId = r.COMP_PROG_REG_NUM;

				c.AddCompName = r.AdditionalComp;
				c.AddCompId = r.AdditionalCompNum;

				//c.Deceased
				c.DeceasedDate = r.DOD;
				c.New_Client = r.New_Client;
				c.UpdatedAt = r.Upload_Date ?? DateTime.Now;
				c.MatchFlag = r.MatchFlag;
				c.FundStatus = r.Fund_Status;


				return c;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
