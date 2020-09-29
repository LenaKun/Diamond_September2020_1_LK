CREATE FUNCTION [dbo].[fnHomecareApprovalStatusId]
(
	@fundStatusId int,
	@approvalStatusId int,
	@birthCountryId int,
	@countryId int
)
RETURNS int
AS
BEGIN
	declare @result int
	
	--Condition 1: If client approval status (JNV status) is “Not Eligible”, then the homecare approval status is Not Eligible 
	select @result = case 
		when @approvalStatusId in (4)
			then 0-- 'Not Eligible'
		when @birthCountryId = (select top 1 id from dbo.BirthCountries where Name = 'Morocco') 
			then 3--'Approval 3'
		when @approvalStatusId = 2 and (select top 1 r.Name
										from countries as c
										join regions as r on c.regionid = r.id
										where c.id = @countryId) in ('Eastern Europe','FSU')
			then 1--'Approval 1'
			when @countryId = 252 --Armenia
			then 1--'Approval 1'
		when @fundStatusId is not null then (
				select top 1 s.Id
				from dbo.FundStatuses as t 
				join dbo.HcStatuses as s on t.HomecareApprovalStatusName = s.Name
				where t.Id = @fundStatusId
			)
		when @approvalStatusId in (1,16) --New and Pending 
		then 2 --'Approval 2'
	end
		
	return @result
		
END
