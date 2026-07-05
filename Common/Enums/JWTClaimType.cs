using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace SMNETCORE.Common.Enums
{
    public class JWTClaimType
    {
        public JWTClaimType() { }
        public const string UserId = "userid";
        public const string UserName = "username";
        public const string Email = "email";
        public const string Sub = "sub";
        public const string Tenant = "tenant";
        public const string Role = "role";
        public const string FirstName = "firstname";
        public const string LastName = "lastname";
        public const string TimeZone = "timezone";
        public const string TimeZoneName = "timezonename";
        public const string AddressId = "addressid";
        public const string Currency = "currency";
    }
}
