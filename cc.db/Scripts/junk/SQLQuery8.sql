select * 
from clientreports as cr 
join clientamountreport as ar on cr.id = ar.clientreportid
where cr.clientid=48309
