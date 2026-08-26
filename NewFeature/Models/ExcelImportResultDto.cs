using System.Collections.Generic;

namespace NewFeature.Models
{
    // Row-level error reported back to the uploader: which physical Excel row failed, which
    // column (when known) caused it, and a plain-language reason. Technical exception details
    // are never placed here - they go to the server log instead (see ExcelImportEngine).
    public class ExcelRowErrorDto
    {
        public int RowNumber { get; set; }
        public string? Column { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // Standard shape every Fleet sub-department bulk-upload endpoint returns, so the frontend
    // (and any future consumer) only has to learn this response once. Mirrors the response shape
    // requested for the Fleet Management Excel import work.
    public class ExcelImportResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // Physical data rows found below the header row, before empty-row filtering.
        public int TotalRows { get; set; }
        // Rows that actually contained data - fully empty rows are never counted here.
        public int DataRows { get; set; }
        public int InsertedRows { get; set; }
        public int UpdatedRows { get; set; }
        public int SkippedRows { get; set; }

        public List<ExcelRowErrorDto> Errors { get; set; } = new();
    }
}
