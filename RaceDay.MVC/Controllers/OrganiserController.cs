using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IApiService _apiService;

        public OrganiserController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser" ||
                string.IsNullOrEmpty(HttpContext.Session.GetString("Email")))
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { role = "Organiser" });
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

            var today = DateTime.Today;
            ViewBag.UserName =
                HttpContext.Session.GetString("UserName") ?? "Organiser";
            ViewBag.Events = events;
            ViewBag.TotalEvents = events.Count;
            ViewBag.TotalEnrolments = 0;
            ViewBag.UpcomingEvents = events.Count(raceEvent =>
                raceEvent.EventDate.Date >= today);
            ViewBag.CompletedEvents = events.Count(raceEvent =>
                raceEvent.EventDate.Date < today);

            return View();
        }
    }
}