CREATE PROCEDURE [dbo].[ImportCriEmergency]
	@Id uniqueidentifier
AS

	-- direct parameter passing resulted in timeout. Something to do with parameter sniffing...
	declare @iid uniqueidentifier
	set @iid = @id

	--table for valid (insertable) data

	declare @t table (Id uniqueidentifier, SubReportId int, ClientId int, ReportDate datetime, TypeId int , Amount money, Discretionary money,Remarks nvarchar(max), UniqueCircumstances nvarchar(max))

	--get the valid data
	insert into @t (id, SubReportId, ClientId, [ReportDate], TypeId, Amount, Discretionary,Remarks, UniqueCircumstances)
	select i.Id, i.SubReportId, i.ClientId, i.[date], i.TypeId, i.Amount, i.Discretionary,i.Remarks,i.UniqueCircumstances
	from importClientReports as i
	join EmergencyReportTypes as ert on ert.id=i.TypeId
	join Clients as c on i.ClientId = c.id
	join SubReports as sr on i.SubReportId=sr.id
	join MainReports as mr on sr.MainReportId = mr.Id
	join AppBudgetServices on sr.AppBudgetServiceId = appbudgetservices.Id
	where 
			(i.ImportId=@iid and i.ReportTypeId=6) and	--relevant import id and type=emergency 
			(c.JoinDate < mr.[End]) and --client joined the agency berfore main report end
			(dbo.fnRepDateValid(c.LeaveDate, c.DeceasedDate,  coalesce(i.Date,mr.[End]), i.remarks, iif(c.AustrianEligible=1 or c.RomanianEligible=1,1,0)) = 1) and
			(c.AgencyId = appbudgetservices.AgencyId) and --not supposed to happen
			(i.[Date] between mr.[Start] and mr.[End])


	merge emergencyreports as target
	using @t as source on  target.SubReportId = source.SubReportId 
				and target.ClientId = source.ClientId 
				and target.ReportDate = source.ReportDate 
				and target.TypeId = source.TypeId
				and target.Amount = source.Amount
				and target.Discretionary = source.Discretionary
				and target.Remarks = source.Remarks
				and target.UniqueCircumstances = source.UniqueCircumstances
	when not matched by target then
		insert (SubReportId,ClientId,TypeId,ReportDate,Amount,Discretionary,Remarks,UniqueCircumstances)
		values (source.SubReportId,source.ClientId,source.TypeId,source.ReportDate,source.Amount,source.Discretionary,source.Remarks, source.UniqueCircumstances)
	;

	
	--cleanup
	delete s 
	from importclientreports as s join @t as t on s.Id = t.Id
	
	--more cleanup
	if not exists (select top 1 1 from importClientReports as i where i.ImportId=@iid and i.ReportTypeId=6)
	begin
		delete from ImportClientReports 
		where Id = @iId
	end


	SELECT @@ROWCOUNT

RETURN 0
