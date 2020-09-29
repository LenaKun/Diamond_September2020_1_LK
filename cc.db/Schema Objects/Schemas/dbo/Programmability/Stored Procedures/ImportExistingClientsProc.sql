CREATE PROCEDURE [dbo].[ImportExistingClientsProc]
	@id uniqueidentifier,
	@agencyId int,
	@agencyGroupId int,
	@regionId int,
	@userId int
AS


	declare @t table(id int, NationalId nvarchar(50), careReceivedId int, originalLeaveDate datetime, insertedLeaveDate datetime);

	declare @isAdmin bit;
	select @isAdmin = case roleid when 1 then 1 else 0 end from Users where Id = @userId;

	with cc as
	( 
		select clients.* from clients
		join Agencies on clients.AgencyId = Agencies.id
		where coalesce(@agencyid, agencies.Id)= clients.AgencyId and coalesce(@agencygroupid, agencies.GroupId) = Agencies.Groupid 
	)
	merge
	cc
	as target
	using (
		select ValidexistingClientsView.* 
		from ValidexistingClientsView
		where  
			--current import
			ImportId = @id
			--premissions
			and coalesce(@agencyid, agencyid) = agencyId
			and coalesce(@agencygroupid, agencyGroupid)=agencyGroupid
			and coalesce(@regionid, regionid) = regionid
	) as source on target.id = source.clientid
	when matched then 
		update set	
			InternalId = source.InternalId,
			MasterId = source.MasterId,
			AgencyId = source.AgencyId,
			--The Government Issued ID field should be blocked for any Clients whose Approval Status is Approved.  Only the Admin should be able to change this field for Approved CC IDs
			NationalId = case when target.ApprovalStatusId!=2 or @isAdmin = 1 then source.NationalId else target.NationalId end,
			NationalIdTypeId = case when target.ApprovalStatusId!=2 or @isAdmin = 1 then source.NationalIdTypeId else target.NationalIdTypeId end,
			FirstName = case when target.ApprovalStatusId!=2 or @isAdmin = 1 then source.FirstName else target.FirstName end,
			MiddleName = case when target.ApprovalStatusId!=2 or @isAdmin = 1 then source.MiddleName else target.MiddleName end,
			LastName = case when target.ApprovalStatusId!=2 or @isAdmin = 1 then source.LastName else target.LastName end,
			Phone = source.Phone,
			BirthDate = source.BirthDate,
			[Address] = source.[Address],
			City = source.City,
			StateId = source.StateId,
			ZIP = source.ZIP,
			JoinDate = source.JoinDate,
			LeaveDate = case when @isAdmin = 0 and target.AdministrativeLeave = 1 then target.LeaveDate else source.LeaveDate end,
			LeaveReasonId = case when @isAdmin = 0 and target.AdministrativeLeave = 1 then target.LeaveReasonId else source.LeaveReasonId end,
			LeaveRemarks = source.LeaveRemarks,
			DeceasedDate = source.DeceasedDate,
			ApprovalStatusId = source.ApprovalStatusId,
			FundStatusId = source.FundStatusId,
			IncomeCriteriaComplied = source.IncomeCriteriaComplied,
			IncomeVerificationRequired = source.IncomeVerificationRequired,
			NaziPersecutionDetails = source.NaziPersecutionDetails,
			Remarks = source.Remarks,
			PobCity = source.PobCity,
			--BirthCountryId = source.BirthCountryId,
			--CountryId = source.CountryId,
			PrevFirstName = source.PrevFirstName,
			PrevLastName = source.PrevLastName,
			OtherFirstName = source.OtherFirstName,
			OtherLastName = source.OtherLastName,
			OtherDob = source.OtherDob,
			OtherId = source.OtherId,
			OtherIdTypeId = source.OtherIdTypeId,
			OtherAddress = source.OtherAddress,
			PreviousAddressInIsrael = source.PreviousAddressInIsrael,
			CompensationProgramName = source.CompensationProgramName,
			IsCeefRecipient = source.IsCeefRecipient,
			CeefId = source.CeefId,
			AddCompName = source.AddCompName,
			AddCompId = source.AddCompId,
			--GfHours = source.GfHours,
			--GovHcHours = source.GovHcHours,
			ExceptionalHours = source.ExceptionalHours,
			MatchFlag = source.MatchFlag,
			New_Client = source.New_Client,
			UpdatedById = source.UpdatedById,
			UpdatedAt = source.UpdatedAt,
			Gender = source.Gender,
			AdministrativeLeave = case when source.LeaveReasonId = 13 and @isAdmin = 1 then 1 else target.AdministrativeLeave end /*Not Eligible*/,
			HomecareWaitlist = source.HomecareWaitlist,
			OtherServicesWaitlist = source.OtherServicesWaitlist,
			CommPrefsId = source.CommPrefsId,
			CareReceivedId = source.CareReceivedId,
			MAFDate = source.MAFDate,
			MAF105Date = source.MAF105Date,
			UnableToSign = source.UnableToSign
	output source.clientid, inserted.NationalId, inserted.CareReceivedId, deleted.LeaveDate, inserted.LeaveDate into @t;
	
	delete i 
		from importclients as i join @t as t on  i.id = t.id
		where i.ImportId = @id;


	if not exists (select top 1 1 from imports where imports.id = @id)
	begin
		delete from imports where imports.id = @id;
	end

	update cfs
	set cfs.ClientResponseIsYes = 1, UpdatedById = @userId
	from CfsRows cfs
	join @t t on cfs.ClientId = t.id
	join CareReceivingOptions cro on t.careReceivedId = cro.Id
	where cro.Id = 3 /*Private Pay Family*/

	update c
	set EndDate = EOMONTH(GETDATE()), EndDateReasonId = 4, UpdatedById = @userId
	from @t t
	join CfsRows c on t.id = c.ClientId
	where c.EndDate is null and t.originalLeaveDate is null and t.insertedLeaveDate is not null
	
	select t.id, t.NationalId
	from @t as t
RETURN 0
