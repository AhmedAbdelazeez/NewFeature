using System;
using System.Collections.Generic;

namespace NewFeature.Models
{
    // One dispatch line, as the Operations page's index table and add/edit form exchange it.
    public class OperationsDispatchRecordDto
    {
        public int Id { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string? DirectionName { get; set; }
        public string RentOrder { get; set; } = string.Empty;
        public string? CustomerAccount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? BusType { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public DateTime? PlannedStart { get; set; }
        public DateTime? PlannedEnd { get; set; }
        public string? DriverNumber { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string? AdditionalDriverNumber { get; set; }
        public string? AdditionalDriverName { get; set; }
        public string? Location { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public double? ActualKm { get; set; }
        public double? PlannedKm { get; set; }
        public double? DieselLiters { get; set; }
        public string Completion { get; set; } = string.Empty;
    }

    // How much of the month's dispatch work one line (Direction) represents.
    public class OperationsDirectionUsageDto
    {
        public string Direction { get; set; } = string.Empty;
        public string? DirectionName { get; set; }
        public int OrdersCount { get; set; }
        public double SharePercentage { get; set; }
    }

    // Ten indicators, every one of them a count, a distinct-count or a sum over the approved
    // Operations dispatch template. No illustrative targets and no metric the sheet can't support:
    // on-time performance, for instance, is absent because the sheet records a planned start and
    // end but never an actual arrival.
    public class OperationsKpisDto
    {
        public int TotalDispatchOrders { get; set; }
        public int RentalOrdersCount { get; set; }
        public int ClientsServedCount { get; set; }
        public int BusesDeployedCount { get; set; }
        public int DriversAssignedCount { get; set; }
        public int CompletedOrdersCount { get; set; }
        public double CompletionRatePercent { get; set; }
        public double TotalPlannedKm { get; set; }
        public double TotalActualKm { get; set; }
        public double TotalDieselLiters { get; set; }
        public double AverageOrdersPerDay { get; set; }

        public List<OperationsDirectionUsageDto> TopDirections { get; set; } = new();
    }
}
