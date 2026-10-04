-- Task 7.1 - Users / Teachers / Students in third normal form (see
-- ER-DIAGRAM.md):
--   * every non-key column depends on the key, the whole key, and nothing
--     but the key - a student's email lives only on Students, a login's
--     password hash only on Users;
--   * login credentials (Users) are split from profile data
--     (Students/Teachers), so a profile can exist without a login and the
--     security table stays small, stable and separately permissioned;
--   * a profile points at its login through a nullable, unique UserId.
-- Business rules that the database can guarantee on its own are declared
-- here as constraints (unique roll number/email/username, age 5-100, score
-- 0-100, role in Teacher/Student), so they hold for EVERY client, not just
-- this API.
--
-- Re-runnable: drops in dependency order first.

-- sqlcmd defaults QUOTED_IDENTIFIER to OFF, which filtered indexes (the
-- UserId ones below) refuse to be created under.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.Students', N'U') IS NOT NULL DROP TABLE dbo.Students;
IF OBJECT_ID(N'dbo.Teachers', N'U') IS NOT NULL DROP TABLE dbo.Teachers;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    Id           INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Username     NVARCHAR(100)      NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    PasswordHash NVARCHAR(200)      NOT NULL,
    Role         NVARCHAR(20)       NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN (N'Teacher', N'Student'))
);
GO

CREATE TABLE dbo.Teachers
(
    Id          INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Teachers PRIMARY KEY,
    Name        NVARCHAR(100)      NOT NULL,
    Email       NVARCHAR(256)      NOT NULL CONSTRAINT UQ_Teachers_Email UNIQUE,
    Designation NVARCHAR(100)      NOT NULL,
    UserId      INT                NULL CONSTRAINT FK_Teachers_Users REFERENCES dbo.Users (Id) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX UX_Teachers_UserId ON dbo.Teachers (UserId) WHERE UserId IS NOT NULL;
GO

CREATE TABLE dbo.Students
(
    Id            INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Students PRIMARY KEY,
    Name          NVARCHAR(100)      NOT NULL,
    Age           INT                NOT NULL CONSTRAINT CK_Students_Age CHECK (Age BETWEEN 5 AND 100),
    RollNumber    NVARCHAR(20)       NOT NULL CONSTRAINT UQ_Students_RollNumber UNIQUE,
    Email         NVARCHAR(256)      NOT NULL CONSTRAINT UQ_Students_Email UNIQUE,
    Score         INT                NOT NULL CONSTRAINT CK_Students_Score CHECK (Score BETWEEN 0 AND 100),
    InternalNotes NVARCHAR(500)      NOT NULL CONSTRAINT DF_Students_InternalNotes DEFAULT (N''),
    EnrolledOn    DATE               NULL,
    UserId        INT                NULL CONSTRAINT FK_Students_Users REFERENCES dbo.Users (Id) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX UX_Students_UserId ON dbo.Students (UserId) WHERE UserId IS NOT NULL;
GO
