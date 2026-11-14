using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Services
{
    public class ApiService : IApiService
    {
        public const string AuthenticationCookieSessionKey =
            "RaceDayApiAuthenticationCookie";

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            using var request = CreateRequest(HttpMethod.Get, endpoint);
            using var response = await SendAsync(request);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data)
        {
            using var request = CreateRequest(HttpMethod.Post, endpoint);
            request.Content = JsonContent.Create(data);
            using var response = await SendAsync(request);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data)
        {
            using var request = CreateRequest(HttpMethod.Put, endpoint);
            request.Content = JsonContent.Create(data);
            using var response = await SendAsync(request);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            using var request = CreateRequest(HttpMethod.Delete, endpoint);
            using var response = await SendAsync(request);
            return true;
        }

        public void ClearAuthenticationCookie()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(
                AuthenticationCookieSessionKey);
        }

        private HttpRequestMessage CreateRequest(
            HttpMethod method,
            string endpoint)
        {
            var request = new HttpRequestMessage(method, endpoint);
            var cookie = _httpContextAccessor.HttpContext?.Session.GetString(
                AuthenticationCookieSessionKey);

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers.TryAddWithoutValidation("Cookie", cookie);
            }

            return request;
        }

        private async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var exception = new ApiRequestException(
                    response.StatusCode,
                    responseBody);
                response.Dispose();
                throw exception;
            }

            SaveAuthenticationCookie(response);
            return response;
        }

        private void SaveAuthenticationCookie(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                return;
            }

            var authCookie = cookies
                .Select(cookie => cookie.Split(';', 2)[0])
                .FirstOrDefault(cookie =>
                    cookie.StartsWith(
                        ".AspNetCore.Cookies=",
                        StringComparison.Ordinal));

            if (!string.IsNullOrWhiteSpace(authCookie))
            {
                _httpContextAccessor.HttpContext?.Session.SetString(
                    AuthenticationCookieSessionKey,
                    authCookie);
            }
        }
    }
}