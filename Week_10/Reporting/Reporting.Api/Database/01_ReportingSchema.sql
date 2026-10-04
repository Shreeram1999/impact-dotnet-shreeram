-- Task 10.4 - the PRE-EXISTING reporting database. In the story, this
-- schema belongs to the institution's data team: their nightly ETL fills it
-- from the student information system, and they own every change to it. The
-- Reporting service doesn't write migrations against it. It scaffolds C#
-- from it (EF Core Database First) and only reads.
--
-- (Created in SSMS from this script; idempotent so docker-compose's db-init
-- can run it on every start.)

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'dbo.Terms', N'U') IS NULL
    CREATE TABLE dbo.Terms
    (
        TermId   INT          NOT NULL CONSTRAINT PK_Terms PRIMARY KEY,
        Name     NVARCHAR(50) NOT NULL CONSTRAINT UQ_Terms_Name UNIQUE,
        StartsOn DATE         NOT NULL,
        EndsOn   DATE         NOT NULL
    );
GO

IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
    CREATE TABLE dbo.Departments
    (
        DepartmentId INT           NOT NULL CONSTRAINT PK_Departments PRIMARY KEY,
        Name         NVARCHAR(100) NOT NULL CONSTRAINT UQ_Departments_Name UNIQUE
    );
GO

IF OBJECT_ID(N'dbo.CourseCatalog', N'U') IS NULL
    CREATE TABLE dbo.CourseCatalog
    (
        CourseCode   NVARCHAR(20)  NOT NULL CONSTRAINT PK_CourseCatalog PRIMARY KEY,
        Title        NVARCHAR(150) NOT NULL,
        DepartmentId INT           NOT NULL CONSTRAINT FK_CourseCatalog_Departments REFERENCES dbo.Departments (DepartmentId),
        Credits      INT           NOT NULL
    );
GO

-- One row per (term, course, student): the fact table.
IF OBJECT_ID(N'dbo.EnrollmentFacts', N'U') IS NULL
    CREATE TABLE dbo.EnrollmentFacts
    (
        EnrollmentFactId  INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_EnrollmentFacts PRIMARY KEY,
        TermId            INT          NOT NULL CONSTRAINT FK_EnrollmentFacts_Terms REFERENCES dbo.Terms (TermId),
        CourseCode        NVARCHAR(20) NOT NULL CONSTRAINT FK_EnrollmentFacts_CourseCatalog REFERENCES dbo.CourseCatalog (CourseCode),
        StudentRollNumber NVARCHAR(20) NOT NULL,
        FinalScore        INT          NULL,   -- NULL while the course is in progress
        Completed         BIT          NOT NULL,
        CONSTRAINT UQ_EnrollmentFacts_Term_Course_Student UNIQUE (TermId, CourseCode, StudentRollNumber)
    );
GO
