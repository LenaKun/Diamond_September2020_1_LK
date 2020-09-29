update s set reportingmethodid=7
from services as s inner join servicetypes as st on s.typeid = st.id
inner join 
(
select 'Client Transportation' as stn, 'Client Transportation' as sn
union select 'Medicine','Medicine'
union select 'Minor Home Modifications','Minor Home Modifications'
union select 'Other Services', 'Respite Care (Adult Day Care)'
union select 'Other Services','Psychiatric Care'
union select 'Other Services','Friendly Visitor Program'
union select 'Annual Home Care Assessment Fee','Annual Home Care Assessment Fee'
) as t on s.Name=t.sn and st.Name=t.stn

