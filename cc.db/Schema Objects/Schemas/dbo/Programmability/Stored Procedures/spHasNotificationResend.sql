CREATE PROCEDURE [dbo].[spHasNotificationResend]
	@from date,
	@to date
AS
	select 
		t.ClientId as ClientId, 
		c.NationalId, 
		c.FirstName, 
		c.LastName, 
		t.ApprovalStatusId as NewApprovalStatusId, 
		p.ApprovalStatusId as OldApprovalStatusId, 
		a.Id as AgencyId,
		a.GroupId as AgencyGroupId, 
		c.LeaveReasonId,
		hc.Name as HcStatusName ,
		p.HcStatusId as OldHcStatusId ,
		t.HcStatusId as NewHcStatusId ,
		cntry.Name as CountryName ,
		bcntry.Name as BirthCountryName

	from clienthcstatuses as t
	outer apply (
		select top 1 *
		from clienthcstatuses as y
		where t.clientid = y.clientid and y.startdate < t.StartDate
		order by y.startdate desc
	) as p
	join clients as c on t.clientid = c.id
	join Agencies as a on c.AgencyId = a.id
	left outer join Countries as cntry on t.CountryId = cntry.Id
	left outer join BirthCountries as bcntry on t.BirthCountryId = bcntry.Id
	left outer join HcStatuses as hc on t.HcStatusId = hc.Id


	where 
		(t.StartDate >= @from and t.StartDate < @to) 
		and coalesce(t.FundStatusId, -1) <> coalesce(p.FundStatusId,-1)
		and coalesce(t.approvalstatusid, -1) = coalesce(p.approvalstatusid,-1)
		and coalesce(t.hcstatusid, -1) <> coalesce(p.hcstatusid,-1)


RETURN 0
