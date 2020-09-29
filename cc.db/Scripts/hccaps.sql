
select * from
(
select t1.*,client.id as clientid, dbo.hccap(t1.StartDate, t1.EndDate, client.Id) as Cap 
,client.agencyid, agencies.name, client.govhchours, client.gfhours, client.joindate, client.leavedate
,h.lastupdatedate as govhchoursupdatedate
,t2.Quantity as ClientQuantity, t2.maxsubmitdate
from
dbo.clients as client
join
(
	select app.Id as AppId,  c.Id as ClientId, sum(ar.Quantity) as Quantity, min(mr.Start) as StartDate, max(mr.[End]) as EndDate, max(mr.submittedat) as maxsubmitdate
	from clientamountreport as ar
	join clientreports as cr on ar.clientreportid=cr.id
	join clients as c on cr.ClientId = c.Id
	join subreports as sr on cr.subreportid=sr.id
	join appbudgetservices as appbudgetservice on sr.appbudgetserviceid = appbudgetservice.id
	join services as s on appbudgetservice.serviceid = s.id
	join servicetypes as st on s.typeid=st.id
	join mainreports as mr on sr.mainreportid = mr.id
	join appbudgets as ab on mr.appbudgetid=ab.id
	join apps as app on ab.appid = app.id
	where s.EnforceTypeConstraints = 1 and s.ReportingMethodId = 5 and mr.statusid in (2,8) --and cr.clientid=218683
	group by app.Id, c.Id
) as t2 on client.id = t2.ClientId
join agencies on client.agencyid = agencies.id
left outer join
(
select referenceid, max(updatedate) as lastUpdatedate
from history 
where tablename='clients' and fieldname='govhchours' 
group by referenceid
) as h on client.id = h.referenceid

join (
	select app.Id as AppId, coalesce(c.MasterId, c.Id) as MasterId, sum(ar.Quantity) as Quantity, min(mr.Start) as StartDate, max(mr.[End]) as EndDate
	from clientamountreport as ar
	join clientreports as cr on ar.clientreportid=cr.id
	join clients as c on cr.ClientId = c.Id
	join subreports as sr on cr.subreportid=sr.id
	join appbudgetservices as appbudgetservice on sr.appbudgetserviceid = appbudgetservice.id
	join services as s on appbudgetservice.serviceid = s.id
	join servicetypes as st on s.typeid=st.id
	join mainreports as mr on sr.mainreportid = mr.id
	join appbudgets as ab on mr.appbudgetid=ab.id
	join apps as app on ab.appid = app.id
	where s.EnforceTypeConstraints = 1 and s.ReportingMethodId = 5 and mr.statusid in (2,8) --and cr.clientid=218683
	group by app.Id, coalesce(c.MasterId,c.Id)
) as t1 on coalesce(client.masterid,client.Id) = t1.MasterId and t2.AppId = t1.AppId


) as t2 where t2.Quantity > t2.Cap
order by t2.maxsubmitdate, appid, masterid,clientid, startdate