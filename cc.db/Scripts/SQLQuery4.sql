declare	@userId int =(select top 1 id from users where username='testadmin'),
	@subReportId int =14354,
	@filterClientName int,
	@filterClientId int,
	@filterReportedOnly int,
	@sort int,
	@sortAsc bit,
	@skip int,
	@take int=1000

	

	declare @mrstart datetime, @mrend datetime, @AgencyId int
	select @mrstart = mr.[Start],  
			@mrend = mr.[End],
			@AgencyId = bs.AgencyId
	from subreports as sr
		join mainreports as mr on sr.mainreportid = mr.id
		join appbudgetservices as bs on sr.appbudgetserviceid = bs.id
	where sr.id = @subreportid

	;with allowedClients as (
		select c.*
		from dbo.clients as c
		join [dbo].[AllowedAgencies](@userid) as aa on c.AgencyId = aa.AgencyId
		where c.AgencyId = @AgencyId
	),
	reportableClients as (
		select c.Id as ClientId,
			c.FirstName,
			c.LastName,
			c.NationalId,
			c.leaveDate,
			c.JoinDate,
			c.IncomeCriteriaComplied,
			case when c.IncomeVerificationRequired = 1 then 1
				when country.IncomeVerificationRequired = 1  and coalesce(fs.IncomeVerificationRequired,1) = 1 then  1
				else 0 
			end as IncomeVerificationRequired

		from allowedClients as c
		join dbo.Agencies as a on c.AgencyId = a.Id
		join dbo.AgencyGroups as ag on a.GroupId = ag.Id
		join dbo.Countries as country on ag.CountryId = country.id
		join dbo.FundStatuses as fs on c.FundStatusId = fs.Id
		where c.AgencyId = @AgencyId
	)

	,
	existingReports as (
		select
		MainReportId,
		SubReportId,
		ClientId,
		Rate,
		ClientReportId, pvt.[0] as Q1, pvt.[1] as Q2, pvt.[2] as Q3
		from (
			select ar.Quantity,
					ar.clientreportid,
					cr.clientid,
					cr.rate,
					cr.Remarks,
					cr.subreportid,
					sr.mainreportid,
					datediff(month, mr.Start, ar.ReportDate) as d
			from clientamountreport as ar
			join ClientReports as cr on ar.ClientReportId = cr.Id
			join SubReports as sr on cr.SubReportId = sr.Id
			join mainreports as mr on sr.mainreportid = mr.id
		) as t
		pivot (
			sum(quantity) 
			for d in ([0],[1],[2])
		) as pvt
		where subreportid = @subReportId
	),
	final as (
	
		select 
		c.Id as ClientId,
				cr.ClientReportId,
				c.ApprovalStatusId,
				c.FirstName,
				c.LastName,
				cr.Rate,
				cr.Q1,
				cr.Q2,
				cr.Q3
			
		from dbo.mainreports as mr
		join dbo.subreports as sr on mr.id = sr.MainReportId
		join dbo.AppBudgetServices as bs on sr.appbudgetserviceId = bs.Id
		join allowedClients as c on bs.AgencyId = c.AgencyId
		left outer join existingReports as cr on c.Id = cr.ClientId
		left outer join reportableClients as rc on c.Id = rc.ClientId
		where sr.Id = @subReportId 
			--include already existing reports or eligible clients
			and coalesce(cr.ClientId, rc.ClientId) is not null
	) 
	select top(@take) 
		*
	from final
	