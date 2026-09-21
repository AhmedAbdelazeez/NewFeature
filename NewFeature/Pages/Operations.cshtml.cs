using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // Restricted to the Operations department role (plus Admin/CEO/Administrator, who can see
    // every department) so an "ops@rawahil.com.sa" login only reaches the Operations page.
    [Authorize(Roles = "Admin,CEO,Administrator,OPS")]
    public class OperationsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
