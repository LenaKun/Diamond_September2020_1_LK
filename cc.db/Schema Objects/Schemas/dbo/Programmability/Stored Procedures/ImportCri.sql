CREATE 
procedure [dbo].[ImportCri]
(
	@Id uniqueidentifier
)
AS
BEGIN

	declare @idLocal uniqueidentifier;
	set @idLocal = newid();
	set @idLocal = @Id;

	declare @t table (Id uniqueidentifier);

	merge clientreports as target 
	using 
	(
		select t1.Id, t1.SubReportId, t1.ClientId, t1.Quantity, t1.TotalAmount as Amount, t1.Remarks as Remarks
		from ImportClientReports as t1 join 
				--unique (subreportid, clientid), take first row out of duplicates
				(
					Select SubReportId, ClientId, min(RowIndex) as RowIndex from ImportClientReports 
					where ImportId = @idLocal
					group by SubReportId, ClientId
				)as t2 on t1.SubReportId = t2.SubReportId and t1.ClientId=t2.ClientId and t1.RowIndex = t2.RowIndex
		join clients on t1.clientid = clients.id
		join subreports on t1.subreportid = subreports.id
		join mainreports on subreports.mainreportid = mainreports.id
		where t1.ImportId = @idLocal and dbo.fnRepDateValid(clients.leavedate, clients.deceaseddate, mainreports.[start], t1.remarks, iif(clients.AustrianEligible=1 or clients.RomanianEligible=1,1,0)) = 1
		
	) as source
				on target.SubReportId = source.SubReportId and target.ClientId = source.ClientId
	when matched then
		Update set Quantity = source.Quantity, Amount = source.Amount, Remarks = source.Remarks

	when not matched by target then
		Insert (SubReportId, ClientId, Quantity, Amount, Remarks)
		Values (source.SubReportId, source.ClientId, source.Quantity, source.Amount, source.Remarks)
	output source.Id into @t;

	
	delete s 
		from importclientreports as s join @t as t on s.Id = t.Id
		
	if not exists (select top 1 1 from importClientReports as i where i.ImportId=@idLocal and i.ReportTypeId=6)
		begin
			delete from ImportClientReports 
			where Id = @idLocal;
		end
	

	SELECT @@ROWCOUNT
END