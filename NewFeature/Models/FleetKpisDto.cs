namespace NewFeature.Models
{
    // 5 new Fleet indicators computed from the Vehicle table (populated via the Fleet Details bulk
    // upload). Displayed under "إدارة الأسطول" (Fleet Department) on the dashboard.
    public class FleetKpisDto
    {
        // 1. Total seating capacity across the whole fleet
        public decimal TotalSeatingCapacityActual { get; set; }
        public decimal TotalSeatingCapacityTarget { get; set; }

        // 2. Average bus age in years (from Vehicle.Year)
        public double AverageBusAgeActual { get; set; }
        public double AverageBusAgeTarget { get; set; }

        // 3. Fleet modernization rate: % of vehicles manufactured in the last 5 years
        public double FleetModernizationRateActual { get; set; }
        public double FleetModernizationRateTarget { get; set; }

        // 4. Bus type variety count: distinct Model values in the fleet
        public int BusTypeVarietyCountActual { get; set; }
        public int BusTypeVarietyCountTarget { get; set; }

        // 5. Average seating capacity per bus
        public double AverageCapacityPerBusActual { get; set; }
        public double AverageCapacityPerBusTarget { get; set; }
    }
}
