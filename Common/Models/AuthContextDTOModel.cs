using SMNETCORE.Common.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SMNETCORE.Common.Models
{
    [Serializable]
    public class AuthContextDTOModel
    {
        public string BearerToken { get; set; } = string.Empty;
        
        public AuthContextDTOModel() { }
        public string sub { get; set; } = string.Empty;
        public string tenant { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public string TimeZoneName { get; set; } = string.Empty;
        public string AddressId { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}
