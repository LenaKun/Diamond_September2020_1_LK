using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Data.Objects.SqlClient;
using CC.Data;

namespace CC.Web.Models
{
	/// <summary>
	/// A class that contains values of a data table filter for the clients list
	/// </summary>
	public class ClientsListDataTableModel : jQueryDataTableParamsWithData<ClientsListEntry>
	{


		#region Filter Values
		public int? AgencyGroupId { get; set; }
		public int? AgencyId { get; set; }
        public int? RegionId { get; set; }
		public int? ApprovalStatusId { get; set; }
		public int? Id { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }

		string _nationalId;

		public string NationalId
		{
			get { return _nationalId; }
			set { _nationalId = value == null ? null : value.Trim(); }
		}

		public bool? Active { get; set; }

		public bool AustrianEligible { get; set; }
		public bool RomanianEligible { get; set; }

		public bool SC { get; set; }
		public bool DCC { get; set; }

        public bool GGReportedOnly { get; set; }

		public DateTime? CreateDateFrom { get; set; }
		public DateTime? CreateDateTo { get; set; }

		public string ExportOption { get; set; }

		public bool HcWaitlistOnly { get; set; }
        
        public bool OtherWaitlistOnly { get; set; }

		#endregion


		public static IQueryable<CC.Data.Client> GetClients(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
		{
			var filtered = db.Clients.Where(permissions.ClientsFilter);

			if (param.AgencyId.HasValue)
			{
				filtered = filtered.Where(f => f.AgencyId == param.AgencyId.Value);
			}
			if (param.AgencyGroupId.HasValue)
			{
				filtered = filtered.Where(f => f.Agency.GroupId == param.AgencyGroupId.Value);
			}
            if (param.RegionId.HasValue)
            {
                filtered = filtered.Where(f => f.Agency.AgencyGroup.Country.RegionId == param.RegionId.Value);
            }
			if (param.ApprovalStatusId.HasValue)
			{
				filtered = filtered.Where(f => f.ApprovalStatusId == param.ApprovalStatusId);
			}
			if (!string.IsNullOrWhiteSpace(param.FirstName))
			{
				filtered = filtered.Where(f => f.FirstName.Contains(param.FirstName.Trim()));
			}
			if (!string.IsNullOrWhiteSpace(param.LastName))
			{
				filtered = filtered.Where(f => f.LastName.Contains(param.LastName.Trim()));
			}
			if (param.Id.HasValue)
			{
				filtered = filtered.Where(f => f.Id == param.Id);
			}
			if (!string.IsNullOrWhiteSpace(param.NationalId))
			{
				filtered = filtered.Where(f => f.NationalId == param.NationalId);
			}
			if (param.Active.HasValue)
			{
				if (param.Active.Value)
				{
					filtered = filtered.Where(c => (c.DeceasedDate == null && c.LeaveDate == null && c.LeaveReasonId == null));
				}
				else
				{
					filtered = filtered.Where(c => !(c.DeceasedDate == null && c.LeaveDate == null && c.LeaveReasonId == null));
				}
			}
			if (param.AustrianEligible && permissions.User.RoleId != 128)
			{
				filtered = filtered.Where(f => f.AustrianEligible == true);
			}

			if (param.RomanianEligible && permissions.User.RoleId != 128)
			{
				filtered = filtered.Where(f => f.RomanianEligible == true);
			}
			if (param.SC)
			{
				filtered = filtered.Where(f => f.SC_Client == true);
			}
			if (param.DCC)
			{
				filtered = filtered.Where(f => f.DCC_Client == true);
			}
            if (param.GGReportedOnly)
            {
                filtered = filtered.Where(f => f.GGReportedCount > 0);
            }
			if (param.CreateDateFrom.HasValue)
			{
				filtered = filtered.Where(f => f.CreatedAt >= param.CreateDateFrom);
			}
			if (param.CreateDateTo.HasValue)
			{
				filtered = filtered.Where(f => f.CreatedAt <= param.CreateDateTo);
			}
			if(param.HcWaitlistOnly)
			{
				filtered = filtered.Where(f => f.HomecareWaitlist);
			}
			if(param.OtherWaitlistOnly)
			{
				filtered = filtered.Where(f => f.OtherServicesWaitlist);
			}
            //if (permissions.User.RoleId != 128)
           // {
                return filtered;
           // }
           // else
           // {
           // return filtered.Select(c => GetClients
           // {
                //ORGID = c.MasterId,
                //Agency = c.Agency.Name,
             //   FirstName = c.FirstName,
             //   LastName = c.LastName
               // M/dleName = c.MiddleName,
               // OtherFirstName = c.OtherFirstName,
               // OtherLastName = c.OtherLastName,
               // PreviousFirstName = c.PreviousFirstName,
               // PreviousLastName = c.PreviousLastName,
             //   Address = c.Address,
              //  City = c.City,
              //  ZIP = c.ZIP,
                //State = c.State,
               // CountryName = c.CountryName,
              //  BirthDate = c.BirthDate,
               // Otherdateofbirth = c.Otherdateofbirth,
                //BirthCity = c.BirthCity,
               // BirthCountry = c.BirthCountry,
               // IDType = c.IDType,
               // OtherIDcard = c.OtherIDcard,
               // OtherIDtype = c.OtherIDtype,
                //GovernmentIssuedID = c.GovernmentIssuedID,
              //  DeceasedDate = c.DeceasedDate,
              //  LeaveDate = c.LeaveDate,
              //  JoinDate = c.JoinDate,
               // LeaveReason = c.LeaveReason,
              //  Remarks = c.Remarks,
              //  IncomeCriteriaComplied = c.IncomeCriteriaComplied,
               // Gender = c.Gender
         //   });

           // }
          }


           // }
            
		//}

       
        public static IQueryable<ClientsListEntry> GetClientsList(CC.Data.Services.IPermissionsBase permissions, CC.Data.ccEntities db, ClientsListDataTableModel param)
		{
			var q = from c in GetClients(db, permissions, param)
					let curfl = c.FunctionalityScores.OrderByDescending(s => s.StartDate).Where(s => s.StartDate < DateTime.Now).Select(s => s.FunctionalityLevel).FirstOrDefault()
					let CurGovHcHours = c.GovHcHours1.Where(a => a.StartDate < DateTime.Now).OrderByDescending(a => a.StartDate).Select(a => (decimal?)a.Value).FirstOrDefault()
					let CurUnmetNeeds = c.UnmetNeeds1.Where(a => a.StartDate < DateTime.Now).OrderByDescending(a => a.StartDate).FirstOrDefault()
					let CurGFHours = c.GrandfatherHours.Where(a => a.StartDate < DateTime.Now).OrderByDescending(a => a.StartDate).Select(a => (decimal?)a.Value).FirstOrDefault()
                    let BirthCountry = c.BirthCountryId == null ? null : db.BirthCountries.Where(p => p.Id == c.BirthCountryId).FirstOrDefault()

                    let CurHcCap = db.HcCapsTableRaws
								.Where(cap => cap.ClientId == c.Id)
								.Where(cap => cap.StartDate <= DateTime.Now)
								.Where(cap => cap.EndDate == null || cap.EndDate > DateTime.Now)
								.OrderByDescending(cap => cap.StartDate)
								.Select(cap => cap.HcCap)
								.FirstOrDefault()
					select new ClientsListEntry
					{
						FirstName = c.FirstName,
						LastName = c.LastName,
						Id = c.Id,
						NationalId = c.NationalId,
						JoinDate = c.JoinDate,
						Address = c.Address,
						City = c.City,
						Phone = c.Phone,
                        BirthCountry = BirthCountry.Name,
                        BirthDate = c.BirthDate,

						ApprovalStatusId = c.ApprovalStatusId,
						AgencyId = c.AgencyId,
						AgencyGroupId = c.Agency.AgencyGroup.Id,

						AgencyName = c.Agency.Name,
						AgencyGroupName = c.Agency.AgencyGroup.DisplayName,
						AgencyRegionName = c.Agency.AgencyGroup.Country.Region.Name,

						ApprovalStatusName = c.ApprovalStatus.Name,
						GfHours = CurGFHours,

						FunctionalityLevelName = curfl.Name,
						CurFuncHcHours = (int?)curfl.HcHoursLimit,
						SubstractGovHcHours = (bool?)curfl.SubstractGovHours,
						CurGovHcHours = CurGovHcHours,
						AllowedHcHours = CurHcCap,


						InternalId = c.InternalId,
						MasterId = c.MasterId,
						UploadDate = c.CreatedAt,
						Active = c.DeceasedDate == null && c.LeaveDate == null && c.LeaveReasonId == null,
						AustrianEligible = c.AustrianEligible,
						RomanianEligible = c.RomanianEligible,
                        GGReportedOnly = c.GGReportedCount != null && c.GGReportedCount > 0,
						UnmetNeedsStartDate=CurUnmetNeeds.StartDate,
						UnmetNeedsValue = CurUnmetNeeds.WeeklyHours
					};



			return q;
		}
		public static IEnumerable<BegExportModel> GetBegExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
		{
			var filtered = GetClients(db, permissions, param);
			return filtered.Select(c => new BegExportModel
			{
				Id = c.Id,
				AgencyId = c.AgencyId,
				AgencyName = c.Agency.Name,
				FundStatusName = c.FundStatus.Name,
				ApprovalStatusName = c.ApprovalStatus.Name,
				CompensationProgramName = c.CompensationProgramName,
				AddCompName = c.AddCompName,
				AddCompRegNum = c.AddCompId,
				LastName = c.LastName,
				FirstName = c.FirstName,
				PrevLastName = c.PrevLastName,
				PrevFirstName = c.PrevFirstName,
				OtherLastName = c.OtherLastName,
				PobCity = c.PobCity,
				BirthCountryName = c.BirthCountry.Name,
				BirthDate = c.BirthDate,
				OtherBirthDate = c.OtherDob,
				Address = c.Address,
				City = c.City,
				Zip = c.ZIP,
				StateCode = c.State.Code,
				CountryCode = c.Country.Code,
			});
		}
		public static IEnumerable<ClientsExportModel> GetExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
		{
			var now = DateTime.Now;
            if (permissions.User.RoleId != 128) //not BMF
            {
                var filtered = from a in GetClients(db, permissions, param)
                               join c in db.viewClientDetails on a.Id equals c.CCID
                               //join  d in db.HcCapsTableRaws on c.CCID equals d.ClientId
                               // where d.EndDate ==null
                               let CurHcCap = db.HcCapsTableRaws
                                     .Where(cap => cap.ClientId == c.CCID)
                                     .Where(cap => cap.StartDate <= DateTime.Now)
                                     .Where(cap => cap.EndDate == null || cap.EndDate > DateTime.Now)
                                     .OrderByDescending(cap => cap.StartDate)
                                     .Select(cap => cap.HcCap)
                                     .FirstOrDefault()

                              // let DAFId = db.Dafs
                                //   .Where(dafid => dafid.ClientId == c.CCID)
                                  // .Select(dafid => dafid.Id)
                                  // .FirstOrDefault()

                               select new ClientsExportModel
                               {
                                   InternalAgencyID = c.InternalAgencyID,
                                   ORGID = c.ORGID,
                                   Agency = c.Agency,
                                   FirstName = c.FirstName,
                                   LastName = c.LastName,
                                   MiddleName = c.MiddleName,
                                   OtherFirstName = c.OtherFirstName,
                                   OtherLastName = c.OtherLastName,
                                   PreviousFirstName = c.PreviousFirstName,
                                   PreviousLastName = c.PreviousLastName,
                                   Address = c.Address,
                                   City = c.City,
                                   ZIP = c.ZIP,
                                   State = c.State,
                                   CountryName = c.CountryName,
                                   BirthDate = c.BirthDate,
                                   Otherdateofbirth = c.Otherdateofbirth,
                                   BirthCity = c.BirthCity,
                                   BirthCountry = c.BirthCountry,
                                   IDType = c.IDType,
                                   OtherIDcard = c.OtherIDcard,
                                   OtherIDtype = c.OtherIDtype,
                                   GovernmentIssuedID = c.GovernmentIssuedID,
                                   DeceasedDate = c.DeceasedDate,
                                   LeaveDate = c.LeaveDate,
                                   JoinDate = c.JoinDate,
                                   LeaveReason = c.LeaveReason,
                                   Remarks = c.Remarks,
                                   IncomeCriteriaComplied = c.IncomeCriteriaComplied,
                                   Gender = c.Gender == (int)Client.Genders.Female ? "Female" : c.Gender == (int)Client.Genders.Male ? "Male" : "",
                                   CCID = c.CCID,
                                   MasterId = c.MasterId,
                                   CreateDate = c.CreatedAt,
                                   FunctionalityLevelName = c.FunctionalityLevelName,
                                   //HCHours = d.HcCap,
                                   //HCHours = c.HcHours,
                                   HCHours = CurHcCap == null ? 0 : CurHcCap,
                                   GrandfatheredHours = c.GrandfatheredHours,
                                   GFStartDate = c.GFStartDate,
                                   GFType = c.GFType,
                                   ApprovalStatus = c.ApprovalStatus,
                                   NaziPersecutionDetails = c.NaziPersecutionDetails,
                                   CompensationProgram = c.CompensationProgram,
                                   HomecareEligibilityStartDate = c.HomecareEligibilityStartDate,
                                   //inclusive end date is desired here
                                   HomecareEligibilityEndDate = c.HomecareEligibilityEndDate,
                                   GovtHChours = c.GovtHChours,
                                   GovtHChoursStartDate = c.GovtHChoursStartDate,
                                   DiagnosticScore = c.DiagnosticScore,
                                   HighestStartDateofDiagnosticScore = c.HighestStartDateofDiagnosticScore,
                                   Deceased = (c.DeceasedDate != null) ? true : false,
                                   Otheraddress = c.Otheraddress,
                                   PreviousaddressinIsrael = c.PreviousaddressinIsrael,
                                   Phone = c.Phone,
                                   AustrianEligible = c.AustrianEligible,
                                   RomanianEligible = c.RomanianEligible,
                                   GGReportedOnly = c.GGReportedCount != null && c.GGReportedCount > 0 ? "Yes" : "No",
                                   UnmetNeedsStartDate = c.UnmetNeedsStartDate,
                                   UnmetNeedsValue = c.UnmetNeedsValue,
                                   HomecareApprovalStatusName = c.HomecareApprovalStatusName,
                                   AppearedAtLeastOnce = c.AppearedAtLeastOnce == true ? "Yes" : "No",
                                   IsCeefRecipient = c.IsCeefRecipient ? "Yes" : "No",
                                   CeefId = c.CeefId,
                                   HomecareWaitlist = c.HomecareWaitlist ? "Yes" : "No",
                                   UnableToSign = c.UnableToSign ? "Yes" : "No",
                                   NursingHome = c.NursingHome ? "Yes" : "No",
                                   AssistedLiving = c.AssistedLiving ? "Yes" : "No",
                                   OtherServicesWaitlist = c.OtherServicesWaitlist ? "Yes" : "No",
                                   MAFDate = c.MAFDate,
                                   MAF105Date = c.MAF105Date,
                                //   HAS2Date = c.Has2Date,
                                   HomeCareEntitled = c.HomecareEligibilityEndDate == null || c.HomecareEligibilityEndDate > now ? "Yes" : "No",
                                   CommPrefs = c.CommPrefs,
                                   CareReceivedVia = c.CareReceivedVia
                                  // DAFID = DAFId == null ? 0 : DAFId

                               };

                return filtered;
            }

            else
            {
                var filtered = from a in GetClients(db, permissions, param)
                               join c in db.viewClientDetails on a.Id equals c.CCID
                               //join  d in db.HcCapsTableRaws on c.CCID equals d.ClientId
                               // where d.EndDate ==null
                               let CurHcCap = db.HcCapsTableRaws
                                     .Where(cap => cap.ClientId == c.CCID)
                                     .Where(cap => cap.StartDate <= DateTime.Now)
                                     .Where(cap => cap.EndDate == null || cap.EndDate > DateTime.Now)
                                     .OrderByDescending(cap => cap.StartDate)
                                     .Select(cap => cap.HcCap)
                                     .FirstOrDefault()

                               select new ClientsExportModel
                               {
                                   InternalAgencyID = c.InternalAgencyID,
                                   ORGID = c.ORGID,
                                   Agency = c.Agency,
                                   FirstName = c.FirstName,
                                   LastName = c.LastName,
                                   MiddleName = c.MiddleName,
                                   OtherFirstName = c.OtherFirstName,
                                   OtherLastName = c.OtherLastName,
                                   PreviousFirstName = c.PreviousFirstName,
                                   PreviousLastName = c.PreviousLastName,
                                   Address = c.Address,
                                   City = c.City,
                                   ZIP = c.ZIP,
                                   State = c.State,
                                   CountryName = c.CountryName,
                                   BirthDate = c.BirthDate,
                                   Otherdateofbirth = c.Otherdateofbirth,
                                   BirthCity = c.BirthCity,
                                   BirthCountry = c.BirthCountry,
                                   IDType = c.IDType,
                                   OtherIDcard = c.OtherIDcard,
                                   OtherIDtype = c.OtherIDtype,
                                   GovernmentIssuedID = c.GovernmentIssuedID,
                                   DeceasedDate = c.DeceasedDate,
                                   LeaveDate = c.LeaveDate,
                                   JoinDate = c.JoinDate,
                                   LeaveReason = c.LeaveReason,
                                   Remarks = c.Remarks,
                                   IncomeCriteriaComplied = c.IncomeCriteriaComplied,
                                   Gender = c.Gender == (int)Client.Genders.Female ? "Female" : c.Gender == (int)Client.Genders.Male ? "Male" : "",
                                   CCID = c.CCID,
                                  // MasterId = c.MasterId,
                                   CreateDate = c.CreatedAt,
                                   FunctionalityLevelName = c.FunctionalityLevelName,
                                   //HCHours = d.HcCap,
                                   //HCHours = c.HcHours,
                                   HCHours = CurHcCap == null ? 0 : CurHcCap,
                                   GrandfatheredHours = c.GrandfatheredHours,
                                   GFStartDate = c.GFStartDate,
                                   GFType = c.GFType,
                                   ApprovalStatus = c.ApprovalStatus,
                                   NaziPersecutionDetails = c.NaziPersecutionDetails,
                                   CompensationProgram = c.CompensationProgram,
                                   HomecareEligibilityStartDate = c.HomecareEligibilityStartDate,
                                   //inclusive end date is desired here
                                   HomecareEligibilityEndDate = c.HomecareEligibilityEndDate,
                                   GovtHChours = c.GovtHChours,
                                   GovtHChoursStartDate = c.GovtHChoursStartDate,
                                   DiagnosticScore = c.DiagnosticScore,
                                   HighestStartDateofDiagnosticScore = c.HighestStartDateofDiagnosticScore,
                                   Deceased = (c.DeceasedDate != null) ? true : false,
                                   Otheraddress = c.Otheraddress,
                                   PreviousaddressinIsrael = c.PreviousaddressinIsrael,
                                   Phone = c.Phone,
                                   AustrianEligible = c.AustrianEligible,
                                   RomanianEligible = c.RomanianEligible,
                                   GGReportedOnly = c.GGReportedCount != null && c.GGReportedCount > 0 ? "Yes" : "No",
                                   UnmetNeedsStartDate = c.UnmetNeedsStartDate,
                                   UnmetNeedsValue = c.UnmetNeedsValue,
                                   HomecareApprovalStatusName = c.HomecareApprovalStatusName,
                                   AppearedAtLeastOnce = c.AppearedAtLeastOnce == true ? "Yes" : "No",
                                   IsCeefRecipient = c.IsCeefRecipient ? "Yes" : "No",
                                   CeefId = c.CeefId,
                                   HomecareWaitlist = c.HomecareWaitlist ? "Yes" : "No",
                                   UnableToSign = c.UnableToSign ? "Yes" : "No",
                                   NursingHome = c.NursingHome ? "Yes" : "No",
                                   AssistedLiving = c.AssistedLiving ? "Yes" : "No",
                                   OtherServicesWaitlist = c.OtherServicesWaitlist ? "Yes" : "No",
                                   MAFDate = c.MAFDate,
                                   MAF105Date = c.MAF105Date,
                                //   HAS2Date = c.Has2Date,
                                   HomeCareEntitled = c.HomecareEligibilityEndDate == null || c.HomecareEligibilityEndDate > now ? "Yes" : "No",
                                   CommPrefs = c.CommPrefs,
                                   CareReceivedVia = c.CareReceivedVia

                               };

                return filtered;

            }
           
		}
		public static IEnumerable<DuplicateExportModel> GetDuplicateExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
		{
			var clients = GetClients(db, permissions, param);
			var filtered = clients.Where(f => f.MasterId != null).Select(c => new { c.MasterId });
			var result = (from c in clients
						  join f in filtered on (c.MasterIdClcd) equals f.MasterId
						  select new DuplicateExportModel
						  {
							  Id = c.Id,
							  AgencyName = c.Agency.Name,
							  MasterId = c.MasterId != null ? c.MasterId : c.Id
						  }).Distinct();
			return result;
		}

        public static IEnumerable<EligibilityPeriodsExportModel> GetEligibilityPeriodsExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
        {
            var filtered = (from c in GetClients(db, permissions, param)
                            join ep in db.HomeCareEntitledPeriods on c.Id equals ep.ClientId
                            select new EligibilityPeriodsExportModel
                            {
                                Id = c.Id,
                                StartDate = ep.StartDate,
                                EndDate = ep.EndDate
                            }).OrderBy(f => f.Id).ThenBy(f => f.StartDate).ToList();
            return filtered.Select(f => new EligibilityPeriodsExportModel
                    {
                        Id = f.Id,
                        StartDate = f.StartDate,
                        EndDate = f.EndDate.HasValue ? f.EndDate.Value.AddDays(-1) : f.EndDate
                    });
        }

        public static IEnumerable<FunctionalityScoresExportModel> GetFunctionalityScoresExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
        {
            return (from c in GetClients(db, permissions, param)
                    join fs in db.FunctionalityScores on c.Id equals fs.ClientId
                   // join d in db.Dafs on fs.Id equals d.FunctionalityScoreId
                   // join fs in db.FunctionalityScores on d.FunctionalityScoreId equals fs.Id
                  // join d in db.viewClientDetails on c.Id equals d.CCID
                   //join d in db.Dafs on fs.Id equals d.FunctionalityScoreId //c.Id equals d.ClientId and d.FunctionalityScoreId equals fs.
                    let DAFId = db.Dafs
                                  .Where(dafid => dafid.ClientId == c.Id) //d.CCID) //c.Id)
                                  .Where(dafid => dafid.FunctionalityScoreId == fs.Id)
                                 // .Where(dafid => dafid.Id != 0)
                                   .Select(dafid => dafid.Id)
                                   .FirstOrDefault()

                    select new FunctionalityScoresExportModel
                    {
                        Id = c.Id,
                        DiagnosticScore = fs.DiagnosticScore,
                        StartDate = fs.StartDate,
                        DAFID = DAFId == null ? 0 : DAFId // : DAFId //d.Id  
                    }).OrderBy(f => f.Id).ThenBy(f => f.StartDate);
        }

        public static IEnumerable<ApprovalStatusChangesExportModel> GetApprovalStatusChangesExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
        {
            var q = (from c in GetClients(db, permissions, param)
                    join h in db.Histories.Where(f => f.TableName == "Clients" && f.FieldName == "ApprovalStatusId") on c.Id equals h.ReferenceId
                    select new
                    {
                        ClientId = c.Id,
                        ApprovalStatusId = h.NewValue,
                        ApprovalStatusDate = h.UpdateDate
                    }).ToList();
            List<tempApprovalStatus> tempList = new List<tempApprovalStatus>();
            foreach(var item in q)
            {
                int apsId;
                int.TryParse(item.ApprovalStatusId, out apsId);
                tempList.Add(new tempApprovalStatus { ClientId = item.ClientId, ApprovalStatusId = apsId, ApprovalStatusDate = item.ApprovalStatusDate});
            }
            return (from a in tempList
                    join aps in db.ApprovalStatuses on a.ApprovalStatusId equals aps.Id
                    select new ApprovalStatusChangesExportModel
                    {
                        ClientId = a.ClientId,
                        ApprovalStatusName = aps.Name,
                        ApprovalStatusDate = a.ApprovalStatusDate
                    }).OrderBy(f => f.ClientId);
        }

		public static IEnumerable<UnmetNeedsOtherExportModel> GetUnmetNeedsOtherExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
		{
			return from c in GetClients(db, permissions, param)
				   join uno in db.UnmetNeedsOthers on c.Id equals uno.ClientId
				   select new UnmetNeedsOtherExportModel
				   {
					   ClientId = c.Id,
					   ServiceType = uno.ServiceType.Name,
					   ServiceTypeImportId = uno.ServiceType.ServiceTypeImportId ?? uno.ServiceTypeId,
					   Amount = uno.Amount,
					   CUR = uno.CurrencyId,
					   AgencyGroupId = c.Agency.GroupId,
					   AgencyId = c.AgencyId
				   };

		}

		public static IEnumerable<GovHcHoursExportModel> GetGovHcHoursExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
		{
			return (from c in GetClients(db, permissions, param)
				   join gh in db.GovHcHours on c.Id equals gh.ClientId
				   select new GovHcHoursExportModel
				   {
					   ClientId = c.Id,
					   StartDate = gh.StartDate,
					   Value = gh.Value
				   }).OrderBy(f => f.ClientId);

		}
		public static IEnumerable<LeaveEntriesExportModel> LeaveEntriesExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
		{
			var q = (from c in GetClients(db, permissions, param)
					join h in db.Histories on c.Id equals h.ReferenceId
					where h.TableName == "Clients" && h.FieldName == "LeaveDate"
					join hl in db.Histories on c.Id equals hl.ReferenceId
					where hl.TableName == "Clients" && hl.FieldName == "LeaveReasonId" && hl.UpdateDate == h.UpdateDate
					where h.NewValue != null && hl.NewValue != null
					select new LeaveEntriesExportModel
					{
						ClientId = c.Id,
						LeaveDate = h.NewValue,
						LeaveReason = hl.NewValue,
					}).OrderBy(f => f.ClientId).ToList();

			DateTime tempDate;
			int templrid;
			return (from item in q
					select new LeaveEntriesExportModel
					{
						ClientId = item.ClientId,
						LeaveDate = DateTime.TryParseExact(item.LeaveDate, "M/d/yyyy h:mm:ss tt", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out tempDate) ? tempDate.ToString("dd MMM yyyy") : "",
						LeaveReason = int.TryParse(item.LeaveReason, out templrid) && db.LeaveReasons.Any(f => f.Id == templrid) ? db.LeaveReasons.FirstOrDefault(f => f.Id == templrid).Name : ""
					});
		}
		public static IEnumerable<HASExportModel> HASExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, ClientsListDataTableModel param)
		{
			return (from c in GetClients(db, permissions, param)
					join has in db.ClientHcStatuses on c.Id equals has.ClientId
					select new HASExportModel
					{
						ClientId = c.Id,
						StatusDate = has.StartDate,
						FundStatus = has.FundStatusId.HasValue != null ? has.FundStatus.Name : "N/A",
						ApprovalStatus = has.ApprovalStatusId.HasValue != null ? has.ApprovalStatus.Name : "N/A",
						HCStatus = has.HcStatusId
					}).OrderBy(f => f.ClientId);

		}
	}

	#region rowModels
	public class BegExportModel
	{
		[Display(Name = "ORG")]
		public int? AgencyId { get; set; }
		[Display(Name = "ORG NAME")]
		public string AgencyName { get; set; }
		[Display(Name = "CCID")]
		public int Id { get; set; }
		[Display(Name = "Fund Status")]
		public string FundStatusName { get; set; }
		[Display(Name = "Approval Status")]
		public string ApprovalStatusName { get; set; }
		[Display(Name = "Compensation program")]
		public string CompensationProgramName { get; set; }
		[Display(Name = "Name/s of any other compensation program/s")]
		public string AddCompName { get; set; }
		[Display(Name = "Registration number/s")]
		public string AddCompRegNum { get; set; }
		[Display(Name = "Last Name")]
		public string LastName { get; set; }
		[Display(Name = "First Name")]
		public string FirstName { get; set; }
		[Display(Name = "Previous Last Name")]
		public string PrevLastName { get; set; }
		[Display(Name = "Previous First Name")]
		public string PrevFirstName { get; set; }
		[Display(Name = "Other Last Name")]
		public string OtherLastName { get; set; }
		[Display(Name = "Birth City")]
		public string PobCity { get; set; }
		[Display(Name = "Birth Country")]
		public string BirthCountryName { get; set; }
		[Display(Name = "Birth Date")]
		public DateTime? BirthDate { get; set; }
		[Display(Name = "Other Date of Birth")]
		public DateTime? OtherBirthDate { get; set; }
		[Display(Name = "Address")]
		public string Address { get; set; }
		[Display(Name = "City")]
		public string City { get; set; }
		[Display(Name = "State")]
		public string StateCode { get; set; }
		[Display(Name = "ZIP")]
		public string Zip { get; set; }
		[Display(Name = "Country")]
		public string CountryCode { get; set; }
	}
	
	public class DuplicateExportModel
	{
		[Display(Name = "CC ID")]
		public int Id { get; set; }
		[Display(Name = "Agency Name")]
		public string AgencyName { get; set; }
		[Display(Name = "Master Id")]
		public int? MasterId { get; set; }
	}
	public class ClientsListEntry
	{
		[Display(Name = "Agency_Id")]
		[ScaffoldColumn(false)]
		public int? AgencyId { get; set; }

		[ScaffoldColumn(false)]
		public int? AgencyGroupId { get; set; }

		[Display(Name = "Agency_Name")]
		[ScaffoldColumn(false)]
		public string AgencyName { get; set; }

		[ScaffoldColumn(false)]
		public string AgencyRegionName { get; set; }

		[ScaffoldColumn(false)]
		public string AgencyGroupName { get; set; }

		[ScaffoldColumn(false)]
		[Display(Name = "Approval_Status_Id")]
		public int ApprovalStatusId { get; set; }

		[Display(Name = "First_Name")]
		public string FirstName { get; set; }

		[Display(Name = "Last_Name")]
		public string LastName { get; set; }

		[Display(Name = "CC_ID")]
		public int Id { get; set; }

		[Display(Name = "Gov_Id")]
		public string NationalId { get; set; }

		public DateTime JoinDate { get; set; }

		public string Address { get; set; }

		public string City { get; set; }

		public string Phone { get; set; }

        [Display(Name = " Birth_Country")]
        public string BirthCountry { get; set; }

        [Display(Name = "Birth_Date")]
		public DateTime? BirthDate { get; set; }

		[Display(Name = "Functionality_Level")]
		public string FunctionalityLevelName { get; set; }

		[Display(Name = "Approval_Status")]
		public string ApprovalStatusName { get; set; }

		[Display(Name = "Current_HomeCareHours")]
		public decimal? CurFuncHcHours { get; set; }

		[Display(Name = "GrandFathered_Hours")]
		public decimal? GfHours { get; set; }



		[Display(Name = "Agency Internal ID")]
		public string InternalId { get; set; }

		[Display(Name = "Client_Master_ID")]
		public int? MasterId { get; set; }

		[Display(Name = "Create Date")]
		public DateTime UploadDate { get; set; }

		public bool Active { get; set; }

		public decimal? CurGovHcHours { get; set; }

		public bool? SubstractGovHcHours { get; set; }

		public decimal? AllowedHcHours { get; set; }

		[Display(Name = "Austrian Eligible")]
		public bool AustrianEligible { get; set; }
		
		[Display(Name = "Romanian Eligible")]
		public bool RomanianEligible { get; set; }

        [Display(Name = "GG Reported Only")]
        public bool GGReportedOnly { get; set; }

		public decimal? UnmetNeedsValue { get; set; }
	
		public DateTime? UnmetNeedsStartDate { get; set; }
	}
    public class EligibilityPeriodsExportModel
    {
        [Display(Name = "CC_ID")]
        public int Id { get; set;}
        
        [Display(Name = "START_DATE")]
        public DateTime StartDate { get; set;}
        
        [Display(Name = "END_DATE")]
        public DateTime? EndDate {get; set;}
    }
    public class FunctionalityScoresExportModel
    {
        [Display(Name = "CC_ID")]
        public int Id { get; set; }

        [Display(Name = "DIAGNOSTIC_SCORE")]
        public decimal DiagnosticScore { get; set; }

        [Display(Name = "START_DATE")]
        public DateTime StartDate { get; set; }

        [Display(Name = "DAF ID", Order = 68)]
        public int? DAFID { get; set; }

    }
    public class ApprovalStatusChangesExportModel
    {
        [Display(Name = "CC ID")]
        public int ClientId { get; set; }
        
        [Display(Name = "Approval Status")]
        public string ApprovalStatusName { get; set; }

        [Display(Name = "Date")]
        public DateTime ApprovalStatusDate { get; set; }
    }
    public class tempApprovalStatus
    {
        public int ClientId { get; set; }
        public int ApprovalStatusId { get; set; }
        public DateTime ApprovalStatusDate { get; set; }
    }
	public class UnmetNeedsOtherExportModel
	{
		[Display(Name = "CC ID")]
		public int ClientId { get; set; }
		[Display(Name = "Service Type Name")]
		public string ServiceType { get; set; }
		[Display(Name = "Service Type Import Id")]
		public int ServiceTypeImportId { get; set; }
		[Display(Name = "Amount")]
		public decimal Amount { get; set; }
		[Display(Name = "Ser CUR")]
		public string CUR { get; set; }
		[Display(Name = "SER ID")]
		public int AgencyGroupId { get; set; }
		[Display(Name = "ORG ID")]
		public int AgencyId { get; set; }
	}
	public class GovHcHoursExportModel
	{
		[Display(Name = "CC ID")]
		public int ClientId { get; set; }
		[Display(Name = "Start Date")]
		public DateTime StartDate { get; set; }
		[Display(Name = "Value")]
		public decimal Value { get; set; }
	}
	public class LeaveEntriesExportModel
	{
		[Display(Name = "CC ID")]
		public int ClientId { get; set; }
		[Display(Name = "Leave Date")]
		public string LeaveDate { get; set; }
		[Display(Name = "Leave Reason")]
		public string LeaveReason { get; set; }
	}
	public class HASExportModel : HASExportModelNoProgramField
	{		
		[Display(Name = "Fund Status", Order = 3)]
		public string FundStatus { get; set; }
		
	}

	public class HASExportModelNoProgramField
	{
		[Display(Name = "CC ID", Order = 1)]
		public int ClientId { get; set; }
		[Display(Name = "Approval Status Date", Order = 2)]
		public DateTime StatusDate { get; set; }
		[Display(Name = "JNV Status", Order = 4)]
		public string ApprovalStatus { get; set; }
		[Display(Name = "Homecare Approval Status", Order = 5)]
		public int? HCStatus { get; set; }
	}
	#endregion
	public class LocalizedDisplayNameAttribute : DisplayNameAttribute
	{

		public Type ResourceType { get; set; }
		public override string DisplayName
		{
			get
			{
				System.Resources.ResourceManager rm = new System.Resources.ResourceManager(this.ResourceType);
				return base.DisplayName;
			}
		}

	}



}