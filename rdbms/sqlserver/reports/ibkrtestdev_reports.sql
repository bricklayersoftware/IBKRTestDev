/*
SELECT lr.* FROM [testdevrdbms].[dbo].[LoadLogRequest] lr
WHERE lr.LoadID = ( SELECT MAX(LoadID) FROM [testdevrdbms].[dbo].[LoadLogEntry] le )
      AND [Symbol] = 'PLTR'
ORDER BY [Symbol] ASC, [Date] ASC, [TimeInterval] ASC
*/

/*
TRUNCATE TABLE [testdevrdbms].[dbo].[HistoricalData]
TRUNCATE TABLE [testdevrdbms].[dbo].[LoadLog]
TRUNCATE TABLE [testdevrdbms].[dbo].[LoadLogEntry]
TRUNCATE TABLE [testdevrdbms].[dbo].[LoadLogRequest]
*/

SELECT [RowID]
      ,[LoadID]
      ,[MessageID]
      ,[Message]
      ,[Event]
      ,[RequestID]
FROM [testdevrdbms].[dbo].[LatestLoadLog]

SELECT  [CountRowID]
    ,[Symbol]
    ,[Date]
    ,[Time]
    ,[OptionType]
    ,[Expiry]
    ,[TimeInterval]
    ,[Strike]
FROM [testdevrdbms].[dbo].[HistoricalDataProfile]

SELECT  [SumCountRowID]
    ,[Symbol]
    ,[Date]
    ,[TimeInterval]
FROM [testdevrdbms].[dbo].[HistoricalDataProfile2]


IF EXISTS(SELECT name FROM tempdb.sys.objects WHERE CHARINDEX('#missingdays',name)>0)  
    DROP TABLE #missingdays;

CREATE TABLE #missingdays
( [Symbol] VARCHAR(MAX), [Date] VARCHAR(MAX), [TimeInterval] VARCHAR(MAX) )

INSERT INTO #missingdays
EXEC  [dbo].[GetMissingTradingDays] 

SELECT * FROM #missingdays 

SELECT * FROM [testdevrdbms].[dbo].[LatestLoadLogRequestCountCheck]
ORDER BY [Symbol] ASC, [Date] ASC, [TimeInterval] ASC, [RequestID] ASC, [LoadID] ASC

SELECT * FROM [testdevrdbms].[dbo].[LatestLoadLogRequest]
ORDER BY [Symbol] ASC, [Date] ASC, [TimeInterval] ASC