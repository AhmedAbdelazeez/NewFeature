using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // The chart of accounts: reference data the monthly figures are classified through. Restricted
    // to the Finance department role (plus Admin/CEO/Administrator, who can see every department).
    [Authorize(Roles = "Admin,CEO,Administrator,FIN")]
    public class FinanceAccountsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
