-- Task 7.4 - two stored procedures the ADO.NET repository calls with
-- CommandType.StoredProcedure. Parameters are typed, so a value can never
-- be interpreted as SQL - same protection as a parameterized command, with
-- the SQL itself living (and being permissioned) in the database.

CREATE OR ALTER PROCEDURE dbo.usp_GetStudentById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Name, Age, RollNumber, Email, Score, InternalNotes, EnrolledOn, UserId
    FROM dbo.Students
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsertStudent
    @Name          NVARCHAR(100),
    @Age           INT,
    @RollNumber    NVARCHAR(20),
    @Email         NVARCHAR(256),
    @Score         INT,
    @InternalNotes NVARCHAR(500),
    @EnrolledOn    DATE,
    @NewId         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Students (Name, Age, RollNumber, Email, Score, InternalNotes, EnrolledOn)
    VALUES (@Name, @Age, @RollNumber, @Email, @Score, @InternalNotes, @EnrolledOn);

    SET @NewId = CAST(SCOPE_IDENTITY() AS INT);
END
GO
