using System.Collections.Generic;
using NewFeature.Services.ExcelImport;

namespace NewFeature.Pages.Shared
{
    // What a department's ERP landing page needs to render. The ERP side of a department is only
    // the data-entry point - download the approved template, upload it filled in - while every KPI
    // computed from that upload is shown on the executive dashboard, never here.
    public class DepartmentUploadLanding
    {
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string Icon { get; init; } = "bi-file-earmark-excel";
        public string TemplateUrl { get; init; } = string.Empty;
        public string UploadUrl { get; init; } = string.Empty;

        // The sheets inside the template. The column guide on the page is generated from these,
        // so it can never disagree with what the upload actually validates.
        public IReadOnlyList<ExcelTemplateDefinition> Sheets { get; init; } = new List<ExcelTemplateDefinition>();

        // Maintenance tags every uploaded work order with the workshop it came from.
        public bool AskForBranch { get; init; }
    }
}
