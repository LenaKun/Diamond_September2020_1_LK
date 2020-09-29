CREATE TABLE [dbo].[SubReports]
(
	Id int not null identity(1,1) primary key,
	MainReportId int not null references dbo.MainReports(Id),
	AppBudgetServiceId int not null references dbo.AppBudgetServices(Id),
	TotalHouseholdsServed int null,
	--ServiceId int not null references dbo.Services(Id),	-- mainreport.appbudget.serviceid
	--AgencyId int not null references dbo.Agencies(Id),	-- mainreport.appbudget.agencyid

	

	constraint UQ__SubReports unique(MainReportId, AppBudgetServiceId),

	ServiceUnits  DECIMAL(18,6) null,
	Amount money null,
	MatchingSum money null ,
	AgencyContribution money null,
	OverFlowReason nvarchar(max)

)
go

--DATABASE ENGINE TUNING ADVISOR - GRDEV - 2015-08-04
go
CREATE NONCLUSTERED INDEX [_dta_index_SubReports_12_2099048__K1_2_3_6_7_8] ON [dbo].[SubReports]
(
	[Id] ASC
)
INCLUDE ( 	[MainReportId],
	[AppBudgetServiceId],
	[Amount],
	[MatchingSum],
	[AgencyContribution]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_SubReports_12_2099048__K3_K1_K2_6] ON [dbo].[SubReports]
(
	[AppBudgetServiceId] ASC,
	[Id] ASC,
	[MainReportId] ASC
)
INCLUDE ( 	[Amount]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_SubReports_12_2099048__K1_K2_6_8337] ON [dbo].[SubReports]
(
	[Id] ASC,
	[MainReportId] ASC
)
INCLUDE ([Amount]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

