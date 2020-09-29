namespace Resources
{
	using CC.Data.Resources;
	using Resources.Abstract;
	using System.Globalization;	
	
	/// <summary>
	/// 
	/// </summary>
	public class Resource
	{
		private static IResourceProvider resourceProvider = new DbResourceProvider();
		public static void InvalidateCache()
		{
			resourceProvider.InvalidateCache();
		}
		public static string GetString(string key)
		{
			return (string)resourceProvider.GetResource(key, CultureInfo.CurrentCulture.Name);
		}
		public static string GetString(string key, string cultureName)
		{
			return (string)resourceProvider.GetResource(key, cultureName ?? CultureInfo.CurrentCulture.Name);
		}
		/* sql script to generate static props below
		 select 'public static string '+ [key]+ '{get{return GetString("'+[key] +'");}}'
		 from (select distinct [key] from dbo.Resources where Culture = 'en-us') as t
		 order by [key]
		*/
		public static string AdditionalComments { get { return GetString("AdditionalComments"); } }
		public static string Agency { get { return GetString("Agency"); } }
		public static string AgencyMismatch { get { return GetString("AgencyMismatch"); } }
		public static string AssessmentDate { get { return GetString("AssessmentDate"); } }
		public static string EffectiveDate { get { return GetString("EffectiveDate"); } }
		public static string AssessmentOutOfRange { get { return GetString("AssessmentOutOfRange"); } }
		public static string EffectiveOutOfRange { get { return GetString("EffectiveOutOfRange"); } }
		public static string Audit { get { return GetString("Audit"); } }
		public static string BackToList { get { return GetString("BackToList"); } }
		public static string Cancel { get { return GetString("Cancel"); } }
		public static string ClientFirstName { get { return GetString("ClientFirstName"); } }
		public static string ClientId { get { return GetString("ClientId"); } }
		public static string ClientLastName { get { return GetString("ClientLastName"); } }
		public static string ClientName { get { return GetString("ClientName"); } }
		public static string ClientNotFound { get { return GetString("ClientNotFound"); } }
		public static string Close { get { return GetString("Close"); } }
		public static string Complete { get { return GetString("Complete"); } }
		public static string Create { get { return GetString("Create"); } }
		public static string Created { get { return GetString("Created"); } }
		public static string CreateDaf { get { return GetString("CreateDaf"); } }
		public static string CreateDate { get { return GetString("CreateDate"); } }
		public static string DafDetails { get { return GetString("DafDetails"); } }
		public static string DafEditIsNotAllowed { get { return GetString("DafEditIsNotAllowed"); } }
		public static string DafId { get { return GetString("DafId"); } }
		public static string DafIsNotEditableInCurrentStatus { get { return GetString("DafIsNotEditableInCurrentStatus"); } }
		public static string DafList { get { return GetString("DafList"); } }
		public static string DafQ1 { get { return GetString("DafQ1"); } }
		public static string DafQ10 { get { return GetString("DafQ10"); } }
		public static string DafQ10A1 { get { return GetString("DafQ10A1"); } }
		public static string DafQ10A2 { get { return GetString("DafQ10A2"); } }
		public static string DafQ10A3 { get { return GetString("DafQ10A3"); } }
		public static string DafQ11 { get { return GetString("DafQ11"); } }
		public static string DafQ11A1 { get { return GetString("DafQ11A1"); } }
		public static string DafQ11A2 { get { return GetString("DafQ11A2"); } }
		public static string DafQ11A3 { get { return GetString("DafQ11A3"); } }
		public static string DafQ11A4 { get { return GetString("DafQ11A4"); } }
		public static string DafQ11A5 { get { return GetString("DafQ11A5"); } }
		public static string DafQ12 { get { return GetString("DafQ12"); } }
		public static string DafQ12A1 { get { return GetString("DafQ12A1"); } }
		public static string DafQ12A2 { get { return GetString("DafQ12A2"); } }
		public static string DafQ12A3 { get { return GetString("DafQ12A3"); } }
		public static string DafQ13 { get { return GetString("DafQ13"); } }
		public static string DafQ13A1 { get { return GetString("DafQ13A1"); } }
		public static string DafQ13A2 { get { return GetString("DafQ13A2"); } }
		public static string DafQ13A3 { get { return GetString("DafQ13A3"); } }
		public static string DafQ14 { get { return GetString("DafQ14"); } }
		public static string DafQ14A1 { get { return GetString("DafQ14A1"); } }
		public static string DafQ14A2 { get { return GetString("DafQ14A2"); } }
		public static string DafQ14A3 { get { return GetString("DafQ14A3"); } }
		public static string DafQ15 { get { return GetString("DafQ15"); } }
		public static string DafQ15A1 { get { return GetString("DafQ15A1"); } }
		public static string DafQ15A2 { get { return GetString("DafQ15A2"); } }
		public static string DafQ15A3 { get { return GetString("DafQ15A3"); } }
		public static string DafQ15A4 { get { return GetString("DafQ15A4"); } }
		public static string DafQ15A5 { get { return GetString("DafQ15A5"); } }
		public static string DafQ16 { get { return GetString("DafQ16"); } }
		public static string DafQ16A1 { get { return GetString("DafQ16A1"); } }
		public static string DafQ16A2 { get { return GetString("DafQ16A2"); } }
		public static string DafQ16A3 { get { return GetString("DafQ16A3"); } }
		public static string DafQ16A4 { get { return GetString("DafQ16A4"); } }
		public static string DafQ17 { get { return GetString("DafQ17"); } }
		public static string DafQ17A1 { get { return GetString("DafQ17A1"); } }
		public static string DafQ17A2 { get { return GetString("DafQ17A2"); } }
		public static string DafQ17A3 { get { return GetString("DafQ17A3"); } }
		public static string DafQ17A4 { get { return GetString("DafQ17A4"); } }
		public static string DafQ18 { get { return GetString("DafQ18"); } }
		public static string DafQ18A1 { get { return GetString("DafQ18A1"); } }
		public static string DafQ18A2 { get { return GetString("DafQ18A2"); } }
		public static string DafQ18A3 { get { return GetString("DafQ18A3"); } }
		public static string DafQ18A4 { get { return GetString("DafQ18A4"); } }
		public static string DafQ19 { get { return GetString("DafQ19"); } }
		public static string DafQ19A1 { get { return GetString("DafQ19A1"); } }
		public static string DafQ19A2 { get { return GetString("DafQ19A2"); } }
		public static string DafQ19A3 { get { return GetString("DafQ19A3"); } }
		public static string DafQ19A4 { get { return GetString("DafQ19A4"); } }
		public static string DafQ1A1 { get { return GetString("DafQ1A1"); } }
		public static string DafQ1A2 { get { return GetString("DafQ1A2"); } }
		public static string DafQ1A3 { get { return GetString("DafQ1A3"); } }
		public static string DafQ1A4 { get { return GetString("DafQ1A4"); } }
		public static string DafQ1A5 { get { return GetString("DafQ1A5"); } }
		public static string DafQ2 { get { return GetString("DafQ2"); } }
		public static string DafQ2A1 { get { return GetString("DafQ2A1"); } }
		public static string DafQ2A2 { get { return GetString("DafQ2A2"); } }
		public static string DafQ2A3 { get { return GetString("DafQ2A3"); } }
		public static string DafQ2A4 { get { return GetString("DafQ2A4"); } }
		public static string DafQ3 { get { return GetString("DafQ3"); } }
		public static string DafQ3A1 { get { return GetString("DafQ3A1"); } }
		public static string DafQ3A2 { get { return GetString("DafQ3A2"); } }
		public static string DafQ3A3 { get { return GetString("DafQ3A3"); } }
		public static string DafQ4 { get { return GetString("DafQ4"); } }
		public static string DafQ4A1 { get { return GetString("DafQ4A1"); } }
		public static string DafQ4A2 { get { return GetString("DafQ4A2"); } }
		public static string DafQ4A3 { get { return GetString("DafQ4A3"); } }
		public static string DafQ4A4 { get { return GetString("DafQ4A4"); } }
		public static string DafQ4A5 { get { return GetString("DafQ4A5"); } }
		public static string DafQ4A6 { get { return GetString("DafQ4A6"); } }
		public static string DafQ5 { get { return GetString("DafQ5"); } }
		public static string DafQ5A1 { get { return GetString("DafQ5A1"); } }
		public static string DafQ5A2 { get { return GetString("DafQ5A2"); } }
		public static string DafQ5A3 { get { return GetString("DafQ5A3"); } }
		public static string DafQ5A4 { get { return GetString("DafQ5A4"); } }
		public static string DafQ5A5 { get { return GetString("DafQ5A5"); } }
		public static string DafQ5A6 { get { return GetString("DafQ5A6"); } }
		public static string DafQ6 { get { return GetString("DafQ6"); } }
		public static string DafQ6A1 { get { return GetString("DafQ6A1"); } }
		public static string DafQ6A2 { get { return GetString("DafQ6A2"); } }
		public static string DafQ6A3 { get { return GetString("DafQ6A3"); } }
		public static string DafQ6A4 { get { return GetString("DafQ6A4"); } }
		public static string DafQ7 { get { return GetString("DafQ7"); } }
		public static string DafQ7A1 { get { return GetString("DafQ7A1"); } }
		public static string DafQ7A2 { get { return GetString("DafQ7A2"); } }
		public static string DafQ7A3 { get { return GetString("DafQ7A3"); } }
		public static string DafQ8 { get { return GetString("DafQ8"); } }
		public static string DafQ8A1 { get { return GetString("DafQ8A1"); } }
		public static string DafQ8A2 { get { return GetString("DafQ8A2"); } }
		public static string DafQ8A3 { get { return GetString("DafQ8A3"); } }
		public static string DafQ8A4 { get { return GetString("DafQ8A4"); } }
		public static string DafQ8A5 { get { return GetString("DafQ8A5"); } }
		public static string DafQ9 { get { return GetString("DafQ9"); } }
		public static string DafQ9A1 { get { return GetString("DafQ9A1"); } }
		public static string DafQ9A2 { get { return GetString("DafQ9A2"); } }
		public static string DafQ9A3 { get { return GetString("DafQ9A3"); } }
		public static string DafQ9A4 { get { return GetString("DafQ9A4"); } }
		public static string DafTakenOfflineOnAnotherDevice { get { return GetString("DafTakenOfflineOnAnotherDevice"); } }
		public static string DafSignDisclaimer { get { return GetString("DafSignDisclaimer"); } }
		public static string DafCompleteDisclaimer { get { return GetString("DafCompleteDisclaimer"); } }
		public static string DafRejectDisclaimer { get { return GetString("DafRejectDisclaimer"); } }
		public static string DafStatus { get { return GetString("DafStatus"); } }
		public static string DafStatusCompleted { get { return GetString("DafStatusCompleted"); } }
		public static string DafStatusOpen { get { return GetString("DafStatusOpen"); } }
		public static string DafStatusSigned { get { return GetString("DafStatusSigned"); } }
		public static string Date { get { return GetString("Date"); } }
		public static string DateFrom { get { return GetString("DateFrom"); } }
		public static string DateTo { get { return GetString("DateTo"); } }
		public static string Delete { get { return GetString("Delete"); } }
		public static string DeletedAt { get { return GetString("DeletedAt"); } }
		public static string DeletedAtFrom { get { return GetString("DeletedAtFrom"); } }
		public static string DeletedAtTo { get { return GetString("DeletedAtTo"); } }
		public static string DeletedDafDetails { get { return GetString("DeletedDafDetails"); } }
		public static string DeletedDafList { get { return GetString("DeletedDafList"); } }
		public static string DiagnostinAssessmentPerformedBySection { get { return GetString("DiagnostinAssessmentPerformedBySection"); } }
		public static string Evaluator { get { return GetString("Evaluator"); } }
		public static string EvaluatorCantBeChangedWhenDafIsNotOpen { get { return GetString("EvaluatorCantBeChangedWhenDafIsNotOpen"); } }
		public static string EvaluatorName { get { return GetString("EvaluatorName"); } }
		public static string EvaluatorNotFound { get { return GetString("EvaluatorNotFound"); } }
		public static string EvaluatorPosition { get { return GetString("EvaluatorPosition"); } }
		public static string EvaluatorSigned { get { return GetString("EvaluatorSigned"); } }
		public static string EvalueatorNotFound { get { return GetString("EvalueatorNotFound"); } }
		public static string ExceptionalHours { get { return GetString("ExceptionalHours"); } }
		public static string FieldName { get { return GetString("FieldName"); } }
		public static string Filter { get { return GetString("Filter"); } }
		public static string FilterBy { get { return GetString("FilterBy"); } }
		public static string FirstName { get { return GetString("FirstName"); } }
		public static string FunctionalCapacity { get { return GetString("FunctionalCapacity"); } }
		public static string Go { get { return GetString("Go"); } }
		public static string GovernmentHours { get { return GetString("GovernmentHours"); } }
		public static string LastName { get { return GetString("LastName"); } }
		public static string NewValue { get { return GetString("NewValue"); } }
		public static string NotAllowed { get { return GetString("NotAllowed"); } }
		public static string NotFound { get { return GetString("NotFound"); } }
		public static string OldValue { get { return GetString("OldValue"); } }
		public static string OnlyAdminIsAllowedToModifyEvaluatorField { get { return GetString("OnlyAdminIsAllowedToModifyEvaluatorField"); } }
		public static string OnlyOpenDafCanBeSigned { get { return GetString("OnlyOpenDafCanBeSigned"); } }
		public static string OnlySignedDafCanBeCompleted { get { return GetString("OnlySignedDafCanBeCompleted"); } }
		public static string OnlySignedDafCanBeRejected { get { return GetString("OnlySignedDafCanBeRejected"); } }
		public static string Password { get { return GetString("Password"); } }
		public static string Reject { get { return GetString("Reject"); } }
		public static string Reset { get { return GetString("Reset"); } }
		public static string ReviewedBy { get { return GetString("ReviewedBy"); } }
		public static string ReviewerSigned { get { return GetString("ReviewerSigned"); } }
		public static string Save { get { return GetString("Save"); } }
		public static string Search { get { return GetString("Search"); } }
		public static string SelectAnOption { get { return GetString("SelectAnOption"); } }
		public static string SER { get { return GetString("SER"); } }
		public static string Sign { get { return GetString("Sign"); } }
		public static string TotalNumericScore { get { return GetString("TotalNumericScore"); } }
		public static string TotalScore { get { return GetString("TotalScore"); } }
		public static string Updated { get { return GetString("Updated"); } }
		public static string UpdatedBy { get { return GetString("UpdatedBy"); } }
		public static string FileIsRequired { get { return GetString("FileIsRequired"); } }
		public static string DafCreateDisclaimer { get { return GetString("DafCreateDisclaimer"); } }
		public static string SignedDafUpload { get { return GetString("SignedDafUpload"); } }
		public static string DafCreateDisclaimerRequired { get { return GetString("DafCreateDisclaimerRequired"); } }
		public static string FormNotFullyFilled { get { return GetString("FormNotFullyFilled"); } }
	}
}