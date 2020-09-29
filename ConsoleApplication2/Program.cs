using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CC.Data;
using CC.Web.Models;
namespace ConsoleApplication2
{
	class Program
	{
		static void Main(string[] args)
		{
			var filename = @"clients.csv";
			if (args.Length > 2 && !string.IsNullOrEmpty(args[1]))
			{
				filename = args[1];
			}


			DateTime start = DateTime.Now;
			int count = 0;
			var csvconf = new CsvHelper.Configuration.CsvConfiguration()
			{
				HasHeaderRecord = true,
				IsStrictMode = true,
				IsCaseSensitive = false,
				Quote = '"'
			};


			using (CsvHelper.CsvReader reader = new CsvHelper.CsvReader(new System.IO.StreamReader(filename), csvconf))
			{
				var provider = System.Globalization.CultureInfo.GetCultureInfo("en-gb");//.InvariantCulture;
				var rowConverter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(zz));
				System.Threading.Thread.CurrentThread.CurrentCulture = provider;
				User admin = null;
				using (var db = new ccEntities())
				{
					admin = db.Users.Single(f => f.UserName == "admin");
				}

				while (reader.Read())
				{
					zz r = null;
					count++;
					try { r = reader.GetRecord<zz>(); }
					catch (IndexOutOfRangeException) { r = reader.GetRecord<zz>(); }
					{

						Client client = null;
						try
						{
							client = (Client)rowConverter.ConvertTo(r, typeof(Client));
						}
						catch (InvalidOperationException ex)
						{
							Console.WriteLine("exception at row " + reader.Parser.Row.ToString() + ": " + ex.Message);
							Console.ReadKey();
						}
						var updateDate = DateTime.Now;
						client.UpdatedAt = updateDate;
						client.CreatedAt = updateDate;
						client.UpdatedById = admin.Id;

						using (var db = new ccEntities())
						{
							var existing = db.Clients.SingleOrDefault(f => f.Id == client.Id);
							if (existing == null)
							{
								if (client.Id == default(int))
								{
									db.Clients.AddObject(client);
								}
								else
								{
									var inserted = db.InsertClient(client.Id, client.FirstName, client.LastName, client.JoinDate, client.ApprovalStatusId, client.UpdatedById, client.UpdatedAt, client.CreatedAt).FirstOrDefault();
									var entry = db.ObjectStateManager.GetObjectStateEntry(inserted);
									entry.ApplyCurrentValues(client);
									db.SaveChanges();
								}
							}
							else
							{
								var entry = db.ObjectStateManager.GetObjectStateEntry(existing);
								entry.ApplyCurrentValues(client);

							}
							var rowsUpdated = db.SaveChanges();
							Console.WriteLine("Row: " + reader.Parser.Row + ", clientid:" + client.Id);
						}
						if (reader.Parser.Row % 100 == 0)
						{
							Console.WriteLine("elapsed: " + (DateTime.Now - start).TotalSeconds);
						}
					}
				}
			}

			Console.WriteLine("count: " + count + "elapsed: " + (DateTime.Now - start).TotalSeconds);

			Console.ReadKey();
		}

		private static DateTime? date(string value, string format)
		{
			DateTime t;
			if (DateTime.TryParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out t))
			{
				return t;
			}
			else
			{
				return null;
			}
		}

		private static void InsertClient(ccEntities db, Client client)
		{
			var clientproperties = typeof(Client).GetProperties();
			var method = typeof(ccEntities).GetMethod("Insert");
			var parameters = method.GetParameters();
			var values = new List<object>();
			var q = from p in parameters
					join prop in clientproperties on p.Name.ToLowerInvariant() equals prop.Name.ToLowerInvariant() into g
					from prop in g.DefaultIfEmpty()
					select prop == null ? null : prop.GetValue(client);
			//db.Insert(client.Id, client.MasterId, client.AgencyId, client.NationalId, client.NationalIdTypeId,
			//	client.FirstName, client.MiddleName, client.LastName, client.Phone, client.BirthDate, client.Address, client.City, client.StateId,
			//	client.ZIP, client.JoinDate, client.LeaveDate, client.LeaveReasonId, client.LeaveRemarks, client.DeceasedDate, client.ApprovalStatusId, client.FundStatusId, client.IncomeCriteriaComplied,
			//	client.IncomeVerificationRequired, client.NaziPersecutionDetails, client.Remarks, client.PobCity, client.PobCountry,
			//	client.EmigrationDate, client.PrevFirstName, client.PrevLastName, client.OtherFirstName, client.OtherLastName,
			//	client.OtherDob, client.OtherId, client.OtherIdTypeId, client.OtherAddress, client.PreviousAddressInIsrael,
			//	client.CompensationProgramName, client.IsCeefRecipient, client.CeefId, client.AddCompName, client.AddCompId, client.GfHours,
			//	client.GovHcHours, client.ExceptionalHours, client.UpdatedById, client.UpdatedAt, client.CreatedAt, client.SS_,
			//	client.A2, client.HARDSHIP, client.CEEF, client.CEEF, client.MatchFlag, client.UserNameMatch, client.DateMatch,
			//	client.Date_of_birth_was_not_matched, client.Provided_other_DOB, client.Identification_Card___was_not_matched,
			//	client.Provided_other_ID_Card, client.Other_addresses_required, client.Provided_other_addresses,
			//	client.Previous_address_in_Israel_required, client.Provided_previous_address_in_Israel, client.New_Client);

			var retval = method.Invoke(db, q.ToArray());
		}
	}






}
