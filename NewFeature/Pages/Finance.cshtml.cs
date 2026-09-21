using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // Restricted to the Finance department role (plus Admin/CEO/Administrator, who can see every
    // department) so a "fin@rawahil.com.sa" login only reaches the Finance page.
    [Authorize(Roles = "Admin,CEO,Administrator,FIN")]
    public class FinanceModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
