using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NewFeature.Models;
using NewFeature.Services.ExcelImport;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class RouteOperationsService : IRouteOperationsService
    {
        private readonly IRepository<Models.Route> _routeRepository;
        private readonly IRepository<Trip> _tripRepository;
        private readonly ILogger<RouteOperationsService> _logger;

        public RouteOperationsService(
            IRepository<Models.Route> routeRepository,
            IRepository<Trip> tripRepository,
            ILogger<RouteOperationsService> logger)
        {
            _routeRepository = routeRepository;
            _tripRepository = tripRepository;
            _logger = logger;
        }

        #region Route Operations Excel Template

        // The approved Route Operations template (the sales-lines sheet). Item Code is the route's
        // business key: it is unique per line, whereas the English name is not - the same name
        // appears under two codes with different distances (e.g. "Makkah - Taif Airport" as 12 and
        // L79). The first alias of each column is the exact header the downloadable template writes.
        public static readonly ExcelTemplateDefinition RouteTemplate = new()
        {
            TemplateName = "نموذج المسارات (Route Operations)",
            IdentityColumnKey = "NameEn",
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = "Code", DisplayName = "Item Code (كود المسار)", Required = true,
                    HeaderAliases = new[] { "Item Code", "route code", "كود المسار", "كود الخط", "كود الصنف", "رمز المسار" } },
                new() { Key = "NameEn", DisplayName = "Route Name (English)", Required = true,
                    HeaderAliases = new[] { "Route Name (English)", "route name (en)", "english name", "name (english)" } },
                new() { Key = "NameAr", DisplayName = "Route Name (Arabic)", Required = true,
                    HeaderAliases = new[] { "Route Name (Arabic)", "route name (ar)", "arabic name", "name (arabic)", "اسم المسار" } },
                // Start/End location is not always known (daily bus rental, on-demand lines are
                // defined only by name), so these four columns are optional both as a header and
                // per-row.
                new() { Key = "StartLocationEn", DisplayName = "Start Location (English)",
                    HeaderAliases = new[] { "Start Location (English)", "start location (en)", "start (english)", "english start" } },
                new() { Key = "StartLocationAr", DisplayName = "Start Location (Arabic)",
                    HeaderAliases = new[] { "Start Location (Arabic)", "start location (ar)", "start (arabic)", "arabic start", "بداية" } },
                new() { Key = "EndLocationEn", DisplayName = "End Location (English)",
                    HeaderAliases = new[] { "End Location (English)", "end location (en)", "end (english)", "english end" } },
                new() { Key = "EndLocationAr", DisplayName = "End Location (Arabic)",
                    HeaderAliases = new[] { "End Location (Arabic)", "end location (ar)", "end (arabic)", "arabic end", "نهاية" } },
                new() { Key = "DistanceKm", DisplayName = "Distance (KM)", Required = true,
                    HeaderAliases = new[] { "Distance (KM)", "distance", "المسافة" } },
            }
        };

        #endregion

        #region Bulk Upload
        public async System.Threading.Tasks.Task<ExcelImportResultDto> BulkUploadRoutesAsync(System.IO.Stream excelStream)
        {
            // Loaded once, up front - no per-row database round-trip.
            var allRoutes = (await _routeRepository.GetAllAsync()).ToList();
            var routesByCode = allRoutes
                .Where(r => !string.IsNullOrWhiteSpace(r.Code))
                .GroupBy(r => NormalizeKey(r.Code))
                .ToDictionary(g => g.Key, g => g.First());

            // Routes created before codes existed are matched once by English name, and take the
            // code of the first row that names them - so the first upload of the coded sheet fills
            // in the codes instead of duplicating every existing route.
            var uncodedByName = allRoutes
                .Where(r => string.IsNullOrWhiteSpace(r.Code))
                .GroupBy(r => NormalizeKey(r.NameEn))
                .ToDictionary(g => g.Key, g => g.First());

            var codesInThisFile = new Dictionary<string, int>(); // code -> first row that used it

            async System.Threading.Tasks.Task<ExcelRowOutcomeResult> ProcessRow(ExcelRowContext ctx)
            {
                var code = ctx.GetString("Code");
                if (string.IsNullOrEmpty(code))
                    return ExcelRowOutcomeResult.Skipped("كود المسار (Item Code) مطلوب لكل مسار.", "Item Code");
                if (code.Length > 50)
                    return ExcelRowOutcomeResult.Skipped("كود المسار (Item Code) لا يتجاوز 50 حرفاً.", "Item Code");

                var codeKey = NormalizeKey(code);
                if (codesInThisFile.TryGetValue(codeKey, out var firstRow))
                    return ExcelRowOutcomeResult.Skipped($"كود المسار \"{code}\" مكرر في الملف (ورد أولاً في الصف {firstRow}). يجب أن يكون لكل مسار كود مختلف.", "Item Code");
                codesInThisFile[codeKey] = ctx.RowNumber;

                var nameEn = ctx.GetString("NameEn");
                if (string.IsNullOrEmpty(nameEn))
                    return ExcelRowOutcomeResult.Skipped("اسم المسار بالإنجليزية (Route Name (English)) مطلوب.", "Route Name (English)");
                if (nameEn.Length > 150)
                    return ExcelRowOutcomeResult.Skipped("اسم المسار بالإنجليزية لا يتجاوز 150 حرفاً.", "Route Name (English)");

                var nameAr = ctx.GetString("NameAr");
                if (string.IsNullOrEmpty(nameAr))
                    return ExcelRowOutcomeResult.Skipped("اسم المسار بالعربية (Route Name (Arabic)) مطلوب.", "Route Name (Arabic)");
                if (nameAr.Length > 150)
                    return ExcelRowOutcomeResult.Skipped("اسم المسار بالعربية لا يتجاوز 150 حرفاً.", "Route Name (Arabic)");

                // Start/End location is optional - blank is a legitimate "not a point-to-point line"
                // value, not an error. A start equal to the end is legitimate too: intra-city lines
                // (Makkah hotel -> Makkah station) are a large part of the sheet.
                var startEn = ctx.GetString("StartLocationEn");
                var startAr = ctx.GetString("StartLocationAr");
                var endEn = ctx.GetString("EndLocationEn");
                var endAr = ctx.GetString("EndLocationAr");
                foreach (var (value, column) in new[]
                {
                    (startEn, "Start Location (English)"), (startAr, "Start Location (Arabic)"),
                    (endEn, "End Location (English)"), (endAr, "End Location (Arabic)")
                })
                {
                    if (value.Length > 200)
                        return ExcelRowOutcomeResult.Skipped($"{column} لا يتجاوز 200 حرف.", column);
                }

                // Zero is accepted: service lines such as daily bus rental carry no distance.
                var distance = ctx.GetDecimal("DistanceKm");
                if (distance == null)
                    return ExcelRowOutcomeResult.Skipped("المسافة (Distance (KM)) مطلوبة ويجب أن تكون رقماً.", "Distance (KM)");
                if (distance < 0 || distance > 100000)
                    return ExcelRowOutcomeResult.Skipped("المسافة (Distance (KM)) يجب أن تكون بين 0 و 100000 كم.", "Distance (KM)");

                var existing = routesByCode.GetValueOrDefault(codeKey);
                if (existing == null && uncodedByName.Remove(NormalizeKey(nameEn), out var legacy))
                {
                    existing = legacy;
                    routesByCode[codeKey] = legacy;
                }

                if (existing != null)
                {
                    existing.Code = code;
                    existing.NameEn = nameEn;
                    existing.NameAr = nameAr;
                    existing.StartLocationEn = startEn;
                    existing.StartLocationAr = startAr;
                    existing.EndLocationEn = endEn;
                    existing.EndLocationAr = endAr;
                    existing.DistanceKm = distance.Value;
                    return ExcelRowOutcomeResult.Updated();
                }

                var route = new Models.Route
                {
                    Code = code,
                    NameEn = nameEn,
                    NameAr = nameAr,
                    StartLocationEn = startEn,
                    StartLocationAr = startAr,
                    EndLocationEn = endEn,
                    EndLocationAr = endAr,
                    DistanceKm = distance.Value
                };
                await _routeRepository.AddAsync(route);
                routesByCode[codeKey] = route;
                return ExcelRowOutcomeResult.Inserted();
            }

            return await ExcelImportEngine.RunAsync(
                excelStream,
                RouteTemplate,
                ProcessRow,
                _routeRepository.SaveChangesAsync,
                _logger);
        }

        private static string NormalizeKey(string? value) =>
            (value ?? string.Empty).Trim().ToUpperInvariant();
        #endregion

        #region KPIs
        public async System.Threading.Tasks.Task<RouteOperationsKpisDto> GetRouteOperationsKpisAsync()
        {
            var routes = (await _routeRepository.GetAllAsync()).ToList();
            var trips = (await _tripRepository.GetAllAsync()).ToList();

            var totalRoutes = routes.Count;
            var totalDistance = routes.Sum(r => r.DistanceKm);
            // Service lines with no distance (daily bus rental and the like) would drag the average
            // toward zero, so it is taken over the lines that actually have a distance.
            var measuredRoutes = routes.Count(r => r.DistanceKm > 0);
            var avgDistance = measuredRoutes > 0 ? System.Math.Round(totalDistance / measuredRoutes, 1) : 0m;

            var longest = routes.OrderByDescending(r => r.DistanceKm).FirstOrDefault();

            var tripCountByRoute = trips
                .GroupBy(t => t.RouteId)
                .Select(g => new { RouteId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefault();

            var routeMap = routes.ToDictionary(r => r.Id, r => r.NameEn);
            var mostUtilizedName = tripCountByRoute != null && routeMap.TryGetValue(tripCountByRoute.RouteId, out var name)
                ? name
                : string.Empty;

            return new RouteOperationsKpisDto
            {
                TotalRoutesActual = totalRoutes,
                TotalRoutesTarget = totalRoutes,

                AverageDistanceKmActual = avgDistance,
                AverageDistanceKmTarget = avgDistance,

                TotalNetworkDistanceKmActual = System.Math.Round(totalDistance, 1),
                TotalNetworkDistanceKmTarget = System.Math.Round(totalDistance, 1),

                LongestRouteName = longest?.NameEn ?? string.Empty,
                LongestRouteDistanceKm = longest?.DistanceKm ?? 0m,

                MostUtilizedRouteName = mostUtilizedName,
                MostUtilizedRouteTripCount = tripCountByRoute?.Count ?? 0
            };
        }
        #endregion
    }
}
