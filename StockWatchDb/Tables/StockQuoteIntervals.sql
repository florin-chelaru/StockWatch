CREATE TABLE [dbo].[StockQuoteIntervals](
	[Symbol] [nvarchar](255) NOT NULL,
	[Market] [nvarchar](255) NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[Open] [money] NOT NULL,
	[Close] [money] NOT NULL,
	[High] [money] NOT NULL,
	[Low] [money] NOT NULL,
	[Volume] [bigint] NOT NULL,
	[CollectionFunction] [nvarchar](255) NULL,
	[CreatedAt] [datetimeoffset](7) NOT NULL,
 [UpdatedAt] DATETIMEOFFSET NULL, 
    CONSTRAINT [PK_StockQuoteIntervals] PRIMARY KEY CLUSTERED 
(
	[Symbol], [StartTime], [EndTime]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
GO

CREATE INDEX [IX_StockQuoteIntervals_Symbol] ON [dbo].[StockQuoteIntervals] ([Symbol])

GO

CREATE INDEX [IX_StockQuoteIntervals_StartEndTimes] ON [dbo].[StockQuoteIntervals] ([StartTime], [EndTime])
