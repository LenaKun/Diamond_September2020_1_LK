CREATE VIEW [dbo].[viewClientDetails]
	AS

select 
				c.AgencyId,
				[country].RegionId,
				c.SC_Client,
				c.DCC_Client,
				c.GGReportedCount,
				c.CreatedAt,
				a.GroupId as AgencyGroupId,
				c.ApprovalStatusId,
				c.InternalId as InternalAgencyID,
				a.Id as ORGID,
				a.[Name] as Agency,
				c.FirstName as FirstName,
				c.LastName as LastName,
				c.MiddleName as MiddleName,
				c.OtherFirstName as OtherFirstName,
				c.OtherLastName as OtherLastName,
				c.PrevFirstName as PreviousFirstName,
				c.PrevLastName as PreviousLastName,
				c.Address as Address,
				c.City as City,
				c.ZIP as ZIP,
				[state].Code as State,
                [country].[Name] as CountryName,
				c.BirthDate as BirthDate,
				c.OtherDob as Otherdateofbirth,
				c.PobCity as BirthCity,
				[bcountry].Name as BirthCountry,
				[nationalidtype].Name as IDType,
				c.OtherId as OtherIDcard,
				[otherNatIdType].Name as OtherIDtype,
				c.NationalId as GovernmentIssuedID,
				c.DeceasedDate as DeceasedDate,
				c.LeaveDate as LeaveDate,
				c.JoinDate as JoinDate,
				[leavereason].Name as LeaveReason,
                c.Remarks as Remarks,
				c.IncomeCriteriaComplied as IncomeCriteriaComplied,
				c.Gender,
				c.Id as CCID,
				c.MasterId as MasterId,
				c.CreatedAt as CreateDate,
				[functionalityLevel].Name as FunctionalityLevelName,
      --          c.CurHcCap as HCHours,
				gf.Value as GrandfatheredHours,
				gf.StartDate as GFStartDate,
				case when gf.Type = 0 then 'Grandfathered'
				when gf.Type = 1 then 'Exceptional'  
				else 'BMF Approved' end as GFType,
				approvalStatus.Name as ApprovalStatus,
				c.NaziPersecutionDetails as NaziPersecutionDetails,
				c.CompensationProgramName as CompensationProgram,
				hcep.StartDate as HomecareEligibilityStartDate,
				dateadd(day, -1, hcep.EndDate) as HomecareEligibilityEndDate,
				gv.Value as GovtHChours,
				gv.StartDate as GovtHChoursStartDate,
				fs.DiagnosticScore as DiagnosticScore,
				fs.StartDate as HighestStartDateofDiagnosticScore,
				--(c.DeceasedDate != null || c.LeaveReasonId == (int)CC.Data.LeaveReasonEnum.Deceased) ? true : false as Deceased,
				c.OtherAddress as Otheraddress,
				c.PreviousAddressInIsrael as PreviousaddressinIsrael,
				c.Phone as Phone,
				c.AustrianEligible as AustrianEligible,
				c.RomanianEligible as RomanianEligible,
                cast(case when c.GGReportedCount > 0 then 1 else 0 end as bit)  as GGReportedOnly,
				wh.StartDate as UnmetNeedsStartDate,
				wh.WeeklyHours as UnmetNeedsValue,
				hcstatus.[Name] as HomecareApprovalStatusName,
                --cast(c.inreport ? "Yes" : "No" as AppearedAtLeastOnce,
				c.IsCeefRecipient,
				CeefId = c.CeefId,
				case when (hcs.HcStatusId = 1 or hcs.HcStatusId in (2,3) and fs.FunctionalityLevelId in (25,26,27,31)) and fs.StartDate >= '2017-01-01' then case when fs.StartDate < '2017-01-01' then case when coalesce(hcs.Cap, 0) <= coalesce(fs.HcHoursLimit, 0) then coalesce(hcs.Cap, 0) else coalesce(fs.HcHoursLimit, 0) end
					 when hcs.HcStatusId in (1,2,3) and fs.StartDate >= '2017-01-01' then fs.HcHoursLimit
					 else 0
				end -- hc status 1 and functionality any or hc status 2 or 3 and functionality < 4
				 when hcs.HcStatusId in (2,3) and fs.FunctionalityLevelId in (32,33,34) and fs.StartDate >= '2017-01-01' then -- hc status 2 or 3 and functionality 4, 5 or 6
					case when coalesce(gf.[Value], 0) > coalesce(hcs.Cap, 0) and coalesce(gf.[Value], 0) <= isnull(fs.HcHoursLimit, 0) then coalesce(gf.[Value], 0)
						 when coalesce(gf.[Value], 0) > coalesce(hcs.Cap, 0) and coalesce(gf.[Value], 0) > isnull(fs.HcHoursLimit, 0) then isnull(case when fs.StartDate < '2017-01-01' then case when coalesce(hcs.Cap, 0) <= coalesce(fs.HcHoursLimit, 0) then coalesce(hcs.Cap, 0) else coalesce(fs.HcHoursLimit, 0) end
					 when hcs.HcStatusId in (1,2,3) and fs.StartDate >= '2017-01-01' then fs.HcHoursLimit
					 else 0
				end, 0)
						 else coalesce(hcs.Cap, case when fs.StartDate < '2017-01-01' then case when coalesce(hcs.Cap, 0) <= coalesce(fs.HcHoursLimit, 0) then coalesce(hcs.Cap, 0) else coalesce(fs.HcHoursLimit, 0) end
					 when hcs.HcStatusId in (1,2,3) and fs.StartDate >= '2017-01-01' then fs.HcHoursLimit
					 else 0
				end, 0) end
				 when fs.StartDate < '2017-01-01' then coalesce(gf.[Value], case when fs.StartDate < '2017-01-01' then case when coalesce(hcs.Cap, 0) <= coalesce(fs.HcHoursLimit, 0) then coalesce(hcs.Cap, 0) else coalesce(fs.HcHoursLimit, 0) end
					 when hcs.HcStatusId in (1,2,3) and fs.StartDate >= '2017-01-01' then fs.HcHoursLimit
					 else 0
				end, 0)
				 else 0 
			end as HcHours,
			cast(case when exists(select top 1 * from rep.finSumDet where clientid = c.id) then 1 else 0 end as bit) as AppearedAtLeastOnce,
			c.HomecareWaitlist,
			c.OtherServicesWaitlist,
			c.MAFDate,
			c.MAF105Date,
			c.Has2Date,
			[commPref].Name as CommPrefs,
			c.CommPrefsId,
			[careOption].Name as CareReceivedVia,
			c.CareReceivedId,
			c.UnableToSign
from clients as c
join agencies as a on c.agencyid = a.Id
left outer join states as [state] on c.StateId = [state].Id
left outer join countries as [country] on c.CountryId = [country].Id
left outer join BirthCountries as [bcountry] on c.BirthCountryId = [bcountry].Id
left outer join NationalIdTypes as [nationalidtype] on c.NationalIdTypeId = [nationalidtype].Id
left outer join LeaveReasons as [leavereason] on c.LeaveReasonId = [leavereason].Id
left outer join ApprovalStatuses as [approvalStatus] on c.ApprovalStatusId = [approvalStatus].Id
left outer join NationalIdTypes as [otherNatIdType] on c.OtherIdTypeId = [otherNatIdType].Id
left outer join CommunicationsPreference as [commPref] on c.CommPrefsId = [commPref].Id
left outer join CareReceivingOptions as [careOption] on c.CareReceivedId = [careOption].Id
outer apply (
	select top 1 * from dbo.HomeCareEntitledPeriod as t
	where t.ClientId = c.Id and t.StartDate < getdate()
	order by t.StartDate desc
) as hcep
outer apply (
	select top 1 t.*, fl.HcHoursLimit from dbo.FunctionalityScores as t
	join dbo.FunctionalityLevels as fl on t.FunctionalityLevelId = fl.Id
	where t.ClientId = c.Id and t.StartDate <  getdate()
	order by t.StartDate desc
) as fs
outer apply (
	select top 1 t.HcStatusId, tt.Cap from dbo.ClientHcStatuses as t
	join dbo.HcStatuses as tt on t.HcStatusId = tt.Id
	where t.ClientId = c.Id and t.StartDate <  getdate()
	order by t.StartDate desc
) as hcs
outer apply (
	select top 1 * from dbo.GovHcHours as t
	where t.ClientId = c.Id and t.StartDate <  getdate()
	order by t.StartDate desc
) as gv
outer apply (
	select top 1 * from dbo.GrandfatherHours as t
	where t.ClientId = c.Id and t.StartDate <  getdate()
	order by t.StartDate desc
) as gf
outer apply (
	select top 1 * from dbo.UnmetNeeds as t
	where t.ClientId = c.Id and t.StartDate <  getdate()
	order by t.StartDate desc
) as wh
left outer join dbo.HcStatuses as hcstatus on hcs.HcStatusId = hcstatus.Id
left outer join dbo.FunctionalityLevels as [functionalityLevel] on fs.FunctionalityLevelId = [functionalityLevel].Id
