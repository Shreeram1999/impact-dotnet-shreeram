-- Week 8 - Task 8.5's registration form collects a name and date of birth,
-- so the Users table grows two nullable columns. Applied as an ALTER (not by
-- editing 02_Schema.sql) because this database is "owned by another team"
-- (Task 7.9): their change script runs, then the EF DB First layer is
-- re-scaffolded to pick the columns up. The EF Code First database gets the
-- same columns from its AddUserProfile migration instead.

IF COL_LENGTH(N'dbo.Users', N'DisplayName') IS NULL
    ALTER TABLE dbo.Users ADD DisplayName NVARCHAR(100) NULL;
GO

IF COL_LENGTH(N'dbo.Users', N'DateOfBirth') IS NULL
    ALTER TABLE dbo.Users ADD DateOfBirth DATE NULL;
GO

UPDATE dbo.Users SET DisplayName = N'Meera Iyer' WHERE Username = N'teacher1' AND DisplayName IS NULL;
UPDATE dbo.Users SET DisplayName = N'Asha Kumar' WHERE Username = N'student1' AND DisplayName IS NULL;
GO
