using System.Net;

namespace RaceDay.MVC.Services
{
    public sealed class ApiRequestException : Exception
    {
        public ApiRequestException(
            HttpStatusCode statusCode,
            string responseBody)
            : base(CreateMessage(statusCode, responseBody))
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public HttpStatusCode StatusCode { get; }

        public string ResponseBody { get; }

        private static string CreateMessage(
            HttpStatusCode statusCode,
            string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return $"The RaceDay API returned {(int)statusCode} ({statusCode}).";
            }

            return $"The RaceDay API returned {(int)statusCode}: {responseBody}";
        }
    }
}
