CREATE FUNCTION [dbo].[fnRepDateValid]
(
	@LeaveDate datetime,
	@DeceasedDate datetime,
	@ReportDate datetime,
	@Remarks nvarchar(256),
	@SpecialEligible bit --  AustrianEligible or RomanianEligible

)
RETURNS bit
--checks if report date is valid for a client with given deceased date and leavedate
AS
BEGIN
	
	return	case when @leavedate < @reportdate then
				case when @SpecialEligible = 0 then
					case when dateadd(day, case when @deceasedDate is null then 0 else 90 end, @leaveDate) >= @reportDate then
						case when len(ltrim(rtrim(@remarks))) > 0 then 1 else 0 end
					else 0 end
				else 1 end
			else 1 end 
END
