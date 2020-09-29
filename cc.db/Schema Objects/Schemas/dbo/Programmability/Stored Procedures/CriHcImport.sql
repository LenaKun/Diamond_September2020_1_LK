CREATE PROCEDURE [dbo].[CriHcImport]
	@Id uniqueidentifier
AS

	SET XACT_ABORT ON

	declare @importid uniqueidentifier
	set  @importid=@id 

		declare @t table(id uniqueidentifier);

		with valid as
		(
			select i.SubReportId, 
					i.ClientId, 
					i.Rate,
					dbo.string_concat(i.Remarks) as Remarks
			from ImportClientReports as i
				join Clients as c on i.ClientId = c.id
				join SubReports as sr on i.SubReportId=sr.id
				join MainReports as mr on sr.MainReportId = mr.Id
				join AppBudgetServices on sr.AppBudgetServiceId = appbudgetservices.Id
				join agencies as a on c.AgencyId = A.Id
				left join [Services] s on AppBudgetServices.ServiceId=s.Id
				join AgencyGroups as ag on a.GroupId=ag.Id
				join Countries on ag.CountryId=Countries.Id
				left outer join FundStatuses on c.FundStatusId=FundStatuses.Id
				
				--ensure uniqueness
				join (select ClientId, [Date], Rate from ImportClientReports where ImportId = @importId group by ClientId, [Date], Rate having count(*) = 1 ) as u on i.ClientId = u.ClientId and i.[Date] = u.[Date] and i.Rate = u.Rate
				where i.ImportId = @importId and
				(--eligibility check
					(c.JoinDate < mr.[End]) and
					(dbo.fnRepDateValid(c.LeaveDate, c.DeceasedDate,  coalesce(i.Date,[End]), i.remarks, iif(c.AustrianEligible=1 or c.RomanianEligible=1,1,0)) = 1) and
					(c.AgencyId = appbudgetservices.AgencyId) and
					(i.[Date] between mr.[Start] and mr.[End]) and
					
					--(c.govhchours is not null)  and 
					(/*c.gfhours > 0 or*/exists
					(select top 1 1 from GrandfatherHours as gfhours where gfhours.ClientId = i.ClientId and gfhours.StartDate < mr.[End] and gfhours.[Value] > 0) or					
					 c.ExceptionalHours > 0 or exists 
						(select top 1 1 from FunctionalityScores as fs join FunctionalityLevels as fl on fs.functionalitylevelid=fl.id where fs.ClientId = i.ClientId and fs.StartDate < mr.[End] and fl.Hchourslimit>0)
					) and
					(exists (select top 1 1 from HomeCareEntitledPeriod as hcep where hcep.ClientId = i.ClientId and hcep.StartDate < mr.[End] and (hcep.EndDate is null or hcep.EndDate > mr.[Start]))) and
					-- clients that are marked as complied or which do not require verification
					-- the verfication is not requere if it is not set on the country level
					(c.IncomeCriteriaComplied = 1 or not (Countries.IncomeVerificationRequired = 1 and coalesce(FundStatuses.IncomeVerificationRequired, 1) = 1))
				)
			group by i.SubReportId, 
					 i.ClientId, 
					 i.Rate
		)
		merge clientreports as target
		using 
		(	
			select	i.SubReportId, 
					i.ClientId, 
					i.Rate,
					left(dbo.string_concat(i.Remarks),255) as Remarks
			from valid as i
			group by	i.SubReportId,
						i.ClientId,
						i.Rate
		) as source
		on	target.SubReportId = source.SubReportId and 
			target.ClientId = source.ClientId and 
			target.Rate = source.Rate
		when not matched by target then
			insert (SubReportId,ClientId,Rate,Remarks)
			values (source.SubReportId, source.ClientId, source.Rate, source.Remarks)
		when matched then
			update set remarks = source.Remarks;


		with valid as
		(
			select distinct i.* 
			from ImportClientReports as i
				join Clients as c on i.ClientId = c.id
				join SubReports as sr on i.SubReportId=sr.id
				join MainReports as mr on sr.MainReportId = mr.Id
				join AppBudgetServices on sr.AppBudgetServiceId = appbudgetservices.Id
				left join [Services] s on AppBudgetServices.ServiceId=s.Id
				join agencies as a on c.AgencyId = A.Id
				join AgencyGroups as ag on a.GroupId=ag.Id
				join Countries on ag.CountryId=Countries.Id
				left outer join FundStatuses on c.FundStatusId=FundStatuses.Id
				join (select ClientId, [Date], Rate from ImportClientReports where ImportId = @importId group by ClientId, [Date], Rate having count(*) = 1 ) as u on i.ClientId = u.ClientId and i.[Date] = u.[Date] and i.Rate = u.Rate
				where i.ImportId = @importId and
				(--eligibility check
					(c.JoinDate < coalesce(case when s.ReportingMethodId = 14 then dateadd(day, 1, i.Date) else dateadd(month, 1, i.Date) end, mr.[End])) and
					(dbo.fnRepDateValid(c.LeaveDate, c.DeceasedDate,  coalesce(i.Date,mr.[End]), i.remarks, iif(c.AustrianEligible=1 or c.RomanianEligible=1,1,0)) = 1) and
					(c.AgencyId = appbudgetservices.AgencyId) and
					(i.[Date] between mr.[Start] and mr.[End]) and
					--(c.govhchours is not null)  and 
					(/*c.gfhours > 0 or*/exists
					(select top 1 1 from GrandfatherHours as gfhours where gfhours.ClientId = i.ClientId and gfhours.StartDate < mr.[End] and gfhours.[Value] > 0) or
					c.ExceptionalHours > 0 or exists 
						(select top 1 1 from FunctionalityScores as fs join FunctionalityLevels as fl on fs.functionalitylevelid=fl.id where fs.ClientId = i.ClientId and fs.StartDate < mr.[End] and fl.Hchourslimit>0)
					) and
					(exists (select top 1 1 from HomeCareEntitledPeriod as hcep where hcep.ClientId = i.ClientId and hcep.StartDate < mr.[End] and (hcep.EndDate is null or hcep.EndDate > mr.[Start]))) and
					-- clients that are marked as complied or which do not require verification
					-- the verfication is not requere if it is not set on the country level
					(c.IncomeCriteriaComplied = 1 or not (Countries.IncomeVerificationRequired = 1 and coalesce(FundStatuses.IncomeVerificationRequired, 1) = 1))
				)
					and dbo.fnApprovalStatusValid(c.ApprovalStatusId, c.ApprovalStatusUpdated, s.TypeId,mr.Start, mr.[End],coalesce(i.[Remarks],i.[UniqueCircumstances]))=1
		
		)
		merge clientAmountReport as target
		using
		(
			--ignore duplicate ([date]) records
			select	i.Id,
					ClientReports.Id as ClientReportId,
					i.[Date],
					I.[Quantity]
				from valid as i
					join ClientReports on I.SubReportId = clientReports.SubReportId and
						I.ClientId = clientReports.ClientId and
						I.Rate = clientReports.rate
		) as source on target.ClientreportId = source.ClientReportId and target.[ReportDate] = source.[date]
		when matched then
			update set Quantity = source.Quantity
		when not matched by target then 
			insert (ClientReportId, ReportDate, Quantity)
			values (source.ClientReportId, source.[Date], source.Quantity)
		output source.Id into @t ;


		--clean up
		--delete used records
		delete ImportClientReports 
		from ImportClientReports join @t as t on ImportClientReports.Id = t.Id
		where ImportId=@importId and ReportTypeId=5;

		--delete the import record
		if not exists (select top 1 1 from ImportClientReports where ImportId = @importId)
		begin
			delete from Imports where Id=@importId;
		end

	SELECT @@ROWCOUNT


RETURN 0