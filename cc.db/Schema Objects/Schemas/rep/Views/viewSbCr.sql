CREATE VIEW [rep].[viewSbCr]
	AS
		SELECT ClientId,
			SubReportId,
			MainReportId,
			AgencyId,
			AppBudgetId,
			ServiceId,
			ServiceTypeId,
			Amount,
			Quantity
		FROM [rep].[viewSbCr1] WITH (NOEXPAND)
		UNION ALL
		SELECT ClientId,
			SubReportId,
			MainReportId,
			AgencyId,
			AppBudgetId,
			ServiceId,
			ServiceTypeId,
			Amount,
			Quantity
		FROM [rep].[viewSbCr2] WITH (NOEXPAND)
		UNION ALL
		SELECT ClientId,
			SubReportId,
			MainReportId,
			AgencyId,
			AppBudgetId,
			ServiceId,
			ServiceTypeId,
			Amount,
			Quantity
		FROM [rep].[viewSbCr4] WITH (NOEXPAND)
		UNION ALL
		SELECT ClientId,
			SubReportId,
			MainReportId,
			AgencyId,
			AppBudgetId,
			ServiceId,
			ServiceTypeId,
			SubReportAmount/Quantity,
			Quantity
		FROM [rep].[viewSbCr5] WITH (NOEXPAND)
		UNION ALL
		SELECT ClientId,
			SubReportId,
			MainReportId,
			AgencyId,
			AppBudgetId,
			ServiceId,
			ServiceTypeId,
			Amount,
			Quantity
		FROM [rep].[viewSbCr6] WITH (NOEXPAND)
		UNION ALL
		SELECT ClientId,
			SubReportId,
			MainReportId,
			AgencyId,
			AppBudgetId,
			ServiceId,
			ServiceTypeId,
			SubReportAmount/Quantity,
			Quantity
		FROM [rep].[viewSbCr7] WITH (NOEXPAND)
GO
