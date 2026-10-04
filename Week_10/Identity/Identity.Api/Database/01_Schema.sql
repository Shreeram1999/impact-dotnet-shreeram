-- Identity service schema (Task 10.2) - IdentityDb belongs to this service
-- alone; no other service connects to it. Promoted from Week 7's dbo.Users
-- plus Week 8's profile columns.
--
-- Idempotent on purpose (create-if-missing, never drop): docker-compose's
-- db-init container runs these scripts on every `docker compose up`, and
-- existing accounts must survive that.

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id           INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username     NVARCHAR(100)      NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        PasswordHash NVARCHAR(200)      NOT NULL,
        Role         NVARCHAR(20)       NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN (N'Teacher', N'Student')),
        DisplayName  NVARCHAR(100)      NULL,
        DateOfBirth  DATE               NULL,
        CreatedAtUtc DATETIME2(0)       NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );
END
GO
