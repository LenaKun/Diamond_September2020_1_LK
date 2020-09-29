CREATE FUNCTION [dbo].[GetStartWeekDay]
(
	@AgencyGroupId int,
	@AppId int
)
RETURNS INT
AS
BEGIN
	return
	(
		select top 1 WeekStartDay 
		from AgencyApps ag
		join Agencies a on ag.AgencyId = a.Id
		where a.GroupId = @AgencyGroupId and ag.AppId = @AppId
	)
END
