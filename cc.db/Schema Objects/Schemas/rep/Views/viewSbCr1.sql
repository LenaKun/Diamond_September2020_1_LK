CREATE VIEW [rep].[viewSbCr1] with schemabinding
	AS 
	select cr.ClientId,
		cr.SubReportId,
		sr.MainReportId,
		bs.AgencyId,
		bs.AppBudgetId,
		s.Id as ServiceId,
		s.TypeId as ServiceTypeId,
		sum(isnull(cr.Amount,0)) as Amount,
		sum(isnull(cr.Quantity,0)) as Quantity,
		COUNT_BIG(*) as CountBig
	from dbo.ClientReports as cr
		join dbo.SubReports as sr on cr.SubReportId = sr.Id
		join dbo.AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
		join dbo.Services as s on bs.ServiceId = s.Id
	where s.ReportingMethodId in (
		1 /*ClientNamesAndCosts*/,
		9 /*ClientUnitAmount*/
	)
	group by cr.ClientId,
		cr.SubReportId,
		sr.MainReportId,
		bs.AgencyId,
		bs.AgencyId,
		bs.AppBudgetId,
		s.Id,
		s.TypeId
GO
CREATE UNIQUE CLUSTERED INDEX [IDX_CLUSTERED_viewSbCr1] ON [rep].[viewSbCr1] 
(
	[SubReportId] ASC,
	[ClientId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_viewSbCr1] ON [rep].[viewSbCr1] 
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
