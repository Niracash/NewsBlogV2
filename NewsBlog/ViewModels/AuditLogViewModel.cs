using System;

namespace NewsBlog.ViewModels
{
    public class AuditLogViewModel
    {
        public string? UserName { get; set; }
        public string? Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }
}
