use cc
go

select * from
(
select coalesce(c.masterid, c.id) as masterid, cr.subreportid, cr.rate, ar.reportdate, sum(ar.quantity) as quantity

from clientamountreport as ar 
join clientreports as cr on ar.clientreportid = cr.id
join clients as c on cr.clientid=c.id
join subreports as sr on cr.subreportid = sr.id
group by coalesce(c.masterid, c.id), cr.subreportid, cr.rate, ar.reportdate
) as t
where t.masterid=218683