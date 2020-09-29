select 
c.id as ccid, c.birthdate [current birthdate], c.otherdob as [other date of birth]
from clients as c
join oc on c.id = oc.client_id
where 
(c.birthdate<>oc.dob or (c.birthdate is null and oc.dob is not null) or (c.birthdate is not null and oc.dob is null))
