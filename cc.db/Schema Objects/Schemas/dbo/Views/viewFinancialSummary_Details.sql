CREATE VIEW [dbo].[viewFinancialSummary_Details]
	AS
	with srs as (
		select	sr.id as SubReportId, 
				sr.MainReportId as MainReportId,
				sr.Amount as Amount,
				s.Id as ServiceId,
				s.Name as ServiceName,
				s.ReportingMethodId,
				st.Id as ServiceTypeId,
				st.Name as ServiceTypeName,
				mr.[Start] as ReportStart,
				mr.[End] as ReportEnd,
				app.Id as appId,
				app.Name as appName,
				a.Id as AgencyId,
				a.Name as AgencyName,
				ag.Id as AgencyGroupId,
				ag.Name as AgencyGroupName,
				app.CCGrant as CcGrant,
				app.CurrencyId as AppCur,
				app.FundId as FundId,
				fund.MasterFundId as MasterFundId,
				mr.StatusId as MainReportStatusId
		from mainreports as mr
		join subreports as sr on mr.id = sr.mainreportid
		join appbudgets as appb on mr.appbudgetid = appb.id
		join apps as app on appb.appid = app.id
		join funds as fund on app.fundid = fund.id
		join appbudgetservices as appbs on sr.appbudgetserviceid = appbs.id
		join Services as s on appbs.serviceid = s.id
		join agencies as a on appbs.agencyid = a.id
		join agencygroups as ag on a.groupid = ag.id
		join servicetypes as st on s.typeid = st.id
	)
	,crs as (
		select SubReportId, ClientId, Sum(Quantity) as Quantity, Sum(TotalAmount) as Amount
		from viewclientreports as cr
		group by SubReportId, ClientId
	)
	,srss as (
		select SubReportId, count(*) as ClientsCount
		from viewclientreports as cr
		group by SubReportId
	)
	select 
		sr.SubReportId
		,c.Id as ClientId
		,c.FirstName as FirstName
		,c.LastName as LastName
		,sr.ServiceTypeName
		,sr.ServiceName
		,sr.ReportStart
		,sr.ReportEnd
		,sr.AppName
		,cr.Quantity
		,cr.Amount as Amount
		,(sr.Amount / nullif(srss.ClientsCount, 0)) as EstAmount
		,sr.CcGrant
		,sr.AppCur
		,sr.AgencyId
		,sr.ServiceTypeId
		,sr.ServiceId
		,sr.MasterFundId
		,sr.FundId
		,sr.appId
		,sr.ReportingMethodId
		,cast(dbo.fnMainReportSubmitted(sr.MainReportStatusId) as bit) as IsSubmitted
	from srs as sr
		join crs as cr on sr.SubReportId = cr.SubReportId
		left outer join srss on sr.ReportingMethodId in (2,8) and sr.SubReportId = srss.SubReportId
		join clients as c on cr.ClientId = c.Id
