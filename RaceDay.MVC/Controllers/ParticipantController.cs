using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private const string ApiUnavailableMessage =
            "Could not connect to the RaceDay API. Check that it is running.";

        private readonly IApiService _apiService;

        public ParticipantController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var accessResult = RequireParticipant();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "dashboard";
            var events = await GetListAsync<RaceEventViewModel>("api/RaceEvent");
            var categories =
                await GetListAsync<CategoryViewModel>("api/Category");
            var registrations =
                await GetListAsync<RegistrationViewModel>("api/Registration");
            var registrationSummaries = CreateRegistrationSummaries(
                registrations,
                events,
                categories);
            var today = DateTime.Today;

            ViewBag.UserName =
                HttpContext.Session.GetString("UserName") ?? "Runner";
            ViewBag.TotalRegistrations = registrations.Count;
            ViewBag.UpcomingRegistrations = registrationSummaries.Count(
                registration => registration.EventDate.Date >= today);
            ViewBag.CompletedRegistrations = registrationSummaries.Count(
                registration => registration.EventDate.Date < today);
            ViewBag.AvailableEvents = events.Count(
                raceEvent => raceEvent.EventDate.Date >= today);
            ViewBag.RecentRegistrations = registrationSummaries
                .OrderByDescending(registration => registration.RegistrationDate)
                .Take(5)
                .ToList();

            return View("participant", events);
        }

        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var accessResult = RequireParticipant();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "events";
            return View(await GetListAsync<RaceEventViewModel>("api/RaceEvent"));
        }

        [HttpGet]
        public async Task<IActionResult> EventDetails(int id)
        {
            var accessResult = RequireParticipant();
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

                var categories = await _apiService.GetAsync<List<CategoryViewModel>>(
                    "api/Category") ?? [];

                ViewBag.ActiveSection = "events";
                return View(new ParticipantEventDetailsViewModel
                {
                    Event = raceEvent,
                    Categories = categories
                        .Where(category => category.EventId == raceEvent.EventId)
                        .ToList()
                });
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

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var accessResult = RequireParticipant();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "categories";
            var categories =
                await GetListAsync<CategoryViewModel>("api/Category");
            var events =
                await GetListAsync<RaceEventViewModel>("api/RaceEvent");
            var eventsById = events.ToDictionary(raceEvent => raceEvent.EventId);

            return View(categories.Select(category =>
                new ParticipantCategoryListItemViewModel
                {
                    Category = category,
                    Event = eventsById.GetValueOrDefault(category.EventId)
                }).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> CreateRegistration(
            int eventId,
            string categoryId)
        {
            var accessResult = RequireParticipant();
            if (accessResult != null)
            {
                return accessResult;
            }

            if (eventId <= 0 || string.IsNullOrWhiteSpace(categoryId))
            {
                TempData["WorkspaceError"] =
                    "Choose an event category before registering.";
                return RedirectToAction(nameof(Categories));
            }

            try
            {
                var raceEvent = await _apiService.GetAsync<RaceEventViewModel>(
                    $"api/RaceEvent/{eventId}");
                var category = await _apiService.GetAsync<CategoryViewModel>(
                    $"api/Category/{Uri.EscapeDataString(categoryId)}");

                if (raceEvent == null || category == null ||
                    category.EventId != eventId)
                {
                    TempData["WorkspaceError"] =
                        "The selected event category could not be found.";
                    return RedirectToAction(nameof(Categories));
                }

                ViewBag.ActiveSection = "registrations";
                return View(new ParticipantRegistrationFormViewModel
                {
                    EventId = eventId,
                    CategoryId = category.CategoryId,
                    EventName = raceEvent.EventName,
                    CategoryName = category.CategoryName,
                    EntryFee = category.EntryFee
                });
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
        public async Task<IActionResult> CreateRegistration(
            ParticipantRegistrationFormViewModel model)
        {
            var accessResult = RequireParticipant();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "registrations";
            if (!ModelState.IsValid)
            {
                await LoadRegistrationFormDetailsAsync(model);
                return View(model);
            }

            try
            {
                await _apiService.PostAsync<
                    ParticipantCreateRegistrationRequest,
                    RegistrationViewModel>(
                        "api/Registration",
                        new ParticipantCreateRegistrationRequest
                        {
                            EventId = model.EventId,
                            CategoryId = model.CategoryId
                        });
                TempData["WorkspaceMessage"] = "Registration created.";
                return RedirectToAction(nameof(Registrations));
            }
            catch (ApiRequestException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                await LoadRegistrationFormDetailsAsync(model);
                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, ApiUnavailableMessage);
                await LoadRegistrationFormDetailsAsync(model);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Registrations()
        {
            var accessResult = RequireParticipant();
            if (accessResult != null)
            {
                return accessResult;
            }

            ViewBag.ActiveSection = "registrations";
            var registrations =
                await GetListAsync<RegistrationViewModel>("api/Registration");
            var events =
                await GetListAsync<RaceEventViewModel>("api/RaceEvent");
            var categories =
                await GetListAsync<CategoryViewModel>("api/Category");

            return View(CreateRegistrationSummaries(
                registrations,
                events,
                categories)
                .OrderByDescending(registration => registration.RegistrationDate)
                .ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var accessResult = RequireParticipant();
            if (accessResult != null)
            {
                return accessResult;
            }

            try
            {
                var profile = await _apiService.GetAsync<ParticipantProfileViewModel>(
                    $"api/Participant/{GetParticipantId()}");
                if (profile == null)
                {
                    TempData["WorkspaceError"] =
                        "The participant profile could not be found.";
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
        public async Task<IActionResult> Profile(ParticipantProfileViewModel model)
        {
            var accessResult = RequireParticipant();
            if (accessResult != null)
            {
                return accessResult;
            }

            model.ParticipantId = GetParticipantId();
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
                    ParticipantProfileUpdateRequest,
                    ParticipantProfileViewModel>(
                        $"api/Participant/{model.ParticipantId}",
                        new ParticipantProfileUpdateRequest
                        {
                            Name = model.Name,
                            Surname = model.Surname,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber,
                            NewPassword = model.NewPassword
                        });

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

        private IActionResult? RequireParticipant()
        {
            if (HttpContext.Session.GetString("Role") == "Participant" &&
                int.TryParse(
                    HttpContext.Session.GetString("UserID"),
                    out _))
            {
                return null;
            }

            return RedirectToAction(
                "Login",
                "Account",
                new { role = "Participant" });
        }

        private int GetParticipantId()
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

        private static List<ParticipantRegistrationSummaryViewModel>
            CreateRegistrationSummaries(
                IEnumerable<RegistrationViewModel> registrations,
                IEnumerable<RaceEventViewModel> events,
                IEnumerable<CategoryViewModel> categories)
        {
            var eventsById = events.ToDictionary(raceEvent => raceEvent.EventId);
            var categoriesById = categories.ToDictionary(
                category => category.CategoryId,
                StringComparer.Ordinal);

            return registrations.Select(registration =>
            {
                eventsById.TryGetValue(registration.EventId, out var raceEvent);
                categoriesById.TryGetValue(registration.CategoryId, out var category);

                return new ParticipantRegistrationSummaryViewModel
                {
                    RegistrationId = registration.RegistrationId,
                    RegistrationDate = registration.RegistrationDate,
                    RegistrationChannel = registration.RegistrationChannel,
                    EventName = raceEvent?.EventName ?? "Event unavailable",
                    EventDate = raceEvent?.EventDate ?? default,
                    CategoryName = category?.CategoryName ?? "Category unavailable"
                };
            }).ToList();
        }

        private async Task LoadRegistrationFormDetailsAsync(
            ParticipantRegistrationFormViewModel model)
        {
            try
            {
                var raceEvent = await _apiService.GetAsync<RaceEventViewModel>(
                    $"api/RaceEvent/{model.EventId}");
                var category = await _apiService.GetAsync<CategoryViewModel>(
                    $"api/Category/{Uri.EscapeDataString(model.CategoryId)}");

                model.EventName = raceEvent?.EventName ?? string.Empty;
                model.CategoryName = category?.CategoryName ?? string.Empty;
                model.EntryFee = category?.EntryFee ?? 0;
            }
            catch (ApiRequestException exception)
            {
                ViewBag.ApiError = exception.Message;
            }
            catch (HttpRequestException)
            {
                ViewBag.ApiError = ApiUnavailableMessage;
            }
        }
    }
}
