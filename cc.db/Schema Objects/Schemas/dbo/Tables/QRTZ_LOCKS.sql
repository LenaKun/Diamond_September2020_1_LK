﻿CREATE TABLE [dbo].[QRTZ_LOCKS] (
  [SCHED_NAME] [NVARCHAR] (100)  NOT NULL ,
  [LOCK_NAME] [NVARCHAR] (40)  NOT NULL 
)
GO
ALTER TABLE [dbo].[QRTZ_LOCKS] ADD
  CONSTRAINT [PK_QRTZ_LOCKS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [LOCK_NAME]
  )