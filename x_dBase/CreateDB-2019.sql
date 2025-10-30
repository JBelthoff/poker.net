-- SQL Server 2019 version of the PokerApp database creation script.
-- Fully backward compatible with all versions of SQL Server from 2008 onward.
-- Retains dbo.DelimitedSplit8K for pipe-delimited parsing to ensure broad
-- compatibility, including environments predating STRING_SPLIT().
-- CreateDB-2022.sql implements native STRING_SPLIT(@s, '|', 1) with ordinal output
-- per guidance from Davide Mauri (@yorek).

USE [PokerApp]
GO

/************************************************************************************
    Database: PokerApp
    Author: John Belthoff
    Original Script Date: 2007
    Last Updated: 10/30/2025

    Target Platform:
    - Optimized for SQL Server 2019
      and backward compatible to SQL Server 2008

    Notes:
    - Maintains full compatibility across SQL Server 2008–2019.
    - Uses dbo.DelimitedSplit8K for string parsing.
    - Avoids 2022-only syntax while retaining transaction safety (SET XACT_ABORT ON).
    - Data types and DDL structures conform to features available since SQL Server 2008.

    Acknowledgment:
    - Thanks to Davide Mauri (@yorek, Azure SQL Product Manager) for suggesting
      the 2022 STRING_SPLIT ordinal pattern; those enhancements are implemented
      in CreateDB-2022.sql.
************************************************************************************/

/****** Object:  UserDefinedFunction [dbo].[DelimitedSplit8K]    Script Date: 10/30/2025 10:05:09 AM ******/
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
/****** Object:  Table [dbo].[Deck]    Script Date: 10/30/2025 10:05:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Deck](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Face] [varchar](10) NULL,
	[Abbrv] [varchar](2) NULL,
	[Suit] [int] NULL,
	[Value] [int] NULL,
 CONSTRAINT [PK_Deck] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Game]    Script Date: 10/30/2025 10:05:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Game](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[GameID] [uniqueidentifier] NOT NULL,
	[DealerID] [int] NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[CreateIP] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Game] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GameCards]    Script Date: 10/30/2025 10:05:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GameCards](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[GameID] [uniqueidentifier] NOT NULL,
	[CardID] [int] NOT NULL,
 CONSTRAINT [PK_GameCards] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Suits]    Script Date: 10/30/2025 10:05:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Suits](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](10) NOT NULL,
	[Abbrv] [char](1) NULL,
	[Color] [varchar](10) NULL,
	[ColorHex] [char](6) NULL,
	[HTMLName] [varchar](10) NULL,
	[UTF8] [varchar](10) NULL,
 CONSTRAINT [PK_Suits] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Deck] ON 
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (1, N'Ace', N'A', 1, 268442665)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (2, N'Ace', N'A', 2, 268446761)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (3, N'Ace', N'A', 3, 268454953)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (4, N'Ace', N'A', 4, 268471337)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (5, N'2', N'2', 1, 69634)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (6, N'2', N'2', 2, 73730)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (7, N'2', N'2', 3, 81922)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (8, N'2', N'2', 4, 98306)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (9, N'3', N'3', 1, 135427)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (10, N'3', N'3', 2, 139523)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (11, N'3', N'3', 3, 147715)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (12, N'3', N'3', 4, 164099)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (13, N'4', N'4', 1, 266757)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (14, N'4', N'4', 2, 270853)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (15, N'4', N'4', 3, 279045)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (16, N'4', N'4', 4, 295429)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (17, N'5', N'5', 1, 529159)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (18, N'5', N'5', 2, 533255)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (19, N'5', N'5', 3, 541447)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (20, N'5', N'5', 4, 557831)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (21, N'6', N'6', 1, 1053707)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (22, N'6', N'6', 2, 1057803)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (23, N'6', N'6', 3, 1065995)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (24, N'6', N'6', 4, 1082379)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (25, N'7', N'7', 1, 2102541)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (26, N'7', N'7', 2, 2106637)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (27, N'7', N'7', 3, 2114829)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (28, N'7', N'7', 4, 2131213)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (29, N'8', N'8', 1, 4199953)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (30, N'8', N'8', 2, 4204049)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (31, N'8', N'8', 3, 4212241)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (32, N'8', N'8', 4, 4228625)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (33, N'9', N'9', 1, 8394515)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (34, N'9', N'9', 2, 8398611)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (35, N'9', N'9', 3, 8406803)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (36, N'9', N'9', 4, 8423187)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (37, N'10', N'10', 1, 16783383)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (38, N'10', N'10', 2, 16787479)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (39, N'10', N'10', 3, 16795671)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (40, N'10', N'10', 4, 16812055)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (41, N'Jack', N'J', 1, 33560861)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (42, N'Jack', N'J', 2, 33564957)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (43, N'Jack', N'J', 3, 33573149)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (44, N'Jack', N'J', 4, 33589533)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (45, N'Queen', N'Q', 1, 67115551)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (46, N'Queen', N'Q', 2, 67119647)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (47, N'Queen', N'Q', 3, 67127839)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (48, N'Queen', N'Q', 4, 67144223)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (49, N'King', N'K', 1, 134224677)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (50, N'King', N'K', 2, 134228773)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (51, N'King', N'K', 3, 134236965)
GO
INSERT [dbo].[Deck] ([ID], [Face], [Abbrv], [Suit], [Value]) VALUES (52, N'King', N'K', 4, 134253349)
GO




SET IDENTITY_INSERT [dbo].[Deck] OFF
GO
SET IDENTITY_INSERT [dbo].[Game] ON 
GO
INSERT [dbo].[Game] ([ID], [GameID], [DealerID], [CreateDate], [CreateIP]) VALUES (1, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 8, CAST(N'2022-02-06T09:47:11.3113305' AS DateTime2), N'::1')
GO
INSERT [dbo].[Game] ([ID], [GameID], [DealerID], [CreateDate], [CreateIP]) VALUES (2, N'ad908d16-2138-48a2-944d-4f11baa714af', 8, CAST(N'2022-02-06T09:47:18.0424598' AS DateTime2), N'::1')
GO
INSERT [dbo].[Game] ([ID], [GameID], [DealerID], [CreateDate], [CreateIP]) VALUES (3, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 8, CAST(N'2022-02-06T09:47:24.1623672' AS DateTime2), N'::1')
GO
INSERT [dbo].[Game] ([ID], [GameID], [DealerID], [CreateDate], [CreateIP]) VALUES (4, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 8, CAST(N'2022-02-06T09:47:32.1592421' AS DateTime2), N'::1')
GO
SET IDENTITY_INSERT [dbo].[Game] OFF
GO





SET IDENTITY_INSERT [dbo].[GameCards] ON 
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (1, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 52)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (2, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 19)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (3, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 3)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (4, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 6)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (5, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 36)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (6, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 31)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (7, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 8)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (8, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 34)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (9, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 35)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (10, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 16)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (11, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 42)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (12, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 47)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (13, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 1)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (14, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 33)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (15, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 18)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (16, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 28)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (17, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 39)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (18, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 48)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (19, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 25)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (20, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 9)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (21, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 46)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (22, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 17)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (23, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 44)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (24, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 4)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (25, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 15)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (26, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 21)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (27, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 24)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (28, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 30)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (29, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 43)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (30, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 29)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (31, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 22)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (32, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 23)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (33, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 51)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (34, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 10)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (35, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 50)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (36, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 12)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (37, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 45)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (38, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 38)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (39, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 32)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (40, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 2)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (41, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 5)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (42, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 14)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (43, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 27)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (44, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 7)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (45, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 26)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (46, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 13)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (47, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 11)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (48, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 41)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (49, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 49)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (50, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 37)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (51, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 20)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (52, N'00932162-9a80-4c37-841b-6dd18e46c9c6', 40)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (53, N'ad908d16-2138-48a2-944d-4f11baa714af', 5)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (54, N'ad908d16-2138-48a2-944d-4f11baa714af', 1)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (55, N'ad908d16-2138-48a2-944d-4f11baa714af', 3)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (56, N'ad908d16-2138-48a2-944d-4f11baa714af', 18)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (57, N'ad908d16-2138-48a2-944d-4f11baa714af', 21)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (58, N'ad908d16-2138-48a2-944d-4f11baa714af', 10)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (59, N'ad908d16-2138-48a2-944d-4f11baa714af', 22)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (60, N'ad908d16-2138-48a2-944d-4f11baa714af', 31)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (61, N'ad908d16-2138-48a2-944d-4f11baa714af', 11)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (62, N'ad908d16-2138-48a2-944d-4f11baa714af', 25)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (63, N'ad908d16-2138-48a2-944d-4f11baa714af', 37)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (64, N'ad908d16-2138-48a2-944d-4f11baa714af', 41)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (65, N'ad908d16-2138-48a2-944d-4f11baa714af', 7)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (66, N'ad908d16-2138-48a2-944d-4f11baa714af', 27)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (67, N'ad908d16-2138-48a2-944d-4f11baa714af', 29)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (68, N'ad908d16-2138-48a2-944d-4f11baa714af', 13)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (69, N'ad908d16-2138-48a2-944d-4f11baa714af', 4)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (70, N'ad908d16-2138-48a2-944d-4f11baa714af', 8)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (71, N'ad908d16-2138-48a2-944d-4f11baa714af', 36)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (72, N'ad908d16-2138-48a2-944d-4f11baa714af', 44)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (73, N'ad908d16-2138-48a2-944d-4f11baa714af', 43)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (74, N'ad908d16-2138-48a2-944d-4f11baa714af', 24)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (75, N'ad908d16-2138-48a2-944d-4f11baa714af', 19)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (76, N'ad908d16-2138-48a2-944d-4f11baa714af', 16)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (77, N'ad908d16-2138-48a2-944d-4f11baa714af', 49)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (78, N'ad908d16-2138-48a2-944d-4f11baa714af', 20)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (79, N'ad908d16-2138-48a2-944d-4f11baa714af', 51)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (80, N'ad908d16-2138-48a2-944d-4f11baa714af', 46)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (81, N'ad908d16-2138-48a2-944d-4f11baa714af', 30)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (82, N'ad908d16-2138-48a2-944d-4f11baa714af', 50)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (83, N'ad908d16-2138-48a2-944d-4f11baa714af', 45)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (84, N'ad908d16-2138-48a2-944d-4f11baa714af', 26)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (85, N'ad908d16-2138-48a2-944d-4f11baa714af', 40)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (86, N'ad908d16-2138-48a2-944d-4f11baa714af', 35)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (87, N'ad908d16-2138-48a2-944d-4f11baa714af', 39)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (88, N'ad908d16-2138-48a2-944d-4f11baa714af', 23)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (89, N'ad908d16-2138-48a2-944d-4f11baa714af', 47)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (90, N'ad908d16-2138-48a2-944d-4f11baa714af', 12)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (91, N'ad908d16-2138-48a2-944d-4f11baa714af', 15)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (92, N'ad908d16-2138-48a2-944d-4f11baa714af', 42)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (93, N'ad908d16-2138-48a2-944d-4f11baa714af', 6)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (94, N'ad908d16-2138-48a2-944d-4f11baa714af', 2)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (95, N'ad908d16-2138-48a2-944d-4f11baa714af', 28)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (96, N'ad908d16-2138-48a2-944d-4f11baa714af', 9)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (97, N'ad908d16-2138-48a2-944d-4f11baa714af', 17)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (98, N'ad908d16-2138-48a2-944d-4f11baa714af', 32)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (99, N'ad908d16-2138-48a2-944d-4f11baa714af', 33)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (100, N'ad908d16-2138-48a2-944d-4f11baa714af', 34)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (101, N'ad908d16-2138-48a2-944d-4f11baa714af', 38)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (102, N'ad908d16-2138-48a2-944d-4f11baa714af', 52)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (103, N'ad908d16-2138-48a2-944d-4f11baa714af', 14)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (104, N'ad908d16-2138-48a2-944d-4f11baa714af', 48)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (105, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 23)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (106, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 20)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (107, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 13)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (108, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 11)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (109, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 19)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (110, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 28)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (111, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 4)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (112, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 38)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (113, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 22)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (114, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 29)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (115, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 49)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (116, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 33)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (117, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 24)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (118, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 31)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (119, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 30)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (120, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 47)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (121, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 27)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (122, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 16)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (123, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 51)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (124, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 35)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (125, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 26)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (126, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 36)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (127, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 52)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (128, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 5)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (129, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 14)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (130, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 46)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (131, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 10)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (132, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 21)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (133, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 3)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (134, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 15)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (135, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 12)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (136, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 44)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (137, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 6)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (138, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 2)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (139, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 42)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (140, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 45)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (141, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 41)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (142, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 37)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (143, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 25)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (144, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 8)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (145, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 50)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (146, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 43)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (147, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 40)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (148, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 32)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (149, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 18)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (150, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 48)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (151, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 34)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (152, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 17)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (153, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 39)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (154, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 7)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (155, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 1)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (156, N'97dca9ce-c974-4d05-9be2-fb2929bcfd45', 9)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (157, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 46)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (158, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 29)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (159, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 13)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (160, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 51)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (161, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 5)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (162, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 19)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (163, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 3)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (164, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 21)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (165, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 28)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (166, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 4)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (167, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 35)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (168, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 9)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (169, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 52)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (170, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 23)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (171, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 15)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (172, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 41)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (173, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 39)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (174, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 20)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (175, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 16)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (176, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 11)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (177, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 32)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (178, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 48)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (179, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 30)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (180, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 1)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (181, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 17)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (182, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 37)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (183, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 42)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (184, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 26)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (185, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 2)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (186, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 6)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (187, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 45)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (188, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 22)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (189, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 44)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (190, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 14)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (191, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 50)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (192, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 47)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (193, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 36)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (194, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 8)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (195, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 49)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (196, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 18)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (197, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 40)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (198, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 38)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (199, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 31)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (200, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 25)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (201, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 24)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (202, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 34)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (203, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 27)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (204, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 12)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (205, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 7)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (206, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 43)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (207, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 33)
GO
INSERT [dbo].[GameCards] ([ID], [GameID], [CardID]) VALUES (208, N'5a4512f7-77a2-4954-a77a-8b0a63be6ac8', 10)
GO
SET IDENTITY_INSERT [dbo].[GameCards] OFF
GO
SET IDENTITY_INSERT [dbo].[Suits] ON 
GO
INSERT [dbo].[Suits] ([ID], [Name], [Abbrv], [Color], [ColorHex], [HTMLName], [UTF8]) VALUES (1, N'Spade', N'S', N'Black', N'000000', N'&spades;', N'&#9824;')
GO
INSERT [dbo].[Suits] ([ID], [Name], [Abbrv], [Color], [ColorHex], [HTMLName], [UTF8]) VALUES (2, N'Heart', N'H', N'Red', N'FF0000', N'&hearts;', N'&#9829;')
GO
INSERT [dbo].[Suits] ([ID], [Name], [Abbrv], [Color], [ColorHex], [HTMLName], [UTF8]) VALUES (3, N'Diamond', N'D', N'Red', N'FF0000', N'&diams;', N'&#9830;')
GO
INSERT [dbo].[Suits] ([ID], [Name], [Abbrv], [Color], [ColorHex], [HTMLName], [UTF8]) VALUES (4, N'Club', N'C', N'Black', N'000000', N'&clubs;', N'&#9827;')
GO
SET IDENTITY_INSERT [dbo].[Suits] OFF
GO
ALTER TABLE [dbo].[Game] ADD  CONSTRAINT [DF_Game_GameID]  DEFAULT (newid()) FOR [GameID]
GO
ALTER TABLE [dbo].[Game] ADD  CONSTRAINT [DF_Game_DealerID]  DEFAULT (8) FOR [DealerID]
GO
ALTER TABLE [dbo].[Game] ADD  CONSTRAINT [DF_Game_CreateDate]  DEFAULT (sysdatetime()) FOR [CreateDate]
GO
/****** Object:  StoredProcedure [dbo].[Game_InsertNewGame]    Script Date: 10/30/2025 10:05:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE Procedure [dbo].[Game_InsertNewGame]

	@Array varchar(8000)
	, @DealerID int
	, @CreateIP varchar(100)
	, @GameID uniqueidentifier Output

As

Set NoCount On
Set XACT_ABORT On

Begin Tran

	Select @GameID = NEWID()
	
	Insert Into dbo.Game ( GameID, [DealerID], [CreateIP] )
	Values ( @GameID, @DealerID, @CreateIP )

	Insert Into dbo.GameCards
	Select @GameID, a.Item
	From dbo.DelimitedSplit8K(@Array, '|') a
	Order By a.ItemNumber
	
Commit Tran


GO
/****** Object:  StoredProcedure [dbo].[GameDeck_GetRawDeck]    Script Date: 10/30/2025 10:05:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[GameDeck_GetRawDeck]

As

Set NoCount On

	Select
		d.ID
		, s.ColorHex Color
		, d.Abbrv Face
		, s.UTF8 Suit
		, d.Value
	From dbo.Deck d
		Inner Join dbo.Suits s
			On s.ID = d.Suit
	Order By d.ID

GO
/****** Object:  StoredProcedure [dbo].[GameDeck_GetShuffledDeck]    Script Date: 10/30/2025 10:05:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==========================================================================================
-- LEGACY NOTICE:
-- This stored procedure is retained for backward compatibility only.
-- The shuffle logic has been moved to the C# Web Application.
-- Use [dbo].[GameDeck_GetRawDeck] instead for the current implementation.
--
-- Legacy Behavior:
-- Returns a shuffled deck using ORDER BY NEWID(), joining Deck → Suits.
-- ==========================================================================================

Create Procedure [dbo].[GameDeck_GetShuffledDeck]

As

Set NoCount On

	Select
		ROW_NUMBER() Over(Order By NewID()) ID
		, s.ColorHex Color
		, d.Abbrv Face
		, s.UTF8 Suit
	From dbo.Deck d
		Inner Join dbo.Suits s
			On s.ID = d.Suit

GO
USE [master]
GO
ALTER DATABASE [PokerApp] SET  READ_WRITE 
GO
