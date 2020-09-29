CREATE PROCEDURE [dbo].[ImportNewClientsProc]
	@id uniqueidentifier,
	@agencyId int,
	@agencyGroupId int,
	@regionId int
AS

	--this one will hold the ids of the rows of the importclients table that were used in the merge
	declare @t table(id int, NationalId nvarchar(50), careReceivedId int);

	
	--insert clients w/o ccid
	
	with cc as
	( 
		select clients.* from clients
		join Agencies on clients.AgencyId = Agencies.id
		where coalesce(@agencyid, agencies.Id)= clients.AgencyId and coalesce(@agencygroupid, agencies.GroupId) = Agencies.Groupid 
	)
	merge cc as target
	using 
	(
		select ValidClientView.* 
		from ValidClientView
		where  
			--current import
			ImportId = @id
			--premissions
			and coalesce(@agencyid, agencyid) = agencyId
			and coalesce(@agencygroupid, agencyGroupid) = agencyGroupid
			and coalesce(@regionid, regionid) = regionid 
	) as source on target.id = source.clientid
	when not matched by target and source.clientid is null then
	insert (InternalId, MasterId, AgencyId, NationalId, NationalIdTypeId, FirstName, MiddleName, LastName, Phone, BirthDate, [Address], City, StateId, ZIP,
		JoinDate, LeaveDate, LeaveReasonId, LeaveRemarks, DeceasedDate,
		ApprovalStatusId, FundStatusId, IncomeCriteriaComplied, IncomeVerificationRequired,
		NaziPersecutionDetails, Remarks, PobCity, BirthCountryId, CountryId, PrevFirstName, PrevLastName,
		OtherFirstName, OtherLastName, OtherDob, OtherId, OtherIdTypeId, OtherAddress, PreviousAddressInIsrael,
		CompensationProgramName,
		IsCeefRecipient, CeefId, AddCompName, AddCompId,
		--GfHours, 
		ExceptionalHours,
		MatchFlag, New_Client, 
		UpdatedById, UpdatedAt, CreatedAt,Gender, AdministrativeLeave, HomecareWaitlist, OtherServicesWaitlist, CommPrefsId, CareReceivedId, MAFDate, MAF105Date, HAS2Date, UnableToSign)
	values (source.InternalId, source.MasterId, source.AgencyId, source.NationalId, source.NationalIdTypeId, source.FirstName, source.MiddleName, source.LastName, source.Phone, source.BirthDate, source.[Address], source.City, source.StateId, source.ZIP,
		source.JoinDate, source.LeaveDate, source.LeaveReasonId, source.LeaveRemarks, source.DeceasedDate,
		source.ApprovalStatusId, source.FundStatusId, coalesce(source.IncomeCriteriaComplied,0), coalesce(source.IncomeVerificationRequired,0),
		source.NaziPersecutionDetails, source.Remarks, source.PobCity, source.BirthCountryId, source.CountryId, source.PrevFirstName, source.PrevLastName,
		source.OtherFirstName, source.OtherLastName, source.OtherDob, source.OtherId, source.OtherIdTypeId, source.OtherAddress, source.PreviousAddressInIsrael,
		source.CompensationProgramName,
		coalesce(source.IsCeefRecipient,0), source.CeefId, source.AddCompName, source.AddCompId,
		--source.GfHours,  
		source.ExceptionalHours,
		source.MatchFlag, source.New_Client,
		source.UpdatedById, source.UpdatedAt, source.CreatedAt, source.Gender, case when source.LeaveReasonId = 13 then 1 else 0 end /*Not Eligible*/, coalesce(source.HomecareWaitlist, 0), coalesce(source.OtherServicesWaitlist, 0),
		source.CommPrefsId, source.CareReceivedId, source.MAFDate, source.MAF105Date, source.HAS2Date, coalesce(source.UnableToSign, 0))
	output inserted.id, inserted.NationalId, inserted.CareReceivedId into @t;


	--insert new clients w/ccids
	set identity_insert clients on;

	with cc as
	( 
		select clients.* from clients
		join Agencies on clients.AgencyId = Agencies.id
		where coalesce(@agencyid, agencies.Id)= clients.AgencyId and coalesce(@agencygroupid, agencies.GroupId) = Agencies.Groupid 
	)
	merge cc as target
	using 
	(
		select ValidClientView.* 
		from ValidClientView
		where  
			--current import
			ImportId = @id
			--premissions
			and coalesce(@agencyid, agencyid) = agencyId
			and coalesce(@agencygroupid, agencyGroupid)=agencyGroupid
			and coalesce(@regionid, regionid)=regionid 
	) as source on target.id = source.clientid
	when not matched by target and source.clientid is not null then
	insert (Id, InternalId, MasterId, AgencyId, NationalId, FirstName, MiddleName, LastName, Phone, BirthDate, [Address], City, StateId, ZIP,
		JoinDate, LeaveDate, LeaveReasonId, LeaveRemarks, DeceasedDate,
		ApprovalStatusId, FundStatusId, IncomeCriteriaComplied, IncomeVerificationRequired,
		NaziPersecutionDetails, Remarks, PobCity, BirthCountryId, CountryId, PrevFirstName, PrevLastName,
		OtherFirstName, OtherLastName, OtherDob, OtherId, OtherIdTypeId, OtherAddress, PreviousAddressInIsrael,
		CompensationProgramName,
		IsCeefRecipient, CeefId, AddCompName, AddCompId,
		--GfHours, 
		ExceptionalHours,
		MatchFlag, New_Client, 
		UpdatedById, UpdatedAt, CreatedAt, Gender, AdministrativeLeave, CommPrefsId, CareReceivedId, MAFDate, MAF105Date)
	values (source.ClientId, source.InternalId, source.MasterId, source.AgencyId, source.NationalId, source.FirstName, source.MiddleName, source.LastName, source.Phone, source.BirthDate, source.[Address], source.City, source.StateId, source.ZIP,
		source.JoinDate, source.LeaveDate, source.LeaveReasonId, source.LeaveRemarks, source.DeceasedDate,
		source.ApprovalStatusId, source.FundStatusId, coalesce(source.IncomeCriteriaComplied,0), coalesce(source.IncomeVerificationRequired,0),
		source.NaziPersecutionDetails, source.Remarks, source.PobCity, source.BirthCountryId, source.CountryId, source.PrevFirstName, source.PrevLastName,
		source.OtherFirstName, source.OtherLastName, source.OtherDob, source.OtherId, source.OtherIdTypeId, source.OtherAddress, source.PreviousAddressInIsrael,
		source.CompensationProgramName,
		coalesce(source.IsCeefRecipient,0), source.CeefId, source.AddCompName, source.AddCompId,
		--source.GfHours, 
		source.ExceptionalHours,
		source.MatchFlag, source.New_Client,
		source.UpdatedById, source.UpdatedAt, source.CreatedAt, source.Gender, case when source.LeaveReasonId = 13 then 1 else 0 end /*Not Eligible*/, source.CommPrefsId, source.CareReceivedId, source.MAFDate, source.MAF105Date)
	output inserted.id, inserted.NationalId, inserted.CareReceivedId into @t;

	set identity_insert clients off;

	--cleanup
	delete i 
		from importclients as i join @t as t on  i.id = t.id
		where i.ImportId = @id;


	if not exists (select top 1 1 from imports where imports.id = @id)
	begin
		delete from imports where imports.id = @id;
	end

	update cfs
	set cfs.ClientResponseIsYes = 1
	from CfsRows cfs
	join @t t on cfs.ClientId = t.id
	join CareReceivingOptions cro on t.careReceivedId = cro.Id
	where cro.Id = 3 /*Private Pay Family*/

	select t.id, t.NationalId
	from @t as t
RETURN 0
