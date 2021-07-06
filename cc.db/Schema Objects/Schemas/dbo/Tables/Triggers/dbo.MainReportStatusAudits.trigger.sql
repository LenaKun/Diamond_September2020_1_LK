CREATE TRIGGER [dbo].[MainReportStatusAuditUpdateTrigger]
    ON [dbo].[MainReports]
    WITH EXECUTE AS CALLER
    AFTER UPDATE
    AS begin
	set nocount on
	
	begin
			if Update(StatusId)
				begin
				insert INTO dbo.MainReportStatusAudits
					(
						MainReportId,
						OldStatusId,
						NewStatusId,
						StatusChangeDate,
						UserId
					)
					select 
						i.Id,
						d.StatusId,
						i.StatusId,
						i.updatedat,
						i.updatedbyId
						
					from inserted as i join deleted as d on i.id=d.id
				--where i.StatusId <> d.StatusId

				
						
			    end	
				
				if exists(SELECT top 2 * FROM MainReportStatusAudits WHERE StatusChangeDate IN
	  ( SELECT StatusChangeDate FROM MainReportStatusAudits  GROUP BY StatusChangeDate HAVING COUNT(*) > 1) order by StatusChangeDate desc, Id desc)

	   delete from MainReportStatusAudits
	  where id = (SELECT max(id)  FROM MainReportStatusAudits WHERE StatusChangeDate IN
	  ( SELECT StatusChangeDate FROM MainReportStatusAudits  GROUP BY StatusChangeDate HAVING COUNT(*) > 1) )
	end


	----------------------------------------------------------- hccaps
	--------------------add submitted quantities
	merge dbo.hccaps as target
	using (
		select t.clientid, t.reportdate, q, oa.hccap, oa.avghccap
		from
		(
			select clientid, reportdate, sum(ar.quantity) as q
			from ClientAmountReport as ar 
				join ClientReports as cr on ar.ClientReportId = cr.Id
				join SubReports as sr on cr.SubReportId = sr.Id
				join inserted on sr.mainreportid = inserted.id
				join deleted on inserted.id = deleted.id
				join AppBudgetServices as appbs on sr.AppBudgetServiceId = appbs.id
				join Services as s on appbs.ServiceId = s.Id
				where s.EnforceTypeConstraints = 1 and s.TypeId = 8 and s.ReportingMethodId in (5, 14)
				and dbo.fnMainReportSubmitted(deleted.statusid) = 0
				and dbo.fnMainReportSubmitted(inserted.statusid) = 1
			group by clientid, reportdate
		) as t
		outer apply dbo.fnHcCapMonthly(t.clientid, t.reportdate) as oa
	) as source on target.clientid = source.clientid and target.reportdate = source.reportdate
	when not matched by target then 
		insert (clientid, reportdate, quantity, monthlycap, dailycap)
		values(source.clientid, source.reportdate, source.q, coalesce(source.hccap,0), coalesce(source.avghccap,0))
	when matched then
		update set quantity = target.quantity + source.q;

	--------------------substract unsubmitted quantities
	merge dbo.hccaps as target
	using (
		select t.clientid, t.reportdate, q, oa.hccap, oa.avghccap
		from
		(
			select clientid, reportdate, sum(ar.quantity) as q
			from ClientAmountReport as ar 
				join ClientReports as cr on ar.ClientReportId = cr.Id
				join SubReports as sr on cr.SubReportId = sr.Id
				join inserted on sr.mainreportid = inserted.id
				join deleted on inserted.id = deleted.id
				join AppBudgetServices as appbs on sr.AppBudgetServiceId = appbs.id
				join Services as s on appbs.ServiceId = s.Id
				where s.EnforceTypeConstraints = 1 and s.TypeId = 8 and s.ReportingMethodId in (5, 14)
				and dbo.fnMainReportSubmitted(deleted.statusid) = 1
				and dbo.fnMainReportSubmitted(inserted.statusid) = 0
			group by clientid, reportdate
		) as t
		outer apply dbo.fnHcCapMonthly(t.clientid, t.reportdate) as oa
	) as source on target.clientid = source.clientid and target.reportdate = source.reportdate
	when matched and target.quantity = source.q then
		delete
	when matched and target.quantity <> source.q then
		update set quantity = target.quantity - source.q;
	
	
	


end	
	