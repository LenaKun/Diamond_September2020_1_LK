using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{

	public class NewGovHcEntryModel:ModelBase
	{
		
		public int ClientId
		{
			get;
			set;
		}
		public NewGovHcEntryModel() : base() { }
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		[Display(Name = "Start Date")]
		[DateFormat]
		[Required]
		public DateTime GovHcStartDate
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

		internal void Insert(ccEntities db, CC.Data.Services.IPermissionsBase permissionsBase)
		{
			var c = db.Clients.Where(permissionsBase.ClientsFilter).SingleOrDefault(f => f.Id == this.ClientId);
			if (c == null)
			{
				throw new Exception("Client not found.");
			}
			else
			{

				db.GovHcHours.AddObject(this.GetGovHcHOur);
				try
				{
					db.SaveChanges();
				}
				catch (System.Data.UpdateException ex)
				{

					var msg = ex.InnerException.Message;
					if (msg.Contains("PK_GovHcHours"))
					{
						throw new Exception("Duplicate entry.", ex);
					}
					else if (msg.Contains("FK_GovHcHours_Clients"))
					{
						throw new Exception("Client not found.", ex);
					}
					else if (msg.Contains("CK_GovHcHours_Value"))
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

		public GovHcHour GetGovHcHOur
		{
			get
			{
				var gh = new GovHcHour { ClientId = this.ClientId, StartDate = this.GovHcStartDate, Value = this.Value };
				return gh;
			}
		}
	}

	public class ClientGovHcHoursTabDataModel : jQueryDataTableParamModel
	{
		public object GetResult(ccEntities db, CC.Data.Services.IPermissionsBase permissionsBase)
		{
			var source = from c in db.Clients.Where(permissionsBase.ClientsFilter)
						 where c.Id == this.ClientId
						 from item in c.GovHcHours1
						 where item.ClientId == this.ClientId
						 select new
						 {
							 StartDate = item.StartDate,
							 Value = item.Value
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
					Value = f.Value.ToString()
				})
			};

		}
	}
}