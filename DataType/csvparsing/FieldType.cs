namespace SMNETCORE.DataType.CsvParsing
{
    /// <summary>
    ///   Indicates whether text fields are delimited or fixed width.
    /// </summary>
    public enum FieldType
    {
        /// <summary>
        ///   Indicates that the fields are delimited.
        /// </summary>
        Delimited = 0,

        /// <summary>
        ///   Indicates that the fields are fixed width.
        /// </summary>
        FixedWidth = 1,
    }
}