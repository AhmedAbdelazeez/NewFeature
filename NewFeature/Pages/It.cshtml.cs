using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    [Authorize]
    public class ItModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
