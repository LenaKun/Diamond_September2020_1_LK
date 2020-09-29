
CREATE PROCEDURE [dbo].[spSrHcDetails]
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

	declare @mrstart datetime, @mrend datetime, 
		@mrstatusid int, @agencyid int, @exceptionalHours bit,
		@userRoleId int, @MasterFundId int;
	
	select @userRoleId = RoleId from users where Id = @userId

	select @mrstart = mr.[Start],
		@mrend = mr.[End],
		@mrstatusid = mr.[StatusId],
		@exceptionalHours = s.ExceptionalHomeCareHours,
		@agencyId  = bs.AgencyId,
		@Cur = app.CurrencyId,
		@MasterFundId = f.MasterFundId
	from SubReports as sr
		join MainReports as mr on sr.MainReportId = mr.Id
		join AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
		join [dbo].[Services] as s on bs.ServiceId = s.Id
		join [dbo].[ServiceTypes] as [st] on [s].[TypeId] = [st].[Id]
		join AppBudgets as b on mr.AppBudgetId = b.Id
		join apps as app on b.AppId = app.Id
		join funds as f on app.fundId = f.Id
	where sr.Id = @subReportId;

	if @userRoleId = 128 and (@masterfundid <> 73 or dbo.fnMainReportSubmitted(@mrstatusid) = 0)
	begin
		return 1
	end
	else if not exists (select top 1 1 from dbo.AllowedAgencies(@userid) as t where t.AgencyId = @agencyid)
	begin
		return 1
	end
	else if dbo.fnMainReportSubmitted(@mrStatusId) = 1 
	begin
		;with clientReports as (
			select SubReportId,ClientId,Rate,Remarks,[0] as [Q1],[1] as [Q2],[2] as [Q3]
			from (
				select ar.Quantity,cr.Rate,cr.Remarks,cr.ClientId,cr.SubReportId,DATEDIFF(month, @mrstart, ar.reportdate) as MonthsOffset
				from dbo.ClientAmountReport as ar
				join dbo.ClientReports as cr on ar.ClientReportId = cr.Id
			) as t
			pivot (
				sum(Quantity)
				for MonthsOffset in ([0],[1],[2])
			) as pvt
		)
		,ordered as (
			select 
				c.FirstName
				,c.LastName
				,c.ApprovalStatusId
				,approvalStatus.Name as ApprovalStatusName
				,cr.ClientId
				,cr.Rate
				,cr.Q1
				,cr.Q2
				,cr.Q3
				,cr.Remarks
				,cast(
					case when a.GroupId = 20275 /* Keren Ser id */
						then dbo.fnCfsReported(c.NationalId, @mrStart)
					else null end
				as bit) as Cfs
				,ROW_NUMBER() over (order by
					(case when @sort = 'lastname' and @sortAsc = 0 then c.lastname when @sort = 'firstname' and @sortAsc = 0 then c.firstname end) desc,
					(case when @sort = 'lastname' and @sortAsc = 1 then c.lastname when @sort = 'firstname' and @sortAsc = 1 then c.firstname end)
				) as RowNumber
				--,(select monthlycap,reportdate from dbo.hccaps as cap where cap.clientid=c.id and cap.reportdate >= @mrstart and cap.reportdate < @mrend for xml auto, root('root')) as hccap
				--,cast((select StartDate,EndDate,HcCap from dbo.hccapstableraw as cap where  cap.clientid=c.id and cap.startdate < @mrend and (cap.enddate is null or cap.enddate > @mrstart) for xml auto) as xml) as hccaps
			from clientreports as cr
			join dbo.clients as c on cr.clientid = c.id
			join Agencies as a on c.AgencyId = a.Id
			join AgencyGroups as ag on a.GroupId = ag.Id
			join Countries as country on ag.CountryId = country.Id
			left outer join FundStatuses as fs on c.FundStatusId = fs.Id
			left outer join ApprovalStatuses as approvalStatus on c.ApprovalStatusId = approvalStatus.Id
			where subreportid = @subReportId
				and (@filterClientId is null or c.Id = @filterClientId)
				and (@filterClientName is null or (c.FirstName + ' ' + c.LastName) like ('%' + @filterClientName + '%'))
				and (@ssearch is null or (c.FirstName like '%' + @ssearch + '%' or c.LastName like '%' + @ssearch + '%'))
		)
		select cast(null as int) as ClientReportId,
			FirstName,
			LastName,
			cast(null as int) as ApprovalStatusId,
			ApprovalStatusName,
			ClientId,
			Rate,
			Q1,
			Q2,
			Q3,
			Remarks,
			Cfs
		from ordered
		where RowNumber > @skip and RowNumber < @skip + @take
		order by RowNumber
		
		select @displayCount = count(*)
		from (
			select ClientId, Rate
			from dbo.ClientReports as cr
			where subreportid = @subReportId
			group by ClientId, Rate
		) as cr
		join clients as c on cr.clientid = c.id
		where
				(@filterClientId is null or c.Id = @filterClientId)
			and (@filterClientName is null or (c.FirstName + ' ' + c.LastName) like ('%' + @filterClientName + '%'))
			and (@ssearch is null or (c.FirstName like '%' + @ssearch + '%' or c.LastName like '%' + @ssearch + '%'))
		return 0;
	end
	else if dbo.fnMainReportSubmitted(@mrStatusId) = 0
	begin
		declare @filtered table (
			ClientReportId int,
			FirstName mediumString,
			LastName mediumString,
			ApprovalStatusId int,
			ApprovalStatusName mediumString,
			ClientId int,
			Rate smallmoney,
			Q1 smallmoney,
			Q2 smallmoney,
			Q3 smallmoney,
			Remarks nvarchar(4000),
			AgencyGroupId int,
			NationalId longString null,
			RowNumber int
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
					,[0] as [Q1]
					,[1] as [Q2]
					,[2] as [Q3]
			from 
			(
				select ar.Quantity
					,ar.ClientReportId
					,cr.Rate
					,cr.Remarks
					,cr.ClientId
					,cr.SubReportId
					,DATEDIFF(month, @mrstart, ar.reportdate) as MonthsOffset
				from dbo.ClientAmountReport as ar
				join dbo.ClientReports as cr on ar.ClientReportId = cr.Id
				where cr.subreportid = @subReportId	
			) as t
			pivot
			(
				sum(Quantity)
				for MonthsOffset in ([0],[1],[2])
			) as pvt
		)
		--select top 1000 * from clientreports
		,agencyClients as (
			select 
				ClientId,
				FirstName,
				LastName,
				AgencyId,
				AgencyGroupId,
				NationalId,
				MasterId,
				ApprovalStatusId,
				ApprovalStatusName,
				JoinDate,
				case when  IncomeVerificationRequired = 1 and IncomeCriteriaComplied = 0 then 0 
					when @exceptionalHours = 1 and HcCap is null then 0
					when @exceptionalHours = 0 and ((HcCap is null or HcCap <= 0) and (DeceasedDate is null or 
						DATEADD(month, 2, DATEFROMPARTS(year(DeceasedDate), month(DeceasedDate), 1)) <= @mrstart)) then 0
					else 1 end as Eligible
			
			from
			(
				select 
					c.Id as ClientId,
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
					c.JoinDate,
					case when country.IncomeVerificationRequired = 1 and  isnull(fs.IncomeVerificationRequired, 1) = 1
						then 1 else 0 end as IncomeVerificationRequired
				from allowedClients as c 
				join Agencies as a on c.AgencyId = a.Id
				join AgencyGroups as ag on a.GroupId = ag.Id
				join Countries as country on ag.CountryId = country.Id
				left outer join FundStatuses as fs on c.FundStatusId = fs.Id
				left outer join ApprovalStatuses as approvalStatus on c.ApprovalStatusId = approvalStatus.Id
				left outer join dbo.HcCapsMonthlyTable(@mrstart, @mrend) as hcCap on c.Id = hcCap.clientid
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
			Rate,
			Q1,
			Q2,
			Q3,
			Remarks,
			AgencyGroupId,
			NationalId,
			RowNumber)
		select 
			cr.ClientReportId,
			c.FirstName,
			c.LastName,
			c.ApprovalStatusId,
			c.ApprovalStatusName,
			c.ClientId,
			cr.Rate,
			cr.Q1,
			cr.Q2,
			cr.Q3,
			cr.Remarks,
			c.AgencyGroupId,
			c.NationalId,
			ROW_NUMBER() over (order by (case when @sort = 'lastname' and @sortAsc = 0 then c.lastname
											  when @sort = 'firstname' and @sortAsc = 0 then c.firstname end) desc,
										(case when @sort = 'lastname' and @sortAsc = 1 then c.lastname
											  when @sort = 'firstname' and @sortAsc = 1 then c.firstname end) ) as RowNumber
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
			Rate,
			Q1,
			Q2,
			Q3,
			Remarks,
			cast (case when AgencyGroupId = 20275 /* Keren Ser id */
				then dbo.fnCfsReported(NationalId, @mrStart)
				else null
				end
			as bit) as Cfs
		from @filtered as filtered
		where RowNumber > @skip

		select @displayCount = count(ClientId) from @filtered

		return 0
	end
RETURN 1
