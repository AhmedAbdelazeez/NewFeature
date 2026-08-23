using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    [Authorize(Roles = "Admin,CEO,Administrator,FLEET,ROUTES")]
    public class RoutesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
