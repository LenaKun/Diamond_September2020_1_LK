
select distinct  c.id as [cc id], c.deceaseddate
from clients as c
join oc on c.id = oc.client_id
where
(c.[deceaseddate]<>oc.[dod] or (c.[deceaseddate] is null and oc.[dod] is not null) or (c.[deceaseddate] is not null and oc.[dod] is null))

