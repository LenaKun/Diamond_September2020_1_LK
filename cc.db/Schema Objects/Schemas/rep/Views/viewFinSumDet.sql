CREATE VIEW [rep].[viewFinSumDet] with schemabinding
	AS

	select t.SubReportId,
		t.ClientId,
		t.Quantity,
		Case 
		WHEN s.Name = 'Funeral Expenses' THEN cast(clientrep.Amount as money)
		ELSE t.Amount
		END as Amount,--t.Amount,
		sr.MainReportId as MainReportId,
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
		appbs.CCGrant as CcGrant,
		app.CurrencyId as AppCur,
		app.FundId as FundId,
		fund.MasterFundId as MasterFundId,
		fund.Name as FundName,
		mr.StatusId as MainReportStatusId,
		cast(case when s.ReportingMethodId in (2,3,8) then 1 else 0 end as bit) as IsEstimated,
		ag.StateId,
		ag.CountryId,
		c.RegionId,
		ag.ExcludeFromReports,
		client.FirstName,
		client.LastName,
		coalesce(client.MasterId, client.Id) as MasterId
	from rep.finSumDet as t
		join dbo.subreports as sr on t.SubReportId = sr.Id
		join dbo.MainReports as mr on sr.MainReportId = mr.Id
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
		join dbo.Clients as client on t.clientid = client.Id
		left outer join dbo.ClientReports as clientrep on t.ClientId = clientrep.ClientId and t.SubReportId = clientrep.SubReportId and clientrep.Amount is not null
GO
CREATE UNIQUE CLUSTERED INDEX [IDX_CLUSTERED_viewFinSumDet] ON [rep].[viewFinSumDet] 
(
	[SubReportId] ASC,
	[ClientId] asc
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_viewFinSumDet] ON [rep].[viewFinSumDet] 
(
	--filter that appear in on financial summary pages
	[IsEstimated] ASC,
	[MainReportStatusId] ASC,
	[ReportStart] ASC,
	[RegionId] ASC,
	[AppId] ASC,
	[CountryId] ASC,
	[StateId] ASC,
	[AgencyId] ASC,
	[AgencyGroupId] ASC,
	[ServiceTypeId] ASC,
	[ServiceId] ASC,
	[MasterFundId] ASC,
	[FundId] ASC,
	[FirstName] asc,
	[LastName] asc
) 
include (Amount, quantity, AppCur, ClientId, ServiceTypeName, ServiceName, FundName, AppName, ReportEnd)
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_viewFinSumDet_Summary
ON [rep].[viewFinSumDet] ([ServiceId],[AgencyId],[MasterFundId])
INCLUDE ([AppId],[CcGrant])
GO
--Client MasterId Import triggers [IDX_CLUSTERED_viewFinSumDet] index update 
--(not sure why it is necessary after MasterId was removed from the nonclustered index)
--The Query Processor estimates that implementing the following index could improve the query cost by 96.8491%.
CREATE NONCLUSTERED INDEX [IDX_viewFinSumDet_1]
ON [rep].[finSumDet] ([ClientId])
INCLUDE ([SubReportId],[Quantity],[Amount])