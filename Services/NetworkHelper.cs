using Microsoft.Extensions.Primitives; 

namespace poker.net.Services
{
    public class NetworkHelper
    {
        public static string GetIPAddress(HttpContext context)
        {
            if (context == null)
                return "Unknown";

            // Try to get the forwarded header (if behind a proxy or load balancer)
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
            {
                var ip = forwardedFor.ToString().Split(',')[0];
                if (!string.IsNullOrWhiteSpace(ip))
                    return ip;
            }

            // Fallback: use the remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
