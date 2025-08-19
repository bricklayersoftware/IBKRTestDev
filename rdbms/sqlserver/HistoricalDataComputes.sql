DROP VIEW [dbo].[HistoricalDataComputes]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[HistoricalDataComputes] AS
WITH MainCTE AS (
SELECT 
 	 [RowID]
	,[Symbol]
	,[Date]
	,[Time]
	,[_Open]
	,[_High]
	,[_Low]
	,[_Close]
	,[_Volume]
	,[_Count]
	,[_WAP]
	,[OptionType]
	,[_Strike]
	,[Expiry]
	,[TimeInterval]
	,[Open]
	,[High]
	,[Low]
	,[Close]
	,[Volume]
	,[Count]
	, CASE WHEN CAST([Count] AS DECIMAL) = 0 THEN NULL ELSE CAST( CAST([Volume] AS DECIMAL)/CAST([Count] AS DECIMAL) AS DECIMAL(10,2)) END AS [AvgOrderSize]
    , CASE WHEN [Open] < [Close] THEN 'RED' ELSE 'GREEN' END AS BarColor
    , ABS([High] - [Low]) AS BarSize
    , ABS([Open] - [Close]) AS BarSizeA
    , [Close] - LAG([Close], 1, 0) OVER (PARTITION BY Symbol, [Date] ORDER BY Time) AS Close_Delta
    , [Open] - LAG([Open], 1, 0) OVER (PARTITION BY Symbol, [Date] ORDER BY Time) AS Open_Delta
    , [High] - LAG([High], 1, 0) OVER (PARTITION BY Symbol, [Date] ORDER BY Time) AS High_Delta
    , [Low] - LAG([Low], 1, 0) OVER (PARTITION BY Symbol, [Date] ORDER BY Time) AS Low_Delta
FROM [dbo].[HistoricalData] )
SELECT 
[RowID]
	,[Symbol]
	,[Date]
	,[Time]
	,[_Open]
	,[_High]
	,[_Low]
	,[_Close]
	,[_Volume]
	,[_Count]
	,[_WAP]
	,[OptionType]
	,[_Strike]
	,[Expiry]
	,[TimeInterval]
	,[Open]
	,[High]
	,[Low]
	,[Close]
	,[Volume]
	,[Count]
	,[AvgOrderSize]
	,BarColor
	, BarSize
	, BarSizeA
	, Close_Delta
	, Open_Delta
	, High_Delta
	, Low_Delta
FROM MainCTE
GO
