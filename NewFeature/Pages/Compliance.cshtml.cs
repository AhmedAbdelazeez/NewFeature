using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace NewFeature.Pages
{
    [Authorize]
    public class ComplianceModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
