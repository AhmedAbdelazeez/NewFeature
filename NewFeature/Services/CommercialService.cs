using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class CommercialService : ICommercialService
    {
        private readonly ApplicationDbContext _context;

        public CommercialService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Contracts CRUD
        public async Task<IEnumerable<CommercialContractDto>> GetAllContractsAsync()
        {
            var items = await _context.CommercialContracts.OrderByDescending(c => c.StartDate).ToListAsync();
            return items.Select(MapToContractDto);
        }

        public async Task<CommercialContractDto?> GetContractByIdAsync(int id)
        {
            var item = await _context.CommercialContracts.FindAsync(id);
            if (item == null) return null;
            return MapToContractDto(item);
        }

        public async Task<CommercialContractDto> CreateContractAsync(CommercialContractDto dto)
        {
            var item = new CommercialContract
            {
                ClientNameEn = dto.ClientNameEn,
                ClientNameAr = dto.ClientNameAr,
                ContractNumber = dto.ContractNumber,
                Value = dto.Value,
                StartDate = dto.StartDate == default ? DateTime.UtcNow : dto.StartDate,
                EndDate = dto.EndDate == default ? DateTime.UtcNow.AddYears(1) : dto.EndDate,
                PreparationDate = dto.PreparationDate == default ? DateTime.UtcNow : dto.PreparationDate,
                ActiveDate = dto.ActiveDate,
                Status = dto.Status,
                IsDisputed = dto.IsDisputed
            };

            _context.CommercialContracts.Add(item);
            await _context.SaveChangesAsync();
            return MapToContractDto(item);
        }

        public async Task<bool> UpdateContractAsync(CommercialContractDto dto)
        {
            var item = await _context.CommercialContracts.FindAsync(dto.Id);
            if (item == null) return false;

            item.ClientNameEn = dto.ClientNameEn;
            item.ClientNameAr = dto.ClientNameAr;
            item.ContractNumber = dto.ContractNumber;
            item.Value = dto.Value;
            item.StartDate = dto.StartDate;
            item.EndDate = dto.EndDate;
            item.PreparationDate = dto.PreparationDate;
            item.ActiveDate = dto.ActiveDate;
            item.Status = dto.Status;
            item.IsDisputed = dto.IsDisputed;

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            var item = await _context.CommercialContracts.FindAsync(id);
            if (item == null) return false;

            _context.CommercialContracts.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Leads CRUD
        public async Task<IEnumerable<CommercialLeadDto>> GetAllLeadsAsync()
        {
            var items = await _context.CommercialLeads.OrderByDescending(l => l.CreatedAt).ToListAsync();
            return items.Select(MapToLeadDto);
        }

        public async Task<CommercialLeadDto?> GetLeadByIdAsync(int id)
        {
            var item = await _context.CommercialLeads.FindAsync(id);
            if (item == null) return null;
            return MapToLeadDto(item);
        }

        public async Task<CommercialLeadDto> CreateLeadAsync(CommercialLeadDto dto)
        {
            var item = new CommercialLead
            {
                LeadName = dto.LeadName,
                Source = dto.Source,
                Status = dto.Status,
                EstimatedValue = dto.EstimatedValue,
                AcquisitionCost = dto.AcquisitionCost,
                CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt
            };

            _context.CommercialLeads.Add(item);
            await _context.SaveChangesAsync();
            return MapToLeadDto(item);
        }

        public async Task<bool> UpdateLeadAsync(CommercialLeadDto dto)
        {
            var item = await _context.CommercialLeads.FindAsync(dto.Id);
            if (item == null) return false;

            item.LeadName = dto.LeadName;
            item.Source = dto.Source;
            item.Status = dto.Status;
            item.EstimatedValue = dto.EstimatedValue;
            item.AcquisitionCost = dto.AcquisitionCost;
            item.CreatedAt = dto.CreatedAt;

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLeadAsync(int id)
        {
            var item = await _context.CommercialLeads.FindAsync(id);
            if (item == null) return false;

            _context.CommercialLeads.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region KPIs
        public async Task<CommercialKpisDto> GetCommercialKpisAsync()
        {
            var contracts = await _context.CommercialContracts.ToListAsync();
            var leads = await _context.CommercialLeads.ToListAsync();

            // 1. Customer Retention Rate
            var totalContracts = contracts.Count(c => c.Status == "Active" || c.Status == "Renewed" || c.Status == "Expired");
            var retainedContracts = contracts.Count(c => c.Status == "Active" || c.Status == "Renewed");
            double retentionRate = totalContracts > 0 ? (double)retainedContracts / totalContracts * 100.0 : 0.0;

            // 2. New Contracts Secured
            int newContracts = contracts.Count(c => c.StartDate.Year == DateTime.UtcNow.Year);

            // 3. Contract Renewal Rate
            var expired = contracts.Count(c => c.Status == "Expired");
            var renewed = contracts.Count(c => c.Status == "Renewed");
            double renewalRate = (renewed + expired) > 0 ? (double)renewed / (renewed + expired) * 100.0 : 0.0;

            // 4. Contract Turnaround Time (in days)
            double turnaroundTime = 0.0;
            var activeContractsWithTimes = contracts.Where(c => c.ActiveDate.HasValue).ToList();
            if (activeContractsWithTimes.Any())
            {
                turnaroundTime = activeContractsWithTimes.Average(c => (c.ActiveDate.Value - c.PreparationDate).TotalDays);
            }

            // 5. Contractual Legal Disputes
            int disputes = contracts.Count(c => c.IsDisputed);

            // 6. Customer Acquisition Cost (CAC)
            decimal totalAcquisitionCost = leads.Sum(l => l.AcquisitionCost);
            int wonLeads = leads.Count(l => l.Status == "Won");
            decimal cac = wonLeads > 0 ? totalAcquisitionCost / wonLeads : 0m;

            // 7. Contract Value Growth Rate
            // Calculate percentage increase over a baseline of last year's average (e.g. 2,000,000 baseline)
            decimal totalValue = contracts.Sum(c => c.Value);
            decimal baselineValue = 2500000m; 
            double valueGrowth = (double)((totalValue - baselineValue) / baselineValue) * 100.0;

            return new CommercialKpisDto
            {
                CustomerRetentionRateActual = retentionRate,
                CustomerRetentionRateTarget = 85.0,

                NewContractsSecuredActual = newContracts,
                NewContractsSecuredTarget = 5,

                ContractRenewalRateActual = renewalRate,
                ContractRenewalRateTarget = 80.0,

                ContractTurnaroundTimeActual = turnaroundTime,
                ContractTurnaroundTimeTarget = 15.0, // We want low turnaround time, target under 15 days

                ContractualLegalDisputesActual = disputes,
                ContractualLegalDisputesTarget = 1, // Target is under 1 dispute

                CustomerAcquisitionCostActual = cac,
                CustomerAcquisitionCostTarget = 20000m, // Target CAC under 20k

                ContractValueGrowthRateActual = valueGrowth,
                ContractValueGrowthRateTarget = 15.0
            };
        }
        #endregion

        #region Mappers
        private CommercialContractDto MapToContractDto(CommercialContract cc) => new CommercialContractDto
        {
            Id = cc.Id,
            ClientNameEn = cc.ClientNameEn,
            ClientNameAr = cc.ClientNameAr,
            ContractNumber = cc.ContractNumber,
            Value = cc.Value,
            StartDate = cc.StartDate,
            EndDate = cc.EndDate,
            PreparationDate = cc.PreparationDate,
            ActiveDate = cc.ActiveDate,
            Status = cc.Status,
            IsDisputed = cc.IsDisputed
        };

        private CommercialLeadDto MapToLeadDto(CommercialLead cl) => new CommercialLeadDto
        {
            Id = cl.Id,
            LeadName = cl.LeadName,
            Source = cl.Source,
            Status = cl.Status,
            EstimatedValue = cl.EstimatedValue,
            AcquisitionCost = cl.AcquisitionCost,
            CreatedAt = cl.CreatedAt
        };
        #endregion
    }
}
