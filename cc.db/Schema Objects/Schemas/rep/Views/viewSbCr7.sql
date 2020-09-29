CREATE VIEW [rep].[viewSbCr7] with schemabinding
	AS 
	
		select cr.ClientId,
			cr.SubReportId,
			sr.MainReportId,
			bs.AgencyId,
			bs.AppBudgetId,
			bs.ServiceId,
			s.TypeId as ServiceTypeId,
			COUNT_BIG(*) as Quantity,
			sr.Amount as SubReportAmount
		from dbo.ClientReports as cr
			join dbo.SubReports as sr on cr.SubReportId = sr.Id
			join dbo.AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
			join dbo.Services as s on bs.ServiceId = s.Id
		where s.ReportingMethodId in (
				2 /*TotalCostWithListOfClientNames*/,
				8 /*ClientUnit*/
			)
		group by cr.ClientId,
			cr.SubReportId,
			sr.MainReportId,
			bs.AgencyId,
			bs.AgencyId,
			bs.AppBudgetId,
			bs.ServiceId,
			s.TypeId,
			sr.Amount
GO
CREATE UNIQUE CLUSTERED INDEX [IDX_CLUSTERED_viewSbCr7] ON [rep].[viewSbCr7] 
(
	[SubReportId] ASC,
	[ClientId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_viewSbCr7] ON [rep].[viewSbCr7] 
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
[SubReportAmount]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

