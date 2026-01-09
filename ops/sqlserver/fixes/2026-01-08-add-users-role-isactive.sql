-- Fix for databases created with older migrations where Users table is missing Role/IsActive.
-- Safe to run multiple times.

IF COL_LENGTH('dbo.Users', 'Role') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [Role] NVARCHAR(16) NOT NULL
        CONSTRAINT [DF_Users_Role] DEFAULT (N'User');
END

IF COL_LENGTH('dbo.Users', 'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [IsActive] BIT NOT NULL
        CONSTRAINT [DF_Users_IsActive] DEFAULT (1);
END
