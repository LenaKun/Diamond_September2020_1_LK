using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace ConsoleApplication2
{
	[System.ComponentModel.TypeConverter(typeof(ClientCsvRowConverter))]
	class zz
	{
		//0
		//  [CsvHelper.Configuration.CsvFieldAttribute(Name="asdf")]
		public int CLIENT_ID { get; set; }

		//1
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public int ORG_ID { get; set; }

		//2
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string LAST_NAME { get; set; }
		//3
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string FIRST_NAME { get; set; }
		//4
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string MIDDLE_NAME { get; set; }
		//5
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		[TypeConverter(typeof(SpecialDateTimeConverter))]
		public DateTime? DOB { get; set; }
		//6
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string ADDRESS { get; set; }
		//7
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string CITY { get; set; }
		//8
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string ZIP { get; set; }
		//9
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string STATE_CODE { get; set; }
		//10
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string COUNTRY_CODE { get; set; }
		//11
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string TYPE_OF_ID { get; set; }
		//12
		[CsvHelper.Configuration.CsvFieldAttribute(Names=new string[]{"Gov_id","ss#"})]
		public string Gov_ID { get; set; }
		//13
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string PHONE { get; set; }
		//14
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		//[CsvHelper.TypeConversion.TypeConverter(typeof(YesNoBoolConverter))]
		[CsvHelper.TypeConversion.TypeConverter(typeof(SpecialBoolConverter))]
		public bool CLIENT_COMP_PROGRAM { get; set; }
		//15
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string COMP_PROG_REG_NUM { get; set; }
		//16
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string AdditionalComp { get; set; }
		//17
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string AdditionalCompNum { get; set; }
		//18
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		[CsvHelper.TypeConversion.TypeConverter(typeof(SpecialBoolConverter))]
		public bool Deceased { get; set; }
		//19
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public DateTime? DOD { get; set; }
		//20
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string New_Client { get; set; }
		//21
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string Place_of_Birth_City { get; set; }
		//22
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string Place_of_Birth_Country { get; set; }
		//23
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		[CsvHelper.TypeConversion.TypeConverter(typeof(SpecialDateTimeConverter))]

		public DateTime? Date_Emigrated { get; set; }
		//24
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string Previous_First_Name { get; set; }
		//25
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string Previous_Last_Name { get; set; }
		//26
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public DateTime? Upload_Date { get; set; }
		//27
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		[CsvHelper.TypeConversion.TypeConverter(typeof(SpecialBoolConverter))]
		public bool MatchFlag { get; set; }
		//28
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		[CsvHelper.TypeConversion.TypeConverter(typeof(FundStatusTypeConverter))]
		[CsvField(Names=new string[]{"Fund_Status", "claim_status"})]
		public FundStatus Fund_Status { get; set; }
		//29
		//  [CsvHelper.Configuration.CsvFieldAttribute()]
		public string CLIENT_MASTER_ID { get; set; }
	}
	
	//class asdf : Client
	//{
	//	[CsvField(Name = "CLIENT_ID")]
	//	public override int Id
	//	{
	//		get
	//		{
	//			return base.Id;
	//		}
	//		set
	//		{
	//			base.Id = value;
	//		}
	//	}

	//	[CsvField(Name="Org_Id")]
	//	[TypeConverter(typeof(AgencyConverter))]
	//	public override Agency Agency
	//	{
	//		get
	//		{
	//			return base.Agency;
	//		}
	//		set
	//		{
	//			base.Agency = value;
	//		}
	//	}

	//	[CsvField(Name="First_Name")]
	//	public override string FirstName
	//	{
	//		get
	//		{
	//			return base.FirstName;
	//		}
	//		set
	//		{
	//			base.FirstName = value;
	//		}
	//	}
	
	//	[CsvField(Name="Middle_name")]
	//	public override string MiddleName
	//	{
	//		get
	//		{
	//			return base.MiddleName;
	//		}
	//		set
	//		{
	//			base.MiddleName = value;
	//		}
	//	}
	
	//	[CsvField(Name="Last_name")]
	//	public override string LastName
	//	{
	//		get
	//		{
	//			return base.LastName;
	//		}
	//		set
	//		{
	//			base.LastName = value;
	//		}
	//	}
	//}


	//ORG_ID,LAST_NAME,FIRST_NAME,MIDDLE_NAME,DOB,ADDRESS,CITY,ZIP,STATE_CODE,COUNTRY_CODE,TYPE_OF_ID,SS#,PHONE,CLIENT_COMP_PROGRAM,COMP_PROG_REG_NUM,AdditionalComp,AdditionalCompNum,Deceased,DOD,New_Client,Place_of_Birth_City,Place_of_Birth_Country,Date_Emigrated,Previous_First_Name,Previous_Last_Name,Upload_Date,MatchFlag,claim_status,CLIENT_MASTER_ID,Internal_Client_ID,
}
