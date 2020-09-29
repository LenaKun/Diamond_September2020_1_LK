using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Models
{
    public class acprowBase
    {
        [Display(Name = "CLIENT_ID", Order = 0)]
        public int CLIENT_ID { get; set; }

        [Display(Name = "ORG_ID", Order = 1)]
        public int? ORG_ID { get; set; }

        [Display(Name = "LAST_NAME", Order = 2)]
        public string LAST_NAME { get; set; }

        [Display(Name = "FIRST_NAME", Order = 3)]
        public string FIRST_NAME { get; set; }

        [Display(Name = "MIDDLE_NAME", Order = 4)]
        public string MIDDLE_NAME { get; set; }

        [Display(Name = "DOB", Order = 5)]
        [DisplayFormat(DataFormatString = "dd/MM/yyyy")]
        public DateTime? DOB { get; set; }

        [Display(Name = "ADDRESS", Order = 6)]
        public string ADDRESS { get; set; }

        [Display(Name = "CITY", Order = 7)]
        public string CITY { get; set; }

        [Display(Name = "ZIP", Order = 8)]
        public string ZIP { get; set; }

        [Display(Name = "STATE_CODE", Order = 9)]
        public string STATE_CODE { get; set; }

        [Display(Name = "COUNTRY_CODE", Order = 10)]
        public string COUNTRY_CODE { get; set; }

        [Display(Name = "TYPE_OF_ID", Order = 11)]
        public string TYPE_OF_ID { get; set; }

        [Display(Name = "SS", Order = 12)]
        public string SS { get; set; }

        [Display(Name = "PHONE", Order = 13)]
        public string PHONE { get; set; }

        [Display(Name = "CLIENT_COMP_PROGRAM", Order = 14)]
        public bool CLIENT_COMP_PROGRAM { get; set; }

        [Display(Name = "COMP_PROG_REG_NUM", Order = 15)]
        public string COMP_PROG_REG_NUM { get; set; }

        [Display(Name = "AdditionalComp", Order = 16)]
        public string AdditionalComp { get; set; }

        [Display(Name = "AdditionalCompNum", Order = 17)]
        public string AdditionalCompNum { get; set; }

        [Display(Name = "Deceased", Order = 18)]
        public bool? Deceased { get; set; }

        [Display(Name = "DOD", Order = 19)]
        [DisplayFormat(DataFormatString = "dd/MM/yyyy")]
        public DateTime? DOD { get; set; }

        [Display(Name = "New_Client", Order = 20)]
        public string New_Client { get; set; }

        [Display(Name = "Place_of_Birth_City", Order = 21)]
        public string Place_of_Birth_City { get; set; }

        [Display(Name = "Place_of_Birth_Country", Order = 22)]
        public string Place_of_Birth_Country { get; set; }

        [Display(Name = "Date_Emigrated", Order = 23)]
        [DisplayFormat(DataFormatString = "dd/MM/yyyy")]
        public DateTime? Date_Emigrated { get; set; }

        [Display(Name = "Previous_First_Name", Order = 24)]
        public string Previous_First_Name { get; set; }

        [Display(Name = "Previous_Last_Name", Order = 25)]
        public string Previous_Last_Name { get; set; }

        [Display(Name = "Upload_Date", Order = 26)]
        [DisplayFormat(DataFormatString = "dd/MM/yyyy")]
        public DateTime Upload_Date { get; set; }

        [Display(Name = "MatchFlag", Order = 27)]
        public bool? MatchFlag { get; set; }        

        [Display(Name = "Internal_Client_ID", Order = 30)]
        public string Internal_Client_ID { get; set; }
    }

	public class acprow : acprowBase
	{
        [Display(Name = "claim_status", Order = 28)]
		public string claim_status { get; set; }

        [Display(Name = "CLIENT_MASTER_ID", Order = 29)]
        public int? CLIENT_MASTER_ID { get; set; }

		/// <summary>
		/// Returns ienumerable collection that is ready to be written to excel as is
		/// </summary>
		/// <param name="clients"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		internal static IQueryable<acprow> GetExportData(IQueryable<Client> clients, ccEntities db, CC.Data.Services.IPermissionsBase Permissions)
		{
			var q = from c in clients
					join dc in db.Clients.Where(Permissions.ClientsFilter) on c.MasterId equals dc.Id into dcg
					from dc in dcg.DefaultIfEmpty()

					select new acprow
					{
						CLIENT_ID = c.Id,
						ORG_ID = c.AgencyId,
						LAST_NAME = c.LastName,
						FIRST_NAME = c.FirstName,
						MIDDLE_NAME = c.MiddleName,
						DOB = c.BirthDate,
						ADDRESS = c.Address,
						CITY = c.City,
						ZIP = c.ZIP,
						STATE_CODE = c.State.Code,
						COUNTRY_CODE = c.Agency.AgencyGroup.Country.Code,
						TYPE_OF_ID = c.NationalIdType.Name,
						SS = c.NationalId,
						PHONE = c.Phone,
						CLIENT_COMP_PROGRAM = c.IsCeefRecipient,
						COMP_PROG_REG_NUM = c.CeefId,
						AdditionalComp = c.AddCompName,
						AdditionalCompNum = c.AddCompId,
						Deceased = (bool?)(c.DeceasedDate.HasValue || c.LeaveReasonId == (int)LeaveReasonEnum.Deceased),
						DOD = c.DeceasedDate,
						New_Client = c.New_Client,
						Place_of_Birth_City = c.PobCity,
						Place_of_Birth_Country = c.BirthCountry.Name,
						Previous_First_Name = c.PrevFirstName,
						Previous_Last_Name = c.PrevLastName,
						Upload_Date = c.CreatedAt,
						MatchFlag = c.MatchFlag,
						claim_status = c.FundStatus.Name,
						CLIENT_MASTER_ID = dc.MasterId,
						Internal_Client_ID = c.InternalId
					};
			return q;
		}



	}
}