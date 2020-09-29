CREATE TRIGGER [PersonnelReportsTableTrigger]
	ON [dbo].[PersonnelReports]
	FOR DELETE, INSERT, UPDATE
	AS
	BEGIN

		SET NOCOUNT ON
		declare @appBudgetServiceId int;
		declare @c money;
		

		update appbs set CcGrant = coalesce(t1.CcGrant, 0)
		from AppBudgetServices as appbs
		join 
		(
			select appbudgetserviceid from inserted
			union select appbudgetserviceid from deleted
		) as t on appbs.Id = t.AppBudgetServiceId
		left outer join
		(
			select pr.AppBudgetServiceId, sum(pr.Salary) as CcGrant
			from PersonnelReports as pr 
			group by pr.AppBudgetServiceId
		) as t1 on appbs.Id = t1.AppBudgetServiceId
		
	END
