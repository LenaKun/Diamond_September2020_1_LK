CREATE VIEW [dbo].[viewFunctionalityScoreChangeReport]
	AS 
	
	SELECT c.Id as 'ClientId', co.RegionId as 'RegionId', co.Id as 'CountryId', ag.Id as 'AgencyGroupId', a.Name as 'AgencyName', 
		fs.DiagnosticScore, fs.UpdatedAt, fs.StartDate
	FROM FunctionalityScores as fs
	join Clients as c on fs.ClientId = c.Id
	join Agencies as a on c.AgencyId = a.Id
	join AgencyGroups as ag on a.GroupId = ag.Id
	join Countries as co on ag.CountryId = co.Id
	WHERE ag.ExcludeFromReports = 0
