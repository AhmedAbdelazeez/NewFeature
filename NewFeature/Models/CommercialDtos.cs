using System;
using System.ComponentModel.DataAnnotations;

namespace NewFeature.Models
{
    public class CommercialContractDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English Client Name is required")]
        public string ClientNameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Client Name is required")]
        public string ClientNameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contract Number is required")]
        public string ContractNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contract Value is required")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Preparation Date is required")]
        public DateTime PreparationDate { get; set; }

        public DateTime? ActiveDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Active";

        public bool IsDisputed { get; set; }
    }

    public class CommercialLeadDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lead Name is required")]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Source is required")]
        public string Source { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "New";

        [Required(ErrorMessage = "Estimated Value is required")]
        public decimal EstimatedValue { get; set; }

        [Required(ErrorMessage = "Acquisition Cost is required")]
        public decimal AcquisitionCost { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class CommercialKpisDto
    {
        public double CustomerRetentionRateActual { get; set; }
        public double CustomerRetentionRateTarget { get; set; }

        public int NewContractsSecuredActual { get; set; }
        public int NewContractsSecuredTarget { get; set; }

        public double ContractRenewalRateActual { get; set; }
        public double ContractRenewalRateTarget { get; set; }

        public double ContractTurnaroundTimeActual { get; set; }
        public double ContractTurnaroundTimeTarget { get; set; }

        public int ContractualLegalDisputesActual { get; set; }
        public int ContractualLegalDisputesTarget { get; set; }

        public decimal CustomerAcquisitionCostActual { get; set; }
        public decimal CustomerAcquisitionCostTarget { get; set; }

        public double ContractValueGrowthRateActual { get; set; }
        public double ContractValueGrowthRateTarget { get; set; }
    }
}
