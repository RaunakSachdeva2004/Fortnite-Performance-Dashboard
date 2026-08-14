

IF DB_ID('FortniteDashboardDb') IS NULL
BEGIN
    CREATE DATABASE FortniteDashboardDb;
END
GO

USE FortniteDashboardDb;
GO

/* -------------------------------------------------------------------------
   Drop existing objects (safe re-run during development)
   ------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Recommendations', 'U') IS NOT NULL DROP TABLE dbo.Recommendations;
IF OBJECT_ID('dbo.Stats', 'U') IS NOT NULL DROP TABLE dbo.Stats;
IF OBJECT_ID('dbo.Players', 'U') IS NOT NULL DROP TABLE dbo.Players;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

/* -------------------------------------------------------------------------
   Users
   Handles authentication and role-based access (Player / Administrator)
   ------------------------------------------------------------------------- */
CREATE TABLE dbo.Users
(
    UserId          INT IDENTITY(1,1)      NOT NULL,
    Name            NVARCHAR(100)          NOT NULL,
    Email           NVARCHAR(256)          NOT NULL,
    PasswordHash    NVARCHAR(512)          NOT NULL,   -- store a hash, never plain text
    Role            NVARCHAR(20)           NOT NULL
        CONSTRAINT DF_Users_Role DEFAULT ('Player'),
    CreatedDate     DATETIME2              NOT NULL
        CONSTRAINT DF_Users_CreatedDate DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Player', 'Administrator'))
);
GO

/* -------------------------------------------------------------------------
   Players
   One-to-one with Users (a user who has linked a Fortnite account)
   ------------------------------------------------------------------------- */
CREATE TABLE dbo.Players
(
    PlayerId            INT IDENTITY(1,1)      NOT NULL,
    UserId               INT                    NOT NULL,
    FortniteUsername     NVARCHAR(100)          NOT NULL,   -- Epic username, links to FortniteAPI.io
    Game                 NVARCHAR(50)           NOT NULL
        CONSTRAINT DF_Players_Game DEFAULT ('Fortnite'),
    Team                 NVARCHAR(100)          NULL,
    CreatedDate          DATETIME2              NOT NULL
        CONSTRAINT DF_Players_CreatedDate DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Players PRIMARY KEY CLUSTERED (PlayerId),
    CONSTRAINT UQ_Players_UserId UNIQUE (UserId),               -- enforces 1:1 with Users
    CONSTRAINT UQ_Players_FortniteUsername UNIQUE (FortniteUsername),
    CONSTRAINT FK_Players_Users FOREIGN KEY (UserId)
        REFERENCES dbo.Users (UserId)
        ON DELETE CASCADE
);
GO

/* -------------------------------------------------------------------------
   Stats
   One Stats record per Player, overwritten/updated on each sync
   ------------------------------------------------------------------------- */
CREATE TABLE dbo.Stats
(
    StatId          INT IDENTITY(1,1)      NOT NULL,
    PlayerId        INT                    NOT NULL,
    Eliminations    INT                    NOT NULL
        CONSTRAINT DF_Stats_Eliminations DEFAULT (0),
    Wins            INT                    NOT NULL
        CONSTRAINT DF_Stats_Wins DEFAULT (0),
    Accuracy        DECIMAL(5,2)           NOT NULL
        CONSTRAINT DF_Stats_Accuracy DEFAULT (0.00),   -- percentage, e.g. 23.45
    KDRatio         DECIMAL(6,2)           NOT NULL
        CONSTRAINT DF_Stats_KDRatio DEFAULT (0.00),
    MatchesPlayed   INT                    NOT NULL
        CONSTRAINT DF_Stats_MatchesPlayed DEFAULT (0),
    WinRate         AS (CASE WHEN MatchesPlayed > 0
                             THEN CAST(Wins AS DECIMAL(6,2)) / MatchesPlayed * 100
                             ELSE 0 END) PERSISTED,      -- computed column, auto win-rate
    LastUpdated     DATETIME2              NOT NULL
        CONSTRAINT DF_Stats_LastUpdated DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Stats PRIMARY KEY CLUSTERED (StatId),
    CONSTRAINT UQ_Stats_PlayerId UNIQUE (PlayerId),             -- enforces 1:1 with Players
    CONSTRAINT FK_Stats_Players FOREIGN KEY (PlayerId)
        REFERENCES dbo.Players (PlayerId)
        ON DELETE CASCADE,
    CONSTRAINT CK_Stats_NonNegative CHECK (
        Eliminations >= 0 AND Wins >= 0 AND MatchesPlayed >= 0
        AND Accuracy >= 0 AND KDRatio >= 0
    )
);
GO

/* -------------------------------------------------------------------------
   Recommendations
   Many-to-one with Players (rule-based coaching output, history kept)
   ------------------------------------------------------------------------- */
CREATE TABLE dbo.Recommendations
(
    RecommendationId    INT IDENTITY(1,1)      NOT NULL,
    PlayerId            INT                    NOT NULL,
    RecommendationText  NVARCHAR(1000)         NOT NULL,
    CreatedDate         DATETIME2              NOT NULL
        CONSTRAINT DF_Recommendations_CreatedDate DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Recommendations PRIMARY KEY CLUSTERED (RecommendationId),
    CONSTRAINT FK_Recommendations_Players FOREIGN KEY (PlayerId)
        REFERENCES dbo.Players (PlayerId)
        ON DELETE CASCADE
);
GO

/* -------------------------------------------------------------------------
   Helpful indexes for common dashboard queries
   ------------------------------------------------------------------------- */
CREATE NONCLUSTERED INDEX IX_Recommendations_PlayerId_CreatedDate
    ON dbo.Recommendations (PlayerId, CreatedDate DESC);

CREATE NONCLUSTERED INDEX IX_Players_UserId
    ON dbo.Players (UserId);
GO

/* -------------------------------------------------------------------------
   Optional: seed data for local development / demo
   ------------------------------------------------------------------------- */
INSERT INTO dbo.Users (Name, Email, PasswordHash, Role)
VALUES
    ('Admin User', 'admin@example.com', 'CHANGE_ME_HASH', 'Administrator'),
    ('Jane Player', 'jane@example.com', 'CHANGE_ME_HASH', 'Player');

INSERT INTO dbo.Players (UserId, FortniteUsername, Game, Team)
VALUES
    (2, 'JanePlaysFN', 'Fortnite', NULL);

INSERT INTO dbo.Stats (PlayerId, Eliminations, Wins, Accuracy, KDRatio, MatchesPlayed)
VALUES
    (1, 250, 12, 21.50, 1.85, 140);

INSERT INTO dbo.Recommendations (PlayerId, RecommendationText)
VALUES
    (1, 'Your accuracy is below 25%. Try spending 15 minutes in Creative aim-training maps before ranked matches.');
GO
