using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    // FLEET can still see this (general fleet oversight), plus its own dedicated VEHICLES role.
    [Authorize(Roles = "Admin,CEO,Administrator,FLEET,VEHICLES")]
    public class VehiclesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
