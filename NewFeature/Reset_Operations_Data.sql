-- ============================================================================
-- Reset Operations upload data before re-importing an Excel file
-- ============================================================================
-- Scope: this clears ONLY the two Operations tables that ACCUMULATE on every
-- upload instead of replacing themselves (confirmed by reading OperationsService.cs):
--
--   - Trips               (from "bulk-upload-trips" - the monthly dispatch sheet)
--   - OperationsDailyPlans (from "bulk-upload-daily-plans")
--
-- Deliberately NOT touched, and why:
--   - Vehicles, Routes    -> shared master data also used by Fleet/Maintenance/
--                            Warehouse. Trips references them by VehicleId/RouteId,
--                            but the vehicles/routes themselves are not upload
--                            artifacts - deleting them could break other departments.
--   - AspNetUsers         -> Trip.DriverId points into the SAME login-users table
--                            used for real system accounts (admin, hr@, ops@, ...).
--                            Never bulk-delete this table.
--   - OfficialDrivers,
--     RouteScheduleRequests -> already self-clear on every upload
--     (OperationsService.cs does `RemoveRange` before inserting), so re-uploading
--     those two replaces old data automatically - no manual reset needed.
--   - OperationsIncidents -> entered manually through the UI, not Excel-imported,
--                            so it's unrelated to "starting a fresh upload".
--
-- If you also want Vehicles/Routes/Drivers that were auto-created purely from a
-- bad trip import cleaned up, that needs a separate, carefully-scoped script
-- (find rows with zero other references) - ask and I'll write that one too.
--
-- HOW TO RUN THIS SAFELY:
--   1. Run ONLY the "Before" SELECT block below first and sanity-check the counts.
--   2. Run the DELETE block. It runs inside a transaction and does NOT auto-commit.
--   3. Check the "After" counts it prints.
--   4. If they look right: run `COMMIT TRAN;`
--      If anything looks wrong: run `ROLLBACK TRAN;` instead - nothing is changed.
-- ============================================================================

-- Step 1: BEFORE counts (safe, read-only - run this first)
SELECT
    (SELECT COUNT(*) FROM dbo.Trips)               AS Trips_Before,
    (SELECT COUNT(*) FROM dbo.OperationsDailyPlans) AS OperationsDailyPlans_Before;

-- Step 2: delete inside an explicit transaction (does not commit by itself)
BEGIN TRAN;

DELETE FROM dbo.Trips;
DELETE FROM dbo.OperationsDailyPlans;

SELECT
    (SELECT COUNT(*) FROM dbo.Trips)               AS Trips_After,
    (SELECT COUNT(*) FROM dbo.OperationsDailyPlans) AS OperationsDailyPlans_After;

-- Step 3: review the "After" counts above (should both be 0), then run exactly ONE of:
-- COMMIT TRAN;
-- ROLLBACK TRAN;
