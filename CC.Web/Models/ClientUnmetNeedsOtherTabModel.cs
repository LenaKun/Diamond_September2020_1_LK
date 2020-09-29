using CC.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Models
{
	public class NewUnmetNeedsOtherEntryModel: ModelBase
	{

		public int ClientId
		{
			get;
			set;
		}
		public NewUnmetNeedsOtherEntryModel() : base() { }

		[Display(Name = "Amount")]
		[Required]
		public decimal Amount
		{
			get;
			set;
		}

		[Display(Name = "Service Type")]
		public int? ServiceTypeId
		{
			get;
			set;
		}

		public string CUR
		{
			get;
			set;
		}

		public SelectList ServiceTypes { get; set; }

		internal void Insert(ccEntities db, CC.Data.Services.IPermissionsBase permissionsBase)
		{
			var c = db.Clients.Where(permissionsBase.ClientsFilter).SingleOrDefault(f => f.Id == this.ClientId);
			if (c == null)
			{
				throw new Exception("Client not found.");
			}
			else
			{

				db.UnmetNeedsOthers.AddObject(this.GetUnmetNeedsOther);
				try
				{
					db.SaveChanges();
				}
				catch (System.Data.UpdateException ex)
				{

					var msg = ex.InnerException.Message;
					if (msg.Contains("PK_UnmetNeedsOther"))
					{
						throw new Exception("Duplicate entry.", ex);
					}
					else if (msg.Contains("FK_UnmetNeedsOther_Clients"))
					{
						throw new Exception("Client not found.", ex);
					}
					else if (msg.Contains("CK_UnmetNeedsOther_Amount"))
					{
						throw new Exception("Invalid Amount Value. Must be >=0", ex);
					}
					else
					{
						throw;
					}
				}
			}
		}

		public UnmetNeedsOther GetUnmetNeedsOther
		{
			get
			{
				var gh = new UnmetNeedsOther { ClientId = this.ClientId, ServiceTypeId = this.ServiceTypeId.HasValue ? this.ServiceTypeId.Value : 0, Amount = this.Amount, CurrencyId = this.CUR };
				return gh;
			}
		}
	}

	public class ClientUnmetNeedsOtherTabDataModel : jQueryDataTableParamModel
	{
		public object GetResult(ccEntities db, CC.Data.Services.IPermissionsBase permissionsBase)
		{
			var source = from c in db.Clients.Where(permissionsBase.ClientsFilter)
						 where c.Id == this.ClientId
						 from item in c.UnmetNeedsOthers
						 where item.ClientId == this.ClientId
						 select new
						 {
							 ServiceTypeID = item.ServiceTypeId,
							 ServiceType = item.ServiceType.Name,
							 Amount = item.Amount,
							 CUR = item.CurrencyId
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
					ServiceTypeId = f.ServiceTypeID,
					ServiceType = f.ServiceType,
					Amount = f.Amount.Format(),
					CUR = f.CUR
				})
			};

		}
	}
}