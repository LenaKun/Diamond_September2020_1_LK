CREATE VIEW [dbo].[ViewClientReports]
	as

	select SubReportId,
		ClientId,
		TypeId,
		TypeName,
		PurposeOfGrant,
		Quantity,
		Rate,
		ReportDate,
		Amount,
		Discretionary,
		Amount + coalesce(discretionary, 0) as TotalAmount
	from
	(
		select SubreportId
			,cr.ClientId
			,null as TypeId
			,null as TypeName
			,cr.Remarks as PurposeOfGrant
			,coalesce(ar.Quantity, cr.Quantity) as Quantity
			,cr.Rate as Rate
			,ar.ReportDate
			,coalesce(cr.rate*ar.Quantity,cr.amount) as amount
			,null as Discretionary
			
			from clientreports as cr
			left outer join clientamountreport as ar on cr.id = ar.clientreportid
	
			union all
			select SubreportId
				,ClientId
				,er.TypeId
				,ert.Name as TypeName
				,er.Remarks as PurposeOfGrant
				,null as Quantity
				,null as Rate
				,er.ReportDate
				,er.Amount
				,er.Discretionary
			from emergencyreports as er join EmergencyReportTypes as ert on er.typeid =ert.id


			union all
			select SubreportId
				,ClientId
				,0 as TypeId
				,'' as TypeName
				,'' as PurposeOfGrant
				,null as Quantity
				,null as Rate
				,null as ReportDate
				,sc.Amount
				,null as Discretionary
			from SupportiveCommunitiesReports as sc 

			union all
			select SubreportId
				,ClientId
				,0 as TypeId
				,'' as TypeName
				,'' as PurposeOfGrant
				,null as Quantity
				,null as Rate
				,null as ReportDate
				,IsNull(dcc.Amount,0)
				,null as Discretionary
			from DaysCentersReports as dcc

			union all
			select SubreportId
				,ClientId
				,0 as TypeId
				,'' as TypeName
				,'' as PurposeOfGrant
				,null as Quantity
				,null as Rate
				,null as ReportDate
				,null as Amount
				,null as Discretionary
			from SoupKitchensReport as skr

	) as t

GO


