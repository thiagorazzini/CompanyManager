using CompanyManager.API.Models;

namespace CompanyManager.API.Models.Responses
{
    public class HealthCheckInfo
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public TimeSpan Uptime { get; set; }
        public string Version { get; set; } = string.Empty;
    }

    public class DetailedHealthCheckInfo
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public TimeSpan Uptime { get; set; }
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, object>? Data { get; set; }
    }

    public class PingResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ReadinessResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object>? Checks { get; set; }
    }

    public class LivenessResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class StartupResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public TimeSpan StartupTime { get; set; }
    }
}
