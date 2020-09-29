-- =============================================
-- Script Template
-- =============================================


use master
go

IF (DB_ID(N'$(DatabaseName)') IS NOT NULL)
BEGIN
	
	DECLARE @rc      int,                       -- return code
			@fn      nvarchar(4000),            -- file name to back up to
			@dir     nvarchar(4000)             -- backup directory

	EXEC @rc = [master].[dbo].[xp_instance_regread] N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'BackupDirectory', @dir output, 'no_output'

	IF (@dir IS NULL)
	BEGIN 
		EXEC @rc = [master].[dbo].[xp_instance_regread] N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData', @dir output, 'no_output'
	END

	IF (@dir IS NULL)
	BEGIN
		EXEC @rc = [master].[dbo].[xp_instance_regread] N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\Setup', N'SQLDataRoot', @dir output, 'no_output'
		SELECT @dir = @dir + N'\Backup'
	END

	
	SELECT  @fn = @dir + N'\' + N'$(DatabaseName)' + N'-' + 
			CONVERT(nchar(8), GETDATE(), 112) + N'-' + 
			RIGHT(N'0' + RTRIM(CONVERT(nchar(2), DATEPART(hh, GETDATE()))), 2) + 
			RIGHT(N'0' + RTRIM(CONVERT(nchar(2), DATEPART(mi, getdate()))), 2) + 
			RIGHT(N'0' + RTRIM(CONVERT(nchar(2), DATEPART(ss, getdate()))), 2) + 
			N'.bak' 
	print 'bck to: ' + @fn
			BACKUP DATABASE [$(DatabaseName)] TO DISK = @fn
END

GO


use [$(DatabaseName)]
go