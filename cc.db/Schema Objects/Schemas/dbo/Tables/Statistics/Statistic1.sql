CREATE STATISTICS [_dta_stat_421576540_1_4] ON [dbo].[AppBudgetServices]([Id], [AppBudgetId])
go

CREATE STATISTICS [_dta_stat_501576825_9_8] ON [dbo].[Apps]([EndDate], [StartDate])
go

CREATE STATISTICS [_dta_stat_501576825_1_8_9] ON [dbo].[Apps]([Id], [StartDate], [EndDate])
go

CREATE STATISTICS [_dta_stat_1380199967_12_2] ON [dbo].[MainReports]([StatusId], [AppBudgetId])
go

CREATE STATISTICS [_dta_stat_1380199967_2_1_12] ON [dbo].[MainReports]([AppBudgetId], [Id], [StatusId])
go

CREATE STATISTICS [_dta_stat_1053246807_1_2] ON [dbo].[Clients]([Id], [MasterId])
go

CREATE STATISTICS [_dta_stat_1380199967_5_12] ON [dbo].[MainReports]([End], [StatusId])
go

CREATE STATISTICS [_dta_stat_1380199967_5_1] ON [dbo].[MainReports]([End], [Id])
go

CREATE STATISTICS [_dta_stat_1380199967_1_12_5] ON [dbo].[MainReports]([Id], [StatusId], [End])
go

CREATE STATISTICS [_dta_stat_2099048_3_1] ON [dbo].[SubReports]([AppBudgetServiceId], [Id])
go

CREATE STATISTICS [_dta_stat_2030630277_5_4] ON [dbo].[FunctionalityScores]([FunctionalityLevelId], [StartDate])
go

CREATE STATISTICS [_dta_stat_2030630277_2_4_5] ON [dbo].[FunctionalityScores]([ClientId], [StartDate], [FunctionalityLevelId])
go

CREATE STATISTICS [_dta_stat_1173579219_3_2] ON [dbo].[HomeCareEntitledPeriod]([StartDate], [ClientId])
go

CREATE STATISTICS [_dta_stat_2099048_2_3_1] ON [dbo].[SubReports]([MainReportId], [AppBudgetServiceId], [Id])
go

CREATE STATISTICS [_dta_stat_2099048_1_2] ON [dbo].[SubReports]([Id], [MainReportId])
go

CREATE STATISTICS [_dta_stat_501576825_4_1_3] ON [dbo].[Apps]([Name], [Id], [AgencyGroupId])
go

