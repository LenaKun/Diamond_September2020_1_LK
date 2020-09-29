CREATE TRIGGER [ClientHcApprovalStatusUpdate]
	ON [dbo].[Clients]
	WITH EXECUTE AS CALLER
    AFTER UPDATE, INSERT
	AS
	BEGIN
		SET NOCOUNT ON
		if(UPDATE(FundStatusId) or UPDATE(ApprovalStatusId) or UPDATE(BirthCountryId) or UPDATE(CountryId) or (Select Count(*) From inserted) > 0 and (Select Count(*) From deleted) = 0)
		begin
			;merge ClientHcStatuses as t
			using (
				select *
				from 
				(
					select c.ID as ClientId,
						--first hc record backdated to the join date (typically after first fund status update of a new client) 
						case when exists (select top 1 1 from dbo.clienthcstatuses as i2 where i2.clientid = c.Id) then cast(getdate() as date) else c.JoinDate end as Startdate,
						c.joinDate,
						c.ApprovalStatusId,
						c.FundStatusId,
						c.BirthCountryId,
						c.CountryId,
						dbo.fnHomecareApprovalStatusId(c.FundStatusId, c.ApprovalStatusId, c.BirthCountryId, c.CountryId) as HcStatusId
					from inserted AS C
					where C.ApprovalStatusId <> 4
				) as i1 
			) as s on t.ClientId = s.ClientId and t.StartDate = s.StartDate
			when not matched by target then
				insert (clientid, ApprovalStatusId, FundStatusId, BirthCountryId, CountryId, HcStatusId, StartDate)
				values (s.ClientId, s.ApprovalStatusId, s.FundStatusId, BirthCountryId, CountryId, s.HcStatusId, s.StartDate)
			when matched then
				update set HcStatusId = s.HcStatusId, 
					ApprovalStatusId = s.ApprovalStatusId, 
					FundStatusId = s.FundStatusId,
					BirthCountryId = s.BirthCountryId,
					CountryId = s.CountryId
			;
		end
	END
