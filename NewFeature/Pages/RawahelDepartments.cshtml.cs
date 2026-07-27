using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    [AllowAnonymous]
    public class RawahelDepartmentsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
