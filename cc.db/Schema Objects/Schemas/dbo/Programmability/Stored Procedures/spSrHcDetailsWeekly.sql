
CREATE PROCEDURE [dbo].[spSrHcDetailsWeekly]
	@userId int,
	@subReportId int,
	@filterClientName nvarchar(50),
	@filterClientId int,
	@filterReportedOnly bit,
	@sort nvarchar(50),
	@sortAsc bit,
	@ssearch nvarchar(50),
	@skip int,
	@take int,
	@displayCount int output,
	@totalCount int output,
	@Cur char(3) output
AS

	if @userId is null 
		return 1
	/*
	*	parameters for table valued function and multiple subqueries
	*/
	declare @mrstart datetime, @mrend datetime, 
		@mrstatusid int, @agencyid int, @exceptionalHours bit,
		@userRoleId int, @dayOfWeek int, @startingWeek datetime, @diff int,
		@agencyGroupId int, @appId int;
	
	select @userRoleId = RoleId from users where Id = @userId

	select @mrstart = mr.[Start],
		@mrend = mr.[End],
		@mrstatusid = mr.[StatusId],
		@exceptionalHours = s.ExceptionalHomeCareHours,
		@agencyId  = bs.AgencyId,
		@Cur = app.CurrencyId,
		@agencyGroupId = a.GroupId,
		@appId = app.Id
	from SubReports as sr
		join MainReports as mr on sr.MainReportId = mr.Id
		join AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
		join Agencies a on bs.AgencyId = a.Id
		join [dbo].[Services] as s on bs.ServiceId = s.Id
		join [dbo].[ServiceTypes] as [st] on [s].[TypeId] = [st].[Id]
		join AppBudgets as b on mr.AppBudgetId = b.Id
		join apps as app on b.AppId = app.Id
	where sr.Id = @subReportId;

	set @dayOfWeek = dbo.GetStartWeekDay(@agencyGroupId, @appId)

	set @startingWeek = @mrstart

	if(@dayOfWeek is not null) 
		set @dayOfWeek = case when @dayOfWeek = 0 then 7 else @dayOfWeek end --sunday in sql DATEFIRST is 7 and in c# is 0
	else set @dayOfWeek = case when DATEPART(WEEKDAY, @mrstart) = 1 then 7 else DATEPART(WEEKDAY, @mrstart) - 1 end --weekday numbering is SUN = 1 till SAT = 7 (if datefirst is set to default, sunday = 7)

	--setting the starting week for week count purposes. weeks greater than 15 will be transformed to be between 1 to 15.
	if(case when (DATEPART(WEEKDAY, @mrstart)) = 1 then 7 else DATEPART(WEEKDAY, @mrstart) - 1 end <> @dayOfWeek and DATEPART(MONTH, @mrstart) > 1)
	begin
		set @diff = DATEPART(WEEKDAY, @mrstart)
		--dayOfWeek is set for DATEFIRST parameter, which is MON = 1 and SUN = 7, but weekday is SUN = 1 and SAT = 7
		if(@diff = 1) set @diff = 7
		else set @diff = @diff - 1
		set @startingWeek = DATEADD(DAY, @dayOfWeek - @diff, @startingWeek)
		if(@startingWeek > @mrstart)
			set @startingWeek = DATEADD(DAY, -7, @startingWeek)
	end

	set DATEFIRST @dayOfWeek --changing the satrting week day for weeks number

	

	declare @filtered table (
		ClientReportId int,
		FirstName mediumString,
		LastName mediumString,
		ApprovalStatusId int,
		ApprovalStatusName mediumString,
		ClientId int,
		MasterIdClcd int,
		HASId int,
		HASName mediumString,
		Rate smallmoney,
		W1 smallmoney,
		W2 smallmoney,
		W3 smallmoney,
		W4 smallmoney,
		W5 smallmoney,
		W6 smallmoney,
		W7 smallmoney,
		W8 smallmoney,
		W9 smallmoney,
		W10 smallmoney,
		W11 smallmoney,
		W12 smallmoney,
		W13 smallmoney,
		W14 smallmoney,
		W15 smallmoney,
		Remarks nvarchar(4000),
		AgencyGroupId int,
		NationalId longString null,
		RowNumber int,
		LeaveDate datetime,
		NursingHome int
	);
	
	/* Clients that are allowed to be retrieved for current user */
	with allowedClients as (
		select c.*
		from dbo.clients as c
		left outer join [dbo].[AllowedAgencies](@userid) as aa on c.AgencyId = aa.AgencyId
		where ((@userRoleId = 128 and c.GGReportedCount > 0) or aa.AgencyId is not null)
	) 
	--select top 1000 * from allowedclients

	/* pivoted clientreports */
	,clientReports as (
		select SubReportId
				,ClientReportId
				,ClientId
				,Rate
				,Remarks
				,[1] as [W1]
				,[2] as [W2]
				,[3] as [W3]
				,[4] as [W4]
				,[5] as [W5]
				,[6] as [W6]
				,[7] as [W7]
				,[8] as [W8]
				,[9] as [W9]
				,[10] as [W10]
				,[11] as [W11]
				,[12] as [W12]
				,[13] as [W13]
				,[14] as [W14]
				,[15] as [W15]
		from 
		(
			select ar.Quantity
				,ar.ClientReportId
				,cr.Rate
				,cr.Remarks
				,cr.ClientId
				,cr.SubReportId
				,case when DATEPART(MONTH, @mrstart) = 1 then DATEPART(WEEK, ar.ReportDate) else (DATEDIFF(DAY, @startingWeek, ar.ReportDate) / 7) + 1 end as WeeksOffset
			from dbo.ClientAmountReport as ar
			join dbo.ClientReports as cr on ar.ClientReportId = cr.Id
			where cr.subreportid = @subReportId	
		) as t
		pivot
		(
			sum(Quantity)
			for WeeksOffset in ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15])
		) as pvt
	)
	--select top 1000 * from clientreports
	,agencyClients as (
		select 
			ClientId,
			MasterIdClcd,
			FirstName,
			LastName,
			AgencyId,
			AgencyGroupId,
			NationalId,
			MasterId,
			ApprovalStatusId,
			ApprovalStatusName,
			HASId,
			HASName,
			JoinDate,
			LeaveDate,
			NursingHome,
			case when  IncomeVerificationRequired = 1 and IncomeCriteriaComplied = 0 then 0 
				when @exceptionalHours = 1 and HcCap is null then 0
				when @exceptionalHours = 0 and ((HcCap is null or HcCap <= 0) and (DeceasedDate is null or 
					DATEADD(day, (4 * 7) + 1, DeceasedDate) <= @mrstart)) then 0
				else 1 end as Eligible
			
		from
		(
			select 
				c.Id as ClientId,
				c.MasterIdClcd,
				MasterIdClcd as MasterId,
				c.FirstName,
				c.LastName,
				c.AgencyId,
				c.NationalId,
				a.GroupId as AgencyGroupId,
				c.ApprovalStatusId,
				c.ApprovalStatusUpdated,
				approvalStatus.Name as ApprovalStatusName,
				c.DeceasedDate,
				hcCap.HcCap,
				c.IncomeCriteriaComplied,
				has.Id as HASId,
				has.Name as HASName,
				c.JoinDate,
				c.LeaveDate,
				c.NursingHome,
				case when country.IncomeVerificationRequired = 1 and  isnull(fs.IncomeVerificationRequired, 1) = 1
					then 1 else 0 end as IncomeVerificationRequired
			from allowedClients as c 
			join Agencies as a on c.AgencyId = a.Id
			join AgencyGroups as ag on a.GroupId = ag.Id
			join Countries as country on ag.CountryId = country.Id
			left outer join FundStatuses as fs on c.FundStatusId = fs.Id
			left outer join ApprovalStatuses as approvalStatus on c.ApprovalStatusId = approvalStatus.Id
			left outer join dbo.HcCapsTable(@mrstart, @mrend) as hcCap on c.Id = hcCap.clientid
			outer apply (
				select top 1 ch.HcStatusId
				from ClientHcStatuses as ch
				where ch.ClientId = c.Id
				order by ch.StartDate desc) as chas 
			left outer join HcStatuses has on chas.HcStatusId = has.Id
			where c.AgencyId = @agencyid
		) as cc
	)
	insert into @filtered(
		ClientReportId,
		FirstName,
		LastName,
		ApprovalStatusId,
		ApprovalStatusName,
		ClientId,
		MasterIdClcd,
		HASId,
		HASName,
		Rate,
		W1,
		W2,
		W3,
		W4,
		W5,
		W6,
		W7,
		W8,
		W9,
		W10,
		W11,
		W12,
		W13,
		W14,
		W15,
		Remarks,
		AgencyGroupId,
		NationalId,
		RowNumber,
		LeaveDate,
		NursingHome)
	select 
		cr.ClientReportId,
		c.FirstName,
		c.LastName,
		c.ApprovalStatusId,
		c.ApprovalStatusName,
		c.ClientId,
		c.MasterIdClcd,
		c.HASId,
		case when c.HASName is not null then c.HASName else '' end as HASName,
		cr.Rate,
		cr.W1,
		cr.W2,
		cr.W3,
		cr.W4,
		cr.W5,
		cr.W6,
		cr.W7,
		cr.W8,
		cr.W9,
		cr.W10,
		cr.W11,
		cr.W12,
		cr.W13,
		cr.W14,
		cr.W15,
		cr.Remarks,
		c.AgencyGroupId,
		c.NationalId,
		ROW_NUMBER() over (order by (case when @sort = 'lastname' and @sortAsc = 0 then c.lastname
										  when @sort = 'firstname' and @sortAsc = 0 then c.firstname end) desc,
									(case when @sort = 'lastname' and @sortAsc = 1 then c.lastname
									      when @sort = 'firstname' and @sortAsc = 1 then c.firstname end) ) as RowNumber,
		c.LeaveDate,
		c.NursingHome
	from agencyClients as c
	left outer join clientReports as cr on c.ClientId = cr.ClientId
	
	where(
		cr.ClientReportId is not null or (dbo.fnMainReportSubmitted(@mrstatusid) = 0 and c.Eligible = 1 and @userRoleId <> 128 /* not bmf */)
		)
	and (@filterClientId is null or c.ClientId = @filterClientId)
	and (@filterClientName is null or (c.FirstName + ' ' + c.LastName) like ('%' + @filterClientName + '%'))
	and (@filterReportedOnly is null or (@filterReportedOnly = 1 and cr.ClientReportId is not null))
	and (@ssearch is null or (c.FirstName like '%' + @ssearch + '%' or c.LastName like '%' + @ssearch + '%'))
	select top (@take)
		ClientReportId,
		FirstName,
		LastName,
		ApprovalStatusId,
		ApprovalStatusName,
		ClientId,
		MasterIdClcd,
		HASId,
		HASName,
		Rate,
		W1,
		W2,
		W3,
		W4,
		W5,
		W6,
		W7,
		W8,
		W9,
		W10,
		W11,
		W12,
		W13,
		W14,
		W15,
		Remarks,
		LeaveDate,
		NursingHome,
		cast (case when AgencyGroupId = 20275 /* Keren Ser id */
			then dbo.fnCfsReported(NationalId, @mrStart)
			else null
			end
		as bit) as Cfs
	from @filtered as filtered
	where RowNumber > @skip and NursingHome <> 1

	select @displayCount = count(ClientId) from @filtered

	set DATEFIRST 7 --changing the starting week day back to default (sunday)

RETURN 0
