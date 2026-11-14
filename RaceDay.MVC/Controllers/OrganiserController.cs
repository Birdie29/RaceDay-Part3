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
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "dashboard";
            var events = await GetListAsync<RaceEventViewModel>("api/RaceEvent");
            var registrations =
                await GetListAsync<RegistrationViewModel>("api/Registration");
            var today = DateTime.Today;

            ViewBag.UserName =
                HttpContext.Session.GetString("UserName") ?? "Organiser";
            ViewBag.Events = events;
            ViewBag.TotalEvents = events.Count;
            ViewBag.TotalEnrolments = registrations.Count;
            ViewBag.UpcomingEvents = events.Count(raceEvent =>
                raceEvent.EventDate.Date >= today);
            ViewBag.CompletedEvents = events.Count(raceEvent =>
                raceEvent.EventDate.Date < today);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "events";
            return View(await GetListAsync<RaceEventViewModel>("api/RaceEvent"));
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "events";
            return View("EventForm", new RaceEventViewModel
            {
                EventDate = DateTime.Today
            });
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            try
            {
                var raceEvent = await _apiService.GetAsync<RaceEventViewModel>(
                    $"api/RaceEvent/{id}");
                if (raceEvent == null)
                {
                    TempData["WorkspaceError"] = "The event could not be found.";
                    return RedirectToAction(nameof(Events));
                }

                ViewBag.ActiveSection = "events";
                return View("EventForm", raceEvent);
            }
            catch (ApiRequestException exception)
            {
                TempData["WorkspaceError"] = exception.Message;
                return RedirectToAction(nameof(Events));
            }
            catch (HttpRequestException)
            {
                TempData["WorkspaceError"] = ApiUnavailableMessage;
                return RedirectToAction(nameof(Events));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEvent(RaceEventViewModel model)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "events";
            if (!ModelState.IsValid)
            {
                return View("EventForm", model);
            }

            try
            {
                if (model.EventId == 0)
                {
                    await _apiService.PostAsync<RaceEventViewModel, RaceEventViewModel>(
                        "api/RaceEvent",
                        model);
                    TempData["WorkspaceMessage"] = "Event created.";
                }
                else
                {
                    await _apiService.PutAsync<RaceEventViewModel, RaceEventViewModel>(
                        $"api/RaceEvent/{model.EventId}",
                        model);
                    TempData["WorkspaceMessage"] = "Event updated.";
                }

                return RedirectToAction(nameof(Events));
            }
            catch (ApiRequestException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View("EventForm", model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, ApiUnavailableMessage);
                return View("EventForm", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            try
            {
                await _apiService.DeleteAsync($"api/RaceEvent/{id}");
                TempData["WorkspaceMessage"] = "Event deleted.";
            }
            catch (ApiRequestException exception)
            {
                TempData["WorkspaceError"] = exception.Message;
            }
            catch (HttpRequestException)
            {
                TempData["WorkspaceError"] = ApiUnavailableMessage;
            }

            return RedirectToAction(nameof(Events));
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "categories";
            return View(await GetListAsync<CategoryViewModel>("api/Category"));
        }

        [HttpGet]
        public async Task<IActionResult> CreateCategory()
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "categories";
            ViewBag.IsEdit = false;
            await LoadCategoryEventsAsync();
            return View("CategoryForm", new CategoryViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(string id)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            try
            {
                var category = await _apiService.GetAsync<CategoryViewModel>(
                    $"api/Category/{Uri.EscapeDataString(id)}");
                if (category == null)
                {
                    TempData["WorkspaceError"] =
                        "The category could not be found.";
                    return RedirectToAction(nameof(Categories));
                }

                ViewBag.ActiveSection = "categories";
                ViewBag.IsEdit = true;
                await LoadCategoryEventsAsync();
                return View("CategoryForm", category);
            }
            catch (ApiRequestException exception)
            {
                TempData["WorkspaceError"] = exception.Message;
                return RedirectToAction(nameof(Categories));
            }
            catch (HttpRequestException)
            {
                TempData["WorkspaceError"] = ApiUnavailableMessage;
                return RedirectToAction(nameof(Categories));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(
            CategoryViewModel model,
            bool isEdit)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "categories";
            ViewBag.IsEdit = isEdit;
            if (!ModelState.IsValid)
            {
                await LoadCategoryEventsAsync();
                return View("CategoryForm", model);
            }

            try
            {
                if (isEdit)
                {
                    await _apiService.PutAsync<CategoryViewModel, CategoryViewModel>(
                        $"api/Category/{Uri.EscapeDataString(model.CategoryId)}",
                        model);
                    TempData["WorkspaceMessage"] = "Category updated.";
                }
                else
                {
                    await _apiService.PostAsync<CategoryViewModel, CategoryViewModel>(
                        "api/Category",
                        model);
                    TempData["WorkspaceMessage"] = "Category created.";
                }

                return RedirectToAction(nameof(Categories));
            }
            catch (ApiRequestException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                await LoadCategoryEventsAsync();
                return View("CategoryForm", model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, ApiUnavailableMessage);
                await LoadCategoryEventsAsync();
                return View("CategoryForm", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            try
            {
                await _apiService.DeleteAsync(
                    $"api/Category/{Uri.EscapeDataString(id)}");
                TempData["WorkspaceMessage"] = "Category deleted.";
            }
            catch (ApiRequestException exception)
            {
                TempData["WorkspaceError"] = exception.Message;
            }
            catch (HttpRequestException)
            {
                TempData["WorkspaceError"] = ApiUnavailableMessage;
            }

            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> Registrations()
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "registrations";
            return View(await GetListAsync<RegistrationViewModel>(
                "api/Registration"));
        }

        [HttpGet]
        public async Task<IActionResult> Results()
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "results";
            return View(await GetListAsync<ResultViewModel>("api/Result"));
        }

        [HttpGet]
        public async Task<IActionResult> CreateResult()
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "results";
            await LoadResultRegistrationsAsync();
            return View("ResultForm", new ResultViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> EditResult(int id)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            try
            {
                var result = await _apiService.GetAsync<ResultViewModel>(
                    $"api/Result/{id}");
                if (result == null)
                {
                    TempData["WorkspaceError"] = "The result could not be found.";
                    return RedirectToAction(nameof(Results));
                }

                ViewBag.ActiveSection = "results";
                await LoadResultRegistrationsAsync();
                return View("ResultForm", result);
            }
            catch (ApiRequestException exception)
            {
                TempData["WorkspaceError"] = exception.Message;
                return RedirectToAction(nameof(Results));
            }
            catch (HttpRequestException)
            {
                TempData["WorkspaceError"] = ApiUnavailableMessage;
                return RedirectToAction(nameof(Results));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(
            ResultViewModel model,
            bool isEdit)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "results";
            if (!ModelState.IsValid)
            {
                await LoadResultRegistrationsAsync();
                return View("ResultForm", model);
            }

            try
            {
                if (isEdit)
                {
                    await _apiService.PutAsync<ResultViewModel, ResultViewModel>(
                        $"api/Result/{model.ResultId}",
                        model);
                    TempData["WorkspaceMessage"] = "Result updated.";
                }
                else
                {
                    await _apiService.PostAsync<ResultViewModel, ResultViewModel>(
                        "api/Result",
                        model);
                    TempData["WorkspaceMessage"] = "Result created.";
                }

                return RedirectToAction(nameof(Results));
            }
            catch (ApiRequestException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                await LoadResultRegistrationsAsync();
                return View("ResultForm", model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, ApiUnavailableMessage);
                await LoadResultRegistrationsAsync();
                return View("ResultForm", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            var organiserId = GetOrganiserId();
            try
            {
                var profile = await _apiService.GetAsync<OrganiserProfileViewModel>(
                    $"api/Organiser/{organiserId}");
                if (profile == null)
                {
                    TempData["WorkspaceError"] = "The organiser profile was not found.";
                    return RedirectToAction(nameof(Dashboard));
                }

                ViewBag.ActiveSection = "profile";
                return View(profile);
            }
            catch (ApiRequestException exception)
            {
                TempData["WorkspaceError"] = exception.Message;
                return RedirectToAction(nameof(Dashboard));
            }
            catch (HttpRequestException)
            {
                TempData["WorkspaceError"] = ApiUnavailableMessage;
                return RedirectToAction(nameof(Dashboard));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(OrganiserProfileViewModel model)
        {
            var accessResult = RequireOrganiser();
            if (accessResult != null)
            {
                return accessResult;
            }

            model.OrganiserId = GetOrganiserId();
            model.NewPassword = string.IsNullOrWhiteSpace(model.NewPassword)
                ? null
                : model.NewPassword;
            ViewBag.ActiveSection = "profile";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _apiService.PutAsync<
                    OrganiserProfileViewModel,
                    OrganiserProfileViewModel>(
                        $"api/Organiser/{model.OrganiserId}",
                        model);
                HttpContext.Session.SetString("Email", model.Email);
                HttpContext.Session.SetString("UserName", model.Name);
                HttpContext.Session.SetString("UserSurname", model.Surname);
                TempData["WorkspaceMessage"] = "Profile updated.";
                return RedirectToAction(nameof(Profile));
            }
            catch (ApiRequestException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, ApiUnavailableMessage);
                return View(model);
            }
        }

        private const string ApiUnavailableMessage =
            "Could not connect to the RaceDay API. Check that it is running.";

        private IActionResult? RequireOrganiser()
        {
            if (HttpContext.Session.GetString("Role") == "Organiser" &&
                int.TryParse(
                    HttpContext.Session.GetString("UserID"),
                    out _))
            {
                return null;
            }

            return RedirectToAction(
                "Login",
                "Account",
                new { role = "Organiser" });
        }

        private int GetOrganiserId()
        {
            return int.Parse(HttpContext.Session.GetString("UserID")!);
        }

        private async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            try
            {
                return await _apiService.GetAsync<List<T>>(endpoint) ?? [];
            }
            catch (ApiRequestException exception)
            {
                ViewBag.ApiError = exception.Message;
            }
            catch (HttpRequestException)
            {
                ViewBag.ApiError = ApiUnavailableMessage;
            }

            return [];
        }

        private async Task LoadCategoryEventsAsync()
        {
            ViewBag.Events = await GetListAsync<RaceEventViewModel>(
                "api/RaceEvent");
        }

        private async Task LoadResultRegistrationsAsync()
        {
            ViewBag.Registrations = await GetListAsync<RegistrationViewModel>(
                "api/Registration");
        }
    }
}
