CREATE VIEW [dbo].[viewDeceasedDateEntryReport]
	AS 
	
	SELECT c.Id as 'ClientId', co.RegionId as 'RegionId', co.Id as 'CountryId', ag.Id as 'AgencyGroupId', a.Name as 'AgencyName',
		c.DeceasedDate, h.UpdateDate
	FROM History as h
	join Clients as c on h.ReferenceId = c.Id
	join Agencies as a on c.AgencyId = a.Id
	join AgencyGroups as ag on a.GroupId = ag.Id
	join Countries as co on ag.CountryId = co.Id
	WHERE h.TableName = 'Clients' and h.FieldName = 'DeceasedDate' and ag.ExcludeFromReports = 0 and c.DeceasedDate is not null
