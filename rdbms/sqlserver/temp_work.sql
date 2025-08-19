WITH MainCTE AS (
SELECT [RowID]
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
      ,[BarColor]
      ,[BarSize]
      ,[BarSizeA]
      ,[Close_Delta]
      ,[Open_Delta]
      ,[High_Delta]
      ,[Low_Delta]
      , AVG(BarSize) OVER (PARTITION BY Symbol, Date ORDER BY Time ASC ) AS AvgBarSize
      , STDEV(BarSize) OVER (PARTITION BY Symbol, Date ORDER BY Time ASC ) AS SDBarSize
      --, RANK() OVER (PARTITION BY Symbol, Date ORDER BY BarSize ASC ) AS BarSizeRank
  FROM [dbo].[HistoricalDataComputes]
WHERE TimeInterval = '5S' AND [Date] = '20250618' AND Symbol = 'GOOGL'
-- ORDER BY Symbol ASC, [Time] ASC
) 
SELECT BarSize, COUNT(BarSize) FROM MainCTE
GROUP BY BarSize
ORDER BY BarSize ASC