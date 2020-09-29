CREATE PROCEDURE [dbo].[UpdateImportClientReportWeekly]
	@Id uniqueidentifier
AS
	set xact_abort on
	declare @importid uniqueidentifier
	set  @importid=@id 

	declare @t table (
		ImportId uniqueidentifier, 
		ReportTypeId int,
		TypeId int, 
		ClientReportId int, 
		SubReportId int, 
		ClientId int, 
		[Date] datetime, 
		Rate smallmoney, 
		Quantity smallmoney,
		Amount smallmoney,
		[Discretionary] smallmoney,
		[TotalAmount] smallmoney,
		[Remarks] nvarchar(255),
		[Errors] nvarchar(max), 
		[UniqueCircumstances] NVARCHAR(MAX), 
		[HoursHoldCost] SMALLMONEY)

	insert into @t
	(ImportId, ReportTypeId, TypeId, ClientReportId, SubReportId, ClientId, [Date], Rate, Quantity, Amount, Discretionary, TotalAmount, Remarks, Errors, UniqueCircumstances, HoursHoldCost)
	select distinct ImportId, ReportTypeId, TypeId, ClientReportId, SubReportId, ClientId, [Date], Rate, sum(Quantity) as Quantity, Amount, Discretionary, TotalAmount, left(dbo.string_concat(Remarks),255), Errors, UniqueCircumstances, HoursHoldCost
	from ImportClientReports
	where ImportId = @importid
	group by ImportId, ReportTypeId, TypeId, ClientReportId, SubReportId, ClientId, [Date], Rate, Amount, Discretionary, TotalAmount, Errors, UniqueCircumstances, HoursHoldCost

	delete from ImportClientReports
	where ImportId = @importid

	insert into ImportClientReports 
	(Id, ImportId, ReportTypeId, TypeId, ClientReportId, SubReportId, ClientId, [Date], Rate, Quantity, Amount, Discretionary, TotalAmount, Remarks, Errors, UniqueCircumstances, HoursHoldCost)
	select newid(), ImportId, ReportTypeId, TypeId, ClientReportId, SubReportId, ClientId, [Date], Rate, Quantity, Amount, Discretionary, TotalAmount, Remarks, Errors, UniqueCircumstances, HoursHoldCost
	from @t

	SELECT @@ROWCOUNT

RETURN 0
