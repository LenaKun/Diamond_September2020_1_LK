CREATE VIEW [dbo].[viewGgQuarterlyHcFls]
	AS 

select * from
(
	select distinct 
		apps.FundId,
		a.GroupId as AgencyGroupId,
		cr.clientid as ClientId, 
		datepart(year, cr.reportdate) as RepYear,
		fl.RelatedLevel as RelatedLevelId,	
		datepart(quarter, cr.reportdate) as RepQuarter 
	from 
	ViewClientReports as cr
	join clients as c on cr.clientid = c.id
	join agencies as a on c.AgencyId = a.Id
	join subreports as sr on cr.subreportid = sr.id
	join appbudgetservices  on sr.appbudgetserviceid = appbudgetservices.id
	join appBudgets on appbudgetservices.AppBudgetId = appbudgets.Id
	join apps on appBudgets.AppId = apps.Id
	join services as s on appbudgetservices.serviceid = s.id

	join (
		select  fss.startdate, fse.startdate as enddate, fss.functionalitylevelid, fss.clientid
		from functionalityscores as fss
		outer apply (select top 1 startdate from functionalityscores where startdate > fss.startdate and clientid = fss.clientid) as fse
	) as fs on cr.clientid = fs.clientid and fs.startdate <= cr.reportdate and (fs.enddate is null or fs.enddate > cr.reportdate)
	join functionalitylevels as fl on fs.functionalitylevelid  = fl.id
	where reportdate is not null and s.typeid=8
) as t
