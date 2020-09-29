update users set agencyid = u.agencyid, agencygroupid = u.agencygroupid, regionid = u.regionid
from users join
(
select 
id,
case roleid when 2 then agencyid else null end as agencyid,
case roleid when 32 then agencygroupid else null end as agencygroupid,
case roleid when 8 then regionid else null end as regionid
 from users as tmp
 ) as u on users.id = u.id
 where not (coalesce(users.agencyid,0)=coalesce(u.agencyid,0) 
 and coalesce(users.agencygroupid,0) = coalesce(u.agencygroupid,0) 
 and coalesce(users.regionid,0) = coalesce(u.regionid,0))
