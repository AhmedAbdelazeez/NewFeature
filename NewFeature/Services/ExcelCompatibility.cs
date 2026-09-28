using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using ClosedXML.Excel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace NewFeature.Services
{
    // Some of the company's real Excel exports (Google Sheets exports, older workstations, etc.)
    // come through as legacy .xls (Excel 97-2003 / OLE2 binary format) instead of .xlsx. ClosedXML,
    // used everywhere else in this project's bulk-upload code, can only read .xlsx (OOXML/zip)
    // files. Rather than rewriting every bulk-upload method, this converts an incoming legacy .xls
    // stream into an equivalent in-memory .xlsx stream up front, so all the existing dynamic
    // column-mapping logic keeps working unchanged for both formats.
    public static class ExcelCompatibility
    {
        // OLE2/BIFF (legacy .xls) files always start with this exact 8-byte signature.
        private static readonly byte[] Ole2Signature = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

        public static bool IsSupportedExcelFile(string? fileName, string? contentType)
        {
            var ext = string.IsNullOrEmpty(fileName) ? null : Path.GetExtension(fileName)?.Trim().ToLowerInvariant();
            if (ext == ".xlsx" || ext == ".xls") return true;

            return string.Equals(contentType, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase)
                || string.Equals(contentType, "application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase);
        }

        // Returns a stream ClosedXML can open directly. If the input is a legacy .xls file it is
        // converted first; otherwise the original stream is returned untouched. The caller owns
        // disposal of whatever stream is returned (it may be a new MemoryStream).
        public static Stream EnsureXlsxStream(Stream input)
        {
            if (!input.CanSeek)
            {
                var buffered = new MemoryStream();
                input.CopyTo(buffered);
                buffered.Position = 0;
                input = buffered;
            }

            var header = new byte[8];
            var startPosition = input.Position;
            int bytesRead = input.Read(header, 0, header.Length);
            input.Position = startPosition;

            bool isLegacyXls = bytesRead == header.Length && HeaderMatches(header, Ole2Signature);
            if (!isLegacyXls)
            {
                return StripDataValidations(input);
            }

            return ConvertLegacyXlsToXlsx(input);
        }

        // ClosedXML refuses to open a workbook whose dropdown lists (data validations) are longer
        // than 255 characters - e.g. a technician list typed straight into the validation's Source
        // box - and the whole upload then fails as "not a valid Excel file". The imports only read
        // cell values, so dropdowns are removed from every worksheet before ClosedXML sees them.
        // Files without any dropdowns are returned untouched, so they cost only a quick scan.
        private static Stream StripDataValidations(Stream input)
        {
            var startPosition = input.Position;
            try
            {
                using (var source = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true))
                {
                    bool hasValidations = false;
                    foreach (var entry in source.Entries)
                    {
                        if (!IsWorksheetEntry(entry)) continue;
                        using var entryStream = entry.Open();
                        if (ContainsDataValidations(entryStream)) { hasValidations = true; break; }
                    }
                    if (!hasValidations)
                    {
                        input.Position = startPosition;
                        return input;
                    }

                    var output = new MemoryStream();
                    using (var target = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        foreach (var entry in source.Entries)
                        {
                            var copy = target.CreateEntry(entry.FullName, CompressionLevel.Fastest);
                            copy.LastWriteTime = entry.LastWriteTime;
                            using var from = entry.Open();
                            using var to = copy.Open();
                            if (IsWorksheetEntry(entry)) CopyWithoutDataValidations(from, to);
                            else from.CopyTo(to);
                        }
                    }
                    output.Position = 0;
                    return output;
                }
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is XmlException)
            {
                // Not a readable .xlsx package - hand back the original bytes and let ClosedXML
                // report it through the normal "could not read the file" path.
                input.Position = startPosition;
                return input;
            }
        }

        private static bool IsWorksheetEntry(ZipArchiveEntry entry) =>
            entry.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase)
            && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        // Matches both the classic <dataValidations> element and the Excel 2010 <x14:dataValidations>
        // extension (used for dropdowns that point at another sheet).
        private static bool IsDataValidations(XmlReader reader) =>
            reader.NodeType == XmlNodeType.Element && reader.LocalName == "dataValidations";

        private static readonly XmlReaderSettings SheetReaderSettings = new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        private static bool ContainsDataValidations(Stream sheetXml)
        {
            using var reader = XmlReader.Create(sheetXml, SheetReaderSettings);
            while (reader.Read())
            {
                if (IsDataValidations(reader)) return true;
            }
            return false;
        }

        // Streams the worksheet XML node by node (sheets can be hundreds of MB), dropping every
        // dataValidations subtree and copying everything else as-is.
        private static void CopyWithoutDataValidations(Stream sheetXml, Stream target)
        {
            using var reader = XmlReader.Create(sheetXml, SheetReaderSettings);
            using var writer = XmlWriter.Create(target, new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = true
            });

            reader.Read();
            while (!reader.EOF)
            {
                if (IsDataValidations(reader))
                {
                    reader.Skip();
                    continue;
                }

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        writer.WriteAttributes(reader, true);
                        if (reader.IsEmptyElement) writer.WriteEndElement();
                        break;
                    case XmlNodeType.EndElement:
                        writer.WriteFullEndElement();
                        break;
                    case XmlNodeType.Text:
                        writer.WriteString(reader.Value);
                        break;
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        writer.WriteWhitespace(reader.Value);
                        break;
                    case XmlNodeType.CDATA:
                        writer.WriteCData(reader.Value);
                        break;
                    case XmlNodeType.ProcessingInstruction:
                        writer.WriteProcessingInstruction(reader.Name, reader.Value);
                        break;
                    case XmlNodeType.Comment:
                        writer.WriteComment(reader.Value);
                        break;
                }
                reader.Read();
            }
        }

        private static bool HeaderMatches(byte[] header, byte[] signature)
        {
            for (int i = 0; i < signature.Length; i++)
            {
                if (header[i] != signature[i]) return false;
            }
            return true;
        }

        private static Stream ConvertLegacyXlsToXlsx(Stream xlsStream)
        {
            var sourceWorkbook = new HSSFWorkbook(xlsStream);
            using var targetWorkbook = new XLWorkbook();

            for (int sheetIndex = 0; sheetIndex < sourceWorkbook.NumberOfSheets; sheetIndex++)
            {
                var sourceSheet = sourceWorkbook.GetSheetAt(sheetIndex);
                var sheetName = string.IsNullOrWhiteSpace(sourceSheet.SheetName) ? $"Sheet{sheetIndex + 1}" : sourceSheet.SheetName;
                var targetSheet = targetWorkbook.Worksheets.Add(SanitizeSheetName(sheetName, sheetIndex));

                if (sourceSheet.LastRowNum < 0) continue;

                for (int r = sourceSheet.FirstRowNum; r <= sourceSheet.LastRowNum; r++)
                {
                    var row = sourceSheet.GetRow(r);
                    if (row == null) continue;

                    for (int c = row.FirstCellNum; c < row.LastCellNum; c++)
                    {
                        if (c < 0) continue;
                        var cell = row.GetCell(c);
                        if (cell == null) continue;

                        CopyCellValue(cell, targetSheet.Cell(r + 1, c + 1));
                    }
                }
            }

            var output = new MemoryStream();
            targetWorkbook.SaveAs(output);
            output.Position = 0;
            return output;
        }

        private static void CopyCellValue(ICell cell, IXLCell targetCell)
        {
            switch (cell.CellType)
            {
                case CellType.String:
                    targetCell.Value = cell.StringCellValue;
                    break;
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        targetCell.Value = cell.DateCellValue;
                    else
                        targetCell.Value = cell.NumericCellValue;
                    break;
                case CellType.Boolean:
                    targetCell.Value = cell.BooleanCellValue;
                    break;
                case CellType.Formula:
                    try
                    {
                        switch (cell.CachedFormulaResultType)
                        {
                            case CellType.String:
                                targetCell.Value = cell.StringCellValue;
                                break;
                            case CellType.Numeric:
                                targetCell.Value = cell.NumericCellValue;
                                break;
                            case CellType.Boolean:
                                targetCell.Value = cell.BooleanCellValue;
                                break;
                            default:
                                targetCell.Value = cell.ToString();
                                break;
                        }
                    }
                    catch
                    {
                        targetCell.Value = cell.ToString();
                    }
                    break;
                case CellType.Blank:
                    break;
                default:
                    targetCell.Value = cell.ToString();
                    break;
            }
        }

        private static string SanitizeSheetName(string name, int fallbackIndex)
        {
            var invalidChars = new[] { '\\', '/', '*', '[', ']', ':', '?' };
            foreach (var c in invalidChars) name = name.Replace(c, '-');
            if (name.Length > 31) name = name.Substring(0, 31);
            return string.IsNullOrWhiteSpace(name) ? $"Sheet{fallbackIndex + 1}" : name;
        }
    }
}
