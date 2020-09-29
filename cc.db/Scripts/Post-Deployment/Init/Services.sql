-- =============================================
-- Script Template
-- =============================================
use cc

delete from dbo.AgencyGroupServices
delete from dbo.AppServices
delete from dbo.AppBudgetServices


delete from dbo.ClientAmountReport
delete from dbo.ClientReports
delete from dbo.SubReports
delete from dbo.MainReportPoComments
delete from dbo.MainReportAgencyComments
delete from dbo.mainreportstatusaudits
delete from dbo.MainReports
delete from dbo.AppBudgets


delete from dbo.Services
delete from dbo.ServiceTypes



declare @t table
(
stn nvarchar(max),
sn nvarchar(max),
rm int
)
set nocount on
insert into @t (stn, sn, rm) values ('Homecare', 'Chore/Housekeeper Services', 5)
insert into @t (stn, sn, rm) values ('Homecare', 'Personal/Nursing Care', 5)
insert into @t (stn, sn, rm) values ('Homecare', 'Skilled Nursing', 5)
insert into @t (stn, sn, rm) values ('Homecare', 'Personnel', 5)
insert into @t (stn, sn, rm) values ('Homecare', 'Program Costs', 5)
insert into @t (stn, sn, rm) values ('Food Programs', 'Meals-On-Wheels', 1)
insert into @t (stn, sn, rm) values ('Food Programs', 'Food Packages', 1)
insert into @t (stn, sn, rm) values ('Food Programs', 'Congregate Meals', 2)
insert into @t (stn, sn, rm) values ('Food Programs', 'Food Vouchers', 1)
insert into @t (stn, sn, rm) values ('Food Programs', 'Personnel', 4)
insert into @t (stn, sn, rm) values ('Client Transportation', 'Client Transportation', 1)
insert into @t (stn, sn, rm) values ('Client Transportation', 'Personnel', 4)
insert into @t (stn, sn, rm) values ('Medical Program', 'Medical Program', 1)
insert into @t (stn, sn, rm) values ('Dental Program', 'Dental Program', 1)
insert into @t (stn, sn, rm) values ('Medical Equipment', 'Medical Equipment', 1)
insert into @t (stn, sn, rm) values ('Medicine', 'Medicine', 1)
insert into @t (stn, sn, rm) values ('Minor Home Modifications', 'Minor Home Modifications', 1)
insert into @t (stn, sn, rm) values ('Winter Relief', 'Winter Relief', 1)
insert into @t (stn, sn, rm) values ('Case Management', 'Personnel', 4)
insert into @t (stn, sn, rm) values ('Other Services', 'Respite Care (Adult Day Care)', 1)
insert into @t (stn, sn, rm) values ('Other Services', 'Rehabilitation', 1)
insert into @t (stn, sn, rm) values ('Other Services', 'Psychiatric Care', 1)
insert into @t (stn, sn, rm) values ('Other Services', 'Therapeutic Sessions', 3)
insert into @t (stn, sn, rm) values ('Other Services', 'Group Therapy', 2)
insert into @t (stn, sn, rm) values ('Other Services', 'Friendly Visitor Program', 1)
insert into @t (stn, sn, rm) values ('Other Services', 'Personnel', 4)
insert into @t (stn, sn, rm) values ('Other Services', 'Other- Associated Costs By Client.', 1)
insert into @t (stn, sn, rm) values ('Other Services', 'Other- Total Cost with Clients Names.', 2)
insert into @t (stn, sn, rm) values ('Other Services', 'Other- Total Cost No Names', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Cafe Europa - Materials and Supplies', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Cafe Europa - Food', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Cafe Europa - Entertainment', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Cafe Europa - Client Transportation', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Cafe Europa - Outreach', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Cafe Europa - Rent of Facility (survivor group only)', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Personnel', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Other- Associated Costs By Client.', 3)
insert into @t (stn, sn, rm) values ('Socialization', 'Other- Total Cost with Clients Names.', 2 )
insert into @t (stn, sn, rm) values ('Socialization', 'Other- Total Cost No Names', 3)
insert into @t (stn, sn, rm) values ('Administrative Overhead', 'Administrative Overhead', 3)
insert into @t (stn, sn, rm) values ('Annual Home Care Assessment Fee', 'Annual Home Care Assessment Fee', 1)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Housing & Related Costs ', 1)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Dental', 1)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Food', 1)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Medical', 1)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Housekeeping/Homecare', 1)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Client Transportation', 1)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Other- Associated Costs By Client.', 1)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Other- Total Cost with Clients Names.', 2)
insert into @t (stn, sn, rm) values ('Emergency Assistance', 'Other- Total Cost No Names', 3)





set nocount off

set identity_insert ServiceTypes on
insert into dbo.ServiceTypes(name,Id) 
	select stn, ROW_NUMBER() OVER (ORDER BY stn) from (select distinct t.stn  from @t as t) as t1
set identity_insert ServiceTypes off


insert into dbo.Services(name, ReportingMethodId, TypeId)
select  t.sn, t.rm, st.id from @t as t inner join dbo.ServiceTypes as st on t.stn = st.name

select * from dbo.ServiceTypes
select * from dbo.Services