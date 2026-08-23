using System;
using System.Collections.Generic;

namespace NewFeature.Models
{
    public class MaintenanceWorkOrderDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Odometer { get; set; }
        public string BreakdownDescription { get; set; } = string.Empty;
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string BranchLocation { get; set; } = string.Empty;
        public string? BreakdownLocation { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public string TechnicianName { get; set; } = string.Empty;
        public WorkOrderStatus Status { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public List<SparePartConsumptionDto> ConsumedParts { get; set; } = new List<SparePartConsumptionDto>();
    }

    public class SparePartConsumptionDto
    {
        public int Id { get; set; }
        public int MaintenanceWorkOrderId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? InventoryItemId { get; set; }
    }

    public class MaintenanceKpisDto
    {
        public double MeanTimeToRepairHours { get; set; } // MTTR
        public int TotalBreakdowns { get; set; }
        public double FleetAvailabilityRate { get; set; } // %
        public decimal TotalSparePartsCost { get; set; }
        public double ActiveBusesRate { get; set; } // %
        public double MaintenanceBacklogRate { get; set; } // %
        public List<BusBreakdownFrequencyDto> TopFrequentBreakdowns { get; set; } = new List<BusBreakdownFrequencyDto>();

        // How much repair work each breakdown location represents (from the on-site branch
        // reports' "موقع العطل" column). Only includes work orders where a location was recorded.
        public List<BreakdownLocationFrequencyDto> TopBreakdownLocations { get; set; } = new List<BreakdownLocationFrequencyDto>();
    }

    public class BusBreakdownFrequencyDto
    {
        public string VehiclePlate { get; set; } = string.Empty;
        public int BreakdownCount { get; set; }
    }

    public class BreakdownLocationFrequencyDto
    {
        public string Location { get; set; } = string.Empty;
        public int BreakdownCount { get; set; }
        public double SharePercentage { get; set; } // % of all located breakdowns this location represents
    }
}
