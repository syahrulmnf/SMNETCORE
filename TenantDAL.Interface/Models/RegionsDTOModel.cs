using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SMNETCORE.DAL.TenantDAL.Interface.Models
{
    public enum RegionType
    {
        Country = 1,
        State = 2,
        City = 3
    }
    [Serializable]
    public class RegionsDTOModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ZipCode { get; set; }

        public long? RegionId { get; set; }
        public long? CountryId { get; set; }
        public int RegionType { get; set; }


    }
}
