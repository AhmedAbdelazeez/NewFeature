using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // Restricted to the Storage department role (plus Admin/CEO/Administrator, who can see every
    // department) so a "store@company.com" login only reaches the Storage/Warehouse page.
    [Authorize(Roles = "Admin,CEO,Administrator,STORE")]
    public class WarehouseModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
