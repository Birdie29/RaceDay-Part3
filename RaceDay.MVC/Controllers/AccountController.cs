using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult
        Login(string? role)
        {
            role = ValidateRole(role);

            if(role == null)
            {
                return
                RedirectToAction("Index", "Home");
            }

            ViewBag.Role = role;
            return View();
        }

        [HttpGet]
        public IActionResult Register(string? role)
        {
            role = ValidateRole(role);

            if(role == null)
            {
                return
                RedirectToAction("Index", "Home");
            }

            ViewBag.Role = role;
            return View();
        }

        private string?
        ValidateRole(string? role)
        {
            if(string.Equals(role, "Organiser",
            StringComparison.OrdinalIgnoreCase))
            {
                return "Organiser";
            }

            if(string.Equals(role, "Participant",
            StringComparison.OrdinalIgnoreCase))
            {
                return "Participant";
            }

            return null;
        }
    }
}