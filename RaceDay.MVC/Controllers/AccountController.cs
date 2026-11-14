using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login(string? role)
        {
            role = ValidateRole(role);

            if (role == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Role = role;
            return View(new LoginViewModel { Role = role });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var role = ValidateRole(model.Role);

            if (role == null)
            {
                return RedirectToAction("Index", "Home");
            }

            model.Role = role;
            ViewBag.Role = role;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            HttpContext.Session.Clear();
            _apiService.ClearAuthenticationCookie();

            try
            {
                var response = await _apiService.PostAsync<
                    ApiLoginRequest,
                    ApiAccountResponse>(
                        $"api/{role}/login",
                        new ApiLoginRequest
                        {
                            Email = model.Email,
                            Password = model.Password
                        });

                if (response == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The API returned an empty login response.");
                    return View(model);
                }

                EstablishSession(role, model.Email, response);
                return RedirectToAction("Dashboard", role);
            }
            catch (ApiRequestException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Could not connect to the RaceDay API. Check that it is running.");
                return View(model);
            }
            catch (JsonException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The RaceDay API returned an invalid login response.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register(string? role)
        {
            role = ValidateRole(role);

            if (role == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Role = role;
            return View(new RegisterViewModel { Role = role });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var role = ValidateRole(model.Role);

            if (role == null)
            {
                return RedirectToAction("Index", "Home");
            }

            model.Role = role;
            ViewBag.Role = role;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            HttpContext.Session.Clear();
            _apiService.ClearAuthenticationCookie();

            ApiAccountResponse? registration;
            try
            {
                registration = role == "Participant"
                    ? await _apiService.PostAsync<
                        ApiParticipantRegistrationRequest,
                        ApiAccountResponse>(
                            "api/Participant/register",
                            new ApiParticipantRegistrationRequest
                            {
                                Name = model.Name,
                                Surname = model.Surname,
                                Email = model.Email,
                                PhoneNumber = model.PhoneNumber,
                                Password = model.Password
                            })
                    : await _apiService.PostAsync<
                        ApiOrganiserRegistrationRequest,
                        ApiAccountResponse>(
                            "api/Organiser/register",
                            new ApiOrganiserRegistrationRequest
                            {
                                Name = model.Name,
                                Surname = model.Surname,
                                Email = model.Email,
                                PhoneNumber = model.PhoneNumber,
                                Password = model.Password
                            });

                if (registration == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The API returned an empty registration response.");
                    return View(model);
                }
            }
            catch (ApiRequestException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Could not connect to the RaceDay API. Check that it is running.");
                return View(model);
            }
            catch (JsonException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The RaceDay API returned an invalid registration response.");
                return View(model);
            }

            try
            {
                var login = await _apiService.PostAsync<
                    ApiLoginRequest,
                    ApiAccountResponse>(
                        $"api/{role}/login",
                        new ApiLoginRequest
                        {
                            Email = model.Email,
                            Password = model.Password
                        });

                if (login == null)
                {
                    TempData["AccountNotice"] =
                        "Your account was created, but automatic sign-in did not complete. Please log in.";
                    return RedirectToAction(nameof(Login), new { role });
                }

                EstablishSession(role, model.Email, login, registration);
                return RedirectToAction("Dashboard", role);
            }
            catch (ApiRequestException)
            {
                TempData["AccountNotice"] =
                    "Your account was created, but automatic sign-in did not complete. Please log in.";
                return RedirectToAction(nameof(Login), new { role });
            }
            catch (HttpRequestException)
            {
                TempData["AccountNotice"] =
                    "Your account was created, but automatic sign-in did not complete. Please log in.";
                return RedirectToAction(nameof(Login), new { role });
            }
            catch (JsonException)
            {
                TempData["AccountNotice"] =
                    "Your account was created, but the sign-in response was invalid. Please log in.";
                return RedirectToAction(nameof(Login), new { role });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _apiService.ClearAuthenticationCookie();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private void EstablishSession(
            string role,
            string email,
            ApiAccountResponse response,
            ApiAccountResponse? registration = null)
        {
            var userId = response.UserId ?? registration?.UserId;
            var userName = response.UserName ?? registration?.UserName;
            var userSurname = response.UserSurname ?? registration?.UserSurname;

            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("Email", email);

            if (userId.HasValue)
            {
                HttpContext.Session.SetString("UserID", userId.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                HttpContext.Session.SetString("UserName", userName);
            }

            if (!string.IsNullOrWhiteSpace(userSurname))
            {
                HttpContext.Session.SetString("UserSurname", userSurname);
            }
        }

        private static string? ValidateRole(string? role)
        {
            if (string.Equals(
                role,
                "Organiser",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Organiser";
            }

            if (string.Equals(
                role,
                "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Participant";
            }

            return null;
        }
    }
}
