using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NewFeature.Models
{
    // One row per customer, per fiscal-year roster sheet imported from the AX "customer turnover"
    // export (العملاء.xlsx). IMPORTANT: despite the source workbook being titled "Customer turnover",
    // it only contains the customer master fields (AX order account/code, customer group/segment,
    // name, currency) - the actual turnover/amount column is not present anywhere in the file
    // (verified: zero numeric cells across all five year sheets). This table is therefore a yearly
    // customer roster used for customer-count and segment KPIs, not a revenue ledger. See the Sales
    // KPI report for which turnover KPIs this data can and cannot support.
    [Index(nameof(FiscalYear), nameof(CustomerCode))]
    public class SalesCustomerRecord
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string CustomerCode { get; set; } = string.Empty; // AX "Order account", e.g. C000001

        [Required, StringLength(300)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CustomerGroup { get; set; } // AX customer group/segment code, e.g. LCLAGNT, FRNHAJ

        [StringLength(10)]
        public string? Currency { get; set; }

        [Required]
        public int FiscalYear { get; set; }

        public int SalesImportBatchId { get; set; }
        public SalesImportBatch? ImportBatch { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
