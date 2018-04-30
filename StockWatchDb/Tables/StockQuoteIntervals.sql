CREATE TABLE [dbo].[StockQuoteIntervals](
	[Symbol] [nvarchar](63) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[Open] [decimal](9,2) NOT NULL,
	[Close] [decimal](9,2) NOT NULL,
	[High] [decimal](9,2) NOT NULL,
	[Low] [decimal](9,2) NOT NULL,
	[Volume] [bigint] NOT NULL,
	[CollectionFunction] [smallint] NOT NULL,
    CONSTRAINT [PK_StockQuoteIntervals] PRIMARY KEY CLUSTERED 
(
	[Symbol], [StartTime], [EndTime]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO

CREATE INDEX [IX_StockQuoteIntervals_Symbol] ON [dbo].[StockQuoteIntervals] ([Symbol])

GO

CREATE INDEX [IX_StockQuoteIntervals_StartEndTimes] ON [dbo].[StockQuoteIntervals] ([StartTime], [EndTime])
