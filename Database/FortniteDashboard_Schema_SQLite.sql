-- ===========================================================
-- FortniteDashboard schema — SQLite version
--
-- This mirrors what EF Core migrations will create automatically;
-- it exists as readable, reviewable evidence of the database design
-- (per the project's "Database Setup" requirement), not as something
-- you need to run by hand. Use the EF Core migration commands in
-- README.md / Docs/SQLite_Migration_Notes.md to actually create
-- fortnite_dashboard.db.
--
-- Changes from the old SQL Server version
-- (see FortniteDashboard_Schema_SqlServer_Legacy.sql):
--   - Stats is now ONE-TO-MANY with Players (a history table, one row
--     per sync) instead of a 1:1 row that got overwritten every time.
--     This is what makes historical trend charts possible.
--   - WinRate is a normal stored column (computed in C# at sync time)
--     instead of a SQL Server "PERSISTED computed column" — SQLite/EF
--     Core has no equivalent for that T-SQL feature.
--   - Added Deaths (derived as MatchesPlayed - Wins) so KDRatio can be
--     computed the way the brief asks: eliminations / deaths.
--   - LastUpdated renamed to RecordedAt to reflect that each row is an
--     immutable snapshot, not a row that gets updated in place.
-- ===========================================================

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS Recommendations;
DROP TABLE IF EXISTS Stats;
DROP TABLE IF EXISTS Players;
DROP TABLE IF EXISTS Users;

-- ---- Users ----
-- Handles authentication and role-based access (Player / Administrator)
CREATE TABLE Users
(
    UserId          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            TEXT    NOT NULL,
    Email           TEXT    NOT NULL UNIQUE,
    PasswordHash    TEXT    NOT NULL,             -- hashed via ASP.NET Core PasswordHasher, never plain text
    Role            TEXT    NOT NULL DEFAULT 'Player'
        CHECK (Role IN ('Player', 'Administrator')),
    CreatedDate     TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- ---- Players ----
-- One-to-one with Users (a user who has linked a Fortnite/Epic account)
CREATE TABLE Players
(
    PlayerId            INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId              INTEGER NOT NULL UNIQUE,   -- UNIQUE enforces 1:1 with Users
    FortniteUsername    TEXT    NOT NULL UNIQUE,   -- Epic username, used to query FortniteAPI.io
    Game                TEXT    NOT NULL DEFAULT 'Fortnite',
    Team                TEXT,
    CreatedDate         TEXT    NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (UserId) REFERENCES Users (UserId) ON DELETE CASCADE
);

-- ---- Stats ----
-- One row PER SYNC per player (history table, NOT 1:1 with Players).
CREATE TABLE Stats
(
    StatId          INTEGER PRIMARY KEY AUTOINCREMENT,
    PlayerId        INTEGER NOT NULL,
    Eliminations    INTEGER NOT NULL DEFAULT 0,
    Wins            INTEGER NOT NULL DEFAULT 0,
    MatchesPlayed   INTEGER NOT NULL DEFAULT 0,
    Deaths          INTEGER NOT NULL DEFAULT 0,   -- derived: MAX(MatchesPlayed - Wins, 0)
    KDRatio         NUMERIC(6,2) NOT NULL DEFAULT 0.00,  -- Eliminations / Deaths, guarded against /0
    WinRate         NUMERIC(6,2) NOT NULL DEFAULT 0.00,  -- Wins / MatchesPlayed * 100, guarded against /0
    Accuracy        NUMERIC(5,2) NOT NULL DEFAULT 0.00,  -- 0 = not available from FortniteAPI.io yet
    RecordedAt      TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (PlayerId) REFERENCES Players (PlayerId) ON DELETE CASCADE,
    CHECK (Eliminations >= 0 AND Wins >= 0 AND MatchesPlayed >= 0 AND Deaths >= 0 AND KDRatio >= 0 AND Accuracy >= 0)
);

-- ---- Recommendations ----
-- Many-to-one with Players (rule-based coaching output, full history kept)
CREATE TABLE Recommendations
(
    RecommendationId    INTEGER PRIMARY KEY AUTOINCREMENT,
    PlayerId            INTEGER NOT NULL,
    RecommendationText  TEXT    NOT NULL,
    CreatedDate         TEXT    NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (PlayerId) REFERENCES Players (PlayerId) ON DELETE CASCADE
);

-- ---- Helpful indexes for common dashboard queries ----
CREATE INDEX IX_Stats_PlayerId_RecordedAt
    ON Stats (PlayerId, RecordedAt);

CREATE INDEX IX_Recommendations_PlayerId_CreatedDate
    ON Recommendations (PlayerId, CreatedDate);
