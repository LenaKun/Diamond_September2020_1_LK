CREATE FUNCTION [dbo].[fnGetDafTotalScore]
(
	@param1 xml (DafStructureXmlSchemaCollection)
)
RETURNS INT
AS
BEGIN
	RETURN @param1.value('sum(/root/item/@s)', 'shortDec')
END
