-- Seed data - the same rows EF Code First seeds through HasData
-- (Data/EfCodeFirst/AppDbContext.cs), so all three data layers start from an
-- identical dataset and the same Postman/.http run passes against each.
-- Password hashes are PBKDF2 (Week 6) of the demo passwords listed in
-- README.md - never the passwords themselves.

SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (Id, Username, PasswordHash, Role) VALUES
    (1, N'teacher1', N'PBKDF2-SHA256$100000$J0Zb75kU8S4WKblBDAMrYQ==$7o3+1kfT10l09ygTNfciYKy0t9ZQzlTN49koV4hImbQ=', N'Teacher'),
    (2, N'student1', N'PBKDF2-SHA256$100000$IvdcCNnm9IZEJ6iNMbW1fQ==$A8dqeb89Q4z+7vZRGjpEH3Iy+X46px+29wuxFOx3SZI=', N'Student');
SET IDENTITY_INSERT dbo.Users OFF;
GO

SET IDENTITY_INSERT dbo.Teachers ON;
INSERT INTO dbo.Teachers (Id, Name, Email, Designation, UserId) VALUES
    (1, N'Meera Iyer', N'meera.iyer@school.example', N'Mathematics', 1),
    (2, N'Arjun Rao', N'arjun.rao@school.example', N'Physics', NULL);
SET IDENTITY_INSERT dbo.Teachers OFF;
GO

SET IDENTITY_INSERT dbo.Students ON;
INSERT INTO dbo.Students (Id, Name, Age, RollNumber, Email, Score, InternalNotes, EnrolledOn, UserId) VALUES
    (1, N'Asha Kumar', 20, N'R001', N'asha.kumar@school.example', 82, N'', '2026-06-01', 2),
    (2, N'Rohit Menon', 21, N'R002', N'rohit.menon@school.example', 64, N'', '2026-06-01', NULL),
    (3, N'Divya Nair', 19, N'R003', N'divya.nair@school.example', 91, N'Scholarship review pending', '2026-06-15', NULL);
SET IDENTITY_INSERT dbo.Students OFF;
GO
