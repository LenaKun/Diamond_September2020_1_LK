using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CC.Web.Models
{

	public class NewGFHoursEntryModel : ModelBase
	{

		public int ClientId
		{
			get;
			set;
		}
		public NewGFHoursEntryModel() : base() { }
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		[Display(Name = "Start Date")]
		[DateFormat]
		[Required]
		public DateTime GFStartDate
		{
			get;
			set;
		}


		[Display(Name = "Value")]
		[Required]
		public decimal Value
		{
			get;
			set;
		}

		[Display(Name = "Type")]
		[Required]
		public int Type
		{
			get;
			set;
		}

		public SelectList GFHoursTypes { get; set; }

		internal void Insert(ccEntities db, CC.Data.Services.IPermissionsBase permissionsBase)
		{
			var c = db.Clients.Where(permissionsBase.ClientsFilter).SingleOrDefault(f => f.Id == this.ClientId);
			if (c == null)
			{
				throw new Exception("Client not found.");
			}
			else
			{
				var gfHour = this.GetGFHour;
				gfHour.UpdatedBy = permissionsBase.User.Id;
				db.GrandfatherHours.AddObject(gfHour);
				try
				{
					db.SaveChanges();
				}
				catch (System.Data.UpdateException ex)
				{

					var msg = ex.InnerException.Message;
					if (msg.Contains("PK_GFHours"))
					{
						throw new Exception("Duplicate entry.", ex);
					}
					else if (msg.Contains("FK_GFHours_Clients"))
					{
						throw new Exception("Client not found.", ex);
					}
					else if (msg.Contains("CK_GFHours_Value"))
					{
						throw new Exception("Invalid Value.", ex);
					}
					else
					{
						throw;
					}
				}
			}
		}

		public GrandfatherHour GetGFHour
		{
			get
			{
				var gh = new GrandfatherHour { ClientId = this.ClientId, StartDate = this.GFStartDate, Value = this.Value, Type = this.Type };
				return gh;
			}
		}
	}

	public class ClientGFHoursTabDataModel : jQueryDataTableParamModel
	{
		public object GetResult(ccEntities db, CC.Data.Services.IPermissionsBase permissionsBase)
		{
			var source = from c in db.Clients.Where(permissionsBase.ClientsFilter)
						 where c.Id == this.ClientId
						 from item in c.GrandfatherHours
						 where item.ClientId == this.ClientId
						 select new
						 {
							 StartDate = item.StartDate,
							 Value = item.Value,
							 Type = item.Type
						 };

			var filtered = source;

			var ordered = filtered.OrderByField(this.sSortCol_0, this.sSortDir_0 == "asc");

			return new jQueryDataTableResult
			{
				sEcho = this.sEcho,
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = source.Count(),
				aaData = ordered.Skip(this.iDisplayStart).Take(this.iDisplayLength).ToList().Select(f => new
				{
					StartDate = f.StartDate.ToShortDateString(),
					IsoStartDate = f.StartDate.ToString("s"),
					Value = f.Value.ToString(),
                    Type = f.Type == 0 ? "Legacy" : f.Type == 1 ? "Exceptional" : "BMF Approved"
                    //Type = f.Type == 0 ? "Grandfathered" : "Exceptional"
                    

                })
			};

		}
	}
}