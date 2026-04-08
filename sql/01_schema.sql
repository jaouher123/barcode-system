-- =============================================
-- Barcode Comparison System - SQL Server Schema
-- =============================================

CREATE DATABASE BarcodeDB;
GO

USE BarcodeDB;
GO

-- =============================================
-- TABLE: Users
-- =============================================
CREATE TABLE Users (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Username    NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role        NVARCHAR(50) NOT NULL CHECK (Role IN ('Operator', 'Supervisor', 'Administrator')),
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- =============================================
-- TABLE: Devices (WiFi scanners)
-- =============================================
CREATE TABLE Devices (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Location    NVARCHAR(100),
    IpAddress   NVARCHAR(45),
    IsOnline    BIT NOT NULL DEFAULT 0,
    LastSeenAt  DATETIME2,
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- =============================================
-- TABLE: Scans
-- =============================================
CREATE TABLE Scans (
    Id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    Barcode1    NVARCHAR(255) NOT NULL,
    Barcode2    NVARCHAR(255) NOT NULL,
    Result      NVARCHAR(10) NOT NULL CHECK (Result IN ('OK', 'NOK')),
    DeviceId    INT NULL REFERENCES Devices(Id),
    UserId      INT NULL REFERENCES Users(Id),
    ScannedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Indexes for fast history queries
CREATE INDEX IX_Scans_ScannedAt   ON Scans(ScannedAt DESC);
CREATE INDEX IX_Scans_Result      ON Scans(Result);
CREATE INDEX IX_Scans_DeviceId    ON Scans(DeviceId);
CREATE INDEX IX_Scans_Barcode1    ON Scans(Barcode1);

GO

-- =============================================
-- VIEW: Daily Statistics
-- =============================================
CREATE VIEW vw_DailyStats AS
SELECT
    CAST(ScannedAt AS DATE)                         AS Day,
    COUNT(*)                                        AS Total,
    SUM(CASE WHEN Result = 'OK'  THEN 1 ELSE 0 END) AS TotalOK,
    SUM(CASE WHEN Result = 'NOK' THEN 1 ELSE 0 END) AS TotalNOK,
    CAST(
        100.0 * SUM(CASE WHEN Result = 'NOK' THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0)
    AS DECIMAL(5,2))                                AS ErrorRate
FROM Scans
GROUP BY CAST(ScannedAt AS DATE);
GO

-- =============================================
-- VIEW: Hourly Scan Count (today)
-- =============================================
CREATE VIEW vw_HourlyScans AS
SELECT
    DATEPART(HOUR, ScannedAt)                        AS Hour,
    COUNT(*)                                         AS Total,
    SUM(CASE WHEN Result = 'OK'  THEN 1 ELSE 0 END)  AS TotalOK,
    SUM(CASE WHEN Result = 'NOK' THEN 1 ELSE 0 END)  AS TotalNOK
FROM Scans
WHERE CAST(ScannedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
GROUP BY DATEPART(HOUR, ScannedAt);
GO

-- =============================================
-- SEED DATA
-- =============================================
INSERT INTO Users (Username, PasswordHash, Role) VALUES
('admin',      'AQAAAAIAAYagAAAAE...', 'Administrator'),
('superviseur','AQAAAAIAAYagAAAAF...', 'Supervisor'),
('operateur1', 'AQAAAAIAAYagAAAAG...', 'Operator');

INSERT INTO Devices (Name, Location, IpAddress, IsOnline) VALUES
('Scanner A', 'Hall 1 - Ligne 1', '192.168.1.101', 1),
('Scanner B', 'Hall 1 - Ligne 2', '192.168.1.102', 1),
('Scanner C', 'Hall 2 - Ligne 1', '192.168.1.103', 0),
('Scanner D', 'Hall 2 - Ligne 2', '192.168.1.104', 1);
GO
