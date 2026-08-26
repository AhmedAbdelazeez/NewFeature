using System;
using System.Collections.Generic;

namespace NewFeature.Models
{
    public class SalesImportResultDto
    {
        public int BatchId { get; set; }
        public string FileType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public int RejectedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

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

    public class SalesCustomerYearSummaryDto
    {
        public int FiscalYear { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int RetainedCustomers { get; set; }
        public int ChurnedCustomers { get; set; }
        // Null when there is no prior-year roster to compare against (first year on file).
        public double? YoYGrowthPercent { get; set; }
        public Dictionary<string, int> SegmentDistribution { get; set; } = new();
    }

    public class SalesCustomersOverviewDto
    {
        public bool HasData { get; set; }
        public List<SalesCustomerYearSummaryDto> Years { get; set; } = new();
        public List<SalesTopSegmentDto> TopSegments { get; set; } = new();
    }

    public class SalesTopSegmentDto
    {
        public string CustomerGroup { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public double SharePercent { get; set; }
    }

    public class SalesFleetCapacityItemDto
    {
        public string? BusCode { get; set; }
        public string BusType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int? ModelYear { get; set; }
        public int NumberOfBuses { get; set; }
        public int SeatsPerBus { get; set; }
        public int TotalSeats { get; set; }
    }

    public class SalesFleetCapacitySummaryDto
    {
        public bool HasData { get; set; }
        public DateTime? SnapshotDate { get; set; }
        public int TotalBuses { get; set; }
        public int TotalSeats { get; set; }
        public double AverageSeatsPerBus { get; set; }
        public List<SalesFleetCapacityItemDto> Items { get; set; } = new();
        public Dictionary<string, int> BusesByCategory { get; set; } = new();
        public Dictionary<string, int> BusesByType { get; set; } = new();
    }

    public class SalesDailyOperationDto
    {
        public int Id { get; set; }
        public string? RentalOrderNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? ExecutionPoint { get; set; }
        public string? Direction { get; set; }
        public string? BusTypeCode { get; set; }
        public int OperationalCount { get; set; }
        public int ScheduledBuses { get; set; }
        public DateTime ExecutionDate { get; set; }
    }

    // Executive aggregate for the Sales dashboard (NewFeature page + the CEO dashboard in `project`).
    // Every *Actual field is computed from real uploaded data; every *Target field is an illustrative
    // business goal (matching the convention already used by CommercialKpisDto/WarehouseKpisDto in
    // this codebase) that the business can tune later - never a measured/fabricated value.
    public class SalesKpisDto
    {
        public bool HasCustomerData { get; set; }
        public bool HasFleetData { get; set; }
        public bool HasDailyOperationsData { get; set; }

        public int LatestFiscalYear { get; set; }

        public int TotalActiveCustomersActual { get; set; }
        public int TotalActiveCustomersTarget { get; set; } // = previous year's active customer count

        public int NewCustomersActual { get; set; }
        public int NewCustomersTarget { get; set; } = 50; // illustrative

        public int ChurnedCustomersActual { get; set; }

        public double CustomerRetentionRateActual { get; set; }
        public double CustomerRetentionRateTarget { get; set; } = 80.0; // illustrative

        // Null when there is no prior-year roster to compare against.
        public double? CustomerGrowthYoYPercent { get; set; }

        public string TopCustomerSegment { get; set; } = string.Empty;
        public double TopCustomerSegmentSharePercent { get; set; }

        public int TotalFleetBuses { get; set; }
        public int TotalFleetSeats { get; set; }
        public double AverageSeatsPerBus { get; set; }

        public int LatestDailyOperationsCount { get; set; }
        public DateTime? LatestDailyOperationsDate { get; set; }
    }
}
