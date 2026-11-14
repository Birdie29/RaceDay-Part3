using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

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
            if (HttpContext.Session.GetString("Role") != "Participant" ||
                string.IsNullOrEmpty(HttpContext.Session.GetString("Email")))
            {
                return RedirectToAction("Login", "Account",
                    new { role = "Participant" });
            }

            var events = new List<RaceEventViewModel>();
            try
            {
                events = await _apiService.GetAsync<List<RaceEventViewModel>>(
                    "api/RaceEvent"
                ) ?? [];
            }
            catch (ApiRequestException exception)
            {
                ViewBag.ApiError = exception.Message;
            }
            catch (HttpRequestException)
            {
                ViewBag.ApiError =
                    "Could not connect to the RaceDay API. Check that it is running.";
            }

            ViewBag.UserName =
                HttpContext.Session.GetString("UserName") ?? "Runner";

            return View("participant", events);
        }
    }
}