-- Revision inspired by Davide Mauri (@yorek, Azure SQL Product Manager)
-- regarding modern STRING_SPLIT(@s, '|', 1) support (SQL Server 2022+),
-- while maintaining backward compatibility via dbo.DelimitedSplit8K for SQL Server 2019.

USE [PokerApp]
GO

/************************************************************************************
    Database: PokerApp
    Author: John Belthoff
    Original Script Date: 2/6/2022
    Last Updated: 10/29/2025

    Notes:
    - This script reflects updates and refinements discussed with Davide Mauri (@yorek),
      Azure SQL Product Manager, who proposed using SQL Server 2022 features such as
      STRING_SPLIT(@s, '|', 1) with ordinal output for modernized data handling.
    - The current implementation remains optimized for SQL Server 2019 compatibility,
      retaining the dbo.DelimitedSplit8K function and transaction-safe patterns.
    - Future revisions may incorporate the 2022+ syntax once environments are upgraded.
************************************************************************************/

/****** Object:  UserDefinedFunction [dbo].[DelimitedSplit8K]    Script Date: 2/6/2022 9:51:09 AM ******/ 
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[DelimitedSplit8K]
--===== Define I/O parameters
        (@pString VARCHAR(8000), @pDelimiter CHAR(1))
--WARNING!!! DO NOT USE MAX DATA-TYPES HERE!  IT WILL KILL PERFORMANCE!
RETURNS TABLE WITH SCHEMABINDING AS
 RETURN
--===== "Inline" CTE Driven "Tally Table" produces values from 1 up to 10,000...
     -- enough to cover VARCHAR(8000)
  WITH E1(N) AS (
                 SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL
                 SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL
                 SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1
                ),                          --10E+1 or 10 rows
       E2(N) AS (SELECT 1 FROM E1 a, E1 b), --10E+2 or 100 rows
       E4(N) AS (SELECT 1 FROM E2 a, E2 b), --10E+4 or 10,000 rows max
 cteTally(N) AS (--==== This provides the "base" CTE and limits the number of rows right up front
                     -- for both a performance gain and prevention of accidental "overruns"
                 SELECT TOP (ISNULL(DATALENGTH(@pString),0)) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) FROM E4
                ),
cteStart(N1) AS (--==== This returns N+1 (starting position of each "element" just once for each delimiter)
                 SELECT 1 UNION ALL
                 SELECT t.N+1 FROM cteTally t WHERE SUBSTRING(@pString,t.N,1) = @pDelimiter
                ),
cteLen(N1,L1) AS(--==== Return start and length (for use in substring)
                 SELECT s.N1,
                        ISNULL(NULLIF(CHARINDEX(@pDelimiter,@pString,s.N1),0)-s.N1,8000)
                   FROM cteStart s
                )
--===== Do the actual split. The ISNULL/NULLIF combo handles the length for the final element when no delimiter is found.
 SELECT ItemNumber = ROW_NUMBER() OVER(ORDER BY l.N1),
        Item       = SUBSTRING(@pString, l.N1, l.L1)
   FROM cteLen l
;
GO

/****** Object:  Table [dbo].[Deck]    Script Date: 2/6/2022 9:51:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Deck]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	 NULL,
	 NULL,
	 NULL,
	[Value] [int] NULL,
	CONSTRAINT [PK_Deck] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Game]
(
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[GameID] [uniqueidentifier] NOT NULL,
	 NOT NULL,
	 NOT NULL,
	CONSTRAINT [PK_Game] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GameCards]
(
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[GameID] [uniqueidentifier] NOT NULL,
	[CardID] [int] NOT NULL,
	CONSTRAINT [PK_GameCards] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Suits]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	 NOT NULL,
	 NULL,
	 NULL,
	 NULL,
	 NULL,
	 NULL,
	CONSTRAINT [PK_Suits] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET IDENTITY_INSERT [dbo].[Deck] ON 
GO
-- (Deck inserts unchanged)
SET IDENTITY_INSERT [dbo].[Deck] OFF
GO

SET IDENTITY_INSERT [dbo].[Game] ON 
GO
-- (Game seed inserts unchanged)
SET IDENTITY_INSERT [dbo].[Game] OFF
GO

SET IDENTITY_INSERT [dbo].[GameCards] ON 
GO
-- (GameCards seed inserts unchanged)
SET IDENTITY_INSERT [dbo].[GameCards] OFF
GO

SET IDENTITY_INSERT [dbo].[Suits] ON 
GO
-- (Suits seed inserts unchanged)
SET IDENTITY_INSERT [dbo].[Suits] OFF
GO

ALTER TABLE [dbo].[Game] ADD  CONSTRAINT [DF_Game_GameID]  DEFAULT (newid()) FOR [GameID]
GO
ALTER TABLE [dbo].[Game] ADD  CONSTRAINT [DF_Game_CreateDate]  DEFAULT (sysdatetime()) FOR [CreateDate]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Game_InsertNewGame]
	@Array varchar(8000),
	@GameID uniqueidentifier OUTPUT
AS
SET NOCOUNT ON;
SET XACT_ABORT ON; -- Without this, the transaction will *always* commit
BEGIN TRAN;

	SELECT @GameID = NEWID();

	INSERT INTO dbo.Game (GameID)
	VALUES (@GameID);

	INSERT INTO dbo.GameCards (GameID, CardID)
	SELECT @GameID, a.Item
	FROM dbo.DelimitedSplit8K(@Array, '|') a
	ORDER BY a.ItemNumber;

COMMIT TRAN;
GO

/****** Object:  StoredProcedure [dbo].[Game_InsertNewGame2]    ******/
-- Updated for transaction safety (XACT_ABORT ON).
-- Thanks to Davide Mauri (@yorek, Azure SQL Product Manager) for highlighting
-- the modern STRING_SPLIT(@s, '|', 1) approach available in SQL Server 2022+.
-- Current implementation retains dbo.DelimitedSplit8K for SQL Server 2019 compatibility.
/*
    Example for SQL Server 2022+:

    INSERT INTO dbo.GameCards (GameID, CardID)
    SELECT @GameID, s.[value]
    FROM STRING_SPLIT(@Array, '|', 1) AS s  -- replaces dbo.DelimitedSplit8K
    ORDER BY s.[ordinal];
*/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Game_InsertNewGame2]
	@Array   varchar(8000),
	@CreateIP varchar(100),
	@GameID  uniqueidentifier OUTPUT
AS
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

	SELECT @GameID = NEWID();

	INSERT INTO dbo.Game (GameID, [CreateIP])
	VALUES (@GameID, @CreateIP);

	-- SQL Server 2019-compatible path
	INSERT INTO dbo.GameCards (GameID, CardID)
	SELECT @GameID, a.Item
	FROM dbo.DelimitedSplit8K(@Array, '|') a
	ORDER BY a.ItemNumber;

COMMIT TRAN;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GameDeck_GetNewDeck]
AS
SET NOCOUNT ON;
SELECT
	ROW_NUMBER() OVER (ORDER BY NEWID()) AS ID,
	s.ColorHex AS Color,
	d.Abbrv    AS Face,
	s.UTF8     AS Suit
FROM dbo.Deck d
JOIN dbo.Suits s ON s.ID = d.Suit;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GameDeck_GetRawDeck]
AS
SET NOCOUNT ON;
SELECT
	d.ID,
	s.ColorHex AS Color,
	d.Abbrv    AS Face,
	s.UTF8     AS Suit,
	d.Value
FROM dbo.Deck d
JOIN dbo.Suits s ON s.ID = d.Suit
ORDER BY d.ID;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GameDeck_GetShuffledDeck]
AS
SET NOCOUNT ON;
SELECT
	ROW_NUMBER() OVER (ORDER BY NEWID()) AS ID,
	s.ColorHex AS Color,
	d.Abbrv    AS Face,
	s.UTF8     AS Suit
FROM dbo.Deck d
JOIN dbo.Suits s ON s.ID = d.Suit;
GO
