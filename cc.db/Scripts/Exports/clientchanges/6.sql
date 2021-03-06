

select distinct  c.id as [cc id], 
c.isceefrecipient as [Article 2 / CEEF recipient?],
--oc.[CLIENT_COMP_PROGRAM] ,
c.ceefid as [Article 2 / CEEF registration number], 
--oc.[COMP_PROG_REG_NUM] ,
c.addcompname as [Name/s of any other compensation program/s] , 
--oc.[AdditionalComp] ,
c.addcompid as [Registration number/s] 
from clients as c
join 
(
select client_id, 
coalesce([COMP_PROG_REG_NUM],'') as [COMP_PROG_REG_NUM] ,
coalesce([AdditionalComp],'') as [AdditionalComp],
coalesce([AdditionalCompNum],'') as [AdditionalCompNum],
case [CLIENT_COMP_PROGRAM] when 'y' then 1  else 0 end as [CLIENT_COMP_PROGRAM]
from oc as t
) as oc  on c.id = oc.client_id
where
(c.[isceefrecipient]<>oc.[CLIENT_COMP_PROGRAM] or (c.[isceefrecipient] is null and oc.[CLIENT_COMP_PROGRAM] is not null) or (c.[isceefrecipient] is not null and oc.[CLIENT_COMP_PROGRAM] is null)) or
(c.[ceefid]<>oc.[COMP_PROG_REG_NUM] or (c.[ceefid] is null and oc.[COMP_PROG_REG_NUM] is not null) or (c.[ceefid] is not null and oc.[COMP_PROG_REG_NUM] is null)) or
(c.[addcompname]<>oc.[AdditionalComp] or (c.[addcompname] is null and oc.[AdditionalComp] is not null) or (c.[addcompname] is not null and oc.[AdditionalComp] is null)) or
(c.[addcompid]<>oc.[AdditionalCompNum] or (c.[addcompid] is null and oc.[AdditionalCompNum] is not null) or (c.[addcompid] is not null and oc.[AdditionalCompNum] is null))
