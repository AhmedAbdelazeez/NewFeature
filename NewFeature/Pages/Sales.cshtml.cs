using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // The Sales landing page: one card per template. Restricted to the Sales department role (plus
    // Admin/CEO/Administrator, who can see every department).
    [Authorize(Roles = "Admin,CEO,Administrator,SALES")]
    public class SalesModel : PageModel
    {
        public void OnGet()
        {
        }
    }

    [Authorize(Roles = "Admin,CEO,Administrator,SALES")]
    public class SalesCustomersModel : PageModel
    {
        public void OnGet()
        {
        }
    }

    [Authorize(Roles = "Admin,CEO,Administrator,SALES")]
    public class SalesFleetModel : PageModel
    {
        public void OnGet()
        {
        }
    }

    [Authorize(Roles = "Admin,CEO,Administrator,SALES")]
    public class SalesOperationsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
