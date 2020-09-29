-- =============================================
-- Script Template
-- =============================================

  use cc

  declare @iid int
  declare @niid int
  select top 1 @iid=id from dbo.regions where name='israel'
  select top 1 @niid=id from dbo.regions where name<>'israel'

  if coalesce(@iid, @niid) is null
  begin
  delete  from dbo.regions
  set identity_insert regions on
  insert into regions(id,name) values(1, 'Israel')
  insert into regions(id,name) values(2,'Non-Israel')
  set identity_insert regions off

  
  end
  select top 1 @iid=id from dbo.regions where name='israel'
  select top 1 @niid=id from dbo.regions where name<>'israel'

  update dbo.agencies set regionid=case when c.code='il' then @iid else @niid end
  
  from dbo.agencies as a inner join
  dbo.agencygroups as ag  on a.GroupId=ag.id inner join 
  dbo.Countries as c on ag.CountryId=c.id

  select * from agencies as a left outer join regions as r on a.regionid=r.Id