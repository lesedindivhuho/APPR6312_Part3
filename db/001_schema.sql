-- Lightweight, idempotent schema for Azure SQL (safe to run on every deployment).
-- Only creates what is missing, so it is quick and stays well within a Basic/B1-sized database.
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Fullname      NVARCHAR(100) NOT NULL,
    Username      NVARCHAR(50)  NOT NULL UNIQUE,
    Email         NVARCHAR(150) NOT NULL,
    Password      NVARCHAR(200) NOT NULL,
    Phone_Number  NVARCHAR(30)  NULL,
    role          NVARCHAR(20)  NOT NULL,
    CreatedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);

IF OBJECT_ID(N'dbo.Donations', N'U') IS NULL
CREATE TABLE dbo.Donations (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    DonationAmount    DECIMAL(18,2) NOT NULL,
    Currency          NCHAR(3)      NOT NULL,
    IsRecurring       BIT           NOT NULL DEFAULT 0,
    DonationDate      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    UserId            NVARCHAR(50)  NULL,
    DonorName         NVARCHAR(100) NULL,
    Email             NVARCHAR(150) NULL,
    CertificateNumber NVARCHAR(30)  NULL
);

IF OBJECT_ID(N'dbo.Clothes', N'U') IS NULL
CREATE TABLE dbo.Clothes (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Size        NVARCHAR(20)  NULL,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Condition   NVARCHAR(50)  NULL
);

IF OBJECT_ID(N'dbo.Food', N'U') IS NULL
CREATE TABLE dbo.Food (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Type NVARCHAR(50)  NULL
);

IF OBJECT_ID(N'dbo.ProjectUpdates', N'U') IS NULL
CREATE TABLE dbo.ProjectUpdates (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    Title      NVARCHAR(150)  NOT NULL,
    Message    NVARCHAR(2000) NOT NULL,
    DatePosted DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    PostedBy   NVARCHAR(100)  NULL
);

IF OBJECT_ID(N'dbo.Volunteers', N'U') IS NULL
CREATE TABLE dbo.Volunteers (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Fullname        NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(150) NOT NULL,
    Phone           NVARCHAR(30)  NULL,
    Skills          NVARCHAR(300) NULL,
    Availability    NVARCHAR(100) NULL,
    Status          NVARCHAR(20)  NOT NULL DEFAULT N'Pending',
    ApplicationDate DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);

-- Schema update example (a later change): add a column only if it is not there yet
IF COL_LENGTH(N'dbo.Donations', N'ReceiptSentAt') IS NULL
    ALTER TABLE dbo.Donations ADD ReceiptSentAt DATETIME2 NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Donations_DonationDate' AND object_id = OBJECT_ID(N'dbo.Donations'))
    CREATE INDEX IX_Donations_DonationDate ON dbo.Donations (DonationDate);

PRINT 'Schema is up to date.';
