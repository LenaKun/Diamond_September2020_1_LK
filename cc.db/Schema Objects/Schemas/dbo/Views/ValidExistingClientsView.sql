CREATE VIEW [dbo].[ValidExistingClientsView]
	AS 

select 
	t.[ImportId]
,	t.[RowIndex]
,	t.Id
,	t.InternalId
,	t.ClientId
,	t.MasterId
,	t.AgencyId
,	t.NationalId
,	t.NationalIdTypeId
,	t.FirstName
,	t.MiddleName
,	t.LastName
,	t.Phone
,	t.BirthDate
,	t.[Address]
,	t.City
,	t.StateId
,	t.ZIP
,	t.JoinDate
,	t.LeaveDate
,	t.LeaveReasonId
,	t.LeaveRemarks
,	t.DeceasedDate
,	t.ApprovalStatusId
,	t.FundStatusId
,	t.IncomeCriteriaComplied
,	t.IncomeVerificationRequired
,	t.NaziPersecutionDetails
,	t.Remarks
,	t.PobCity
,	t.BirthCountryId
,   t.CountryId
,	t.PrevFirstName
,	t.PrevLastName
,	t.OtherFirstName
,	t.OtherLastName
,	t.OtherDob
,	t.OtherId
,	t.OtherIdTypeId
,	t.OtherAddress
,	t.PreviousAddressInIsrael
,	t.CompensationProgramName
,	t.IsCeefRecipient
,	t.CeefId
,	t.AddCompName
,	t.AddCompId
--,	t.GfHours
,	t.ExceptionalHours
,	t.MatchFlag
,	t.New_Client
,	t.UpdatedById
,	t.UpdatedAt
,	t.CreatedAt
,	t.Gender
,	agencyGroup.id as AgencyGroupId
,	country.RegionId as RegionId
,	t.HomecareWaitlist
,	t.OtherServicesWaitlist
,	t.CommPrefsId
,	t.CareReceivedId
,	t.MAFDate
,	t.MAF105Date
,   t.HAS2Date
,   t.UnableToSign
,   t.NursingHome
,   t.AssistedLiving
from
(
	Select
		i.Id,
		i.ClientId,
		i.ImportId,
		i.RowIndex,
		coalesce(i.InternalId, c.InternalId) as InternalId,
		coalesce(i.MasterId, c.MasterId) as MasterId,
		coalesce(i.AgencyId, c.AgencyId) as AgencyId,
		coalesce(i.NationalId, c.NationalId) as NationalId,
		coalesce(i.NationalIdTypeId, c.NationalIdTypeId) as NationalIdTypeId,
		coalesce(i.FirstName, c.FirstName) as FirstName,
		coalesce(i.MiddleName, c.MiddleName) as MiddleName,
		coalesce(i.LastName, c.LastName) as LastName,
		coalesce(i.Phone, c.Phone) as Phone,
		coalesce(i.BirthDate, c.BirthDate) as BirthDate,
		coalesce(i.[Address], c.[Address]) as [Address],
		coalesce(i.City, c.City) as City,
		coalesce(i.StateId, c.StateId) as StateId,
		coalesce(i.ZIP, c.ZIP) as ZIP,
		coalesce(i.JoinDate, c.JoinDate) as JoinDate,
		coalesce(i.LeaveDate, c.LeaveDate) as LeaveDate,
		coalesce(i.LeaveReasonId, c.LeaveReasonId) as LeaveReasonId,
		coalesce(i.LeaveRemarks, c.LeaveRemarks) as LeaveRemarks,
		coalesce(i.DeceasedDate, c.DeceasedDate) as DeceasedDate,
		coalesce(i.ApprovalStatusId, c.ApprovalStatusId) as ApprovalStatusId,
		coalesce(i.FundStatusId, c.FundStatusId) as FundStatusId,
		coalesce(i.IncomeCriteriaComplied, c.IncomeCriteriaComplied) as IncomeCriteriaComplied,
		coalesce(i.IncomeVerificationRequired, c.IncomeVerificationRequired) as IncomeVerificationRequired,
		coalesce(i.NaziPersecutionDetails, c.NaziPersecutionDetails) as NaziPersecutionDetails,
		coalesce(i.Remarks, c.Remarks) as Remarks,
		coalesce(i.PobCity, c.PobCity) as PobCity,
		coalesce(i.BirthCountryId, c.BirthCountryId) as BirthCountryId,
		coalesce(i.CountryId, c.CountryId) as CountryId,
		coalesce(i.PrevFirstName, c.PrevFirstName) as PrevFirstName,
		coalesce(i.PrevLastName, c.PrevLastName) as PrevLastName,
		coalesce(i.OtherFirstName, c.OtherFirstName) as OtherFirstName,
		coalesce(i.OtherLastName, c.OtherLastName) as OtherLastName,
		coalesce(i.OtherDob, c.OtherDob) as OtherDob,
		coalesce(i.OtherId, c.OtherId) as OtherId,
		coalesce(i.OtherIdTypeId, c.OtherIdTypeId) as OtherIdTypeId,
		coalesce(i.OtherAddress, c.OtherAddress) as OtherAddress,
		coalesce(i.PreviousAddressInIsrael, c.PreviousAddressInIsrael) as PreviousAddressInIsrael,
		coalesce(i.CompensationProgramName, c.CompensationProgramName) as CompensationProgramName,
		coalesce(i.IsCeefRecipient, c.IsCeefRecipient) as IsCeefRecipient,
		coalesce(i.CeefId, c.CeefId) as CeefId,
		coalesce(i.AddCompName, c.AddCompName) as AddCompName,
		coalesce(i.AddCompId, c.AddCompId) as AddCompId,
		--coalesce(i.GfHours, c.GfHours) as GfHours,
		coalesce(i.ExceptionalHours, c.ExceptionalHours) as ExceptionalHours,
		coalesce(i.MatchFlag, c.MatchFlag) as MatchFlag,
		coalesce(i.New_Client, c.New_Client) as New_Client,
		coalesce(i.UpdatedById, c.UpdatedById) as UpdatedById,
		coalesce(i.UpdatedAt, c.UpdatedAt) as UpdatedAt,
		coalesce(i.CreatedAt, c.CreatedAt) as CreatedAt,
		coalesce(i.Gender, c.Gender) as Gender,
		coalesce(i.HomecareWaitlist, c.HomecareWaitlist) as HomecareWaitlist,
		coalesce(i.OtherServicesWaitlist, c.OtherServicesWaitlist) as OtherServicesWaitlist,
		coalesce(i.CommPrefsId, c.CommPrefsId) as CommPrefsId,
		coalesce(i.CareReceivedId, c.CareReceivedId) as CareReceivedId,
		coalesce(i.MAFDate, c.MAFDate) as MAFDate,
		coalesce(i.MAF105Date, c.MAF105Date) as MAF105Date,
		coalesce(i.HAS2Date, c.HAS2Date) as  HAS2Date,
		coalesce(i.UnableToSign, c.UnableToSign) as UnableToSign,
		coalesce(i.NursingHome, c.NursingHome) as NursingHome,
		coalesce(i.AssistedLiving, c.AssistedLiving) as AssistedLiving
	from ImportClients as i
		left outer join clients as c on i.ClientId = c.Id
) as t
  
		left outer join (
			select importid, clientid from importclients
			group by importid, clientid
			having count(*) = 1
		) as ii on t.importid = ii.ImportId and t.clientid = ii.ClientId
		left outer join ApprovalStatuses as approvalStatus on t.ApprovalStatusId = approvalStatus.Id
		left outer join Agencies as agency on t.AgencyId = agency.Id
		left outer join AgencyGroups as agencyGroup on agency.GroupId = agencygroup.id
		left outer join BirthCountries as birthCountry on t.BirthCountryId = birthCountry.Id
		left outer join 
			(
			select c.id,c.RegionId, count(s.id) as StatesCount 
			from Countries as c left outer join states as s on c.id = s.Countryid
			group by c.id, c.RegionId
			) 
		as country on t.CountryId  = country.id
		left outer join regions as region on country.RegionId = region.id

		left outer join States as states on t.StateId = states.id 
where 

			(not (t.leaveDate is not null and t.JoinDate is not null and t.leaveDate < t.JoinDate))
			and (not (t.LeaveDate is not null and t.DeceasedDate is not null and t.DeceasedDate < t.LeaveDate))
			and (not (t.DeceasedDate is not null and t.JoinDate is not null and t.DeceasedDate < t.JoinDate))
			and (not (t.DeceasedDate is not null and t.BirthDate is not null and t.DeceasedDate < t.BirthDate))
			and (not (t.LeaveDate is not null and t.LeaveReasonId is null))
			and (not (t.LeaveReasonId is not null and t.LeaveDate is null))
			and (not (t.DeceasedDate is not null and t.LeaveDate is null))
			and (not (t.DeceasedDate is not null and t.LeaveReasonId is null))
			and (not (t.LeaveReasonId is not null and t.LeaveReasonId=2 and (t.DeceasedDate is null or t.LeaveDate is null)))
			and (not (t.StateId is not null and states.CountryId is not null  and states.CountryId <> country.id))
			and (not (t.BirthCountryId is not null and birthCountry.id is null))
			and (not (t.CountryId is not null and country.id is null))
			and (not (t.AgencyId is not null and agency.id is null))
			and (not (country.StatesCount>0 and states.Id is null))

