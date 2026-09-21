using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly ApplicationDbContext _context;

        public WarehouseService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Inventory CRUD (paginated + date filtered)
        public async Task<PagedResultDto<InventoryItemDto>> GetInventoryItemsAsync(int page, int pageSize, DateTime? fromDate, DateTime? toDate)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 200) pageSize = 200;

            var query = _context.InventoryItems.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.LastAuditDate >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(i => i.LastAuditDate < toDate.Value.Date.AddDays(1));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.LastAuditDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => MapToDto(i))
                .ToListAsync();

            return new PagedResultDto<InventoryItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<InventoryItemDto?> GetInventoryItemByIdAsync(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            return item == null ? null : MapToDto(item);
        }

        public async Task<InventoryItemDto> CreateInventoryItemAsync(InventoryItemDto dto)
        {
            var item = new InventoryItem
            {
                ItemNameEn = dto.ItemNameEn,
                ItemNameAr = dto.ItemNameAr,
                Category = dto.Category,
                Quantity = dto.Quantity,
                ReorderLevel = dto.ReorderLevel,
                UnitPrice = dto.UnitPrice,
                LastAuditDate = dto.LastAuditDate,
                DiscrepancyCount = dto.DiscrepancyCount
            };

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            dto.Id = item.Id;
            return dto;
        }

        public async Task<bool> UpdateInventoryItemAsync(InventoryItemDto dto)
        {
            var item = await _context.InventoryItems.FindAsync(dto.Id);
            if (item == null) return false;

            item.ItemNameEn = dto.ItemNameEn;
            item.ItemNameAr = dto.ItemNameAr;
            item.Category = dto.Category;
            item.Quantity = dto.Quantity;
            item.ReorderLevel = dto.ReorderLevel;
            item.UnitPrice = dto.UnitPrice;
            item.LastAuditDate = dto.LastAuditDate;
            item.DiscrepancyCount = dto.DiscrepancyCount;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInventoryItemAsync(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null) return false;

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region KPIs
        public async Task<WarehouseKpisDto> GetWarehouseKpisAsync()
        {
            var items = await _context.InventoryItems.ToListAsync();

            var totalItems = items.Count;
            var totalStockValue = items.Sum(i => i.Quantity * i.UnitPrice);
            var lowStockCount = items.Count(i => i.Quantity <= i.ReorderLevel);
            var accurateCount = items.Count(i => i.DiscrepancyCount == 0);
            var accuracyRate = totalItems > 0 ? ((double)accurateCount / totalItems) * 100.0 : 100.0;
            var avgUnitPrice = totalItems > 0 ? items.Average(i => i.UnitPrice) : 0m;

            return new WarehouseKpisDto
            {
                TotalItemsActual = totalItems,
                TotalItemsTarget = System.Math.Max(totalItems, 50),

                TotalStockValueActual = System.Math.Round(totalStockValue, 2),
                TotalStockValueTarget = System.Math.Round(totalStockValue * 1.1m, 2),

                LowStockItemsActual = lowStockCount,
                LowStockItemsTarget = 0,

                InventoryAccuracyRateActual = System.Math.Round(accuracyRate, 1),
                InventoryAccuracyRateTarget = 99.0,

                AverageUnitPriceActual = System.Math.Round(avgUnitPrice, 2),
                AverageUnitPriceTarget = System.Math.Round(avgUnitPrice, 2)
            };
        }
        #endregion

        #region Bulk Upload
        // Dynamic mapper for a storage/inventory Excel sheet. Column names are located by keyword
        // (Arabic + English) since exact headers can vary between warehouse submissions, mirroring
        // the approach used across the other bulk-upload services in this project.
        public async Task<(int SuccessCount, List<string> Errors)> BulkUploadInventoryAsync(Stream excelStream)
        {
            var errors = new List<string>();
            int successCount = 0;

            try
            {
                using var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return (0, new List<string> { "Excel file is empty." });

                var firstRow = worksheet.Row(1);
                var lastUsedCell = firstRow.LastCellUsed();
                if (lastUsedCell == null) return (0, new List<string> { "Excel file has no header row." });

                var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 1; i <= lastUsedCell.Address.ColumnNumber; i++)
                {
                    var val = firstRow.Cell(i).GetString().Trim();
                    if (!string.IsNullOrEmpty(val)) headers[val] = i;
                }

                // Arabic/English aliases must be checked with the "(arabic)"/"(english)" qualifier
                // first - the approved template's headers are "Item Name (Arabic)" and "Item Name
                // (English)", and both contain the generic substring "item name". Without the
                // qualifier, a plain "item name" search matches whichever of the two columns comes
                // first in the header row (Arabic), so the English column never gets located and
                // every English-only row (no Arabic name filled in) is silently skipped further
                // down as "blank".
                int nameArCol = FindColumn(headers, "item name (arabic)", "name (arabic)", "arabic name",
                    "اسم الصنف", "الصنف بالعربي", "الصنف", "item name ar", "name ar");
                int nameEnCol = FindColumn(headers, "item name (english)", "name (english)", "english name",
                    "الصنف بالانجليزي", "item name", "name en");
                int categoryCol = FindColumn(headers, "الفئة", "التصنيف", "category");
                int quantityCol = FindColumn(headers, "الكمية", "quantity", "qty");
                int reorderCol = FindColumn(headers, "حد الطلب", "الحد الأدنى", "reorder");
                int priceCol = FindColumn(headers, "سعر الوحدة", "السعر", "unit price", "price");
                int dateCol = FindColumn(headers, "تاريخ الجرد", "تاريخ آخر جرد", "audit date", "التاريخ", "date");
                int discrepancyCol = FindColumn(headers, "الفروقات", "فروقات الجرد", "discrepancy");

                var rows = worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    try
                    {
                        var nameAr = nameArCol != -1 ? row.Cell(nameArCol).GetString().Trim() : string.Empty;
                        var nameEn = nameEnCol != -1 ? row.Cell(nameEnCol).GetString().Trim() : string.Empty;

                        if (string.IsNullOrEmpty(nameAr) && string.IsNullOrEmpty(nameEn))
                            continue; // blank row

                        if (string.IsNullOrEmpty(nameAr)) nameAr = nameEn;
                        if (string.IsNullOrEmpty(nameEn)) nameEn = nameAr;

                        var category = categoryCol != -1 ? row.Cell(categoryCol).GetString().Trim() : string.Empty;
                        if (string.IsNullOrEmpty(category)) category = "Spare Parts";

                        int.TryParse(quantityCol != -1 ? row.Cell(quantityCol).GetString().Trim() : "0", out int quantity);
                        int.TryParse(reorderCol != -1 ? row.Cell(reorderCol).GetString().Trim() : "0", out int reorderLevel);
                        decimal.TryParse(priceCol != -1 ? row.Cell(priceCol).GetString().Trim() : "0", out decimal unitPrice);
                        int.TryParse(discrepancyCol != -1 ? row.Cell(discrepancyCol).GetString().Trim() : "0", out int discrepancy);

                        DateTime auditDate = DateTime.UtcNow.Date;
                        if (dateCol != -1)
                        {
                            var dateStr = row.Cell(dateCol).GetString().Trim();
                            var parsedDate = FlexibleDateParser.Parse(dateStr);
                            if (parsedDate != null)
                                auditDate = parsedDate.Value;
                        }

                        // Update existing item with the same Arabic name if present, otherwise create new
                        var existing = (await _context.InventoryItems.ToListAsync())
                            .FirstOrDefault(i => i.ItemNameAr.Equals(nameAr, StringComparison.OrdinalIgnoreCase));

                        if (existing != null)
                        {
                            existing.Quantity = quantity;
                            existing.ReorderLevel = reorderLevel;
                            existing.UnitPrice = unitPrice;
                            existing.LastAuditDate = auditDate;
                            existing.DiscrepancyCount = discrepancy;
                            if (!string.IsNullOrEmpty(category)) existing.Category = category;
                        }
                        else
                        {
                            var item = new InventoryItem
                            {
                                ItemNameAr = nameAr,
                                ItemNameEn = nameEn,
                                Category = category,
                                Quantity = quantity,
                                ReorderLevel = reorderLevel,
                                UnitPrice = unitPrice,
                                LastAuditDate = auditDate,
                                DiscrepancyCount = discrepancy
                            };
                            _context.InventoryItems.Add(item);
                        }

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    }
                }

                if (successCount > 0) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing Excel file: {ex.Message}");
            }

            return (successCount, errors);
        }

        private int FindColumn(Dictionary<string, int> headers, params string[] searchTerms)
        {
            foreach (var term in searchTerms)
            {
                var match = headers.Keys.FirstOrDefault(k => k.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match != null) return headers[match];
            }
            return -1;
        }
        #endregion

        #region Mapper
        private static InventoryItemDto MapToDto(InventoryItem i) => new()
        {
            Id = i.Id,
            ItemNameEn = i.ItemNameEn,
            ItemNameAr = i.ItemNameAr,
            Category = i.Category,
            Quantity = i.Quantity,
            ReorderLevel = i.ReorderLevel,
            UnitPrice = i.UnitPrice,
            LastAuditDate = i.LastAuditDate,
            DiscrepancyCount = i.DiscrepancyCount
        };
        #endregion
    }
}
