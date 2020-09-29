using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{

	public class NewUnmetNeedsEntryModel : ModelBase
	{

		public int ClientId
		{
			get;
			set;
		}
		public NewUnmetNeedsEntryModel() : base() { }
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		[Display(Name = "Start Date")]
		[DateFormat]
		[Required]
		public DateTime WeeklyStartDate
		{
			get;
			set;
		}


		[Display(Name = "Weekly Hours")]
		[Required]
		public decimal WeeklyHours
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

				db.UnmetNeeds.AddObject(this.GetUnmetNeed);
				try
				{
					db.SaveChanges();
				}
				catch (System.Data.UpdateException ex)
				{

					var msg = ex.InnerException.Message;
					if (msg.Contains("PK_UnmetNeeds"))
					{
						throw new Exception("Duplicate entry.", ex);
					}
					else if (msg.Contains("FK_UnmetNeeds_Clients"))
					{
						throw new Exception("Client not found.", ex);
					}
					else if (msg.Contains("CK_UnmetNeeds_WeeklyHours"))
					{
						throw new Exception("Invalid Weekly Hours Value. Must be >=0 and <=168.", ex);
					}
					else
					{
						throw;
					}
				}
			}
		}

		public UnmetNeed GetUnmetNeed
		{
			get
			{
                this.WeeklyStartDate  = DateTime.Today;
				var gh = new UnmetNeed { ClientId = this.ClientId, StartDate = this.WeeklyStartDate, WeeklyHours = this.WeeklyHours };
                //var gh = new UnmetNeed { ClientId = this.ClientId, StartDate = this.s, WeeklyHours = this.WeeklyHours };
                return gh;
			}
		}
	}

	public class ClientUnmetNeedsTabDataModel : jQueryDataTableParamModel
	{
		public object GetResult(ccEntities db, CC.Data.Services.IPermissionsBase permissionsBase)
		{
			var source = from c in db.Clients.Where(permissionsBase.ClientsFilter)
						 where c.Id == this.ClientId
						 from item in c.UnmetNeeds1
						 where item.ClientId == this.ClientId
						 select new
						 {
							 StartDate = item.StartDate,
							 WeeklyHours = item.WeeklyHours
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
					WeeklyHours = f.WeeklyHours.ToString()
				})
			};

		}
	}
}
