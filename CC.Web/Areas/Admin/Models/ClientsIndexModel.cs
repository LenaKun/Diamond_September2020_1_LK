using CsvHelper;
using System;
using System.Linq.Expressions;
using CsvHelper.Configuration;
using System.Linq.Dynamic;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using CC.Data;
using System.Web;
using CC.Extensions.Reflection;
using System.Data.Objects.SqlClient;
namespace CC.Web.Areas.Admin.Models
{
	public class ClientsIndexModel : AdminModelBase
	{
		public ClientsIndexModel()
		{
			HistoryExportModel = new HistoryExportModel();

			using (var db = new ccEntities())
			{
				UpdateClientsCount(db);
			}
		}

		public int NewClientsCount { get; set; }

		public int NewClientsExportedCount { get; set; }

		public void UpdateClientsCount(ccEntities db)
		{
			var newClients = db.Clients.Where(f => f.ApprovalStatusId == (int)ApprovalStatusEnum.New)
				.Where(f => !AgencyGroup.TestIds.Contains(f.Agency.GroupId));

			NewClientsCount = newClients.Count();
			NewClientsExportedCount = newClients.Where(f => f.ACPExported).Count();
		}
		
		public HistoryExportModel HistoryExportModel { get; set; }

		public IEnumerable<string> GetFundStatusFieldNames()
		{
			return CC.Web.Helpers.ImportHelper.GetCsvFileHeaders<CC.Web.Areas.Admin.Controllers.ClientApprovalStatusController.ApprovalStatusCsvMap>();
		}

	}

    public class ClientsServiceDateExportModel
    {
        [CsvField(Name = "CC ID")]
        public int ClientId { get; set; }

        [CsvField(Name = "Master ID")]
        public string MasterId { get; set; }


        [CsvField(Name = "Last time received Services")] // Format = "yyyy-MM-dd HH:ss"
        public string LastDate { get; set; }

		[CsvField(Name = "Includes Homecare?")]
		public string IncludesHomecare { get; set; }


        public IEnumerable<ClientsServiceDateExportModel> GetClientServiceDateReport(ccEntities db, CC.Data.Services.IPermissionsBase Permissions)
        {
            var data = (from v in db.ViewClientsServiceDate_New.ToList()                       
                        orderby v.ClientId
                        select new ClientsServiceDateExportModel()
                        {
                            ClientId = v.ClientId,
                            MasterId = v.MasterId.HasValue ? v.MasterId.Value.ToString() : "",
                            LastDate = v.LastDate,
							IncludesHomecare = v.IsHomecare == true ? "Yes" : "No"
                        }).ToList();



            return data;


        }

    }


	public class HistoryExportModel
	{

		public HistoryExportModel()
		{


			this.ExportableProperties = HistoryExportAllFields.GetExportableProperties();

			this.Presets = new List<Preset>();

			this.AddPreset(new[] { 
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f) => f.BirthDate),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f) => f.OtherDob)
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.NationalId),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.NationalType)
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.OtherNationalId),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.OtherNationalIdType)
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.Address),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.City),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.StateCode),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.Zip)
			});
			this.AddPreset(new[] { 
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f) => f.DeceasedDate)
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.FirstName),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.MiddleName),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.LastName),
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.PrevFirstName),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.PrevLastName),
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.OtherFirstName),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.OtherLastName)
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.IsCeefRecipient),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.CeefId)
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.AddCompName),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.AddCompId)
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.InternalId)
			});
			this.AddPreset(new[]{
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.LeaveDate),
				ReflectionExtensions.GetPropertyInfo((HistoryExportAllFields f)=>f.LeaveReason)
			});

		}
		private void AddPreset(IEnumerable<PropertyInfo> propNames, string Display = null)
		{
			var pds = from a in propNames
					  join b in HistoryExportAllFields.GetExportableProperties() on a.Name equals b.Name
					  select b;
			
			this.Presets.Add(new Preset
			{
				
				i = this.Presets.Count,
				value = string.Join(",", pds.Select(f => f.Name)),
				display = Display ?? string.Join(", ", pds.Select(f =>
					f.GetCustomAttributes(typeof(CsvFieldAttribute), false).Cast<CsvFieldAttribute>().Select(a => a.Name).SingleOrDefault() ?? f.Name))
			});
		}
		public struct Preset
		{
			public int i;
			public string value;
			public string display;
		}

		public IEnumerable<PropertyInfo> ExportableProperties { get; set; }
		public Dictionary<string, string> AvailableFields { get; set; }

		[Required()]
		[Display(Name = "Changed Since")]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		[UIHint("Date")]
		public DateTime? Start { get; set; }

		[Display(Name = "To")]
		[DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)]
		public DateTime? End { get; set; }

		[Required(ErrorMessage = "Please select Fields to export")]
		public string Fields { get; set; }

		public List<Preset> Presets { get; set; }

		public List<string> FieldsList
		{
			get
			{
				if (Fields == null)
				{
					return new List<String>();
				}
				else
				{
					return Fields.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
						.ToList()
						.InsertAt(0, "Id");
				}
			}
		}

		public IEnumerable<HistoryExportAllFields> GetExportData(ccEntities db, CC.Data.Services.IPermissionsBase Permissions)
		{
			var fields = this.FieldsList.ToArray();
			bool deceased = fields.Contains("DeceasedDate");
			bool leave = fields.Contains("LeaveDate");

			var dbfields = (from f in this.FieldsList
							join p in typeof(HistoryExportAllFields).GetProperties() on f equals p.Name
							where p.IsDefined(typeof(StringAttribute), false)
							select p.GetCustomAttributes(typeof(StringAttribute), false).Cast<StringAttribute>().Single().StringValue).ToArray();

			var history = from h in db.Histories
						  where (h.TableName == "Clients")
						  where (dbfields.Contains(h.FieldName))
						  where (h.UpdateDate >= Start)
						  select h;

			if (this.End.HasValue) { history = history.Where(f => f.UpdateDate < this.End); }
			var testAgencyIds = new int[] { 176, 894, 1601, 1602 };
			var result = (
				from h in history
				group h by h.ReferenceId into hg
				join c in db.Clients.Where(Permissions.ClientsFilter) on hg.Key equals c.Id
				where c.ApprovalStatusId!=(int)ApprovalStatusEnum.New
				where !(testAgencyIds.Any(f=>f==c.AgencyId))
				where !deceased || c.DeceasedDate.HasValue
				where !leave || c.LeaveDate.HasValue
				select new HistoryExportAllFieldsTemp
				{
					Id = c.Id,
					BirthDate = c.BirthDate,
					OtherDob = c.OtherDob,
					NationalId = c.NationalId,
					NationalType = c.NationalIdType.Name,
					OtherNationalId = c.OtherId,
					OtherNationalIdType = c.OtherNationalIdType.Name,
					Address = c.Address,
					City = c.City,
					StateCode = c.State.Code,
					Zip = c.ZIP,
					DeceasedDate = c.DeceasedDate,
					FirstName = c.FirstName,
					MiddleName = c.MiddleName,
					LastName = c.LastName,
					OtherFirstName = c.OtherFirstName,
					OtherLastName = c.OtherLastName,
					PrevFirstName=c.PrevFirstName,
					PrevLastName=c.PrevLastName,
					IsCeefRecipient = c.IsCeefRecipient,
					CeefId = c.CeefId,
					AddCompName = c.AddCompName,
					AddCompId = c.AddCompId,
					InternalId = c.InternalId,
					LeaveDate = c.LeaveDate,
					LeaveReason = c.LeaveReason.Name
				});

			var dr = result.Select(CreateNewStatement<HistoryExportAllFieldsTemp, HistoryExportAllFields>("Id," + this.Fields));

			var records = dr.ToList();

			return records;
		}

		Expression<Func<Source, Target>> CreateNewStatement<Source, Target>(string fields)
		{
			// input parameter "o"
			var xParameter = Expression.Parameter(typeof(Source), "o");


			// create initializers
			var bindings = fields.Split(',').Select(o => o.Trim())
				.Select(o =>
				{

					// property "Field1"
					var mi = typeof(Target).GetProperty(o);

					// original value "o.Field1"
					var xOriginal = Expression.Property(xParameter, typeof(Source).GetProperty(o));

					// set value "Field1 = o.Field1"
					return Expression.Bind(mi, xOriginal);
				}
			);

			// new statement "new Data()"
			var xNew = Expression.New(typeof(Target));

			// initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
			var xInit = Expression.MemberInit(xNew, bindings);

			// expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
			var lambda = Expression.Lambda<Func<Source, Target>>(xInit, xParameter);

			// compile to Func<Data, Data>
			return lambda;
		}

		public IEnumerable<PropertyInfo> GetExportProperties()
		{
			var props = from p in this.ExportableProperties
						join f in this.FieldsList on p.Name equals f
						select p;
			return props;
		}
	}

	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	sealed class StringAttribute : Attribute
	{
		// See the attribute guidelines at 
		//  http://go.microsoft.com/fwlink/?LinkId=85236
		readonly string positionalString;

		// This is a positional argument
		public StringAttribute(string TableFieldName)
		{
			this.positionalString = TableFieldName;
		}

		public string StringValue
		{
			get { return positionalString; }
		}
	}

	/// <summary>
	/// used as intermediate obj in clients -> view -> partial select (HistoryExportAllFields)
	/// </summary>
	class HistoryExportAllFieldsTemp : HistoryExportAllFields
	{
	}


	/// <summary>
	/// Transfers data to csv result
	/// StringAttribute maps
	/// </summary>
	public class HistoryExportAllFields
	{
		public static string GetTableFieldName(string viewPropName)
		{
			var ownProp = typeof(HistoryExportAllFields).GetProperties().SingleOrDefault(f => f.Name == viewPropName);
			if (ownProp == null) { throw new ArgumentException("I dont have property " + viewPropName); }
			var myAttr = ownProp.GetCustomAttributes(typeof(StringAttribute), false).Cast<StringAttribute>().FirstOrDefault();
			if (myAttr == null)
			{
				return ownProp.Name;
			}
			else
			{
				return myAttr.StringValue;
			}
		}
		public static IEnumerable<PropertyInfo> GetExportableProperties()
		{
			var q = from p in typeof(HistoryExportAllFields).GetProperties()
					where p.GetCustomAttributes(typeof(StringAttribute), false).Cast<StringAttribute>().Any()
					select p;
			return q;
		}


		[CsvField(Name = "CC ID")]
		[String("Id")]
		public int Id { get; set; }

		[String("BirthDate")]
		[CsvField(Name = "Current Birthdate", Format = "yyyy-MM-dd")]
		public DateTime? BirthDate { get; set; }
		[String("OtherDob")]
		[CsvField(Name = "other date of birth", Format = "yyyy-MM-dd")]
		public DateTime? OtherDob { get; set; }

		[String("NationalId")]
		[CsvField(Name = "national id")]
		public string NationalId { get; set; }

		[String("NationalIdTypeId")]
		[CsvField(Name = "national type")]
		public string NationalType { get; set; }

		[String("OtherId")]
		[CsvField(Name = "Other ID card")]
		public string OtherNationalId { get; set; }

		[String("OtherIdTypeId")]
		[CsvField(Name = "Other ID type")]
		public string OtherNationalIdType { get; set; }

		[String("Address")]
		[CsvField(Name = "Address")]
		public string Address { get; set; }

		[String("City")]
		[CsvField(Name = "City")]
		public string City { get; set; }

		[String("StateId")]
		[CsvField(Name = "State Code")]
		public string StateCode { get; set; }

		[String("Zip")]
		[CsvField(Name = "ZIP")]
		public string Zip { get; set; }

		[String("DeceasedDate")]
		[CsvField(Name = "deceased date", Format = "yyyy-MM-dd")]
		public DateTime? DeceasedDate { get; set; }

		[String("FirstName")]
		[CsvField(Name = "First Name")]
		public string FirstName { get; set; }

		[String("MiddleName")]
		[CsvField(Name = "Middle Name")]
		public string MiddleName { get; set; }

		[String("LastName")]
		[CsvField(Name = "Last Name")]
		public string LastName { get; set; }

		[String("PrevFirstName")]
		public string PrevFirstName { get; set; }

		[String("PrevLastName")]
		public string PrevLastName { get; set; }

		[CsvField(Name = "Other First Name")]
		[String("OtherLastName")]
		public string OtherLastName { get; set; }

		[CsvField(Name = "Other Last Name")]
		[String("OtherFirstName")]
		public string OtherFirstName { get; set; }

		[CsvField(Name = "Article 2 / CEEF recipient?")]
		[String("IsCeefRecipient")]
		public bool IsCeefRecipient { get; set; }

		[CsvField(Name = "Article 2 / CEEF registration number")]
		[String("CeefId")]
		public string CeefId { get; set; }

		[CsvField(Name = "Name/s of any other compensation program/s")]
		[String("AddCompName")]
		public string AddCompName { get; set; }

		[CsvField(Name = "Registration number/s")]
		[String("AddCompId")]
		public string AddCompId { get; set; }

		[CsvField(Name = "Internal ID")]
		[String("InternalId")]
		public string InternalId { get; set; }

		[CsvField(Name = "Leave Date", Format = "yyyy-MM-dd")]
		[String("LeaveDate")]
		public DateTime? LeaveDate { get; set; }

		[String("LeaveReason")]
		[CsvField(Name="Leave Reason")]
		public string LeaveReason { get; set; }
	}


    //rejected pending report
	public class ClientsRejectedPendingReportModel
	{
    
        [Display(Name = "Org ID")]
        public int? ORGID { get; set; }

        [Display(Name = "Org Name")]
        public string Agency { get; set; }

        [Display(Name = "CC ID")]
        public int CCID { get; set; }

        [Display(Name = "Master Id")]
        public int? MasterId { get; set; }

        [Display(Name = "Fund Status")]
        public string FundStatus { get; set; }

        [Display(Name = "JNV Status")]
        public string ApprovalStatus { get; set; }

        [Display(Name = "Date of Status Changed in Diamond")]
        public DateTime? DateOfStatusChanged { get; set; }

        [Display(Name = "Compensation Program")]
        public string CompensationProgramName { get; set; }

        [Display(Name = "Name/s of any other compensation program/s")]
        public string AddCompName { get; set; }

        [Display(Name = "Registration Numbers")]
        public string AddCompId { get; set; }



        [Display(Name = "Last Name")]
        public string LastName { get; set; }

		[Display(Name = "First Name")]
		public string FirstName { get; set; }

        [Display(Name = "Previous Last Name")]
        public string PreviousLastName { get; set; }

        [Display(Name = "Previous First Name")]
        public string PreviousFirstName { get; set; }

        [Display(Name = "Other Last Name")]
        public string OtherLastName { get; set; }

        [Display(Name = "Birth City")]
        public string BirthCity { get; set; }
        [Display(Name = "Birth Country")]
        public string BirthCountry { get; set; }

        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Other Date of Birth")]
        public DateTime? Otherdateofbirth { get; set; }


		[Display(Name = "Address")]
		public string Address { get; set; }
		[Display(Name = "City")]
		 public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }
		[Display(Name = "ZIP")]
		public string ZIP { get; set; }
		
		[Display(Name = "Country")]
		public string CountryName { get; set; }

        [Display(Name = "Internal Agency ID")]
        public string InternalAgencyID { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "GG Reported?")]
        public string GGReported { get; set; }

        [Display(Name = "Appeared at least once for any report on any fund")]
        public string AppearedAtLeastOnce { get; set; }

        [Display(Name = "Date Last Report on GG")]
        public DateTime? GGLastDate { get; set; }

        [Display(Name = "Deceased True/False")]
        public bool Deceased { get; set; }

        [Display(Name = "Deceased Date from Diamond")]
        public DateTime? DeceasedDate { get; set; }
        
        [Display(Name = "Leave Date")]

        public DateTime? LeaveDate { get; set; }

        [Display(Name = "Leave Reason")]
        public string LeaveReason { get; set; }


        public  IEnumerable<ClientsRejectedPendingReportModel> GetRejectedPendingData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
        {
            var data = from c in db.Clients.Where(f=>f.ApprovalStatusId==256)
                           //rejected pending
                      
                       select new
                           {
                               c = c,
                               inreport = db.ViewClientReports.Any(f => f.ClientId == c.Id),
                               date_change = db.Histories.Where(f => f.ReferenceId == c.Id && f.FieldName == "ApprovalStatusId" && f.NewValue=="256").OrderBy(f=>f.UpdateDate).FirstOrDefault(),
                               date_gg=from r in db.ViewClientReports.Where(f=>f.ClientId==c.Id)
                                       join s in db.SubReports on r.SubReportId equals s.Id
                                       where s.MainReport.AppBudget.App.Fund.MasterFundId==73
                                  
                                       select new
                                       {
                                           LastGGDate=s.MainReport.Start
                                       }



                                       


                           };


           


            return data.OrderBy(f=>f.c.AgencyId).Select(c => new ClientsRejectedPendingReportModel
            {
                
                ORGID = c.c.AgencyId,
                Agency = c.c.Agency.Name,
                CCID = c.c.Id,
                MasterId = c.c.MasterId,
               
                FundStatus=c.c.FundStatus.Name,
                ApprovalStatus = c.c.ApprovalStatus.Name,
                DateOfStatusChanged=c.date_change==null ? null : (DateTime?) c.date_change.UpdateDate,
                CompensationProgramName=c.c.CompensationProgramName,
                AddCompName=c.c.AddCompName,
                AddCompId=c.c.AddCompId,

                LastName = c.c.LastName,
                FirstName = c.c.FirstName,

                PreviousLastName = c.c.PrevLastName,
                PreviousFirstName = c.c.PrevFirstName,

                OtherLastName = c.c.OtherLastName,
                BirthCity = c.c.PobCity,
                BirthCountry = c.c.BirthCountry.Name,
                BirthDate = c.c.BirthDate,
                Otherdateofbirth = c.c.OtherDob,
            
  
                Address = c.c.Address,
                City = c.c.City,
                State = c.c.State.Code,
                ZIP = c.c.ZIP,
                CountryName = c.c.Country.Name,

                InternalAgencyID = c.c.InternalId,
                Remarks = c.c.Remarks,

            
                GGReported = c.c.GGReportedCount != null && c.c.GGReportedCount > 0 ? "Yes" : "No",
  
                AppearedAtLeastOnce = c.inreport ? "Yes" : "No",
        
                //date last report on gg
                GGLastDate=c.date_gg.OrderByDescending(f=>f.LastGGDate).FirstOrDefault().LastGGDate,
               
                Deceased = (c.c.DeceasedDate != null || c.c.LeaveReasonId == (int)CC.Data.LeaveReasonEnum.Deceased) ? true : false,
        
                DeceasedDate = c.c.DeceasedDate,
                LeaveDate = c.c.LeaveDate,
                LeaveReason = c.c.LeaveReason.Name,
   
        
                 
                
                
              
            });
        }


	

	}

	
   
}

