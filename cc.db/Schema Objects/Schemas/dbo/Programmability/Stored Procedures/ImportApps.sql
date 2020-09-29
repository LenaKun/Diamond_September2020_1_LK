CREATE PROCEDURE [dbo].[ImportApps]
	@id uniqueidentifier
AS

	--this will hold import key - app key pairs

	
	declare @t table (id int, rowid int);	
	with import as (
		select * from AppsImport 
		where id = @id and StartDate is not null and EndDate is not null
	)
	merge dbo.apps as target
	using (
		--get only apps with unique name and valid fks (inner join)
		select i.*
		from (select name from import group by name having count(*)=1) as iname
		join import as i on iname.name = i.name
		join funds on i.fundid = funds.id
		join agencygroups on i.agencygroupid = agencygroups.id
		join currencies as c on i.currencyid = c.id
	) as source on target.name = source.name
	when matched then 
		update set FundId = source.FundId
			,AgencyGroupId = source.AgencyGroupId
			,Name = source.Name
			,CurrencyId = source.CurrencyId
			,AgencyContribution = source.AgencyContribution
			,CcGrant = source.CcGrant
			,RequiredMatch = source.RequiredMatch
			,StartDate = source.StartDate
			,EndDate = source.EndDate
			,MaxAdminAmount = source.MaxAdminAmount
			,MaxNonHcAmount = source.MaxNonHcAmount
			,HistoricalExpenditureAmount = source.HistoricalExpenditureAmount
			,EndOfYearValidationOnly = source.EndOfYearValidationOnly
			,InterlineTransfer = source.InterlineTransfer
			,AvgReimbursementCost = source.AvgReimbursementCost
	when not matched by target then
		insert (FundId
			,AgencyGroupId
			,CurrencyId
			,Name
			,AgencyContribution
			,CcGrant
			,RequiredMatch
			,MaxAdminAmount
			,MaxNonHcAmount
			,HistoricalExpenditureAmount
			,StartDate
			,EndDate
			,EndOfYearValidationOnly
			,InterlineTransfer
			,AvgReimbursementCost)
		values(source.FundId
			,source.AgencyGroupId
			,source.CurrencyId
			,source.Name
			,source.AgencyContribution
			,source.CcGrant
			,source.RequiredMatch
			,source.MaxAdminAmount
			,source.MaxNonHcAmount
			,source.HistoricalExpenditureAmount
			,source.StartDate
			,source.EndDate
			,source.EndOfYearValidationOnly
			,source.InterlineTransfer
			,source.AvgReimbursementCost)
	output  inserted.id, source.RowId into @t;

	set nocount on
	
	merge dbo.appExchangeRates as target
	using (
		--get ids of affected rows in prev merge and join the exchange rates
		select t1.id as appid, t2.currencyid, t2.value
		from @t as t1
		join ( 
			select rowid, upper(substring(cur,1,3)) as currencyid, val as Value
			from AppsImport
			unpivot
			(
				val
				for cur in (USDRate,ILSRate,EURRate)
			) as u
		) as t2 on t1.rowid = t2.rowid
	) as source on target.appid = source.appid and target.curid = source.currencyid
	when matched and source.value is not null then
		update set value = source.value
	when not matched by target then
		insert (appid, curid, value) 
		values(source.appid, source.currencyid, source.value)
	when matched and source.Value is null then
		delete
	when not matched by source then
		delete;

	delete tar
	from AppsImport as tar
	join @t as t on tar.rowid = t.rowid
			

RETURN 0
