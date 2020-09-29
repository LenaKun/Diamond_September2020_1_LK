﻿CREATE TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] (
  [SCHED_NAME] [NVARCHAR] (100)  NOT NULL ,
  [TRIGGER_NAME] [NVARCHAR] (150)  NOT NULL ,
  [TRIGGER_GROUP] [NVARCHAR] (150)  NOT NULL ,
  [REPEAT_COUNT] [INTEGER] NOT NULL ,
  [REPEAT_INTERVAL] [BIGINT] NOT NULL ,
  [TIMES_TRIGGERED] [INTEGER] NOT NULL
)
GO
ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] ADD
  CONSTRAINT [FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  ) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] ADD
  CONSTRAINT [PK_QRTZ_SIMPLE_TRIGGERS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  )