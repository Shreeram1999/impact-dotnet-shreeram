-- The ONLY two ways the Identity service touches dbo.Users. In production
-- the service's SQL login would be granted EXECUTE on these procedures and
-- nothing else - no direct SELECT/INSERT on the table at all.

CREATE OR ALTER PROCEDURE dbo.usp_GetUserByUsername
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Username, PasswordHash, Role, DisplayName, DateOfBirth
    FROM dbo.Users
    WHERE Username = @Username;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsertUser
    @Username     NVARCHAR(100),
    @PasswordHash NVARCHAR(200),
    @Role         NVARCHAR(20),
    @DisplayName  NVARCHAR(100),
    @DateOfBirth  DATE,
    @NewId        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users (Username, PasswordHash, Role, DisplayName, DateOfBirth)
    VALUES (@Username, @PasswordHash, @Role, @DisplayName, @DateOfBirth);

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);
END
GO
