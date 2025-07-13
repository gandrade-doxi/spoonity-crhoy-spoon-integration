using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRHoyWebhooks.Models
{
    public class RewardLog
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? SessionKey { get; set; }
        public string Token { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
