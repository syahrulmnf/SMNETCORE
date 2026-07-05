using SMNETCORE.DAL.BaseDAL.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.DAL.BaseDAL.Models
{
    [Serializable]
    public class TenantDTOModel:DalContextBaseModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Secrets { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public TenantDTOModel() { }
    }
}
