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

        // The approved Route Operations template. Column semantics match the existing Route data
        // model exactly (NameEn/NameAr/StartLocationEn/StartLocationAr/EndLocationEn/EndLocationAr/
        // DistanceKm) - the previous upload code read these same 7 fields positionally with only a
        // loose keyword sanity check; this rebuild adds real header matching, per-row validation,
        // and a structured result on top of the same underlying data shape.
        private static readonly ExcelTemplateDefinition RouteTemplate = new()
        {
            TemplateName = "Route Operations",
            IdentityColumnKey = "NameEn",
            Columns = new List<ExcelColumnDefinition>
            {
                new() { Key = "NameEn", DisplayName = "Route Name (English)", Required = true,
                    HeaderAliases = new[] { "route name (english)", "route name (en)", "english name", "name (english)" } },
                new() { Key = "NameAr", DisplayName = "Route Name (Arabic)", Required = true,
                    HeaderAliases = new[] { "route name (arabic)", "route name (ar)", "arabic name", "name (arabic)", "اسم المسار" } },
                // Start/End location is not always known when a route is first entered (e.g. an
                // employee-shuttle or on-demand route defined only by name and distance), so these
                // four columns are optional both as a header and per-row.
                new() { Key = "StartLocationEn",
                    HeaderAliases = new[] { "start location (english)", "start location (en)", "start (english)", "english start" } },
                new() { Key = "StartLocationAr",
                    HeaderAliases = new[] { "start location (arabic)", "start location (ar)", "start (arabic)", "arabic start", "بداية" } },
                new() { Key = "EndLocationEn",
                    HeaderAliases = new[] { "end location (english)", "end location (en)", "end (english)", "english end" } },
                new() { Key = "EndLocationAr",
                    HeaderAliases = new[] { "end location (arabic)", "end location (ar)", "end (arabic)", "arabic end", "نهاية" } },
                new() { Key = "DistanceKm", DisplayName = "Distance (KM)", Required = true,
                    HeaderAliases = new[] { "distance (km)", "distance", "المسافة" } },
            }
        };

        #endregion

        #region Bulk Upload
        public async System.Threading.Tasks.Task<ExcelImportResultDto> BulkUploadRoutesAsync(System.IO.Stream excelStream)
        {
            // Loaded once, up front - no per-row database round-trip.
            var existingRoutes = (await _routeRepository.GetAllAsync())
                .GroupBy(r => NormalizeName(r.NameEn))
                .ToDictionary(g => g.Key, g => g.First());

            async System.Threading.Tasks.Task<ExcelRowOutcomeResult> ProcessRow(ExcelRowContext ctx)
            {
                var nameEn = ctx.GetString("NameEn");
                if (string.IsNullOrEmpty(nameEn))
                    return ExcelRowOutcomeResult.Skipped("Route Name (English) is required.", "Route Name (English)");
                if (nameEn.Length > 150)
                    return ExcelRowOutcomeResult.Skipped("Route Name (English) cannot exceed 150 characters.", "Route Name (English)");

                var nameAr = ctx.GetString("NameAr");
                if (string.IsNullOrEmpty(nameAr))
                    return ExcelRowOutcomeResult.Skipped("Route Name (Arabic) is required.", "Route Name (Arabic)");
                if (nameAr.Length > 150)
                    return ExcelRowOutcomeResult.Skipped("Route Name (Arabic) cannot exceed 150 characters.", "Route Name (Arabic)");

                // Start/End location is optional - blank is a legitimate "not yet known" value, not
                // an error. Only validate length when something was actually entered.
                var startEn = ctx.GetString("StartLocationEn");
                if (startEn.Length > 200)
                    return ExcelRowOutcomeResult.Skipped("Start Location (English) cannot exceed 200 characters.", "Start Location (English)");

                var startAr = ctx.GetString("StartLocationAr");
                if (startAr.Length > 200)
                    return ExcelRowOutcomeResult.Skipped("Start Location (Arabic) cannot exceed 200 characters.", "Start Location (Arabic)");

                var endEn = ctx.GetString("EndLocationEn");
                if (endEn.Length > 200)
                    return ExcelRowOutcomeResult.Skipped("End Location (English) cannot exceed 200 characters.", "End Location (English)");

                var endAr = ctx.GetString("EndLocationAr");
                if (endAr.Length > 200)
                    return ExcelRowOutcomeResult.Skipped("End Location (Arabic) cannot exceed 200 characters.", "End Location (Arabic)");

                var distanceStr = ctx.GetString("DistanceKm");
                if (!decimal.TryParse(distanceStr, out var distance))
                    return ExcelRowOutcomeResult.Skipped("Distance (KM) is required and must be a valid number.", "Distance (KM)");
                if (distance <= 0 || distance > 100000)
                    return ExcelRowOutcomeResult.Skipped("Distance (KM) must be greater than 0 and at most 100000.", "Distance (KM)");

                // A route cannot start and end at the same place - but only when both are actually
                // filled in (a route with no start/end recorded yet obviously isn't a same-place error).
                if (startEn.Length > 0 && endEn.Length > 0 &&
                    string.Equals(startEn.Trim(), endEn.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    return ExcelRowOutcomeResult.Skipped("Start Location and End Location cannot be the same.", "End Location (English)");

                // Route Name (English) is the business key: an existing route name is updated in
                // place, a new one is inserted. (No formal Route Code exists in the data model yet -
                // see the Template Guide for a recommendation to add one for more reliable matching.)
                var key = NormalizeName(nameEn);
                if (existingRoutes.TryGetValue(key, out var existing))
                {
                    existing.NameAr = nameAr;
                    existing.StartLocationEn = startEn;
                    existing.StartLocationAr = startAr;
                    existing.EndLocationEn = endEn;
                    existing.EndLocationAr = endAr;
                    existing.DistanceKm = distance;
                    return ExcelRowOutcomeResult.Updated();
                }

                var route = new Models.Route
                {
                    NameEn = nameEn,
                    NameAr = nameAr,
                    StartLocationEn = startEn,
                    StartLocationAr = startAr,
                    EndLocationEn = endEn,
                    EndLocationAr = endAr,
                    DistanceKm = distance
                };
                await _routeRepository.AddAsync(route);
                existingRoutes[key] = route;
                return ExcelRowOutcomeResult.Inserted();
            }

            return await ExcelImportEngine.RunAsync(
                excelStream,
                RouteTemplate,
                ProcessRow,
                _routeRepository.SaveChangesAsync,
                _logger);
        }

        private static string NormalizeName(string name) =>
            (name ?? string.Empty).Trim().ToUpperInvariant();
        #endregion

        #region KPIs
        public async System.Threading.Tasks.Task<RouteOperationsKpisDto> GetRouteOperationsKpisAsync()
        {
            var routes = (await _routeRepository.GetAllAsync()).ToList();
            var trips = (await _tripRepository.GetAllAsync()).ToList();

            var totalRoutes = routes.Count;
            var totalDistance = routes.Sum(r => r.DistanceKm);
            var avgDistance = totalRoutes > 0 ? System.Math.Round(totalDistance / totalRoutes, 1) : 0m;

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
