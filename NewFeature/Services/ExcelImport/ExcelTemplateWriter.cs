using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace NewFeature.Services.ExcelImport
{
    // One data sheet to write into a downloadable template: the column contract it must follow,
    // plus any rows to pre-fill (the Finance template ships the live chart of accounts inside it,
    // so a coordinator downloads the tree they are expected to report against rather than an empty
    // grid) and any per-column dropdowns.
    public class ExcelTemplateSheetSpec
    {
        public ExcelTemplateDefinition Definition { get; init; } = new();

        // Sheet tab name. Defaults to the definition's own SheetDisplayName.
        public string? SheetName { get; init; }

        // Rows to pre-fill, keyed by the definition's column Key. Empty for a blank template.
        public IReadOnlyList<IReadOnlyDictionary<string, string?>> Rows { get; init; } =
            Array.Empty<IReadOnlyDictionary<string, string?>>();

        // Allowed values for a column, rendered as an Excel dropdown - the same in-cell validation
        // the department's own sheet already carries, so a wrong status can't be typed in the first
        // place rather than only being rejected after upload.
        public IReadOnlyDictionary<string, string[]> Dropdowns { get; init; } =
            new Dictionary<string, string[]>();
    }

    // Builds the .xlsx a department downloads, straight from the ExcelTemplateDefinition the
    // importer validates against. Generating both from one source is the point: a template that
    // drifts from its own validation rules is what produces "I used your file and it was rejected".
    public static class ExcelTemplateWriter
    {
        private const string HeaderFill = "#1F3864";
        private const string RequiredHeaderFill = "#8B1E3F";

        public static byte[] Build(string templateTitle, IEnumerable<ExcelTemplateSheetSpec> sheets, IEnumerable<string>? instructionLines = null)
        {
            using var workbook = new XLWorkbook();

            foreach (var spec in sheets)
            {
                var name = SanitizeSheetName(spec.SheetName
                                             ?? (string.IsNullOrWhiteSpace(spec.Definition.SheetDisplayName)
                                                 ? spec.Definition.TemplateName
                                                 : spec.Definition.SheetDisplayName));
                var sheet = workbook.Worksheets.Add(name);
                WriteSheet(sheet, spec);
            }

            WriteInstructions(workbook, templateTitle, sheets, instructionLines);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void WriteSheet(IXLWorksheet sheet, ExcelTemplateSheetSpec spec)
        {
            var columns = spec.Definition.Columns;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var cell = sheet.Cell(1, i + 1);

                // The header text is the first alias, because that is the exact spelling the
                // importer matches on. The human-readable DisplayName goes into the cell note, so
                // the sheet stays machine-readable while still explaining itself.
                cell.Value = column.HeaderAliases.FirstOrDefault() ?? column.DisplayName;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(column.Required ? RequiredHeaderFill : HeaderFill);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.WrapText = true;

                cell.CreateComment().AddText(column.Required
                    ? $"{column.DisplayName} — إلزامي"
                    : $"{column.DisplayName} — اختياري");

                if (spec.Dropdowns.TryGetValue(column.Key, out var allowed) && allowed.Length > 0)
                {
                    var validation = sheet.Range(2, i + 1, 1000, i + 1).CreateDataValidation();
                    validation.List($"\"{string.Join(",", allowed)}\"", true);
                    validation.ErrorStyle = XLErrorStyle.Stop;
                    validation.ErrorTitle = "قيمة غير مقبولة";
                    validation.ErrorMessage = $"القيم المسموح بها: {string.Join(" / ", allowed)}";
                }
            }

            int row = 2;
            foreach (var data in spec.Rows)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    if (data.TryGetValue(columns[i].Key, out var value) && !string.IsNullOrEmpty(value))
                    {
                        // Written as text on purpose: account numbers and bus numbers are
                        // identifiers, and Excel silently turns "00049024" into 49024 otherwise.
                        sheet.Cell(row, i + 1).SetValue(value);
                    }
                }
                row++;
            }

            sheet.SheetView.FreezeRows(1);
            sheet.RangeUsed()?.SetAutoFilter();
            sheet.Columns().AdjustToContents(1, 60, 12, 45);
            sheet.RightToLeft = true;
        }

        private static void WriteInstructions(
            IXLWorkbook workbook,
            string templateTitle,
            IEnumerable<ExcelTemplateSheetSpec> sheets,
            IEnumerable<string>? instructionLines)
        {
            var sheet = workbook.Worksheets.Add("تعليمات");
            sheet.RightToLeft = true;

            int row = 1;
            sheet.Cell(row, 1).Value = templateTitle;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            sheet.Cell(row, 1).Style.Font.FontSize = 14;
            row += 2;

            sheet.Cell(row++, 1).Value =
                "احتفظ بصف العناوين وبأسماء الأوراق كما هي، واحذف صفوف المثال قبل الرفع. ترتيب الأعمدة غير مهم، لكن أسماء العناوين مهمة.";
            sheet.Cell(row++, 1).Value =
                "الأعمدة الإلزامية مظللة باللون الأحمر في صف العناوين، والصف الذي ينقصه أي منها يُرفض مع بيان سبب الرفض.";
            row++;

            foreach (var spec in sheets)
            {
                var sheetName = spec.SheetName
                                ?? (string.IsNullOrWhiteSpace(spec.Definition.SheetDisplayName)
                                    ? spec.Definition.TemplateName
                                    : spec.Definition.SheetDisplayName);

                sheet.Cell(row, 1).Value = $"ورقة: {sheetName}";
                sheet.Cell(row, 1).Style.Font.Bold = true;
                row++;

                sheet.Cell(row, 1).Value = "العمود";
                sheet.Cell(row, 2).Value = "الوصف";
                sheet.Cell(row, 3).Value = "إلزامي؟";
                sheet.Range(row, 1, row, 3).Style.Font.Bold = true;
                row++;

                foreach (var column in spec.Definition.Columns)
                {
                    sheet.Cell(row, 1).Value = column.HeaderAliases.FirstOrDefault() ?? column.DisplayName;
                    sheet.Cell(row, 2).Value = column.DisplayName;
                    sheet.Cell(row, 3).Value = column.Required ? "نعم" : "لا";
                    row++;
                }
                row++;
            }

            if (instructionLines != null)
            {
                sheet.Cell(row, 1).Value = "المؤشرات المحسوبة من هذا النموذج";
                sheet.Cell(row, 1).Style.Font.Bold = true;
                row++;
                foreach (var line in instructionLines)
                {
                    sheet.Cell(row++, 1).Value = line;
                }
            }

            sheet.Columns().AdjustToContents(1, 3, 15, 70);
        }

        private static string SanitizeSheetName(string name)
        {
            foreach (var c in new[] { '\\', '/', '*', '[', ']', ':', '?' }) name = name.Replace(c, '-');
            if (name.Length > 31) name = name.Substring(0, 31);
            return string.IsNullOrWhiteSpace(name) ? "Sheet1" : name;
        }
    }
}
