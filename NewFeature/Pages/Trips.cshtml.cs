using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    [Authorize(Roles = "Admin,CEO,Administrator,FLEET,TRIPS")]
    public class TripsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
