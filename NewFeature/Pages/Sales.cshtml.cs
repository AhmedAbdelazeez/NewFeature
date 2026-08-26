using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // Restricted to the Sales department role (plus Admin/CEO/Administrator, who can see every
    // department), mirroring WarehouseModel/MaintenanceModel so a "sales@company.com" login only
    // reaches the Sales page.
    [Authorize(Roles = "Admin,CEO,Administrator,SALES")]
    public class SalesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
