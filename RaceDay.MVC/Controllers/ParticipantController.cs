using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserID")
            ))
            {
                return RedirectToAction("Login", "Account",
                    new { role = "Participant" });
            }

            string? role = HttpContext.Session.GetString("Role");

            if (role != "Participant")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}