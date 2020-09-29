CREATE VIEW [dbo].[DiamondClientsByApp]
	AS 
	SELECT DISTINCT a.Id as Org_ID, ser.Name as Ser, app.Id as AppID, app.StartDate as PeriodFromDate, DATEADD(DAY, -1, app.EndDate) as PeriodToDate, c.Id as ClientID
	FROM Apps app
	join AgencyGroups ser on app.AgencyGroupId = ser.Id
	join Agencies a on ser.Id = a.GroupId
	join Clients c on a.Id = c.AgencyId
