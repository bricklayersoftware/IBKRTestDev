WITH DataCTE([RowID], [Symbol], [Date], [Time], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [OptionType], [_Strike], [Expiry], [TimeInterval], [Open], [High], [Low], [Close], [Volume], [Count]) AS
(
    SELECT
          [RowID]
        , [Symbol]
        , [Date]
        , [Time]
        , [_Open]
        , [_High]
        , [_Low]
        , [_Close]
        , [_Volume]
        , [_Count]
        , [_WAP]
        , COALESCE([OptionType],'') AS [OptionType]
        , COALESCE([_Strike],'') AS [_Strike]
        , COALESCE([Expiry],'')AS [Expiry]
        , [TimeInterval]
        , [Open]
        , [High]
        , [Low]
        , [Close]
        , [Volume]
        , [Count]
    FROM [dbo].[HistoricalData] h
),
StepCTE([RowID], [Symbol], [Date], [Time], [_Open], [_High], [_Low], [_Close], [_Volume], [_Count], [_WAP], [OptionType], [_Strike], [Expiry], [TimeInterval], [Open], [High], [Low], [Close], [Volume], [Count]) AS
(
    SELECT
      [RowID]
    , [Symbol]
    , [Date]
    , [Time]
    , [_Open]
    , [_High]
    , [_Low]
    , [_Close]
    , [_Volume]
    , [_Count]
    , [_WAP]
    , COALESCE([OptionType],'') AS [OptionType]
    , COALESCE([_Strike],'') AS [_Strike]
    , COALESCE([Expiry],'')AS [Expiry]
    , [TimeInterval]
    , [Open]
    , [High]
    , [Low]
    , [Close]
    , [Volume]
    , [Count]
    FROM [dbo].[HistoricalData] k
),
FinalCTE(hRowID, kRowID, Symbol) AS (
    SELECT
        h.RowID AS hRowID, 
        k.RowID AS kRowID, 
        h.[Symbol] AS Symbol
    FROM DataCTE h
    JOIN StepCTE k ON 
    (
        --h.[RowID] = k.[RowID] AND 
        h.[Symbol] = k.[Symbol] AND 
        h.[Date] = k.[Date] AND 
        --h.[Time] = k.[Time] AND 
        h.[_Open] = k.[_Open] AND 
        h.[_High] = k.[_High] AND 
        h.[_Low] = k.[_Low] AND 
        h.[_Close] = k.[_Close] AND 
        h.[_Volume] = k.[_Volume] AND 
        h.[_Count] = k.[_Count] AND 
        h.[_WAP] = k.[_WAP] AND 
        h.[OptionType] = k.[OptionType] AND 
        h.[_Strike] = k.[_Strike] AND 
        h.[Expiry] = k.[Expiry] AND 
        h.[TimeInterval] = k.[TimeInterval] AND 
        h.[Open] = k.[Open] AND 
        h.[High] = k.[High] AND 
        h.[Low] = k.[Low] AND 
        h.[Close] = k.[Close] AND 
        h.[Volume] = k.[Volume] AND 
        h.[Count] = k.[Count]
    )
    WHERE h.[RowID] < k.[RowID] AND k.[TimeInterval] = '1D'
)
--DELETE
SELECT *
FROM FinalCTE