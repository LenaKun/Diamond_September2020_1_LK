CREATE TRIGGER [GgReportedCountTrigger]
	ON [dbo].[MainReports]
	FOR UPDATE
	AS
	BEGIN
		SET NOCOUNT ON
		print getdate()
		print 'Hello [GgReportedCountTrigger]'
		if update([StatusId])
		begin
			with updatedReports as(
				select inserted.id as Id, inserted.StatusId, inserted.AppBudgetId 
				from inserted join deleted on inserted.Id = deleted.Id
				where   (inserted.StatusId = 2 and deleted.StatusId <> 2) or
						(inserted.StatusId <> 2 and deleted.StatusId = 2)
			),
			relatedClients as (
				select c.* 
				from updatedReports as mr
				join AppBudgets as b on mr.AppBudgetId = b.Id
				join Apps as app on b.AppId = app.Id
				join Funds as fund on app.FundId = fund.Id
				join AgencyGroups as ag on app.AgencyGroupId = ag.Id
				join Agencies as a on ag.Id = a.GroupId
				join clients as c on a.Id = c.AgencyId
				where fund.MasterFundId = 73 
			),
			relatedReports as (
				select c.Id as ClientId, case when mr.StatusId = 2 then 1 end as GgApproved
				from relatedClients as c
				join viewClientReports as cr on c.Id = cr.ClientId
				join SubReports as sr on cr.SubreportId = sr.Id
				join MainReports as mr on sr.MainReportId = mr.Id
				join AppBudgetServices appbs on sr.AppBudgetServiceId = appbs.Id
				join AppBudgets appb on appbs.AppBudgetId = appb.Id
				join Apps a on appb.AppId = a.Id
				join AgencyGroups ag on a.AgencyGroupId = ag.Id
				join Funds f on a.FundId = f.Id
				where f.MasterFundId = 73
			),
			ggcounts as (
				select clientid, count(ggapproved) as ggApprovedCount
				from relatedReports
				group by ClientId
			)
			update c set GGReportedCount = t.ggApprovedCount
			from clients as c
			join ggcounts as t on c.Id = t.ClientId
		end
		print getdate()
		print 'Farewell [GgReportedCountTrigger]'
	END
