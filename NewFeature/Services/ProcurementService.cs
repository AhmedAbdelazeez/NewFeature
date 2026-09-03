using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class ProcurementService : IProcurementService
    {
        private readonly ApplicationDbContext _context;

        public ProcurementService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Requests CRUD
        public async Task<IEnumerable<ProcurementRequestDto>> GetAllRequestsAsync()
        {
            var requests = await _context.ProcurementRequests.OrderByDescending(r => r.RequestDate).ToListAsync();
            return requests.Select(MapToRequestDto);
        }

        // Paginated + search listing for the Procurement page's Requests table (mirrors the
        // "fetch everything, then filter/order/Skip/Take in memory" pattern used by
        // FleetService.GetVehiclesPagedAsync). GetAllRequestsAsync above is left untouched.
        public async Task<PagedResultDto<ProcurementRequestDto>> GetRequestsPagedAsync(int page, int pageSize, string? search, string? status)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var requests = (await _context.ProcurementRequests.ToListAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                requests = requests.Where(r =>
                    Contains(r.TitleEn, term) ||
                    Contains(r.TitleAr, term) ||
                    Contains(r.RequesterName, term) ||
                    Contains(r.SupplierName, term) ||
                    Contains(r.Status, term));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status);
            }

            var ordered = requests.OrderByDescending(r => r.RequestDate).ToList();
            var totalCount = ordered.Count;

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToRequestDto)
                .ToList();

            return new PagedResultDto<ProcurementRequestDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private static bool Contains(string? haystack, string term) =>
            !string.IsNullOrEmpty(haystack) && haystack.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        public async Task<ProcurementRequestDto?> GetRequestByIdAsync(int id)
        {
            var req = await _context.ProcurementRequests.FindAsync(id);
            if (req == null) return null;
            return MapToRequestDto(req);
        }

        public async Task<ProcurementRequestDto> CreateRequestAsync(ProcurementRequestDto dto)
        {
            var req = new ProcurementRequest
            {
                TitleEn = dto.TitleEn,
                TitleAr = dto.TitleAr,
                RequesterName = dto.RequesterName,
                SupplierName = dto.SupplierName,
                Amount = dto.Amount,
                Status = dto.Status,
                RequestDate = dto.RequestDate == default ? DateTime.UtcNow : dto.RequestDate,
                DeliveryDate = dto.Status == "Received" ? (dto.DeliveryDate ?? DateTime.UtcNow) : dto.DeliveryDate,
                BudgetCompliant = dto.BudgetCompliant
            };

            _context.ProcurementRequests.Add(req);
            await _context.SaveChangesAsync();
            return MapToRequestDto(req);
        }

        public async Task<bool> UpdateRequestAsync(ProcurementRequestDto dto)
        {
            var req = await _context.ProcurementRequests.FindAsync(dto.Id);
            if (req == null) return false;

            req.TitleEn = dto.TitleEn;
            req.TitleAr = dto.TitleAr;
            req.RequesterName = dto.RequesterName;
            req.SupplierName = dto.SupplierName;
            req.Amount = dto.Amount;
            req.Status = dto.Status;
            req.RequestDate = dto.RequestDate;
            req.BudgetCompliant = dto.BudgetCompliant;

            if (req.Status != "Received" && dto.Status == "Received")
            {
                req.DeliveryDate = dto.DeliveryDate ?? DateTime.UtcNow;
            }
            else if (dto.Status != "Received")
            {
                req.DeliveryDate = null;
            }
            else
            {
                req.DeliveryDate = dto.DeliveryDate;
            }

            _context.Entry(req).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRequestAsync(int id)
        {
            var req = await _context.ProcurementRequests.FindAsync(id);
            if (req == null) return false;

            _context.ProcurementRequests.Remove(req);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Inventory CRUD
        public async Task<IEnumerable<InventoryItemDto>> GetAllInventoryItemsAsync()
        {
            var items = await _context.InventoryItems.OrderBy(i => i.ItemNameEn).ToListAsync();
            return items.Select(MapToInventoryItemDto);
        }

        // Paginated + search listing for the Procurement page's Inventory table.
        public async Task<PagedResultDto<InventoryItemDto>> GetInventoryPagedAsync(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var items = (await _context.InventoryItems.ToListAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                items = items.Where(i =>
                    Contains(i.ItemNameEn, term) ||
                    Contains(i.ItemNameAr, term) ||
                    Contains(i.Category, term));
            }

            var ordered = items.OrderBy(i => i.ItemNameEn).ToList();
            var totalCount = ordered.Count;

            var pagedItems = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToInventoryItemDto)
                .ToList();

            return new PagedResultDto<InventoryItemDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<InventoryItemDto?> GetInventoryItemByIdAsync(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null) return null;
            return MapToInventoryItemDto(item);
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
                LastAuditDate = dto.LastAuditDate == default ? DateTime.UtcNow : dto.LastAuditDate,
                DiscrepancyCount = dto.DiscrepancyCount
            };

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            return MapToInventoryItemDto(item);
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

            _context.Entry(item).State = EntityState.Modified;
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

        #region KPIs Calculation
        public async Task<ProcurementKpisDto> GetProcurementKpisAsync()
        {
            var requests = await _context.ProcurementRequests.ToListAsync();
            var items = await _context.InventoryItems.ToListAsync();

            // 1. Avg Procurement Cycle Time
            double avgCycle = 6.0; // target is 7 days, lower is better
            var completedReqs = requests.Where(r => r.Status == "Received" && r.DeliveryDate.HasValue).ToList();
            if (completedReqs.Any())
            {
                avgCycle = Math.Round(completedReqs.Average(r => (r.DeliveryDate.Value - r.RequestDate).TotalDays), 1);
            }

            // 2. Cost Savings Rate
            double savingsRate = 12.0; // target is 10%
            if (requests.Any())
            {
                double compliantRatio = (double)requests.Count(r => r.BudgetCompliant) / requests.Count;
                savingsRate = Math.Round(5.0 + (compliantRatio * 15.0), 1); // range 5% - 20%
            }

            // 3. Supplier Performance Rating
            double supplierRating = 88.0; // target 90%
            if (completedReqs.Any())
            {
                // portion delivered in <= 7 days
                int onTime = completedReqs.Count(r => (r.DeliveryDate.Value - r.RequestDate).TotalDays <= 7);
                supplierRating = Math.Round((double)onTime / completedReqs.Count * 100, 1);
            }

            // 4. Budget Compliance
            double budgetComp = 97.0; // target 95%
            if (requests.Any())
            {
                budgetComp = Math.Round((double)requests.Count(r => r.BudgetCompliant) / requests.Count * 100, 1);
            }

            // 5. Critical Spare Parts Availability
            double sparePartsAvail = 96.0; // target 98%
            var spareParts = items.Where(i => i.Category == "Spare Parts").ToList();
            if (spareParts.Any())
            {
                int available = spareParts.Count(i => i.Quantity > i.ReorderLevel);
                sparePartsAvail = Math.Round((double)available / spareParts.Count * 100, 1);
            }

            // 6. Inventory Accuracy Rate
            double invAccuracy = 98.5; // target 99%
            if (items.Any())
            {
                int accurate = items.Count(i => i.DiscrepancyCount == 0);
                invAccuracy = Math.Round((double)accurate / items.Count * 100, 1);
            }

            // 7. Active Supply Contracts Signed
            int activeContracts = 35; // target 30
            if (requests.Any())
            {
                // Count unique suppliers in approved/ordered/received state
                activeContracts = requests.Where(r => r.Status == "Approved" || r.Status == "Ordered" || r.Status == "Received")
                                          .Select(r => r.SupplierName)
                                          .Distinct()
                                          .Count();
            }

            return new ProcurementKpisDto
            {
                AvgProcurementCycleTimeActual = avgCycle,
                AvgProcurementCycleTimeTarget = 7.0,

                CostSavingsRateActual = savingsRate,
                CostSavingsRateTarget = 10.0,

                SupplierPerformanceRatingActual = supplierRating,
                SupplierPerformanceRatingTarget = 90.0,

                BudgetComplianceActual = budgetComp,
                BudgetComplianceTarget = 95.0,

                CriticalSparePartsAvailabilityActual = sparePartsAvail,
                CriticalSparePartsAvailabilityTarget = 98.0,

                InventoryAccuracyRateActual = invAccuracy,
                InventoryAccuracyRateTarget = 99.0,

                ActiveSupplyContractsActual = activeContracts,
                ActiveSupplyContractsTarget = 30
            };
        }
        #endregion

        #region Mappers
        private static ProcurementRequestDto MapToRequestDto(ProcurementRequest r)
        {
            return new ProcurementRequestDto
            {
                Id = r.Id,
                TitleEn = r.TitleEn,
                TitleAr = r.TitleAr,
                RequesterName = r.RequesterName,
                SupplierName = r.SupplierName,
                Amount = r.Amount,
                Status = r.Status,
                RequestDate = r.RequestDate,
                DeliveryDate = r.DeliveryDate,
                BudgetCompliant = r.BudgetCompliant
            };
        }

        private static InventoryItemDto MapToInventoryItemDto(InventoryItem i)
        {
            return new InventoryItemDto
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
        }
        #endregion
    }
}
