using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewFeature.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Send single-department logins (e.g. store@company.com, maint@company.com) straight to
            // their own page instead of the general "all departments" hub, so they don't have to click
            // through department cards that aren't theirs. Admin/CEO/Administrator accounts still
            // land on the normal hub since they're meant to see everything.
            bool isExecutive = User.IsInRole("Admin") || User.IsInRole("CEO") || User.IsInRole("Administrator");

            if (!isExecutive)
            {
                if (User.IsInRole("STORE")) return RedirectToPage("/Warehouse");
                if (User.IsInRole("MAINT")) return RedirectToPage("/Maintenance");
                if (User.IsInRole("VEHICLES")) return RedirectToPage("/Vehicles");
                if (User.IsInRole("ROUTES")) return RedirectToPage("/Routes");
                if (User.IsInRole("TRIPS")) return RedirectToPage("/Trips");
                if (User.IsInRole("FLEET")) return RedirectToPage("/Fleet");
                if (User.IsInRole("SALES")) return RedirectToPage("/Sales");
                if (User.IsInRole("FIN")) return RedirectToPage("/Finance");
                if (User.IsInRole("OPS")) return RedirectToPage("/Operations");
            }

            return Page();
        }
    }
}
