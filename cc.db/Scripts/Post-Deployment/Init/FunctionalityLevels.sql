-- =============================================
-- Script Template
-- =============================================

USE [cc]


declare @t tABLE (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [dbo].[mediumString] NULL,
	[Active] [bit] NOT NULL,
	[MinScore] [dbo].[shortDec] NOT NULL,
	[MaxScore] [dbo].[shortDec] NOT NULL,
	[HcHoursLimit] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[RelatedLevel] nvarchar(max) NOT NULL)



INSERT INTO @t           ([Name]           ,[Active]           ,[MinScore]           ,[MaxScore]           ,[HcHoursLimit]           ,[StartDate]           ,[RelatedLevel])     VALUES
('High Functional Capacity',1	,3.25,	4.5,	4,	'01/01/1900',	'High' )
INSERT INTO @t           ([Name]           ,[Active]           ,[MinScore]           ,[MaxScore]           ,[HcHoursLimit]           ,[StartDate]           ,[RelatedLevel])     VALUES
('Moderate Functional Capacity',1,	4.75,	10.75,	10,	'01/01/1900',	'Moderate')
INSERT INTO @t           ([Name]           ,[Active]           ,[MinScore]           ,[MaxScore]           ,[HcHoursLimit]           ,[StartDate]           ,[RelatedLevel])     VALUES
('Low Functional Capacity',1,	11	,25,	25,	'01/01/1900',	'Low')


declare @c int
select @c=count(*)
	from @t as t left outer join dbo.RelatedFunctionalityLevels as r on t.relatedlevel=r.name where r.id is null
if @c>0
begin
	insert into dbo.RelatedFunctionalityLevels (name)
	select distinct t.relatedlevel
	from @t as t left outer join dbo.RelatedFunctionalityLevels as r on t.relatedlevel=r.name where r.id is null
end


insert into dbo.functionalityLevels ([Name]           ,[Active]           ,[MinScore]           ,[MaxScore]           ,[HcHoursLimit]           ,[StartDate]           ,[RelatedLevel])
select t.Name           ,t.[Active]           ,t.[MinScore]           ,t.[MaxScore]           ,t.[HcHoursLimit]           ,t.[StartDate]           ,r.Id
	from @t as t inner join dbo.RelatedFunctionalityLevels as r on t.relatedlevel=r.name