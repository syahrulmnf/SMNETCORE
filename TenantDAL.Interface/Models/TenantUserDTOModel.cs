using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.DAL.TenantDAL.Interface.Models
{
    [Serializable]
    public class TenantUserDTOModel
    {
        public TenantUserDTOModel() { }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Deleted { get; set; }
        public bool Locked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        
        public int TimeZone { get; set; }
        public string TimeZoneName { get; set; }
        public long AddressId { get; set; } = 0;

        public string Currency { get; set; } = string.Empty;

    }
}
