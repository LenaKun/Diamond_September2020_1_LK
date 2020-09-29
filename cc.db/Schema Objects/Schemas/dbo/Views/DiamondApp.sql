CREATE VIEW [dbo].[DiamondApp]
	AS 
	SELECT a.Id as Org_ID, ser.Name as Ser, app.Id as AppID, app.StartDate as PeriodFromDate, DATEADD(DAY, -1, app.EndDate) as PeriodToDate, f.Id as FundID,
		f.Name as FundName, app.CcGrant as AppAmount, app.CurrencyId as AppAmountCurrencyCode
	FROM Apps app
	join Funds f on app.FundId = f.Id
	join AgencyGroups ser on app.AgencyGroupId = ser.Id
	join Agencies a on ser.Id = a.GroupId
