SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HistoricalData]') AND type in (N'U'))
DROP TABLE [dbo].[HistoricalData]
GO

CREATE TABLE [dbo].[HistoricalData](
	[RowID]                 [int] IDENTITY(1,1) NOT NULL,
    [Symbol]                [varchar](200) NULL,
    [Date]                  [varchar](200) NULL,
    [Time]                  [varchar](200) NULL,
    [_Open]                 [varchar](200) NULL,
    [_High]                 [varchar](200) NULL,
    [_Low]                  [varchar](200) NULL,
    [_Close]                [varchar](200) NULL,
    [_Volume]               [varchar](200) NULL,
    [_Count]                [varchar](200) NULL,
    [_WAP]                  [varchar](200) NULL,
    [OptionType]            [varchar](200) NULL,
    [_Strike]               [varchar](200) NULL,
    [Expiry]                [varchar](200) NULL,
    [TimeInterval]          [varchar](200) NULL,
    [Open]                  AS CONVERT(DECIMAL(10,2),[_Open]),
    [High]                  AS CONVERT(DECIMAL(10,2),[_High]),
    [Low]                   AS CONVERT(DECIMAL(10,2),[_Low]),
    [Close]                 AS CONVERT(DECIMAL(10,2),[_Close]),
    [Volume]                AS CAST(100*CONVERT(DECIMAL(10,2),[_Volume]) AS INT),
    [Count]                 AS CAST(CONVERT(DECIMAL(10,2),[_Count]) AS INT)
) ON [PRIMARY]
GO


INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('QQQ','20250618','530.1','532.55','527.4','528.99','294121.49','191761','530.362','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('AAPL','20250618','195.92','197.57','195.07','196.58','254437.92','133386','196.419','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('MSFT','20250618','478','481','474.46','480.24','68076.35','41604','478.929','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('NVDA','20250618','144.02','145.65','143.12','145.48','1132939.5','431314','144.907','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('AMZN','20250618','215.09','217.96','212.51','212.52','229702.63','116382','215.407','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('NFLX','20250618','1229.55','1242','1220.5','1222.29','7726.75','5350','1229.115','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('AVGO','20250618','250.82','255.64','249.41','251.26','111049.23','66365','252.028','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('META','20250618','698.17','701.59','694.9','695.77','34936.94','21212','698.411','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('TSLA','20250618','317.31','329.32','315.45','322.05','695319.39','306605','323.442','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('GOOGL','20250618','176.02','176.56','173.2','173.32','152035.02','96610','175.123','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('GOOG','20250618','177.29','177.83','173.92','173.98','127499.66','77770','176.018','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('TMUS','20250618','222.23','223','220.56','220.99','31792.96','21339','221.941','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('COST','20250618','980.59','982.65','974.07','974.9','3530.86','2402','978.132','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('PLTR','20250618','139.1','140.36','137.49','139.96','443689.37','179937','139.003','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('CSCO','20250618','65.68','66.34','65.38','65.84','134311.73','65130','65.936','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('PEP','20250618','129.65','129.68','128.59','129.07','49409.65','27652','129.236','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('LIN','20250618','460.9','462.43','458.27','458.7','3476.29','2563','460.267','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('ISRG','20250618','512.55','513.96','506.53','509.49','4046.13','3059','510.813','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('INTU','20250618','762.37','765.59','750.38','754.83','8140.6','5014','755.263','1D')
GO
INSERT INTO [dbo].[HistoricalData] ( [Symbol], [Date], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [TimeInterval] ) VALUES ('BKNG','20250618','5306.9','5351.9','5277.33','5286.26','301.88','257','5303.977','1D')
GO



DROP VIEW [dbo].[HistoricalDataComputes]
GO

CREATE VIEW HistoricalDataComputes AS
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
	, CAST( CAST([Volume] AS DECIMAL)/CAST([Count] AS DECIMAL) AS DECIMAL(10,2)) AS [AvgOrderSize]
    , CASE WHEN [Open] < [Close] THEN 'RED' ELSE 'GREEN' END AS BarColor
    , ABS([High] - [Low]) AS BarSize
    , ABS([Open] - [Close]) AS BarSizeA
FROM [dbo].[HistoricalData]
GO

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
  FROM [dbo].[HistoricalDataComputes]