using System;
using System.Collections.Generic;

namespace NewFeature.Models
{
    public class MaintenanceWorkOrderDto
    {
        public int Id { get; set; }
        public string? WorkOrderNumber { get; set; }
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public string? BusNumber { get; set; }
        public DateTime Date { get; set; }
        public int Odometer { get; set; }
        public string BreakdownDescription { get; set; } = string.Empty;
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string BranchLocation { get; set; } = string.Empty;
        public string? BreakdownLocation { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public string TechnicianName { get; set; } = string.Empty;
        public string? TechnicianName2 { get; set; }
        public string? TechnicianName3 { get; set; }
        public string? TechnicianName4 { get; set; }
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

    // Eight indicators, every one of them answerable from the approved "Internal work orders"
    // sheet alone (work-order number, bus, odometer, fault, in/out date and hour, up to four
    // technicians, status, supervisor, notes). Spare-parts cost and breakdown-location share were
    // dropped: the approved sheet carries neither a parts column nor a location column, so both
    // could only ever have reported figures no uploaded file actually supports.
    public class MaintenanceKpisDto
    {
        public int TotalWorkOrders { get; set; }
        public int CompletedWorkOrders { get; set; }
        public double CompletionRatePercent { get; set; }
        public double MeanTimeToRepairHours { get; set; } // MTTR, from date+hour in to date+hour out
        public int WaitingPartsCount { get; set; }        // "متوقف علي قطع غيار"
        public int InProgressCount { get; set; }          // "جاري العمل"
        public double MaintenanceBacklogRate { get; set; } // % of work orders not yet closed
        public int VehiclesServicedCount { get; set; }    // distinct buses that entered the workshop
        public int ActiveTechniciansCount { get; set; }   // distinct technicians named across all four slots

        // Not shown on the Maintenance department's own cards, because the approved work-orders
        // sheet carries neither a whole-fleet view nor a parts column. They stay on this DTO
        // because the Fleet department's cards legitimately need them, and they are computed from
        // the Vehicles and SparePartConsumptions tables those other modules maintain.
        public double FleetAvailabilityRate { get; set; }
        public decimal TotalSparePartsCost { get; set; }

        public List<BusBreakdownFrequencyDto> TopFrequentBreakdowns { get; set; } = new List<BusBreakdownFrequencyDto>();
    }

    public class BusBreakdownFrequencyDto
    {
        public string VehiclePlate { get; set; } = string.Empty;
        public string? BusNumber { get; set; }
        public int BreakdownCount { get; set; }
    }
}
