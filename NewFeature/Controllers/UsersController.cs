using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFeature.Models;

namespace NewFeature.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private bool IsArabic()
        {
            if (Request.Headers.TryGetValue("Accept-Language", out var lang))
            {
                if (lang.ToString().ToLower().Contains("ar")) return true;
            }
            if (Request.Headers.TryGetValue("X-Language", out var xLang))
            {
                if (xLang.ToString().ToLower().Contains("ar")) return true;
            }
            return false;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();
            var isAr = IsArabic();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullNameEn = user.FullNameEn,
                    FullNameAr = user.FullNameAr,
                    IsActive = user.IsActive,
                    Role = roles.FirstOrDefault() ?? "No Role",
                    FullName = isAr ? user.FullNameAr : user.FullNameEn
                });
            }

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var isAr = IsArabic();
            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullNameEn = user.FullNameEn,
                FullNameAr = user.FullNameAr,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? "No Role",
                FullName = isAr ? user.FullNameAr : user.FullNameEn
            });
        }

        [Authorize(Policy = "ManageSettings")]
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail != null) return BadRequest("Email is already in use.");

            var existingUsername = await _userManager.FindByNameAsync(dto.Username);
            if (existingUsername != null) return BadRequest("Username is already in use.");

            if (string.IsNullOrEmpty(dto.Password)) return BadRequest("Password is required for new users.");

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                FullNameEn = dto.FullNameEn,
                FullNameAr = dto.FullNameAr,
                IsActive = dto.IsActive,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            if (!string.IsNullOrEmpty(dto.Role) && await _roleManager.RoleExistsAsync(dto.Role))
            {
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            dto.Id = user.Id;
            dto.Password = null; // Clear password out of security concern
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, dto);
        }

        [Authorize(Policy = "ManageSettings")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = dto.Email;
            user.FullNameEn = dto.FullNameEn;
            user.FullNameAr = dto.FullNameAr;
            user.IsActive = dto.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _userManager.AddToRoleAsync(user, dto.Role);
                }
            }

            // Change password if provided
            if (!string.IsNullOrEmpty(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!passResult.Succeeded)
                {
                    return BadRequest(passResult.Errors.Select(e => e.Description));
                }
            }

            return NoContent();
        }

        [Authorize(Policy = "ManageSettings")]
        [HttpPost("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok(new { isActive = user.IsActive });
        }

        [Authorize(Policy = "ManageSettings")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent self-deletion if logged in
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == id)
            {
                return BadRequest("You cannot delete your own account.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return NoContent();
        }
    }
}
