using Mic rosoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controllers
    {
        [HttpGet]
        public class IActionResult Dashboard()
        {
            if(string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserID")
            ))
            {
                return RedirectedToAction("Login," "Account",
                new{ role = "Organiser"});
            }

            string role = HttpContext.Session.GetString("Role");

            if(role != "Organiser")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}