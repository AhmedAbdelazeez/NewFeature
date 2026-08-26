using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    // Identifies which of the three Sales workbook shapes an uploaded file matches.
    // Detection is based on structural inspection (headers/sheet names, never the filename alone) -
    // see SalesService.DetectFileType.
    public enum SalesFileType
    {
        CustomerRoster,   // "العملاء" - yearly customer/turnover roster export from AX (one sheet per fiscal year)
        FleetCapacity,    // "عدد وانواع الحافلات" - bus fleet capacity by type/category/model
        DailyOperations   // "يومية التشغيل" - daily rental/trip order log
    }

    public enum SalesImportStatus
    {
        Success,
        PartialSuccess,
        Failed,
        DuplicateSkipped
    }

    // Audit/traceability record for every Sales Excel upload attempt - one row per upload call,
    // regardless of how many sheets/years the workbook contained internally. Mirrors the audit
    // fields already used by other departments' import flows (uploaded by/when, row counts, status).
    public class SalesImportBatch
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public SalesFileType FileType { get; set; }

        [Required, StringLength(64)]
        public string UploadedByUserId { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Comma-joined sheet name(s) actually parsed, e.g. "2024,2025,2026".
        [StringLength(300)]
        public string? SourceSheet { get; set; }

        // Comma-joined reporting period(s) covered, e.g. fiscal years "2024,2025,2026",
        // or a date range for daily-operations uploads.
        [StringLength(100)]
        public string? ReportingPeriod { get; set; }

        public int ImportedRowCount { get; set; }
        public int RejectedRowCount { get; set; }

        [Required]
        public SalesImportStatus Status { get; set; }

        // Truncated human-readable error summary (full per-row errors are not persisted to keep
        // this table small; the same list is returned to the caller in the upload response).
        [StringLength(4000)]
        public string? ErrorDetails { get; set; }

        // SHA-256 of the raw uploaded bytes (pre conversion). Used to reject re-uploading the exact
        // same file twice - see SalesService.BulkUpload*Async.
        [Required, StringLength(64)]
        public string FileHash { get; set; } = string.Empty;
    }
}
