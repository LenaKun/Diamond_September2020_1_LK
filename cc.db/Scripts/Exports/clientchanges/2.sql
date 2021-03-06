select
c.id as [cc id], c.nationalid as [national id], t.name as [national type], t1.name as [Other ID type], c.otherid as [Other ID card]
from clients as c
left outer join nationalidtypes as t on c.nationalidtypeid = t.id
left outer join nationalidtypes as t1 on c.otheridtypeid = t1.id
join oc on c.id = oc.client_id
where 

(t.name<>oc.type_of_id or (t.name is null and oc.type_of_id is not null) or (t.name is not null and oc.type_of_id is null)) or
(c.nationalid<>oc.[ss#] or (c.nationalid is null and oc.[ss#] is not null) or (c.nationalid is not null and oc.[ss#] is null)) 
