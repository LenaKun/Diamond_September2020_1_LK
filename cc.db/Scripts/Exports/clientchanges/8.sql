--8 New Clients Export
select 
	c.Id as [CLIENT_ID],
	c.AgencyId as [ORG_ID],
	c.LastName as [LAST_NAME],
	c.FirstName as [FIRST_NAME],
	c.MiddleName as [MIDDLE_NAME],
	c.BirthDate as [DOB],
	c.Address as [ADDRESS],
	c.City as [CITY],
	c.ZIP as [ZIP],
	s.Code as [STATE_CODE],
	country.Code as [COUNTRY_CODE],
	nit.Name as [TYPE_OF_ID],
	c.NationalId as [SS],
	c.Phone as [PHONE],
	c.IsCeefRecipient as [CLIENT_COMP_PROGRAM],
	c.CeefId as [COMP_PROG_REG_NUM],
	c.AddCompName as [AdditionalComp],
	c.AddCompId as [AdditionalCompNum],
	case when c.deceaseddate is not null or c.leavereasonid = 2 then 1 else 0 end as [Deceased],
	c.DeceasedDate as [DOD],
	c.New_Client as [New_Client],
	c.PobCity as [Place_of_Birth_City],
	c.PobCountry as [Place_of_Birth_Country],
	c.EmigrationDate as [Date_Emigrated],
	c.PrevFirstName as [Previous_First_Name],
	c.PrevLastName as [Previous_Last_Name],
	c.CreatedAt as [Upload_Date],
	c.MatchFlag as [MatchFlag],
	fs.Name as [claim_status],
	c.MasterId as [CLIENT_MASTER_ID],
	c.InternalId as [Internal_Client_ID]
from clients as c
join agencies as a on c.agencyid = a.id
join agencygroups as ag on a.groupid=ag.id
join countries as country on ag.countryid = country.id
left outer join states as s on c.stateid = s.id
left outer join nationalidtypes as nit on c.nationalidtypeid = nit.id
left outer join fundstatuses as fs on c.fundstatusid = fs.id
where c.ApprovalStatusId = 1 --1=new
