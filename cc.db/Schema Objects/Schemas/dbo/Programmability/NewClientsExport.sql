CREATE PROCEDURE [dbo].[NewClientsExport]
	@approvalStatusId int
AS

declare  @t table( id int)
insert into @t 
select clients.id from clients 
join agencies as a on clients.AgencyId = a.Id
where ApprovalStatusId = @approvalStatusId and a.groupid not in (70823,70824,70825)--test agency groups (testser,Test_Ser_CC,JDC FSU TEST SER)

update c set ACPExported=1
from clients as c 
join @t as t on c.id = t.id

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
	bcountry.Name as [Place_of_Birth_Country],
	c.PrevFirstName as [Previous_First_Name],
	c.PrevLastName as [Previous_Last_Name],
	c.CreatedAt as [Upload_Date],
	c.MatchFlag as [MatchFlag],
	fs.Name as [claim_status],
	c.MasterId as [CLIENT_MASTER_ID],
	c.InternalId as [Internal_Client_ID],
	case when c.Gender = 1 then 'Female' when c.Gender = 0 then 'Male' else '' end as [Gender]
from @t as t
join clients as c  on t.id=c.id
join agencies as a on c.agencyid = a.id
join agencygroups as ag on a.groupid=ag.id
join countries as country on ag.countryid = country.id
join BirthCountries as bcountry on c.BirthCountryId = bcountry.Id
left outer join states as s on c.stateid = s.id
left outer join nationalidtypes as nit on c.nationalidtypeid = nit.id
left outer join fundstatuses as fs on c.fundstatusid = fs.id

	
RETURN 0
