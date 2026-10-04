-- Demo accounts (PBKDF2 hashes of Teacher@123 / Student@123 - the same
-- values used since Week 7). Inserted only if missing.

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'teacher1')
    INSERT INTO dbo.Users (Username, PasswordHash, Role, DisplayName)
    VALUES (N'teacher1', N'PBKDF2-SHA256$100000$J0Zb75kU8S4WKblBDAMrYQ==$7o3+1kfT10l09ygTNfciYKy0t9ZQzlTN49koV4hImbQ=', N'Teacher', N'Meera Iyer');

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'student1')
    INSERT INTO dbo.Users (Username, PasswordHash, Role, DisplayName)
    VALUES (N'student1', N'PBKDF2-SHA256$100000$IvdcCNnm9IZEJ6iNMbW1fQ==$A8dqeb89Q4z+7vZRGjpEH3Iy+X46px+29wuxFOx3SZI=', N'Student', N'Asha Kumar');
GO
