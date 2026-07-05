using System;

namespace SMNETCORE.DataType.Extensions
{
    public class ConversionResult<T>
    {
        public ConversionResult()
        {
            Success = true;
        }
        /// <summary>
        /// Gets the value indicating whether the conversion was successful.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Gets the converted value.
        /// </summary>
        public T Value { get; set; }
        /// <summary>
        /// Gets an Exception instance, if an error occurred during the operation; otherwise null.
        /// </summary>
        public Exception Error { get; set; }
        /// <summary>
        /// Gets property type.
        /// </summary>
        public Type PropertyType { get; set; }
    }
}
