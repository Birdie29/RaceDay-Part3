using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserID")
            ))
            {
                return RedirectToAction("Login", "Account",
                    new { role = "Organiser" });
            }

            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organiser")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}