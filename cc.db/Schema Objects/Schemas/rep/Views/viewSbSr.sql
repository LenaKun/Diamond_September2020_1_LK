CREATE VIEW [rep].[viewSbSr] with schemabinding
	AS
			select	sr.id as SubReportId, 
				sr.MainReportId as MainReportId,
				sr.Amount as Amount,
				s.Id as ServiceId,
				s.Name as ServiceName,
				s.ReportingMethodId,
				st.Id as ServiceTypeId,
				st.Name as ServiceTypeName,
				mr.[Start] as ReportStart,
				mr.[End] as ReportEnd,
				app.Id as AppId,
				app.Name as appName,
				a.Id as AgencyId,
				a.Name as AgencyName,
				ag.Id as AgencyGroupId,
				ag.Name as AgencyGroupName,
				app.CCGrant as CcGrant,
				app.CurrencyId as AppCur,
				app.FundId as FundId,
				fund.MasterFundId as MasterFundId,
				fund.Name as FundName,
				mr.StatusId as MainReportStatusId,
				cast(case when s.ReportingMethodId in (2,3,8) then 1 else 0 end as bit) as IsEstimated,
				ag.StateId,
				ag.CountryId,
				c.RegionId,
				ag.ExcludeFromReports
		from dbo.MainReports as mr
			join dbo.subreports as sr on mr.id = sr.mainreportid
			join dbo.appbudgetservices as appbs on sr.appbudgetserviceid = appbs.id
			join dbo.appbudgets as appb on appbs.AppBudgetId = appb.id
			join dbo.apps as app on appb.appid = app.id
			join dbo.funds as fund on app.fundid = fund.id
			join dbo.masterfunds as mfund on fund.masterfundid = mfund.id
			join dbo.Services as s on appbs.serviceid = s.id
			join dbo.servicetypes as st on s.typeid = st.id
			join dbo.agencies as a on appbs.agencyid = a.id
			join dbo.agencygroups as ag on a.groupid = ag.id
			join dbo.countries as c on ag.countryid = c.id
		where ag.ExcludeFromReports  = 0
GO
CREATE UNIQUE CLUSTERED INDEX [IDX_CLUSTERED_viewSbSr] ON [rep].[viewSbSr] 
(
	[SubReportId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_viewSbSr] ON [rep].[viewSbSr] 
(
	[ReportStart] ASC,
	[ReportEnd] ASC,
	[RegionId] ASC,
	[CountryId] ASC,
	[StateId] ASC,
	[MainReportStatusId] ASC,
	[ReportingMethodId] ASC,
	[AgencyId] ASC,
	[ServiceTypeId] ASC,
	[ServiceId] ASC,
	[MasterFundId] ASC,
	[FundId] ASC,
	[AppId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
