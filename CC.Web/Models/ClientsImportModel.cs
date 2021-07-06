using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.Data.SqlClient;

namespace CC.Web.Models
{

	public class ImportPreviewModel
	{
		public Guid Id { get; set; }
		public string ImportAction { get; set; }
	}

	public class ClientsImportModelBase
	{
		public ClientsImportModelBase() { this.Id = Guid.NewGuid(); }
		public Guid Id { get; set; }
		public HttpPostedFileBase File { get; set; }
		public CC.Data.Services.IPermissionsBase Permissions { get; set; }
        public string Title { get; set; }
	}

	public class NewClientsImportModel : ClientsImportModelBase
	{

		public NewClientsImportModel()
			: base()
		{
            this.Title = "New Clients - Import";
		}

		public virtual IEnumerable<string> GetCsvColumnNames()
		{
			return CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<ClientCsvMap>();
		}

		internal void ProcessFile(bool insert)
		{
			var ImportId = this.Id;

			string fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), ImportId.ToString());

			var csvConf = new CsvHelper.Configuration.CsvConfiguration()
			{
				IsStrictMode = false,
				IsCaseSensitive = false,
				SkipEmptyRecords = true
			};

            if(insert)
            {
                csvConf.ClassMapping<ClientCsvMap>();
            }
            else
            {
                csvConf.ClassMapping<ExistingClientCsvMap>();
            }


			var updatedAt = DateTime.Now;
			var updatedBy = this.Permissions.User.Id;

			using (var csvReader = new CsvHelper.CsvReader(new System.IO.StreamReader(File.InputStream), csvConf))
			{
				var csvChunkSize = 10000;
				var recordIndex = 1;

				using (var db = new ccEntities())
				{
					db.Imports.AddObject(new CC.Data.Import()
					{
						Id = ImportId,
						StartedAt = DateTime.Now,
						UserId = this.Permissions.User.Id
					});
					db.SaveChanges();
				}

				foreach (var csvChunk in csvReader.GetRecords<ImportClient>().Split(csvChunkSize))
				{
					string connectionString = ConnectionStringHelper.GetProviderConnectionString();

					using (var sqlBulk = new System.Data.SqlClient.SqlBulkCopy(connectionString, SqlBulkCopyOptions.KeepNulls))
					{
						foreach (var record in csvChunk)
						{
							record.RowIndex = recordIndex++;
							record.ImportId = ImportId;
							record.UpdatedAt = updatedAt;
							record.UpdatedById = updatedBy;
							if (this.Permissions.User.RoleId != (int)FixedRoles.Admin)
							{
								using (var db = new ccEntities())
								{
									var regionId = (from a in db.Agencies
													where a.Id == record.AgencyId
													select a.AgencyGroup.Country.RegionId).SingleOrDefault();
									if (regionId == 2) //Israel
									{
										record.CountryId = 344;
										record.NationalIdTypeId = 1;
									}
								}
							}
                            if(!string.IsNullOrEmpty(record.NationalId) && record.NationalId.Length < 9 && record.NationalIdTypeId == 1)
                            {
                                record.NationalId = record.NationalId.PadLeft(9, '0');
                            }
							if (insert)
							{
								record.ApprovalStatusId = (int)ApprovalStatusEnum.New;
							}							
						}

						var dataTable = csvChunk.ToDataTable();
						var q = dataTable.Columns.OfType<System.Data.DataColumn>().Where(f => f.DataType == typeof(Int32)).Select(f => new
						{
							c = f.ColumnName,
							values = dataTable.Rows.OfType<System.Data.DataRow>().Select((r, i) => r[f.ColumnName])
						});

						sqlBulk.DestinationTableName = "ImportClients";
						sqlBulk.NotifyAfter = 1000;
						MapColumns(sqlBulk);
						sqlBulk.SqlRowsCopied += (s, e) =>
						{
							System.Diagnostics.Debug.Write(e.RowsCopied);

						};

						sqlBulk.WriteToServer(dataTable);
					}

				}
			}
		}

		private static void MapColumns(SqlBulkCopy sqlBulk)
		{
			sqlBulk.ColumnMappings.Add("AddCompId", "AddCompId");
			sqlBulk.ColumnMappings.Add("AddCompName", "AddCompName");
			sqlBulk.ColumnMappings.Add("Address", "Address");
			sqlBulk.ColumnMappings.Add("AgencyId", "AgencyId");
			sqlBulk.ColumnMappings.Add("ApprovalStatusId", "ApprovalStatusId");
			sqlBulk.ColumnMappings.Add("BirthDate", "BirthDate");
			sqlBulk.ColumnMappings.Add("CeefId", "CeefId");
            sqlBulk.ColumnMappings.Add("CountryId", "CountryId");
			sqlBulk.ColumnMappings.Add("City", "City");
			sqlBulk.ColumnMappings.Add("ClientId", "ClientId");
			sqlBulk.ColumnMappings.Add("CompensationProgramName", "CompensationProgramName");
			sqlBulk.ColumnMappings.Add("CreatedAt", "CreatedAt");
			sqlBulk.ColumnMappings.Add("DeceasedDate", "DeceasedDate");
			sqlBulk.ColumnMappings.Add("ExceptionalHours", "ExceptionalHours");
			sqlBulk.ColumnMappings.Add("FirstName", "FirstName");
			sqlBulk.ColumnMappings.Add("Gender", "Gender");
			sqlBulk.ColumnMappings.Add("FundStatusId", "FundStatusId");
			sqlBulk.ColumnMappings.Add("Id", "Id");
			sqlBulk.ColumnMappings.Add("ImportId", "ImportId");
			sqlBulk.ColumnMappings.Add("IncomeCriteriaComplied", "IncomeCriteriaComplied");
			sqlBulk.ColumnMappings.Add("IncomeVerificationRequired", "IncomeVerificationRequired");
			sqlBulk.ColumnMappings.Add("InternalId", "InternalId");
			sqlBulk.ColumnMappings.Add("IsCeefRecipient", "IsCeefRecipient");
			sqlBulk.ColumnMappings.Add("JoinDate", "JoinDate");
			sqlBulk.ColumnMappings.Add("LastName", "LastName");
			sqlBulk.ColumnMappings.Add("LeaveDate", "LeaveDate");
			sqlBulk.ColumnMappings.Add("LeaveReasonId", "LeaveReasonId");
			sqlBulk.ColumnMappings.Add("LeaveRemarks", "LeaveRemarks");
			sqlBulk.ColumnMappings.Add("MasterId", "MasterId");
			sqlBulk.ColumnMappings.Add("MatchFlag", "MatchFlag");
			sqlBulk.ColumnMappings.Add("MiddleName", "MiddleName");
			sqlBulk.ColumnMappings.Add("NationalId", "NationalId");
			sqlBulk.ColumnMappings.Add("NationalIdTypeId", "NationalIdTypeId");
			sqlBulk.ColumnMappings.Add("NaziPersecutionDetails", "NaziPersecutionDetails");
			sqlBulk.ColumnMappings.Add("New_Client", "New_Client");
			sqlBulk.ColumnMappings.Add("OtherAddress", "OtherAddress");
			sqlBulk.ColumnMappings.Add("OtherDob", "OtherDob");
			sqlBulk.ColumnMappings.Add("OtherFirstName", "OtherFirstName");
			sqlBulk.ColumnMappings.Add("OtherId", "OtherId");
			sqlBulk.ColumnMappings.Add("OtherIdTypeId", "OtherIdTypeId");
			sqlBulk.ColumnMappings.Add("OtherLastName", "OtherLastName");
			sqlBulk.ColumnMappings.Add("Phone", "Phone");
			sqlBulk.ColumnMappings.Add("PobCity", "PobCity");
			sqlBulk.ColumnMappings.Add("BirthCountryId", "BirthCountryId");
			sqlBulk.ColumnMappings.Add("PrevFirstName", "PrevFirstName");
			sqlBulk.ColumnMappings.Add("PreviousAddressInIsrael", "PreviousAddressInIsrael");
			sqlBulk.ColumnMappings.Add("PrevLastName", "PrevLastName");
			sqlBulk.ColumnMappings.Add("Remarks", "Remarks");
			sqlBulk.ColumnMappings.Add("RowIndex", "RowIndex");
			sqlBulk.ColumnMappings.Add("StateId", "StateId");
			sqlBulk.ColumnMappings.Add("UpdatedAt", "UpdatedAt");
			sqlBulk.ColumnMappings.Add("UpdatedById", "UpdatedById");
			sqlBulk.ColumnMappings.Add("ZIP", "ZIP");
			sqlBulk.ColumnMappings.Add("HomecareWaitlist", "HomecareWaitlist");
			sqlBulk.ColumnMappings.Add("OtherServicesWaitlist", "OtherServicesWaitlist");
			sqlBulk.ColumnMappings.Add("CommPrefsId", "CommPrefsId");
			sqlBulk.ColumnMappings.Add("CareReceivedId", "CareReceivedId");
			sqlBulk.ColumnMappings.Add("MAFDate", "MAFDate");
			sqlBulk.ColumnMappings.Add("MAF105Date", "MAF105Date");
            //sqlBulk.ColumnMappings.Add("HAS2Date", "HAS2Date");
            sqlBulk.ColumnMappings.Add("UnableToSign", "UnableToSign");
            sqlBulk.ColumnMappings.Add("NursingHome", "NursingHome");
            sqlBulk.ColumnMappings.Add("AssistedLiving", "AssistedLiving");
        }
	}



	public class ExistingClientsImportModel : NewClientsImportModel
	{
        public ExistingClientsImportModel()
            :base()
        {
            this.Title = "Update Clients - Import"; 
        }
		public override IEnumerable<string> GetCsvColumnNames()
		{
			return CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<ExistingClientCsvMap>();
		}
	}



	/// <summary>
	/// Maps properties to csv column names
	/// </summary>
	public class ClientCsvMap : CsvHelper.Configuration.CsvClassMap<CC.Data.ImportClient>
	{
		public ClientCsvMap()
		{
			Map(f => f.InternalId).Name("INTERNAL_ID").TypeConverter<StringTypeConverter>();
			Map(f => f.AgencyId).Name("ORG_ID").TypeConverter<AgencyIdTypeConverter>();
			Map(f => f.FirstName).Name("FIRST_NAME").TypeConverter<NameRegexStringConverter>();
			Map(f => f.LastName).Name("LAST_NAME").TypeConverter<NameRegexStringConverter>();
			Map(f => f.MiddleName).Name("MIDDLE_NAME").TypeConverter<StringTypeConverter>();
			Map(f => f.OtherFirstName).Name("OTHER_FIRST_NAME").TypeConverter<StringTypeConverter>();
			Map(f => f.OtherLastName).Name("OTHER_LAST_NAME").TypeConverter<StringTypeConverter>();
			Map(f => f.PrevFirstName).Name("PREVIOUS_FIRST_NAME").TypeConverter<StringTypeConverter>();
			Map(f => f.PrevLastName).Name("PREVIOUS_LAST_NAME").TypeConverter<StringTypeConverter>();
			Map(f => f.Address).Name("ADDRESS").TypeConverter<AddressRegexStringConverter>();
            Map(f => f.CountryId).Name("COUNTRY_OF_RESIDENCE").TypeConverter<CountryIdConverter>();
			Map(f => f.City).Name("CITY").TypeConverter<StringTypeConverter>();
			Map(f => f.ZIP).Name("ZIP").TypeConverter<StringTypeConverter>();
			Map(f => f.StateId).Name("STATE_CODE").TypeConverter<StateIdTypeConverter>();
			Map(f => f.BirthDate).Name("DOB").TypeConverter<InvariantDateTypeConverter>();
			Map(f => f.OtherDob).Name("OTHER_DOB").TypeConverter<InvariantDateTypeConverter>();
			Map(f => f.PobCity).Name("PLACE_OF_BIRTH_CITY").TypeConverter<StringTypeConverter>();
			Map(f => f.BirthCountryId).Name("PLACE_OF_BIRTH_COUNTRY").TypeConverter<BirthCountryIdConverter>();
			Map(f => f.NationalIdTypeId).Name("TYPE_OF_ID").TypeConverter<NationalIdTypeConverter>();
			Map(f => f.OtherId).Name("OTHER_GOV_ID").TypeConverter<StringTypeConverter>();
			Map(f => f.OtherIdTypeId).Name("OTHER_TYPE_OF_ID").TypeConverter<NationalIdTypeConverter>();
			Map(f => f.NationalId).Name("GOV_ID").TypeConverter<StringTypeConverter>();
			Map(f => f.DeceasedDate).Name("DOD").TypeConverter<InvariantDateTypeConverter>();
			Map(f => f.LeaveDate).Name("LEAVE_DATE").TypeConverter<InvariantDateTypeConverter>();
			Map(f => f.JoinDate).Name("JOIN_DATE").TypeConverter<InvariantDateTypeConverter>();
			Map(f => f.LeaveReasonId).Name("LEAVE_REASON_ID").TypeConverter<LeaveReasonIdConverter>();
            Map(f => f.Remarks).Name("Remarks").TypeConverter<StringTypeConverter>();
			Map(f => f.IsCeefRecipient).Name("Article 2 / CEEF recipient?");
			Map(f => f.CeefId).Name("Article 2 / CEEF registration number").TypeConverter<StringTypeConverter>();
			Map(f => f.AddCompName).Name("Name/s of any other compensation program/s").TypeConverter<StringTypeConverter>();
			Map(f => f.AddCompId).Name("Registration number/s").TypeConverter<StringTypeConverter>();
			Map(f => f.IncomeCriteriaComplied).Name("INCOME_CRITERIA_COMPLIED");
			Map(f => f.Gender).Name("Gender").TypeConverter<GenderConverter>();
			Map(f => f.Phone).Name("Phone").TypeConverter<StringTypeConverter>();
			Map(f => f.HomecareWaitlist).Name("Homecare Waitlist").TypeConverter<NullBoolTypeConverter>();
			Map(f => f.OtherServicesWaitlist).Name("Other Services Waitlist").TypeConverter<NullBoolTypeConverter>();
			Map(f => f.CommPrefsId).Name("Comm Prefs").TypeConverter<CommPrefConverter>();
			Map(f => f.CareReceivedId).Name("Care Received Via").TypeConverter<CareReceivedConverter>();
			Map(f => f.MAFDate).Name("MAF Date").TypeConverter<InvariantDateTypeConverter>();
			Map(f => f.MAF105Date).Name("MAF 105+ Date").TypeConverter<InvariantDateTypeConverter>();
           // Map(f => f.HAS2Date).Name("HAS2 Date").TypeConverter<InvariantDateTypeConverter>();
            Map(f => f.UnableToSign).Name("Unable To Sign").TypeConverter<NullBoolTypeConverter>();
            Map(f => f.NursingHome).Name("Nursing Home").TypeConverter<NullBoolTypeConverter>();
            Map(f => f.AssistedLiving).Name("Assisted Living").TypeConverter<NullBoolTypeConverter>();
        }

	}

    public class ExistingClientCsvMap : CsvHelper.Configuration.CsvClassMap<CC.Data.ImportClient>
	{
		public ExistingClientCsvMap()
			: base()
		{
			Map(f => f.ClientId).Name(CC.Data.Client.ccidColumnName);
            Map(f => f.InternalId).Name("INTERNAL_ID").TypeConverter<StringTypeConverter>();
            Map(f => f.AgencyId).Name("ORG_ID").TypeConverter<AgencyIdTypeConverter>();
			Map(f => f.FirstName).Name("FIRST_NAME").TypeConverter<NameRegexStringConverter>();
			Map(f => f.LastName).Name("LAST_NAME").TypeConverter<NameRegexStringConverter>();
            Map(f => f.MiddleName).Name("MIDDLE_NAME").TypeConverter<StringTypeConverter>();
            Map(f => f.OtherFirstName).Name("OTHER_FIRST_NAME").TypeConverter<StringTypeConverter>();
            Map(f => f.OtherLastName).Name("OTHER_LAST_NAME").TypeConverter<StringTypeConverter>();
            Map(f => f.PrevFirstName).Name("PREVIOUS_FIRST_NAME").TypeConverter<StringTypeConverter>();
            Map(f => f.PrevLastName).Name("PREVIOUS_LAST_NAME").TypeConverter<StringTypeConverter>();
			Map(f => f.Address).Name("ADDRESS").TypeConverter<AddressRegexStringConverter>();
            Map(f => f.City).Name("CITY").TypeConverter<StringTypeConverter>();
            Map(f => f.ZIP).Name("ZIP").TypeConverter<StringTypeConverter>();
            Map(f => f.StateId).Name("STATE_CODE").TypeConverter<StateIdTypeConverter>();
            Map(f => f.BirthDate).Name("DOB").TypeConverter<InvariantDateTypeConverter>();
            Map(f => f.OtherDob).Name("OTHER_DOB").TypeConverter<InvariantDateTypeConverter>();
            Map(f => f.PobCity).Name("PLACE_OF_BIRTH_CITY").TypeConverter<StringTypeConverter>();
            Map(f => f.NationalIdTypeId).Name("TYPE_OF_ID").TypeConverter<NationalIdTypeConverter>();
            Map(f => f.OtherId).Name("OTHER_GOV_ID").TypeConverter<StringTypeConverter>();
            Map(f => f.OtherIdTypeId).Name("OTHER_TYPE_OF_ID").TypeConverter<NationalIdTypeConverter>();
            Map(f => f.NationalId).Name("GOV_ID").TypeConverter<StringTypeConverter>();
            Map(f => f.DeceasedDate).Name("DOD").TypeConverter<InvariantDateTypeConverter>();
            Map(f => f.LeaveDate).Name("LEAVE_DATE").TypeConverter<InvariantDateTypeConverter>();
            Map(f => f.JoinDate).Name("JOIN_DATE").TypeConverter<InvariantDateTypeConverter>();
            Map(f => f.LeaveReasonId).Name("LEAVE_REASON_ID").TypeConverter<LeaveReasonIdConverter>();
            Map(f => f.Remarks).Name("Remarks").TypeConverter<StringTypeConverter>();
            Map(f => f.IsCeefRecipient).Name("Article 2 / CEEF recipient?");
            Map(f => f.CeefId).Name("Article 2 / CEEF registration number").TypeConverter<StringTypeConverter>();
            Map(f => f.AddCompName).Name("Name/s of any other compensation program/s").TypeConverter<StringTypeConverter>();
            Map(f => f.AddCompId).Name("Registration number/s").TypeConverter<StringTypeConverter>();
            Map(f => f.IncomeCriteriaComplied).Name("INCOME_CRITERIA_COMPLIED");
            Map(f => f.Gender).Name("Gender").TypeConverter<GenderConverter>();
			Map(f => f.Phone).Name("Phone").TypeConverter<StringTypeConverter>();
			Map(f => f.HomecareWaitlist).Name("Homecare Waitlist").TypeConverter<NullBoolTypeConverter>();
			Map(f => f.OtherServicesWaitlist).Name("Other Services Waitlist").TypeConverter<NullBoolTypeConverter>();
			Map(f => f.CommPrefsId).Name("Comm Prefs").TypeConverter<CommPrefConverter>();
			Map(f => f.CareReceivedId).Name("Care Received Via").TypeConverter<CareReceivedConverter>();
			Map(f => f.MAFDate).Name("MAF Date").TypeConverter<InvariantDateTypeConverter>();
			Map(f => f.MAF105Date).Name("MAF 105+ Date").TypeConverter<InvariantDateTypeConverter>();
          //  Map(f => f.HAS2Date).Name("HAS2 Date").TypeConverter<InvariantDateTypeConverter>();
            Map(f => f.UnableToSign).Name("Unable To Sign").TypeConverter<NullBoolTypeConverter>();
            Map(f => f.NursingHome).Name("Nursing Home").TypeConverter<NullBoolTypeConverter>();
            Map(f => f.AssistedLiving).Name("Assisted Living").TypeConverter<NullBoolTypeConverter>();
        }
	}


}