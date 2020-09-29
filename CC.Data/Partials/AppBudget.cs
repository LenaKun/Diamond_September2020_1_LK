using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects;

namespace CC.Data
{
	[MetadataType(typeof(CC.Data.MetaData.AppBudgetMetaData))]
	public partial class AppBudget : IValidatableObject
	{
		public AppBudget()
		{
			this.ApprovalStatus = AppBudgetApprovalStatuses.New;
		}


		#region misc

		public bool IsRequiredMatchOk()
		{
			if (this.App == null)
			{
				return false;
			}
			else if (this.App.RequiredMatch > 0)
			{
				var actual = this.AppBudgetServices.Sum(f => f.RequiredMatch);
				return actual >= this.App.RequiredMatch; //WAS == corrected
			}
			else
			{
				return true;
			}
		}


		public virtual string StatusName
		{
			get
			{
				return this.ApprovalStatus.ToString().SplitCapitalizedWords();
			}
		}


		public bool IsConditional()
		{
			return this.ValidUntill.HasValue;
		}

		public IEnumerable<string> AdditionalStatuses
		{
			get
			{
				if (this.IsConditional())
					yield return "Conditional";
				if (this.Revised)
					yield return " *Revised";
			}
		}

		[UIHint("Enum")]
		public AppBudgetApprovalStatuses ApprovalStatus { get { return (AppBudgetApprovalStatuses)this.StatusId; } set { this.StatusId = (int)value; } }

		public string StatusDisplay { get { return this.ApprovalStatus.ToString() + " (" + string.Join(", ", this.AdditionalStatuses) + ")"; } }

		public Func<AppBudget, bool> CanSubmitReport
		{
			get
			{

				Func<AppBudget, bool> f = (a) =>
				{
					return (a.ApprovalStatus == AppBudgetApprovalStatuses.Approved);
					//&& fund period is valid
					//&& conditional approved untill is valid)

				};


				return f;
			}
		}

		public IEnumerable<KeyValuePair<int?, string>> ApprovalStatuses
		{
			get
			{
				return EnumExtensions.EnumToDictionary<AppBudgetApprovalStatuses>();
			}
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var instance = validationContext.ObjectInstance as AppBudget;
			
			if (instance.ValidUntill.HasValue)
			{
				if (instance.ValidUntill.Value == instance.App.StartDate || instance.ValidUntill.Value == instance.App.EndDate)
				{
					yield return new ValidationResult("Valid Until date can not be equal to app start date or app end date");
				}
				else if (instance.ValidUntill.Value <= instance.App.StartDate || instance.App.EndDate <= instance.ValidUntill.Value)
				{
					var message = string.Format("Valid Until date must be between app start date ({0}) and app end date ({1}).", instance.App.StartDate, instance.App.EndDate);
					yield return new ValidationResult(message, new string[] { instance.GetPropertyInfo((AppBudget f) => f.ValidUntill).Name });
				}
			}

			if (this.Id != default(int) && AppBudget.EditableStatuses.Contains((AppBudgetApprovalStatuses) this.StatusId))
			{
				//validate total cc grant
				using (var db = new ccEntities())
				{
					var ccGrant = db.AppBudgetServices.Where(f => f.AppBudgetId == this.Id).Sum(f => (decimal?)f.CcGrant);
                    if (ccGrant.HasValue)
                    {
                        var appgrant = db.AppBudgets.Where(f => f.Id == this.Id).Select(f => f.App.CcGrant).SingleOrDefault();
												if (CCDecimals.Truncate(ccGrant - appgrant, CCDecimals.CompareDigits) > 0)
                        {
                            yield return new ValidationResult("Total CC Grant requested exceeded CC Grant for this budget.");
                        }
                    }
				}				

				using (var db = new ccEntities())
				{
                    var appbs = db.AppBudgetServices.Where(f => f.AppBudgetId == this.Id);

                    var totalAmount = appbs.Sum(f => (decimal?)f.CcGrant);

                    var excHours = from c in Queries.GetServiceConstraints(db, this.AppId)
                            join s in db.Services on c.ServiceId equals s.Id
                            where s.ExceptionalHomeCareHours
                            join a in
                                (from appbudgetService in appbs
                                 group appbudgetService by appbudgetService.ServiceId into appBudgetServiceGroup
                                 select new
                                 {
                                     ServiceTypeId = appBudgetServiceGroup.Key,
                                     Amount = (decimal?)appBudgetServiceGroup.Sum(f => f.CcGrant) ?? 0
                                 }) on c.ServiceId equals a.ServiceTypeId into ag
                            from a in ag.DefaultIfEmpty()
                            select new
                            {
                                ServiceId = c.ServiceId,
                                Name = s.Name,
                                Max = c.MaxExpPercentage,
                                Min = c.MinExpPercentage,
                                Amount = (decimal?)a.Amount ?? 0
                            };
                    foreach (var item in excHours.Where(f => EntityFunctions.Truncate(f.Amount - totalAmount * f.Max, CCDecimals.CompareDigits) > 0))
                    {
                        yield return new ValidationResult(string.Format("CC Grant requested for \"{0}\" service exceeded maximum {1} of total CC Grant for this budget.",
                            item.Name,
                            (item.Max ?? 0).FormatPercentage()));
                    }

                    var appbudgetServices = from ab in db.AppBudgetServices
                                            where ab.AppBudgetId == this.Id
                                            select ab;

                    var appBudget = db.AppBudgets.Where(f => f.Id == this.Id).Single();

                    //get the total sum of the services
                    var model = (from ab in appbudgetServices
                                 group ab by ab.AppBudgetId into abg
                                 select new
                                 {
                                     AppBudgetId = abg.Key,
                                     CcGrant = (decimal?)abg.Sum(f => f.CcGrant) ?? 0,
                                     RequiredMatch = (decimal?)abg.Sum(a => a.RequiredMatch) ?? 0,
                                     AgencyContribution = (decimal?)abg.Sum(a => a.AgencyContribution) ?? 0,

                                 }).Single();

                    //total amount from financial repoprts

                    var HcCcGrant = (appbudgetServices.Where(f => f.Service.ServiceType.Name == "HomeCare").Sum(f => (decimal?)f.CcGrant) ?? 0);
                    var AdminCcGrant = (appbudgetServices.Where(f => f.Service.ServiceType.Name == "Administrative Overhead").Sum(f => (decimal?)f.CcGrant) ?? 0);
                    var OtherGrant = (appbudgetServices.Where(f => f.Service.ServiceType.Name != "HomeCare" && f.Service.ServiceType.Name != "Administrative Overhead").Sum(f => (decimal?)f.CcGrant) ?? 0);

                    var AdminPercentage = model.CcGrant != 0 ? AdminCcGrant / model.CcGrant : 0;
                    var HcPercentage = model.CcGrant != 0 ? HcCcGrant / model.CcGrant : 0;
                    var OtherPercentage = OtherGrant > 0 ? 1 - HcPercentage - AdminPercentage : 0;

                    var q = (from a in db.Apps.Where(f => f.Id == appBudget.AppId)
                             join fund in db.Funds on a.FundId equals fund.Id
                             select new
                             {
                                 OtherServicesMax = (a.OtherServicesMax ?? fund.OtherServicesMax ?? 100) / 100,
                                 HomecareMin = (a.HomecareMin ?? fund.HomecareMin ?? 0) / 100,
                                 AdminMax = (a.AdminMax ?? fund.AdminMax ?? 100) / 100
                             }).Single();

                    if(AdminPercentage > q.AdminMax)
                    {
                        yield return new ValidationResult(string.Format("CC Grant requested for \"Administrative Overhead\" services exceeded maximum {0} of total CC Grant for this budget.",
                            (q.AdminMax).FormatPercentage()));
                    }
                    if(HcPercentage < q.HomecareMin)
                    {
                        yield return new ValidationResult(string.Format("\"Homecare\" minimum percentage ({0}) is not met", (q.HomecareMin).FormatPercentage()));
                    }
                    if(OtherPercentage > q.OtherServicesMax)
                    {
                        yield return new ValidationResult(string.Format("CC Grant requested for \"Other\" services exceeded maximum {0} of total CC Grant for this budget.",
                            (q.OtherServicesMax).FormatPercentage()));
                    }


                    if (instance.Revised)
                    {
                        var ytdYear = DateTime.Now.Year;
                        var servicetotals = (from a in
                                                 (from item in db.viewSubreportAmounts
                                                  join innerabs in db.AppBudgetServices.Where(f => f.AppBudgetId == this.Id) on item.AppBudgetServiceId equals innerabs.Id
                                                  join mr in db.MainReports.Where(f => f.StatusId == (int)MainReport.Statuses.Approved) on item.MainReportId equals mr.Id
                                                  where mr.Start.Year == ytdYear
                                                  select new
                                                  {
                                                      ServiceId = innerabs.ServiceId,
                                                      Amount = item.Amount
                                                  })
                                             group a by a.ServiceId into g
                                             select new
                                             {
                                                 Id = g.Key,
                                                 Amount = g.Sum(f => f.Amount),
                                             }).ToList();
                        var budgetTotalReported = servicetotals != null ? servicetotals.Sum(f => f.Amount) : 0;
                        var budgetTotal = instance.AppBudgetServices.Sum(f => f.CcGrant);
                        if (budgetTotal < budgetTotalReported)
                        {
                            yield return new ValidationResult("Total Budget must be equal to or greater than total Reported amount.");
                        }
                    }

					//AppId can be equal to 0, for example - when updating a budget status
					if (instance.AppId != default(int))
					{
						var app = db.Apps.FirstOrDefault(f => f.Id == instance.AppId);
						if (app.MaxHcCaseManagementPersonnel.HasValue)
						{
							const int hcCaseManagementPersonnelServiceId = 442;
							var x = ValidateServiceCaps(appbudgetServices
									, new[] { hcCaseManagementPersonnelServiceId }
									, app.MaxHcCaseManagementPersonnel.Value
									, "Agency {0} exceeded Cap for Homecare - Case Management Personnel.");
							foreach (var item in x) { yield return item; }
						}
						if (app.MaxServicesPersonnelOther.HasValue)
						{
							var serviceids = new[] { 373, 433 };
							var x = ValidateServiceCaps(appbudgetServices
									, serviceids
                                    , app.MaxServicesPersonnelOther.Value
                                    //, app.MaxHcCaseManagementPersonnel.Value
                                    , "Agency {0} exceeded Cap for Other Services - Personnel - Other / Other Services - Personnel - Other with names.");
							foreach (var item in x) { yield return item; }
						}
					}
				}
			}
		}
		

		private IEnumerable<ValidationResult> ValidateServiceCaps(IQueryable<AppBudgetService> appBudgetServices, int[] serviceIds, decimal cap, string msg)
		{
			var y = from a in appBudgetServices
					 where serviceIds.Contains(a.ServiceId )
					 group a by a.Agency.Name into g
					 select new
					 {
						 AgencyName = g.Key,
						 Amount = g.Sum(f => f.CcGrant)
					 } into ag
					 where ag.Amount > cap
					 select ag;
			foreach(var item in y)
			{
				var resultMessage = string.Format(msg, item.AgencyName, item.Amount, cap);
				yield return new ValidationResult(resultMessage);
			}
		}
		public static int? ValidateInterlineTranfer(int id)
		{
			using (var db = new ccEntities())
			{
				var prev = from a in db.ApprovedAppBudgetServices
						   where a.AppBudgetId == id
						   join s in db.Services on a.ServiceId equals s.Id
						   group a by s.ServiceLevel into ag
						   select new
						   {
							   ServiceLevel = ag.Key,
							   CcGrant = ag.Sum(f => f.CcGrant)
						   };
				var current = from a in db.AppBudgetServices
							  where a.AppBudgetId == id
							  join s in db.Services on a.ServiceId equals s.Id
							  group a by s.ServiceLevel into ag
							  select new
							  {
								  ServiceLevel = ag.Key,
								  CcGrant = ag.Sum(f => f.CcGrant)
							  };
				var change = (from level in Enumerable.Range(1, 3)
							  join o in prev.ToList() on level equals o.ServiceLevel into og
							  from o in og.DefaultIfEmpty()
							  join n in current.ToList() on level equals n.ServiceLevel into ng
							  from n in ng.DefaultIfEmpty()
							  select new
							  {
								  level = level,
								  o = o == null ? 0 : o.CcGrant,
								  n = n == null ? 0 : n.CcGrant
							  }).Select(f => new { l = f.level, o = f.o, n = f.n, c = f.n - f.o });

				var shift = 0M;
				foreach (var item in change.OrderBy(f => f.l))
				{
					if (item.c < 0)
					{
						shift -= item.c;
					}
					else if (item.c > 0 && item.c > shift)
					{
						return item.l;
					}
				}
			}
			return null;
		}

		public IEnumerable<ValidationResult> GetWarnings(ValidationContext validationContext)
		{
			var instance = validationContext.ObjectInstance as AppBudget;
			if (instance.App != null && instance.AppBudgetServices != null)
			{
				if (CCDecimals.Truncate(instance.App.CcGrant - instance.AppBudgetServices.Sum(f => f.CcGrant), CCDecimals.CompareDigits) < 0)
				{
					yield return new ValidationResult("\"Total Grant as specified by agency\" must be equal-or-lower than \"Total CC Grant\"");
				}
				if (instance.App.RequiredMatch > 0)
				{
					if (CCDecimals.Truncate(instance.App.RequiredMatch - instance.AppBudgetServices.Sum(f => f.RequiredMatch), CCDecimals.CompareDigits) > 0)
					{
						yield return new ValidationResult("\"Total Required Match as specified by agency\" must be equal-or-greater than \"Total Required match\"");
					}
				}
				if (instance.App.AgencyContribution)
				{
					if (instance.AppBudgetServices.Sum(f => f.AgencyContribution) <= 0)
					{
						yield return new ValidationResult("\"The total Agency’s Contribution is 0, Are you sure?");
					}

				}
				if (instance.App.InterlineTransfer && instance.Revised)
				{
					if (ValidateInterlineTranfer(instance.Id) != null)
					{
						yield return new ValidationResult("Warning: This budget content does not meet validation requirements for automatic approval. After you hit submit the budget will go to the Program Officer for review.");
					}
				}
                if(instance.Revised)
                {
                    using (var db = new ccEntities())
                    {
                        var ytdYear = DateTime.Now.Year;
                        var servicetotals = (from a in
                                                   (from item in db.viewSubreportAmounts
                                                    join innerabs in db.AppBudgetServices.Where(f => f.AppBudgetId == this.Id) on item.AppBudgetServiceId equals innerabs.Id
                                                    join mr in db.MainReports.Where(f => f.StatusId == (int)MainReport.Statuses.Approved) on item.MainReportId equals mr.Id
                                                    where mr.Start.Year == ytdYear
                                                    select new
                                                    {
                                                        ServiceId = innerabs.ServiceId,
                                                        AgencyId = innerabs.AgencyId,
                                                        Amount = item.Amount                                                        
                                                    })
                                               group a by new {a.ServiceId, a.AgencyId} into g
                                               select new
                                               {
                                                   ServiceId = g.Key.ServiceId,
                                                   AgencyId = g.Key.AgencyId,
                                                   Amount = g.Sum(f => f.Amount),
                                               });
                        var q = (from abs in instance.AppBudgetServices
                                 join abst in servicetotals on new { abs.ServiceId, abs.AgencyId } equals new { abst.ServiceId, abst.AgencyId }
                                 where abs.CcGrant < abst.Amount
                                 select new
                                 {
                                     ServiceId = abst.ServiceId,
                                     Amount = abst.Amount,
                                     ServiceName = abs.Service.Name
                                 }).Distinct().OrderBy(f => f.ServiceId).ToList();

                        if(q.Any())
                        {
                            string error = "The revised Budget for service(s) ";
                            foreach(var item in q)
                            {
                                error += item.ServiceName;
                                if(q.Last().ServiceId != item.ServiceId)
                                {
                                    error += ", ";
                                }
                            }
                            error += " is less than the amount already reported for the year.  Budgeted amounts must be equal to or greater than the reported amount for each service.";
                            yield return new ValidationResult(error);
                        }
                    }
                }
			}
		}

		#endregion


		#region Statics

		public static IEnumerable<AppBudgetApprovalStatuses> ValidStatuses { get { return new List<AppBudgetApprovalStatuses>() { AppBudgetApprovalStatuses.Approved }; } }
		public static Func<AppBudget, bool> IsApproved { get { return f => f.StatusId == (int)AppBudgetApprovalStatuses.Approved; } }
		public static IEnumerable<AppBudgetApprovalStatuses> EditableStatuses
		{
			get
			{
				return new[] { AppBudgetApprovalStatuses.New, AppBudgetApprovalStatuses.Rejected, AppBudgetApprovalStatuses.ReturnedToAgency };
			}
		}
		public static IEnumerable<int> EditableStatusIds { get { return EditableStatuses.Select(f => (int)f); } }
		public static bool IsEditable(AppBudget f)
		{
			return f.StatusId == (int)AppBudgetApprovalStatuses.New
			|| f.StatusId == (int)AppBudgetApprovalStatuses.Rejected
			|| f.StatusId == (int)AppBudgetApprovalStatuses.ReturnedToAgency;
		}

		#endregion
	}
}
