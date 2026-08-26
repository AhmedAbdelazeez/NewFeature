namespace NewFeature.Models
{
    // Route Operations KPIs, computed from the Route and Trip tables. Displayed under "إدارة المسارات"
    // (Route Operations) on the dashboard, as its own distinct section from Fleet/Vehicle Management.
    public class RouteOperationsKpisDto
    {
        // 1. Total approved routes on file
        public int TotalRoutesActual { get; set; }
        public int TotalRoutesTarget { get; set; }

        // 2. Average route distance across the whole network
        public decimal AverageDistanceKmActual { get; set; }
        public decimal AverageDistanceKmTarget { get; set; }

        // 3. Total network distance: sum of every distinct route's distance
        public decimal TotalNetworkDistanceKmActual { get; set; }
        public decimal TotalNetworkDistanceKmTarget { get; set; }

        // 4. Longest route on file
        public string LongestRouteName { get; set; } = string.Empty;
        public decimal LongestRouteDistanceKm { get; set; }

        // 5. Most-utilized route by scheduled trip count
        public string MostUtilizedRouteName { get; set; } = string.Empty;
        public int MostUtilizedRouteTripCount { get; set; }
    }
}
