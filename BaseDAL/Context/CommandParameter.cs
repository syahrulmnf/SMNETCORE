using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DAL.BaseDAL.Context
{
    [Serializable]
    public class CommandParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the direction of the parameter. Default value is Input.
        /// </summary>
        public ParameterDirection Direction { get; set; }

        public CommandParameter(string name, object value)
        {
            Name = name;
            Value = value;
            Direction = ParameterDirection.Input;
        }

        public CommandParameter(string name, object value, ParameterDirection direction)
            : this(name, value)
        {
            Direction = direction;
        }
    }
}
