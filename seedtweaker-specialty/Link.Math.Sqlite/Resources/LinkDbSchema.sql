--
-- File generated with SQLiteStudio v3.2.1 on Mon Jan 25 16:00:04 2021
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: GameData
CREATE TABLE GameData (Key STRING COLLATE NOCASE PRIMARY KEY, Value STRING);

-- Table: BetConfiguration
CREATE TABLE BetConfiguration (Id INTEGER PRIMARY KEY, Lines INTEGER NOT NULL, BetPerLine INTEGER NOT NULL, SideBet INTEGER NOT NULL, ExtraBet INTEGER NOT NULL, CustomBetInfo STRING);

-- Table: GameConfiguration
CREATE TABLE GameConfiguration (Id INTEGER PRIMARY KEY, PaytableIndex INTEGER NOT NULL, TotalBet INTEGER NOT NULL, PersistenceId INTEGER NOT NULL, IsLinear bit NOT NULL, BetConfigurationId INTEGER NOT NULL REFERENCES BetConfiguration (Id), FOREIGN KEY (BetConfigurationId) REFERENCES BetConfiguration (Id) ON DELETE CASCADE);

-- Table: Games
CREATE TABLE Games (Id INTEGER NOT NULL PRIMARY KEY, TotalWin INTEGER NOT NULL, GameSectionMask INTEGER NOT NULL, Occurrences INTEGER NOT NULL, GameConfigurationId INTEGER NOT NULL REFERENCES GameConfiguration (Id), RawRandomNumbers blob (2147483647), ProgressiveInfo STRING NOT NULL, FOREIGN KEY (GameConfigurationId) REFERENCES GameConfiguration (Id) ON DELETE CASCADE);

-- Index: Index_GameConfigurations
CREATE INDEX Index_GameConfigurations ON GameConfiguration (PaytableIndex, "PersistenceId", BetConfigurationId);

-- Index: Index_Games
CREATE INDEX Index_Games ON Games ("TotalWin", "GameSectionMask", GameConfigurationId, "ProgressiveInfo");

-- Index: Index_StakeConfigurations
CREATE INDEX Index_StakeConfigurations ON BetConfiguration ("Lines", "BetPerLine", "SideBet", "ExtraBet", CustomBetInfo);

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
