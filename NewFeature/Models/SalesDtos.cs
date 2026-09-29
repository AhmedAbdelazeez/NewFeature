using System;
using System.Collections.Generic;

namespace NewFeature.Models
{
    public class SalesImportBatchDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string UploadedByUserId { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string? SourceSheet { get; set; }
        public string? ReportingPeriod { get; set; }
        public int ImportedRowCount { get; set; }
        public int RejectedRowCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    // One customer on one fiscal year's roster - a row of the العملاء page and of the upload.
    // Numbers and dates are nullable on every Sales DTO so a blank form field reaches the shared
    // validation (and gets its "مطلوب" message) instead of failing JSON binding.
    public class SalesCustomerDto
    {
        public int Id { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerGroup { get; set; }
        public string? Currency { get; set; }
        public int? FiscalYear { get; set; }
    }

    public class SalesFleetCapacityDto
    {
        public int Id { get; set; }
        public string? BusCode { get; set; }
        public string? BusType { get; set; }
        public string? Category { get; set; }
        public int? ModelYear { get; set; }
        public int? NumberOfBuses { get; set; }
        public int? SeatsPerBus { get; set; }
        // Left blank, it is computed as NumberOfBuses x SeatsPerBus.
        public int? TotalSeats { get; set; }
    }

    public class SalesDailyOperationDto
    {
        public int Id { get; set; }
        public string? RentalOrderNumber { get; set; }
        public string? ConfirmationNumber { get; set; }
        public string? RequestType { get; set; }
        public string? CustomerName { get; set; }
        public string? ExecutionPoint { get; set; }
        public string? Direction { get; set; }
        public string? BusTypeCode { get; set; }
        public int? OperationalCount { get; set; }
        public int? ScheduledBuses { get; set; }
        public DateTime? ExecutionDate { get; set; }
        // "HH:mm", the way the page's form and the sheet both write it.
        public string? ExecutionTime { get; set; }
        public string? Notes { get; set; }
    }

    // Dropdown contents for each page's index, taken from what the uploaded data actually carries.
    public class SalesCustomerFilterOptionsDto
    {
        public List<int> Years { get; set; } = new();
        public List<string> Groups { get; set; } = new();
    }

    public class SalesFleetFilterOptionsDto
    {
        public List<string> Categories { get; set; } = new();
        public List<string> BusTypes { get; set; } = new();
    }

    public class SalesOperationFilterOptionsDto
    {
        public List<string> RequestTypes { get; set; } = new();
        public List<string> ExecutionPoints { get; set; } = new();
    }

    // The Sales KPIs the executive dashboard (project repo) shows: plain counts and two ratios,
    // one small group per template. Every figure is computed from uploaded rows; nothing here is
    // an assumed target.
    public class SalesKpisDto
    {
        public bool HasCustomerData { get; set; }
        public bool HasFleetData { get; set; }
        public bool HasDailyOperationsData { get; set; }

        // Customer roster - the latest fiscal year uploaded.
        public int LatestFiscalYear { get; set; }
        public int ActiveCustomers { get; set; }
        // Null when only one fiscal year is on file (nothing to compare against).
        public int? PreviousYearCustomers { get; set; }
        public int? NewCustomers { get; set; }
        public string TopCustomerGroup { get; set; } = string.Empty;
        public double TopCustomerGroupSharePercent { get; set; }

        // Fleet capacity - the current snapshot.
        public int TotalFleetBuses { get; set; }
        public int TotalFleetSeats { get; set; }

        // Daily operations - the latest execution date uploaded.
        public DateTime? LatestOperationsDate { get; set; }
        public int LatestOrdersCount { get; set; }
        public int LatestRequestedBuses { get; set; }
        public int LatestScheduledBuses { get; set; }
        // Scheduled / requested buses on that day.
        public double? SchedulingCoveragePercent { get; set; }
        // Scheduled buses on that day / buses in the fleet snapshot.
        public double? FleetUtilizationPercent { get; set; }
    }
}
