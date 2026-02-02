using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace HMS.Services
{
    public static class FingerprintService
    {
        public static string Generate(HttpRequest request)
        {
            var userAgent = request.Headers["User-Agent"].ToString();
            var ipAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString();
            var acceptLanguage = request.Headers["Accept-Language"].ToString();

            // Combine relevant request details
            var rawFingerprint = $"{userAgent}-{ipAddress}-{acceptLanguage}";

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawFingerprint));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
