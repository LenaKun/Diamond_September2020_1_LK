CREATE PROCEDURE [dbo].[ImportClientFundStatusProc]
	@importId uniqueidentifier
AS
	
	declare @t table(
		Id int, 
		NationalId nvarchar(50), 
		AgencyId int, 
		FirstName nvarchar(50), 
		LastName nvarchar(50), 
		OldApprovalStatusId int, 
		NewApprovalStatusId int, 
		LeaveReasonId int null,
		CountryId int,
		BirthCountryId int,
		DeceasedDate datetime null

		
	);
	declare @t2 table(
		Id int, 
		OldHcStatusId int
	);

	declare @d datetime = getdate();

	insert into @t2 (id, OldHcStatusId)
	select importclients.ClientId,
		(select top 1 HcStatusId from dbo.ClientHcStatuses as e where e.ClientId = ImportClients.ClientId order by e.StartDate desc)
	from ImportClients
	where ImportClients.ImportId = @importid

	merge clients as target
	using 
	(
		select importclients.id, 
			importclients.ClientId, 
			fundstatuses.id as FundStatusId, 
			approvalstatuses.id as ApprovalStatusId,
			importclients.UpdatedById,
			importclients.LeaveReasonId,
			importclients.DeceasedDate
		from ImportClients
			join FundStatuses on importclients.fundstatusid = fundstatuses.id
			join approvalstatuses on fundstatuses.approvalstatusname = approvalstatuses.name
		where ImportClients.ImportId = @importid
	) as source on target.Id = source.ClientId
	when matched then
	update  set FundstatusId = source.FundStatusId, 
				ApprovalStatusId = source.ApprovalStatusId,
				UpdatedById = source.UpdatedById,
				UpdatedAt = @d,
				ApprovalStatusUpdated = @d
	output inserted.id, 
		inserted.NationalId, 
		inserted.AgencyId, 
		inserted.FirstName, 
		inserted.LastName, 
		deleted.ApprovalStatusId, 
		inserted.ApprovalStatusId, 
		inserted.LeaveReasonId,
		inserted.CountryId,
		inserted.BirthCountryId,
		inserted.DeceasedDate

		
	into @t;


	delete i 
	from importclients as i join @t as t on  i.id = t.id
	where i.ImportId = @importId;


	if not exists (select top 1 1 from imports where imports.id = @importId)
	begin
		delete from imports where imports.id = @importId;
	end
	
	select t.Id as ClientId, t.NationalId, t.FirstName, t.LastName, t.NewApprovalStatusId, t.OldApprovalStatusId, a.Id as AgencyId, a.GroupId as AgencyGroupId, t.LeaveReasonId, t.DeceasedDate
	,hc.HcStatusName as HcStatusName 
	,t2.OldHcStatusId
	,hc.HcStatusId as NewHcStatusId
	,cntry.Name as CountryName
	,bcntry.Name as BirthCountryName
	

	from @t as t
	join Agencies as a on t.AgencyId = a.id
	left outer join @t2 as t2 on t.id = t2.Id
	left outer join Countries as cntry on t.CountryId = cntry.Id
	left outer join BirthCountries as bcntry on t.BirthCountryId = bcntry.Id
	outer apply (
		select top 1 HcStatusId, 
			ext2.Name as HcStatusName 
		from dbo.ClientHcStatuses as ext1 
		join dbo.HcStatuses as ext2 on ext1.HcStatusId = ext2.Id
		where ext1.ClientId = t.Id
		order by ext1.StartDate desc
	) as hc
	where	(t.NewApprovalStatusId <> t.OldApprovalStatusId) or 
			(t.NewApprovalStatusId is null and t.OldApprovalStatusId is not null) or 
			(t.NewApprovalStatusId is not null and t.OldApprovalStatusId is null) or
			(t2.OldHcStatusId <> hc.HcStatusId) or
			(t2.OldHcStatusId is null and hc.HcStatusId is not null) or
			(t2.OldHcStatusId is not null and hc.HcStatusId is null)

RETURN 0
