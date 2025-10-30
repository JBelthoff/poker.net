use [PokerApp]
go

/****** Object:  Table [dbo].[Deck]    Script Date: 10/30/2025 9:44:05 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Deck]') AND type in (N'U'))
DROP TABLE [dbo].[Deck]
GO


CREATE TABLE [dbo].[Deck](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Face] [varchar](10) NULL,
	[Abbrv] [varchar](2) NULL,
	[Suit] int NULL,
	[Value] [int] NULL,
 CONSTRAINT [PK_Deck] PRIMARY KEY CLUSTERED 
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



USE [PokerApp]
GO

/****** Object:  StoredProcedure [dbo].[GameDeck_GetNewDeck]    Script Date: 10/30/2025 9:47:39 AM ******/
DROP PROCEDURE [dbo].[GameDeck_GetNewDeck]
GO



USE [PokerApp]
GO

/****** Object:  StoredProcedure [dbo].[Game_InsertNewGame]    Script Date: 10/30/2025 9:51:43 AM ******/
DROP PROCEDURE [dbo].[Game_InsertNewGame]
GO

USE [PokerApp]
GO

/****** Object:  StoredProcedure [dbo].[Game_InsertNewGame2]    Script Date: 10/30/2025 9:52:02 AM ******/
DROP PROCEDURE [dbo].[Game_InsertNewGame2]
GO


-- Create a temporary table
CREATE TABLE #TempGame
(
    ID bigint,
    GameID uniqueidentifier,
    CreateDate datetime2(7),
    CreateIP varchar(100)
);

-- Insert all rows from the main Game table into the temp table
INSERT INTO #TempGame (ID, GameID, CreateDate, CreateIP)
SELECT [ID], [GameID], [CreateDate], [CreateIP]
FROM [dbo].[Game];



/****** Object:  Table [dbo].[Game]    Script Date: 10/30/2025 12:01:04 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Game]') AND type in (N'U'))
DROP TABLE [dbo].[Game]
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

ALTER TABLE [dbo].[Game] ADD  CONSTRAINT [DF_Game_GameID]  DEFAULT (newid()) FOR [GameID]
GO
ALTER TABLE [dbo].[Game] ADD  CONSTRAINT [DF_Game_DealerID]  DEFAULT (8) FOR [DealerID]
GO
ALTER TABLE [dbo].[Game] ADD  CONSTRAINT [DF_Game_CreateDate]  DEFAULT (sysdatetime()) FOR [CreateDate]
GO

SET IDENTITY_INSERT [dbo].[Game] ON

INSERT INTO [dbo].[Game] (ID, GameID, CreateDate, CreateIP)
SELECT [ID], [GameID], [CreateDate], [CreateIP]
FROM #TempGame;


SET IDENTITY_INSERT [dbo].[Game] OFF

-- Drop the temp table
DROP TABLE #TempGame;


USE [PokerApp]
GO

/****** Object:  StoredProcedure [dbo].[Game_InsertNewGame]    Script Date: 10/30/2025 9:51:07 AM ******/
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