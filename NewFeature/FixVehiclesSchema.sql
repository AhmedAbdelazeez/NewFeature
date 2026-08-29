/*
  FixVehiclesSchema.sql
  ----------------------
  Run this once against the Transpottions database (SSMS, or sqlcmd) BEFORE starting
  NewFeature again. It brings the actual Vehicles table up to date by hand, independent of
  `dotnet ef` / `Database.Migrate()`, so it works even if that auto-migration step failed or
  was skipped. Every step is guarded with an existence check, so it's safe to run more than
  once.

  Why this is needed: the Vehicles table was missing 8 new columns (BusNumber, ChassisNumber,
  BusTypeCode, HasAirConditioning, MaxKilometers, IsInStorage, ResponsibilityCode,
  ResponsibleEmployeeName) plus the Capacity column needed to become nullable and possibly
  Mileage was never added either - the app's code (EF's runtime model) expects all of these,
  so every query against Vehicles was throwing "Invalid column name ..." and every Fleet KPI
  card that depends on vehicle data came back empty.

  How to run:
    1. Open SQL Server Management Studio (or Azure Data Studio / sqlcmd).
    2. Connect to Zone-CAI-0118\SQLEXPRESS08 (matches appsettings.json's DefaultConnection).
    3. Open this file, make sure the connection's default database is Transpottions
       (or the USE statement below will select it), then Execute (F5).
    4. Check the messages pane - it should print "Vehicles schema is now up to date." at the
       end with no red errors above it.
    5. Start NewFeature normally (dotnet run, or F5 in Visual Studio). It will skip
       migrations (already applied per __EFMigrationsHistory) and go straight to seeding the
       702 real vehicles into the now-correct table.
    6. Refresh the dashboard (project, localhost:7290) - the Fleet KPI cards should populate.
*/

USE [Transpottions];
GO

IF COL_LENGTH('dbo.Vehicles', 'Mileage') IS NULL
BEGIN
    PRINT 'Adding Vehicles.Mileage ...';
    ALTER TABLE [dbo].[Vehicles] ADD [Mileage] decimal(18,2) NULL;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Vehicles') AND name = 'Capacity' AND is_nullable = 0
)
BEGIN
    PRINT 'Making Vehicles.Capacity nullable ...';
    ALTER TABLE [dbo].[Vehicles] ALTER COLUMN [Capacity] decimal(18,2) NULL;
END
GO

IF COL_LENGTH('dbo.Vehicles', 'BusNumber') IS NULL
BEGIN
    PRINT 'Adding Vehicles.BusNumber ...';
    ALTER TABLE [dbo].[Vehicles] ADD [BusNumber] nvarchar(30) NULL;
END
GO

IF COL_LENGTH('dbo.Vehicles', 'BusTypeCode') IS NULL
BEGIN
    PRINT 'Adding Vehicles.BusTypeCode ...';
    ALTER TABLE [dbo].[Vehicles] ADD [BusTypeCode] nvarchar(30) NULL;
END
GO

IF COL_LENGTH('dbo.Vehicles', 'ChassisNumber') IS NULL
BEGIN
    PRINT 'Adding Vehicles.ChassisNumber ...';
    ALTER TABLE [dbo].[Vehicles] ADD [ChassisNumber] nvarchar(30) NULL;
END
GO

IF COL_LENGTH('dbo.Vehicles', 'HasAirConditioning') IS NULL
BEGIN
    PRINT 'Adding Vehicles.HasAirConditioning ...';
    ALTER TABLE [dbo].[Vehicles] ADD [HasAirConditioning] bit NOT NULL DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Vehicles', 'IsInStorage') IS NULL
BEGIN
    PRINT 'Adding Vehicles.IsInStorage ...';
    ALTER TABLE [dbo].[Vehicles] ADD [IsInStorage] bit NOT NULL DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Vehicles', 'MaxKilometers') IS NULL
BEGIN
    PRINT 'Adding Vehicles.MaxKilometers ...';
    ALTER TABLE [dbo].[Vehicles] ADD [MaxKilometers] decimal(18,2) NULL;
END
GO

IF COL_LENGTH('dbo.Vehicles', 'ResponsibilityCode') IS NULL
BEGIN
    PRINT 'Adding Vehicles.ResponsibilityCode ...';
    ALTER TABLE [dbo].[Vehicles] ADD [ResponsibilityCode] nvarchar(30) NULL;
END
GO

IF COL_LENGTH('dbo.Vehicles', 'ResponsibleEmployeeName') IS NULL
BEGIN
    PRINT 'Adding Vehicles.ResponsibleEmployeeName ...';
    ALTER TABLE [dbo].[Vehicles] ADD [ResponsibleEmployeeName] nvarchar(150) NULL;
END
GO

-- Tell EF these two migrations are already applied, so Database.Migrate() on next startup
-- doesn't try to run them again (which would fail with "column already exists").
IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260824120000_AddVehicleMileage')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260824120000_AddVehicleMileage', N'10.0.9');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260824130000_ExtendVehicleFleetData')
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260824130000_ExtendVehicleFleetData', N'10.0.9');
GO

PRINT 'Vehicles schema is now up to date.';
GO
