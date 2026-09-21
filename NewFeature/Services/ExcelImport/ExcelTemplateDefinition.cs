using System.Collections.Generic;

namespace NewFeature.Services.ExcelImport
{
    // One column the approved template expects. HeaderAliases are matched case-insensitively and
    // by substring against the workbook's actual header row (English + Arabic variants) - the
    // same keyword-matching approach every existing bulk-upload service in this project already
    // uses (the FindColumn helpers in FleetService/MaintenanceService/WarehouseService),
    // centralized here instead of being copy-pasted per service.
    public class ExcelColumnDefinition
    {
        public string Key { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string[] HeaderAliases { get; init; } = System.Array.Empty<string>();
        public bool Required { get; init; }
    }

    // The full approved structure for one sub-department's upload template. IdentityColumnKey
    // names the one column whose presence in the header row is what proves "this is the right
    // sheet" - if it can't be found, the file is rejected up front with a clear message instead
    // of silently reading garbage from the wrong template (mirrors the existing plate-column /
    // bus-column guard clauses already used in this codebase).
    public class ExcelTemplateDefinition
    {
        public string TemplateName { get; init; } = string.Empty;
        public string IdentityColumnKey { get; init; } = string.Empty;
        public List<ExcelColumnDefinition> Columns { get; init; } = new();

        // Which worksheet inside the workbook this definition describes. Empty means "the first
        // sheet", which is what every single-sheet department template uses. Finance is the one
        // template that legitimately carries two data sheets in one file (the chart of accounts
        // and the monthly balances), so each of its two definitions names its own sheet here and
        // the same engine runs twice over the same workbook.
        public string[] SheetNameAliases { get; init; } = System.Array.Empty<string>();

        // The sheet's Arabic name as the downloadable template writes it - used only to build the
        // "this workbook has no <sheet> sheet" message, so the uploader is told exactly which tab
        // is missing rather than just "wrong template".
        public string SheetDisplayName { get; init; } = string.Empty;

        public ExcelColumnDefinition? GetColumn(string key) =>
            Columns.Find(c => c.Key == key);
    }
}
