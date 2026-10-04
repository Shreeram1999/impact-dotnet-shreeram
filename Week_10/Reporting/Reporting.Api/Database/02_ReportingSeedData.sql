-- What the data team's ETL has loaded so far: a finished spring term and
-- the in-progress monsoon term. Inserted only if the fact table is empty.

IF NOT EXISTS (SELECT 1 FROM dbo.Terms)
    INSERT INTO dbo.Terms (TermId, Name, StartsOn, EndsOn) VALUES
        (1, N'Spring 2026', '2026-01-05', '2026-05-15'),
        (2, N'Monsoon 2026', '2026-06-01', '2026-11-20');

IF NOT EXISTS (SELECT 1 FROM dbo.Departments)
    INSERT INTO dbo.Departments (DepartmentId, Name) VALUES
        (1, N'Mathematics'),
        (2, N'Physics'),
        (3, N'Computer Science');

IF NOT EXISTS (SELECT 1 FROM dbo.CourseCatalog)
    INSERT INTO dbo.CourseCatalog (CourseCode, Title, DepartmentId, Credits) VALUES
        (N'MATH101', N'Calculus I', 1, 4),
        (N'MATH201', N'Linear Algebra', 1, 3),
        (N'PHYS101', N'Mechanics', 2, 4),
        (N'CS101', N'Programming in C#', 3, 3),
        (N'CS201', N'Web APIs with ASP.NET Core', 3, 3);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.EnrollmentFacts)
BEGIN
    INSERT INTO dbo.EnrollmentFacts (TermId, CourseCode, StudentRollNumber, FinalScore, Completed) VALUES
        -- Spring 2026 (finished: every row has a final score)
        (1, N'MATH101', N'R001', 78, 1), (1, N'MATH101', N'R002', 35, 1), (1, N'MATH101', N'R003', 92, 1), (1, N'MATH101', N'R004', 61, 1),
        (1, N'PHYS101', N'R001', 70, 1), (1, N'PHYS101', N'R002', 55, 1), (1, N'PHYS101', N'R005', 38, 1),
        (1, N'CS101',   N'R003', 95, 1), (1, N'CS101',   N'R004', 81, 1), (1, N'CS101',   N'R005', 67, 1), (1, N'CS101', N'R006', 74, 1),
        -- Monsoon 2026 (in progress: some scores not in yet)
        (2, N'MATH201', N'R001', NULL, 0), (2, N'MATH201', N'R003', 88, 1), (2, N'MATH201', N'R004', NULL, 0),
        (2, N'PHYS101', N'R002', 64, 1), (2, N'PHYS101', N'R006', NULL, 0),
        (2, N'CS201',   N'R001', 82, 1), (2, N'CS201',   N'R003', 90, 1), (2, N'CS201',   N'R005', NULL, 0), (2, N'CS201', N'R006', 39, 1);
END
GO
