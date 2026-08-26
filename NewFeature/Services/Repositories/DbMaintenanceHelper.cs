using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NewFeature.Services.Repositories
{
    // Bulk Excel imports insert thousands of rows in one shot via raw AddRange/SaveChanges, which
    // doesn't trigger SQL Server's synchronous auto-update-statistics threshold the way normal
    // incremental traffic does. Left alone, the first query against the freshly-loaded table can get
    // a badly-sniffed execution plan cached against stale row-count estimates - e.g. a routine trip
    // list query going from under a second to a full command timeout once real data (tens of
    // thousands of rows) replaces the small seed dataset. Refreshing statistics right after a bulk
    // import keeps the query planner's estimates accurate for whatever just landed.
    public static class DbMaintenanceHelper
    {
        public static async Task RefreshStatisticsAsync(DbContext context, ILogger logger, params string[] tableNames)
        {
            foreach (var table in tableNames)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync($"UPDATE STATISTICS [{table}] WITH FULLSCAN");
                }
                catch (Exception ex)
                {
                    // Never let a stats-refresh failure fail the import itself - the data is already
                    // committed at this point, this is purely a query-planner optimization.
                    logger.LogWarning(ex, "Failed to refresh statistics on table {Table} after bulk import.", table);
                }
            }
        }
    }
}
