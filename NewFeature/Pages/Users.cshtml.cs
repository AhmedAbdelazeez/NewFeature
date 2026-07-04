using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    [Authorize(Policy = "ManageSettings")]
    public class UsersModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
