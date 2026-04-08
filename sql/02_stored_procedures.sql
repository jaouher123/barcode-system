USE BarcodeDB;
GO

-- =============================================
-- SP: Add a scan comparison result
-- =============================================
CREATE PROCEDURE sp_AddScan
    @Barcode1  NVARCHAR(255),
    @Barcode2  NVARCHAR(255),
    @DeviceId  INT = NULL,
    @UserId    INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result NVARCHAR(10) = CASE WHEN @Barcode1 = @Barcode2 THEN 'OK' ELSE 'NOK' END;

    INSERT INTO Scans (Barcode1, Barcode2, Result, DeviceId, UserId)
    VALUES (@Barcode1, @Barcode2, @Result, @DeviceId, @UserId);

    -- Return the inserted row
    SELECT TOP 1 * FROM Scans WHERE Id = SCOPE_IDENTITY();
END;
GO

-- =============================================
-- SP: Search scan history with filters
-- =============================================
CREATE PROCEDURE sp_GetHistory
    @SearchCode  NVARCHAR(255) = NULL,
    @ResultFilter NVARCHAR(10) = NULL,
    @DeviceId    INT = NULL,
    @DateFrom    DATETIME2 = NULL,
    @DateTo      DATETIME2 = NULL,
    @PageNumber  INT = 1,
    @PageSize    INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.Id, s.Barcode1, s.Barcode2, s.Result, s.ScannedAt,
        d.Name AS DeviceName, d.Location AS DeviceLocation,
        u.Username
    FROM Scans s
    LEFT JOIN Devices d ON s.DeviceId = d.Id
    LEFT JOIN Users u ON s.UserId = u.Id
    WHERE
        (@SearchCode IS NULL OR s.Barcode1 LIKE '%' + @SearchCode + '%' OR s.Barcode2 LIKE '%' + @SearchCode + '%')
        AND (@ResultFilter IS NULL OR s.Result = @ResultFilter)
        AND (@DeviceId IS NULL OR s.DeviceId = @DeviceId)
        AND (@DateFrom IS NULL OR s.ScannedAt >= @DateFrom)
        AND (@DateTo IS NULL OR s.ScannedAt <= @DateTo)
    ORDER BY s.ScannedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =============================================
-- SP: Get today's global counters
-- =============================================
CREATE PROCEDURE sp_GetTodayStats
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(*) AS Total,
        SUM(CASE WHEN Result='OK'  THEN 1 ELSE 0 END) AS TotalOK,
        SUM(CASE WHEN Result='NOK' THEN 1 ELSE 0 END) AS TotalNOK,
        CAST(100.0 * SUM(CASE WHEN Result='NOK' THEN 1 ELSE 0 END) / NULLIF(COUNT(*),0) AS DECIMAL(5,2)) AS ErrorRate
    FROM Scans
    WHERE CAST(ScannedAt AS DATE) = CAST(GETUTCDATE() AS DATE);
END;
GO
