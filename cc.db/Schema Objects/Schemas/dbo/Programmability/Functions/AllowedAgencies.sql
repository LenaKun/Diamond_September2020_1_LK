--Returns a list of allowed agencies based on userid
CREATE FUNCTION [dbo].[AllowedAgencies]
(
	@userId int
)
RETURNS @returntable TABLE
(
	AgencyId int
)
AS
BEGIN

	declare @roleId int
	select @roleId = roleid from users where id = @userId

	--all agencies for admins/gpos
	if @roleId in (1, 16, 256, 1024)
		begin
		insert into @returntable 
		select id from Agencies
	end
	--single agency for agencyuser and agencyuserandreviewer
	else if @roleId in (2, 16384)
		begin
		insert into @returntable
		select agencyid from users where id = @userId
	end
	--agencies of single ser for ser and serandreviewer
	else if @roleId in (32, 32768)
		begin
		insert into @returntable
		select agencies.Id
		from users join Agencies on Users.AgencyGroupId = agencies.groupid
		where Users.Id = @userId
	end
	--agencies of multiple sers for po/pa
	else if @roleId in (8, 64)
		begin
		insert into @returntable
		select agencies.Id
		from UserAgencyGroups 
		join agencies on UserAgencyGroups.AgencyGroupId = Agencies.GroupId
		where UserAgencyGroups.UserId = @userId
	end
	--agencies of multiple sers for region read only
	else if @roleId in (512)
		begin
		insert into @returntable
		select agencies.Id
		from users join countries on Users.RegionId = Countries.RegionId
		join AgencyGroups on Countries.Id = AgencyGroups.CountryId
		join agencies on AgencyGroups.Id = Agencies.GroupId
		where Users.Id = @userId
	end
	
	RETURN
END
