using SMNETCORE.DataType.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMNETCORE.DAL.TenantDAL.Interface.Models
{
    [Serializable]
    public class TenantRoleDTOModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string[] NameList => Name.SplitGroupsTo<string>(',').ToArray();
    }
}
