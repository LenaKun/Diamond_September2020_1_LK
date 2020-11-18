CREATE view [dbo].[HcCapsTableRaw_Original]

AS

	
	with hcs as (
             select t1.ClientId, 
                    t1.StartDate, 
                    t4.StartDate as EndDate,
                    coalesce(t2.Cap, 0) as Cap,
                    t1.HcStatusId
             from ClientHcStatuses as t1
             join HcStatuses as t2 on t1.HcStatusId = t2.Id
             outer apply (
                    select top 1 startdate from ClientHcStatuses as t3 
                    where t3.ClientId = t1.ClientId and t3.StartDate > t1.StartDate
                    order by t3.StartDate
             ) as t4
       ), fs as (
             select * from
             (
                    select 
                           s.ClientId, fl.HcHoursLimit, fl.SubstractGovHours, s.FunctionalityLevelId,
                           cast(s.StartDate as date) as StartDate, 
                           cast((select min(startdate) from FunctionalityScores as e where s.clientid = e.clientid and s.StartDate < e.StartDate) as date) as EndDate
                    from FunctionalityScores as s
                    join FunctionalityLevels as fl on s.functionalitylevelid = fl.id
             ) as fs1
       ),
       ghc as(
             select gov1.ClientId, gov1.Value, 
             cast(gov1.StartDate as date) as StartDate, 
             cast((select min(gov2.StartDate) from GovHcHours as gov2 where gov1.ClientId = gov2.ClientId and gov1.StartDate < gov2.StartDate) as date) as EndDate
             from GovHcHours as gov1
       ),
       hcep as (
             select ClientId, 
                    cast(StartDate as date) as StartDate,
                    cast(EndDate as date) as EndDate
                    from HomeCareEntitledPeriod 
       ),
       gfh as(
             select gf1.ClientId, gf1.Value,
             cast(gf1.StartDate as date) as StartDate,
             cast((select min(gf2.StartDate) from GrandfatherHours as gf2 where gf1.ClientId = gf2.ClientId and gf1.StartDate < gf2.StartDate) as date) as EndDate
             from GrandfatherHours as gf1
       ),
       c as (
             select clients.Id as ClientId, a.GroupId as AgencyGroupId,
                    cast(JoinDate as date) as JoinDate,
                    cast(LeaveDate as date)    as LeaveDate,
                    cast(DeceasedDate as date) as DeceasedDate,
                    MAFDate, MAF105Date
					,Has2Date  --by IZ
                    --,GfHours 
             from dbo.Clients join dbo.Agencies as a on dbo.clients.AgencyId = a.Id
       ),
    combined as (
        select distinct ClientId
            ,AgencyGroupId
            ,JoinDate
            ,LeaveDate
            ,DeceasedDate
                    ,SubstractGovHours --added by Lena
            ,FunctionalityLevelId
            ,FSHours
            ,HasStatusId
            ,HasHours
            ,GovHours
            ,gfhours
			,gfstartdate --29
			,gfenddate  --29
            ,MAFDate
            ,MAF105Date
			,HAS2Date --by IZ
            ,StartDate
            ,EndDate
        from (
            select c.ClientId as ClientId, c.AgencyGroupId,
                    c.JoinDate,
                    c.LeaveDate,
                    c.DeceasedDate,
                    fs.SubstractGovHours,
                    fs.FunctionalityLevelId,
            fs.HcHoursLimit as FSHours,
            hcs.HcStatusId as HasStatusId,
            hcs.Cap as HasHours,
                    ghc.Value as GovHours,
                    gfh.Value as gfhours,
					gfh.StartDate as gfstartdate, --29
					gfh.EndDate as gfenddate,	--29
            mafdate, maf105date,
			has2date, --by IZ
                    (select max(mxsd.StartDate) from (
                                                                   select fs.startDate as StartDate union
                                                                   select hcs.StartDate union
                                                                   select hcep.startDate union
                                                                   select ghc.StartDate union
                                                                   --select gfh.StartDate union
																   --select c.MAFDate union
																   --select c.MAF105Date union
                                                                   select c.JoinDate ) as mxsd
                    ) as StartDate,
                    (select min(mxsd.EndDate) from (
                                                                   select fs.EndDate as EndDate union
                                                                   select hcs.EndDate union
                                                                   select hcep.EndDate union
                                                                   select ghc.EndDate union
                                                                   --select gfh.EndDate union
                                                                   select case when c.DeceasedDate is not null then DATEADD(day, (4 * 7) + 1, c.DeceasedDate) else case when c.LeaveDate is not null then DATEADD(day, 1, c.LeaveDate) else c.LeaveDate end end) as mxsd
                    ) as EndDate
                    from c
                  join hcep on hcep.clientid=c.ClientId
					
                     join hcs on c.Clientid = hcs.clientid and (
                           (hcep.EndDate is null and hcs.EndDate is null) or
                           (hcep.EndDate is null or isnull(hcep.EndDate,'') > hcs.StartDate) or  --29
                           (hcs.EndDate is null or isnull(hcs.EndDate,'') > hcep.StartDate)  --29
                   )
					 --cast(t.StartDate as date)
                    join fs on c.Clientid = fs.clientid and (
                           (hcep.EndDate is null and fs.EndDate is null) or
                           (hcep.EndDate is null or isnull(hcep.EndDate,'') > fs.StartDate) or --29
                          (fs.EndDate is null or isnull(fs.EndDate,'') > hcep.StartDate)  --29
                    )--
                  join ghc on c.ClientId  = ghc.ClientId and (
                           (hcep.EndDate is null and ghc.EndDate is null) or
                           (hcep.EndDate is null or isnull(hcep.EndDate,'') > ghc.StartDate) or --29
                           (ghc.EndDate is null or isnull(ghc.EndDate,'') > hcep.StartDate) --29
                    )
					
                    left outer join gfh on gfh.ClientId =  c.ClientId  and (
				   --full outer join gfh on gfh.ClientId = c.ClientId and (
					
                          (hcep.EndDate is null and gfh.EndDate is null) or
                          (hcep.EndDate is null or isnull(hcep.EndDate,'') > gfh.StartDate) or  --29
                          (gfh.EndDate is null or isnull(gfh.EndDate,'')  > hcep.StartDate ) --29
						 		--EZ
                    )
        ) 
		as t
		

        where EndDate is null or isnull(EndDate,'') > isnull(StartDate,'')
		
    ),
    mafSplits as (
        select  ClientId
            ,AgencyGroupId
            ,JoinDate
            ,LeaveDate
            ,DeceasedDate
            ,FunctionalityLevelId
            ,SubstractGovHours --added by Lena
            ,FSHours
            ,HasStatusId
            ,HasHours
            ,GovHours
            ,gfhours
			,gfstartdate
			,gfenddate
            ,MAFDate
            ,MAF105Date
			,HAS2Date -- by IZ
            ,(select max(StartDate) from (select t.StartDate union select maf.StartDate union select Maf105.StartDate union select Has2.StartDate union select GF2.StartDate  ) as maxt) as StartDate
            ,(select min(EndDate) from (select t.EndDate union select maf.EndDate union select Maf105.EndDate union select Has2.EndDate union select GF2.EndDate ) as maxt) as EndDate
        from combined as t
           --maf & maf105 should split each hc period
            outer apply (
                select StartDate, EndDate, [Value] from (
                    select null as StartDate, MAFDate as EndDate, 0 as [Value]
                    union 
                    select MafDate as StartDate, null as EndDate, 1 as [Value]
                ) as m
                where (t.EndDate is not null and m.StartDate between t.StartDate and t.EndDate)
                    or (t.EndDate is null and t.StartDate < m.StartDate)
                    or (t.EndDate is not null and m.EndDate between t.StartDate and t.EndDate)
                    or (t.EndDate is null and t.StartDate < isnull(m.EndDate, ''))
                
            ) as maf

            outer apply (
                select StartDate, EndDate, [Value] from (
                    select null as StartDate, Maf105Date as EndDate, 0 as [Value]
                    union 
                    select Maf105Date as StartDate, null as EndDate, 1 as [Value]
                ) as m
                where (t.EndDate is not null and m.StartDate between t.StartDate and t.EndDate)
                    or (t.EndDate is null and t.StartDate < m.StartDate)
                    or (t.EndDate is not null and m.EndDate between t.StartDate and t.EndDate)
                    or (t.EndDate is null and t.StartDate < isnull(m.EndDate, ''))
            ) as Maf105
			 outer apply ( --byIZ
                select StartDate, EndDate, [Value] from (
                    select null as StartDate, Has2Date as EndDate, 0 as [Value]
                    union 
                    select Has2Date as StartDate, null as EndDate, 1 as [Value]
                ) as m
                where (t.EndDate is not null and m.StartDate between t.StartDate and t.EndDate)
                    or (t.EndDate is null and t.StartDate < m.StartDate)
                    or (t.EndDate is not null and m.EndDate between t.StartDate and t.EndDate)
                    or (t.EndDate is null and t.StartDate < m.EndDate)
            ) as Has2
			outer apply ( --29
                select StartDate, EndDate, [Value] from (
                    select null as StartDate, gfstartdate as EndDate, 0 as [Value]
                    union 
                    select gfstartdate as StartDate, null as EndDate, 1 as [Value]
                ) as m
                where (t.EndDate is not null and m.StartDate between t.StartDate and t.EndDate)
                    or (t.EndDate is null and t.StartDate <m.StartDate)
                    or (t.EndDate is not null and m.EndDate between t.StartDate and t.EndDate)
                    or (t.EndDate is null and t.StartDate < m.EndDate)
            ) as GF2

    )
    ,mafs as (
        select t.ClientId
            ,t.HasStatusId
                    ----Mike code
            --,case when startdate < MAFDate then 0 else 1 end as Maf
            --,case when StartDate < MAF105Date then 0 else 1 end as Maf105
                    ---Elvira changed:
                    ,case when Isnull(MAFDate,'') = '' then 0
                             when startdate < MAFDate then 0 
                             else 1 end as Maf
            ,case when Isnull(MAF105Date,'') = '' then 0
                             when StartDate < isnull(MAF105Date,'') then 0 else 1 end as Maf105
			-- ,case when Isnull(Has2Date,'') = '' then 0  --by IZ
                            -- when StartDate < isnull(Has2Date,'') then 0 else 1 end as Has
            ,case when Isnull(Has2Date,'') = '' then 0 
							when StartDate < isnull(Has2Date,'') then 0 else 1 end as Has2 --by LK
                              --else 1 end as Has2
				
                    --
            ,t.FSHours
            ,t.GovHours
			,case when Isnull(t.gfstartdate,'') = '' then 0 
							when StartDate < isnull(gfstartdate,'') then 0 else t.gfhours end as gfhours --29
            --,t.gfhours
            ,t.HasHours
            ,t.FunctionalityLevelId
                    ,t.SubstractGovHours --added by Lena
            ,t.StartDate
            ,t.EndDate
            ,t.AgencyGroupId
            ,t.JoinDate
            ,t.LeaveDate
            ,t.DeceasedDate
        from mafSplits as t
        where t.EndDate is null or t.StartDate < t.EndDate

    )
    , hccaps as 
    (
        select t.*,
            CASE --this has to go to the top ez
				--#14      has: Any            fs: 150 or 168             maf: Yes or No           maf105: Yes or Not         gf: none
                --9 hours assigned, no subtraction of gov HC hours
				 when FunctionalityLevelId in (29, 30) and isnull(gfhours,0) = 0 then
                    9
                --#1 has: 1       fs: any             maf: Yes or No             maf105: Yest        gf: any
                --Functionality is dominant, subtract government homecare hours
                when HasStatusId = 1 and MAF105 = 1 then
                    FsHours - GovHours
                --#2 has: 1       fs: any             maf: Yes             maf105: Not         gf: any
                --Functionality is dominant but capped at 105 hours, subtract government homecare hours
                when HasStatusId = 1 and MAF = 1 and MAF105 = 0 then 
                    (select min([value]) from (select 105 as [value] union select FsHours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                --#3 has: 1       fs: any             maf: No              maf105: Not         gf: any
                --Functionality is dominant but capped at 40 hours, subtract government homecare hours
                when HasStatusId = 1 and MAF = 0 and MAF105 = 0 then 
                    (select min([value]) from (select 40 as [value] union select FsHours) as maxt) - GovHours
				--when HasStatusId = 2 and FunctionalityLevelId  = 6 and maf105 = 1  and HAS2 = 1   then --by IZ
				  when HasStatusId = 2 and FunctionalityLevelId  = 34   and HAS2 = 1   then --by LK
                    (select min([value]) from (select 168 as [value] union select FsHours) as maxt) - iif(SubstractGovHours = 1,GovHours,0)
                           ----Mike code
                ----#4     has: 2       fs: Any             maf: Yes or No             maf105: Yes or Not         gf: none
                ----Functionality is dominant but capped at 40 hours, subtract government homecare hours
                --when HasStatusId = 2 and isnull(gfhours,0) = 0 then
                           ----Elvira changed: took out #4 and added #4a and #4b
                           --#4a  has: 2       fs: Any             maf: No             maf105: No          gf: none
                --Functionality is dominant but capped at 40 hours, subtract government homecare hours
                when HasStatusId = 2 and isnull(gfhours,0) = 0 and MAF = 0 and MAF105 = 0  then
                                 (select min([value]) from (select 40 as [value] union select FsHours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                           --#4b  has: 2       fs: Any             maf: Yes or             maf105: Yes         gf: none
                --Functionality is dominant but capped at 40 hours, subtract government homecare hours
                when HasStatusId = 2 and isnull(gfhours,0) = 0 and (MAF = 1 or MAF105 = 1) then-- what's changed:if maf and/or maf105 , then 56 according to Ali
                    (select min([value]) from (select 56 as [value] union select FsHours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                           ----Mike code
                ----#5     has: 2       fs: any             maf: Yes or No             maf105: Yes or Not         gf: < 40
                ----Functionality is dominant but capped at 40 hours, subtract government homecare hours
                --when HasStatusId = 2 and gfhours < 40 then
                --    (select min([value]) from (select 40 as [value] union select FsHours) as maxt) - GovHours
                           ----Elvira changed: took out #5 and added #5a and #5b
                           --#5a  has: 2       fs: any             maf: No             maf105: No          gf: < 40
                --Functionality is dominant but capped at 40 hours, subtract government homecare hours
                when HasStatusId = 2 and gfhours < 40 then
                    (select min([value]) from (select 40 as [value] union select FsHours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                           --#5b  has: 2       fs: any             maf: Yes or             maf105: Yes         gf: < 40
                --Functionality is dominant but capped at 56 hours, subtract government homecare hours
                when HasStatusId = 2 and gfhours <= 56 and (MAF = 1 or MAF105 = 1) then-- what's changed:if maf and/or maf105 , then 56 and gfhours < 56, not gfhours < 40 according to Ali  ---need <= instead of < ez
                    (select min([value]) from (select 56 as [value] union select FsHours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                --#6 has: 2       fs: any             maf: no              maf105: not         gf: > 40
                --GF Hours is dominant but capped at 40 hours, subtract government homecare hours
                when HasStatusId = 2 and maf = 0 and maf105 = 0  and gfhours > 40 then
                    (select min([value]) from (select 40 as [value] union select gfhours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                --#7 has: 2       fs: any             maf: yes             maf105: not         gf: > 40
                --GF Hours is dominant but capped at 105 hours, subtract government homecare hours
                           ----Mike code
                           ----when HasStatusId = 2 and  maf = 1 and maf105 = 0 and gfhours > 40 then
                           ----Elvira changed:
                when HasStatusId = 2 and  maf = 1 and maf105 = 0 and gfhours > 56 then --what's changed:gfhours > 56 according to Ali
                    (select min([value]) from (select 105 as [value] union select gfhours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                --#8 has: 2       fs: any             maf: Yes or No             maf105: yes         gf: > 56
                --GF Hours is dominant, subtract government homecare hours
                           ----Mike code
                           ----when HasStatusId = 2 and maf105 = 1 and gfhours > 40 then
                           ----Elvira changed:
                when HasStatusId = 2 and maf105 = 1 and gfhours > 56 then -- what's changed:and gfhours > 56, not gfhours > 40 according to Ali
                gfhours - GovHours
					-- (select min([value]) from (select 56 as [value] union select FsHours) as maxt) - iif(SubstractGovHours = 1,GovHours,0)
                           ----Mike code, Elvira combined with #10
                --#9 has: 3       fs: Any             maf: Yes or No             maf105: Yes or Not         gf: No
                --Functionality is dominant but capped at 25 hours, subtract government homecare hours
                --when HasStatusId = 3 and maf105 = 1 and coalesce(gfhours,0) = 0 then
               --    (select min([value]) from (select 25 as [value] union select fshours) as maxt) - GovHours
                --#10      has: 3       fs: any             maf: Yes or No             maf105: Yes or Not         gf: < 25
                --Functionality is dominant but capped at 25 hours, subtract government homecare hours
                           ----Mike code
                           ----when HasStatusId = 3 and gfhours < 25 then
                           ----Elvira changed:
                when HasStatusId = 3 and coalesce(gfhours,0) < 25 then
                                 ----Mike code
                                 ----(select min([value]) from (select 40 as [value] union select fshours) as maxt) - GovHours
                                 ----Elvira changed:
                    (select min([value]) from (select 25 as [value] union select fshours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali -- what's changed:should be 25, not 40 acording to Ali
                --#11      has: 3       fs: any             maf: no              maf105: not         gf: > 25
                ---GF Hours is dominant but capped at 40 hours, subtract government homecare hours
                when HasStatusId = 3 and maf = 0 and maf105 = 0 and gfhours > 25 then
                    (select min([value]) from (select 40 as [value] union select gfhours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                --#12      has: 3       fs: any             maf: yes             maf105: not         gf: > 25
                --GF Hours is dominant but capped at 105 hours, subtract government homecare hours
                when HasStatusId = 3 and  maf = 1 and maf105 = 0 and gfhours > 25 then
                    (select min([value]) from (select 105 as [value] union select gfhours) as maxt) - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                --#13      has: 3       fs: any             maf: Yes or No             maf105: yest        gf: > 25
                --GF Hours is dominant, subtract government homecare hours
                when HasStatusId = 3 and  maf105 = 1 and gfhours > 25 then 
                               gfhours - iif(SubstractGovHours = 1,GovHours,0) --changed by Elvira from GovHours according to Ali
                
               
                else 0
            end as HcCap,
            ----Mike code
            ----FSHours - (select max([value]) from (select HasHours as [value] union select GovHours) as maxt) as HospiceCareCap
                    ----accorging to Ali this is the calculation:
                    ----IF HAS= 2 IF MAF = NULL and MAF105 = NULL  then Hospice = 0
                    ----IF MAF = Yes and MAF105 = No then (select min([value]) from (select 105 as [value] union select FSHours) as mint) – (select(max([value]) from (GFhours,HasHours) – GovHours 
                    ----IF MAF105 = YES then FSHours - (select(max([value]) from (GFhours,HasHours) – GovHours
                    case when HasStatusId = 2 and maf105 = 1  then FSHours - (select max([value1]) from (select 56 as [value1] union select gfhours) as maxt)  - GovHours
                           when HasStatusId = 2 and maf = 1 and maf105 = 0 then
                                        (select min([value]) from (select 105 as [value] union select FSHours) as mint) - (select max([value1]) from (select 56 as [value1] union select gfhours) as maxt)  - GovHours
                           else 0 
                           end as HospiceCareCap
        from mafs as t
    )
	--select * 
       select t.ClientId,  
             t.AgencyGroupId,
             cast(t.StartDate as date) as StartDate,
             cast(t.EndDate as date) as EndDate,
             t.JoinDate,
             t.LeaveDate,
             t.DeceasedDate,
			 iif(t.HcCap<=0,0,t.HcCap) as HcCap,
			 --55 as HcCap,
             iif(HospiceCareCap < 0, 0,HospiceCareCap) as HospiceCareCap,
			-- iif(HospiceCareCap < 0, 55,HospiceCareCap) as HospiceCareCap,
			 --55.00 as HospiceCareCap, 
             iif(t.GovHours < 0, 0,t.GovHours) as GovHcHours,
			 t.HasStatusId,
			 t.FunctionalityLevelId,
			 t.gfhours,
			 t.Has2,
			 t.Maf,
			 t.Maf105
			 
            -- t.HcCap as HCCAp1
             --case when HospiceCareCap < 0 then 0 else HospiceCareCap end as HospiceCareCap,
             --case when t.GovHours < 0 then 0 else t.GovHours end as GovHcHours--,
             --t.FSHours,t.HasHours,t.maf105,t.maf, t.gfhours,FunctionalityLevelId,HasStatusId
       from hccaps as t
	   --where ClientId = 291110
	  




















GO



