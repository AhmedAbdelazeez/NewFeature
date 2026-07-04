using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFeature.Models;
using NewFeature.Services.Repositories;

namespace NewFeature.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;

        public ProjectService(IRepository<Project> projectRepository, IRepository<Client> clientRepository)
        {
            _projectRepository = projectRepository;
            _clientRepository = clientRepository;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            var clients = await _clientRepository.GetAllAsync();
            var clientMap = clients.ToDictionary(c => c.Id, c => c.NameEn);

            return projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = clientMap.TryGetValue(p.ClientId, out var name) ? name : "Unknown",
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status
            }).ToList();
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(int id)
        {
            var p = await _projectRepository.GetByIdAsync(id);
            if (p == null) return null;

            var client = await _clientRepository.GetByIdAsync(p.ClientId);

            return new ProjectDto
            {
                Id = p.Id,
                ClientId = p.ClientId,
                ClientName = client?.NameEn ?? "Unknown",
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                DescriptionEn = p.DescriptionEn,
                DescriptionAr = p.DescriptionAr,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status
            };
        }

        public async Task<ProjectDto> CreateProjectAsync(ProjectDto dto)
        {
            var project = new Project
            {
                ClientId = dto.ClientId,
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            dto.Id = project.Id;
            return dto;
        }

        public async Task<bool> UpdateProjectAsync(ProjectDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(dto.Id);
            if (project == null) return false;

            project.ClientId = dto.ClientId;
            project.NameEn = dto.NameEn;
            project.NameAr = dto.NameAr;
            project.DescriptionEn = dto.DescriptionEn;
            project.DescriptionAr = dto.DescriptionAr;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.Status = dto.Status;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null) return false;

            await _projectRepository.DeleteAsync(project);
            await _projectRepository.SaveChangesAsync();
            return true;
        }
    }
}
