select distinct  c.id as [cc id], 
c.Internalid as [internal id]
--oc.[Internal_Client_ID] 
from clients as c
join oc on c.id = oc.client_id
where

(c.[internalid]<>c.id or (c.[internalid] is null and c.id is not null) or (c.[internalid] is not null and c.id is null))
