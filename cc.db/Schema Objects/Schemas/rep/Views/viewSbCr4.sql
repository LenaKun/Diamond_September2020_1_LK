CREATE VIEW [rep].[viewSbCr4] with schemabinding
	AS
		select cr.ClientId,
			cr.SubReportId,
			sr.MainReportId,
			bs.AgencyId,
			bs.AppBudgetId,
			bs.ServiceId,
			s.TypeId as ServiceTypeId,
			sum(isnull(cr.Rate,0) * isnull(ar.Quantity,0)) as Amount,
			sum(isnull(ar.Quantity,0)) as Quantity,
			COUNT_BIG(*) as CountBig
		from dbo.ClientReports as cr
			join dbo.ClientAmountReport as ar on cr.Id = ar.ClientReportId
			join dbo.SubReports as sr on cr.SubReportId = sr.Id
			join dbo.AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
			join dbo.Services as s on bs.ServiceId = s.Id
		where s.ReportingMethodId in (
				5 /*Homecare*/,
				14 /*HomecareWeekly*/
			)
		group by cr.ClientId,
			cr.SubReportId,
			sr.MainReportId,
			bs.AgencyId,
			bs.AppBudgetId,
			bs.ServiceId,
			s.TypeId
GO
CREATE UNIQUE CLUSTERED INDEX [IDX_CLUSTERED_viewSbCr4] ON [rep].[viewSbCr4] 
(
	[SubReportId] ASC,
	[ClientId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_viewSbCr4] ON [rep].[viewSbCr4] 
(
	[ClientId] ASC,
	[SubReportId] ASC,
	[MainReportId] ASC,
	[AgencyId] ASC,
	[AppBudgetId] ASC,
	[ServiceId] ASC,
	[ServiceTypeId] ASC
)
INCLUDE ( [Quantity],
[Amount]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
