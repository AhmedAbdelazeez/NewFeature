using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    [Authorize]
    public class CommercialModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
