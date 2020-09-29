using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

using System.Web;
using CC.Data;

namespace CC.Web.Models.Import.ClientReports
{
	public static class ClientReportImportFactory
	{
		public static IClientReportImportHelper GetByReportingTypeId(int id, CC.Data.Services.IPermissionsBase permissions)
		{
			switch ((Service.ReportingMethods)id)
			{

				case Service.ReportingMethods.TotalCostWithListOfClientNames:
					return new CriImportHelper(permissions);

				case Service.ReportingMethods.ClientNamesAndCosts:
					return new CriClientNamesAndCostsHelper(permissions);

				case Service.ReportingMethods.ClientUnit:
					return new CriClientUnitHelper(permissions);

				case Service.ReportingMethods.ClientUnitAmount:
					return new CriQuantiyAmountHelper(permissions);

				case Service.ReportingMethods.Emergency:
					return new CriEmergencyHelper(permissions);

				case Service.ReportingMethods.Homecare:
					return new CriHcImportHelper(permissions);

				case Service.ReportingMethods.HomecareWeekly:
					return new CriHcWeeklyImportHelper(permissions);
                
                case Service.ReportingMethods.SupportiveCommunities:
                    return new CriSupportiveCommunitiesHelper(permissions);

				case Service.ReportingMethods.SoupKitchens:
					return new CriSoupKitchensHelper(permissions);

                case Service.ReportingMethods.ClientEventsCount:
                    return new CriClientEventsCountHelper(permissions);
				default:
					throw new NotImplementedException();


			}
		}

		
	}

	
	


}