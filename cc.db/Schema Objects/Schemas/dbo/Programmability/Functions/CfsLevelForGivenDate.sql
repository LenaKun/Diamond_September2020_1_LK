CREATE FUNCTION [dbo].[CfsLevelForGivenDate]
(
	@clientId int,
	@givenDate datetime
)
RETURNS INT
AS
BEGIN
       declare @result int
	   declare @DeceasedDate date

       --select @result = (case when (HAS.HcStatusId = 1  and currentHours.HcCap > 25 and currentHours.HcCap <= 40 and (currentHours.HcCap + coalesce(govh.GovHours, 0)) <= 40)
       --                                      or (HAS.HcStatusId = 1 and HAS.MAFDate is null and HAS.MAF105Date is null and daf.[Name] in ('4', '5', '6'))
       --                                      or(HAS.HcStatusId = 2 and currentHours.HcCap > 25 and currentHours.HcCap <= 40 and (currentHours.HcCap + coalesce(govh.GovHours, 0)) <= 40) then 4
       --                               when currentHours.HcCap > 0 and currentHours.HcCap <= 4 then 1
       --                               when currentHours.HcCap > 4 and currentHours.HcCap <= 10 then 2
       --                               when currentHours.HcCap > 10 and currentHours.HcCap <= 25 then 3
       --                               when HAS.HcStatusId in(1,2) and currentHours.HcCap > 40 and currentHours.HcCap <= 56 then 5
       --                               when currentHours.HcCap > 56 and currentHours.HcCap <= 105 then 6
       --                               when currentHours.HcCap > 105 and currentHours.HcCap <= 168 then 7 else 0 end)
       select @result = (case when HAS.HcStatusId in( 1,2)  and currentHours.HcCap > 25 and HAS.MAFDate is null and HAS.MAF105Date is null and daf.[Name] in ('4', '5', '6') then 4
                                        when currentHours.HcCap > 0 and currentHours.HcCap <= 4 then 1
                                        when currentHours.HcCap > 4 and currentHours.HcCap <= 10 then 2
                                        when currentHours.HcCap > 10 and currentHours.HcCap <= 25 then 3
                                        when HAS.HcStatusId in(1,2) and currentHours.HcCap > 25 and currentHours.HcCap <= 56 then 5
                                        when currentHours.HcCap > 56 and currentHours.HcCap <= 105 then 6
                                        when currentHours.HcCap > 105 and currentHours.HcCap <= 168 then 7 else 0 end)
       from (
             select top 1 HcStatusId, c.MAFDate, c.MAF105Date
             from ClientHcStatuses h
             join Clients c on h.ClientId = c.Id
             where h.ClientId = @clientId and h.StartDate <= @givenDate
             order by h.StartDate desc
       ) as HAS
       outer apply (
             select top 1 HcCap
             from HcCapsTableRaw
			 join Clients on HcCapsTableRaw.ClientId = Clients.Id
             where ClientId = @clientId and StartDate <= @givenDate and (EndDate is null or EndDate > @givenDate or (Clients.LeaveDate is not null and Clients.LeaveReasonId =1 ))
             order by StartDate desc
       ) as currentHours
       outer apply (
             select top 1 fl.[Name] 
             from FunctionalityScores fs
             join FunctionalityLevels fl on fs.FunctionalityLevelId = fl.Id
             where fs.ClientId = @clientId and fs.StartDate <= @givenDate
             order by fs.StartDate desc
       ) as daf
       --outer apply (
       --     select top 1 g.[Value] as GovHours
       --     from GovHcHours g
       --     where g.ClientId = @clientId and g.StartDate <= @givenDate
       --     order by g.StartDate desc
       --) as govh
	   select @DeceasedDate =  DeceasedDate from Clients where Id = @clientId
       if @result is null or @DeceasedDate is not null begin set @result = 0 end

       return @result
END

