CREATE VIEW [rep].[viewSbCr3] with schemabinding
	AS 
	
		select cr.ClientId,
			cr.SubReportId,
			sr.MainReportId,
			bs.AgencyId,
			bs.AppBudgetId,
			bs.ServiceId,
			s.TypeId as ServiceTypeId,
			sum(isnull(cr.Amount,0) + isnull(cr.Discretionary,0)) as Amount,
			COUNT_BIG(*) as Quantity
		from dbo.EmergencyReports as cr
			join dbo.SubReports as sr on cr.SubReportId = sr.Id
			join dbo.AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
			join dbo.Services as s on bs.ServiceId = s.Id
		where s.ReportingMethodId in (
				6 /*Emergency*/
			)
		group by cr.ClientId,
			cr.SubReportId,
			sr.MainReportId,
			bs.AgencyId,
			bs.AppBudgetId,
			bs.ServiceId,
			s.TypeId
GO
CREATE UNIQUE CLUSTERED INDEX [IDX_CLUSTERED_viewSbCr3] ON [rep].[viewSbCr3] 
(
	[SubReportId] ASC,
	[ClientId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_viewSbCr3] ON [rep].[viewSbCr3] 
(
	[ClientId] ASC,
	[SubReportId] ASC,
	[MainReportId] ASC,
	[AgencyId] ASC,
	[AppBudgetId] ASC,
	[ServiceId] ASC,
	[ServiceTypeId] ASC
)
INCLUDE ( [Amount]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
