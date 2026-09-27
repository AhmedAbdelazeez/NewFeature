using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // The monthly trial balance: one row per account per reporting date per branch. Restricted to
    // the Finance department role (plus Admin/CEO/Administrator, who can see every department).
    [Authorize(Roles = "Admin,CEO,Administrator,FIN")]
    public class FinanceBalancesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
