using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // Restricted to the Maintenance department role (plus Admin/CEO/Administrator, who can see every
    // department) so a "maint@company.com" login only reaches the Maintenance page.
    [Authorize(Roles = "Admin,CEO,Administrator,MAINT")]
    public class MaintenanceModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
