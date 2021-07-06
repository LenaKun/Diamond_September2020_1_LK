CREATE VIEW [dbo].[ValidClientView]
	AS 
	SELECT [i].[ImportId]
      ,[i].[RowIndex]
      ,[i].[Id]
      ,[i].[ClientId]
      ,[i].[MasterId]
      ,[agency].[Id] as AgencyId
      ,[i].[NationalId]
      ,[i].[NationalIdTypeId]
      ,[i].[FirstName]
      ,[i].[MiddleName]
      ,[i].[LastName]
      ,[i].[Phone]
      ,[i].[BirthDate]
      ,[i].[Address]
      ,[i].[City]
      ,[i].[StateId]
      ,[i].[ZIP]
      ,[i].[JoinDate]
      ,coalesce([i].LeaveDate, [i].DeceasedDate) as [LeaveDate]
      ,coalesce([i].LeaveReasonId, case when [i].DeceasedDate is not null then 2 /*(CC.Data.LeaveReasonEnum.Deceased)*/ else null end) as [LeaveReasonId]
      ,[i].[LeaveRemarks]
      ,coalesce([i].DeceasedDate, case when i.LeaveReasonId = 2 then [i].LeaveDate else null end) as [DeceasedDate]
      ,[i].[ApprovalStatusId]
      ,[i].[FundStatusId]
      ,[i].[IncomeCriteriaComplied]
      ,[i].[IncomeVerificationRequired]
      ,[i].[NaziPersecutionDetails]
      ,[i].[Remarks]
      ,[i].[PobCity]
      ,[i].[BirthCountryId]
	  ,[i].CountryId
      ,[i].[PrevFirstName]
      ,[i].[PrevLastName]
      ,[i].[OtherFirstName]
      ,[i].[OtherLastName]
      ,[i].[OtherDob]
      ,[i].[OtherId]
      ,[i].[OtherIdTypeId]
      ,[i].[OtherAddress]
      ,[i].[PreviousAddressInIsrael]
      ,[i].[CompensationProgramName]
      ,[i].[IsCeefRecipient]
      ,[i].[CeefId]
      ,[i].[AddCompName]
      ,[i].[AddCompId]
      --,[i].[GfHours]
      ,[i].[ExceptionalHours]
      ,[i].[MatchFlag]
      ,[i].[New_Client]
      ,[i].[UpdatedById]
      ,[i].[UpdatedAt]
      ,[i].[CreatedAt]
	  ,[i].[InternalId]
	  ,agency.GroupId as AgencyGroupId
	  ,country.RegionId as RegionId
	  ,[i].Gender
	  ,[i].HomecareWaitlist
	  ,[i].OtherServicesWaitlist
	  ,[i].CommPrefsId
	  ,[i].CareReceivedId
	  ,[i].MAFDate
	  ,[i].MAF105Date
	  ,[i].HAS2Date
	  ,[i].UnableToSign
	  ,[i].NursingHome
	  ,[i].AssistedLiving
  from ImportClients as i
		join ApprovalStatuses as approvalStatus on i.ApprovalStatusId = approvalStatus.Id
		join Agencies as agency on i.AgencyId = agency.Id
		join AgencyGroups as agencyGroup on agency.GroupId = agencygroup.id
		join 
			(
			select c.id,c.RegionId, count(s.id) as StatesCount 
			from Countries as c left outer join states as s on c.id = s.Countryid
			group by c.id, c.RegionId
			) 
		as country on i.CountryId  = country.id
		join regions as region on country.RegionId = region.id

		left outer join States as states on i.StateId = states.id 
		join BirthCountries as bc on i.BirthCountryId = bc.Id
where 
			nullif(LTRIM(RTRIM(i.FirstName)),'') is not null
			and nullif(LTRIM(RTRIM(i.lastname)),'') is not null
			and i.AgencyId is not null
			and nullif(LTRIM(RTRIM(i.city)),'') is not null
			and nullif(LTRIM(RTRIM(i.[address])),'') is not null
			and i.BirthDate is not null
			and i.JoinDate is not null
			and bc.Id is not null
			--ivalidateobject
			and not (i.BirthDate is not null and i.JoinDate is not null and i.JoinDate < i.BirthDate) 
			and not (i.BirthDate is not null and i.BirthDate > convert(datetime,'1946-02-28'))

			and not (i.leaveDate is not null and i.leaveDate < i.JoinDate)
			and not (i.LeaveDate is not null and i.DeceasedDate is not null and i.DeceasedDate < i.LeaveDate)
			and not (i.DeceasedDate is not null and i.JoinDate is not null and i.DeceasedDate < i.JoinDate)
			and not (i.DeceasedDate is not null and i.BirthDate is not null and i.DeceasedDate < i.BirthDate)
			and not (i.LeaveDate is not null and i.LeaveReasonId is null)
			and not (i.LeaveReasonId is not null and i.LeaveDate is null)
			and not (i.LeaveReasonId is not null and i.LeaveReasonId=2 and (i.DeceasedDate is null or i.LeaveDate is null))
			and not (country.StatesCount > 0 and (i.StateId is null or states.id is null)) 
			and not (i.StateId is not null and states.CountryId is not null  and states.CountryId <> country.id)
