using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class MaintenanceWorkOrder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vehicle ID is required")]
        public int VehicleId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, 5000000, ErrorMessage = "Please enter a valid odometer reading")]
        public int Odometer { get; set; }

        [Required(ErrorMessage = "Breakdown Description is required")]
        [StringLength(500)]
        public string BreakdownDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Time In is required")]
        public DateTime TimeIn { get; set; }

        public DateTime? TimeOut { get; set; }

        [StringLength(100)]
        public string BranchLocation { get; set; } = string.Empty;

        [StringLength(150)]
        public string SupervisorName { get; set; } = string.Empty;

        [StringLength(150)]
        public string TechnicianName { get; set; } = string.Empty;

        [Required]
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Pending;

        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        // Navigation properties
        public Vehicle? Vehicle { get; set; }
        public ICollection<SparePartConsumption> ConsumedParts { get; set; } = new List<SparePartConsumption>();
    }
}
