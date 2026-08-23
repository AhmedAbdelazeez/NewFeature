using System.Collections.Generic;

namespace NewFeature.Models
{
    // Generic paginated response wrapper used by list endpoints that support
    // page/pageSize/date-range filtering (Maintenance work orders, Warehouse inventory, etc.)
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling(TotalCount / (double)PageSize) : 0;
    }
}
