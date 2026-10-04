-- Task 7.1 - the hand-built database that the ADO.NET layer (Days 1-2) talks
-- to and that the EF DB First layer (Day 4) scaffolds from. EF Code First
-- (Day 3) generates its OWN database (StudentPortal_Week7_EfCodeFirst) from
-- migrations instead - see Data/EfCodeFirst.
--
-- Run the whole setup from the Week_7 folder:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/01_CreateDatabase.sql
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -d StudentPortal_Week7 -i Database/02_Schema.sql -i Database/03_StoredProcedures.sql -i Database/04_SeedData.sql

IF DB_ID(N'StudentPortal_Week7') IS NULL
    CREATE DATABASE StudentPortal_Week7;
GO
