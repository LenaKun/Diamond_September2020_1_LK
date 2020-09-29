CREATE FUNCTION [dbo].[fnApprovalStatusValid]
(
	@ApprovalStatusId int,
	@ApprovalStatusUpdated datetime,
	@ServiceType int,
	@DateFrom datetime,
	@DateTo datetime,
	@Remarks nvarchar(max)
	
)
RETURNS bit
--checks for clients with status Approval Home Care only
AS
BEGIN
--1024 - approval status HomeCare only
--8 - service type Home Care	
	return	case when @ApprovalStatusId!=1024 then 1 else
				case when  @ServiceType=8 then 1 else
					case when @ApprovalStatusUpdated>@DateTo then 1 else
					case when @ApprovalStatusUpdated>=@DateFrom and @ApprovalStatusUpdated<=@DateTo and IsNull(@Remarks,'')<>'' then 1 else 0 end
					      end    
					 end
               end
END
GO
