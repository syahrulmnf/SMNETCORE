using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.DAL.TenantDAL.Interface.Models
{
    [Serializable]
    public class AddressDTOModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public long CityId { get; set; }
        public long StateId { get; set; }
        public long CountryId { get; set; }
        public string ZipCode { get; set; }
        public bool Deleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PhoneNumber { get; set; }


    }
}
