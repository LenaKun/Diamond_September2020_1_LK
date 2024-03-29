select distinct   c.id as ccid, 
c.firstname as [first name],
oc.first_name, c.middlename as [Middle Name],c.lastname as [Last name], c.prevfirstname as [Previous first name], c.otherlastname as [Other last name], c.otherfirstname as [other first name]
from clients as c
join oc on c.id = oc.client_id
where
(c.[firstname]<>oc.[first_name] or (c.firstname is null and oc.first_name is not null) or (c.firstname is not null and oc.first_name is null)) or
(c.[middlename]<>oc.[middle_name] or (c.[middlename] is null and oc.[middle_name] is not null) or (c.[middlename] is not null and oc.[middle_name] is null)) or
(c.[lastname]<>oc.[last_name] or (c.[lastname] is null and oc.[last_name] is not null) or (c.[lastname] is not null and oc.[last_name] is null)) or 
(c.[prevfirstname]<>oc.[previous_first_name] or (c.[prevfirstname] is null and oc.[previous_first_name] is not null) or (c.[prevfirstname] is not null and oc.[previous_first_name] is null)) or
(c.[prevlastname]<>oc.[previous_last_name] or (c.[prevlastname] is null and oc.[previous_last_name] is not null) or (c.[prevlastname] is not null and oc.[previous_last_name] is null)) or
c.otherfirstname is not null or c.otherlastname is not null
