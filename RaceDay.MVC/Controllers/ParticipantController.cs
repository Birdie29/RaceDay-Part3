using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDay.MVC.Services;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IApiService _apiService;

        public ParticipantController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
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

            var events = await _apiService.GetAsync<List<JsonElement>>(
                "api/RaceEvent/GetEvents"
            );

            return View("participant", events);
        }
    }
}