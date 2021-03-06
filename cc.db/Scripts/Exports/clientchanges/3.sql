select distinct c.id as [cc id], c.address, c.city, t.code, c.zip
from clients as c
	left outer join states as t on c.stateid = t.id
	join oc on c.id = oc.client_id
where 
(c.[address]<>oc.[address] or (c.[address] is null and oc.[address] is not null) or (c.[address] is not null and oc.[address] is null)) or
(t.name<>oc.state_code or (t.name is null and oc.state_code is not null) or (t.name is not null and oc.state_code is null)) or
(c.[city]<>oc.[city] or (c.[city] is null and oc.[city] is not null) or (c.[city] is not null and oc.[city] is null)) or
(c.[zip]<>oc.[zip] or (c.[zip] is null and oc.[zip] is not null) or (c.[zip] is not null and oc.[zip] is null))
