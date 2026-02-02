using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.Controllers
{
    [Authorize(AuthenticationSchemes = "AccessTokenCookie")] // Protects all actions in this controller
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // You can access user claims here, e.g., User.FindFirst(ClaimTypes.Email)?.Value
            return View("Index");
        }
    }
}
