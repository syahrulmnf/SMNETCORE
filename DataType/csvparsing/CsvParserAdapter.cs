#region Using Directives

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;

#endregion Using Directives

namespace SMNETCORE.DataType.CsvParsing
{
    /// <summary>
    ///   The <see cref="CsvParserAdapter"/> is used to modify the <see cref="CsvParser"/>
    ///   to allow it parse a file and place them into various formats.
    /// </summary>
    /// <threadsafety static="false" instance="false"/>
    public class CsvParserAdapter : CsvParser
    {
        #region Constants

        /// <summary>
        ///   Defines the default value for including the file line number (false).
        /// </summary>
        public const bool DefaultIncludeFileLineNumber = false;

        /// <summary>
        ///   Defines the number of skip ending data rows (0).
        /// </summary>
        public const int DefaultSkipEndingDataRows = 0;

        private const string XML_INCLUDE_FILE_LINE_NUMBER = "IncludeFileLineNumber";
        private const string XML_SKIP_ENDING_DATA_ROWS = "SkipEndingDataRows";
        private const string FILE_LINE_NUMBER = "FileLineNumber";

        #endregion Constants

        #region Static Code

        /// <summary>
        ///   Adds a column name to the given <see cref="DataTable"/>, such that
        ///   it ensures a unique column name.
        /// </summary>
        /// <param name="dtData">The <see cref="DataTable"/> to add the column to.</param>
        /// <param name="strColumnName">The desired column name to add.</param>
        private static void AddColumnToTable(DataTable dtData, string strColumnName)
        {
            if (strColumnName != null)
            {
                if (dtData.Columns[strColumnName] == null)
                {
                    dtData.Columns.Add(strColumnName);
                    return;
                }

                // Below code will make question duplicating
                //else
                //{
                //    string strNewColumnName;
                //    int intCount = 0;

                //    // Looks like we need to generate a new column name.
                //    do
                //    {
                //        strNewColumnName = string.Format("{0}{1}", strColumnName, ++intCount);
                //    } while (dtData.Columns[strNewColumnName] != null);

                //    dtData.Columns.Add(strNewColumnName);
                //}
            }

            dtData.Columns.Add();
        }

        #endregion Static Code

        #region Constructors

        /// <summary>
        ///   Constructs an instance of a <see cref="CsvParserAdapter"/>
        ///   with the default settings.
        /// </summary>
        /// <remarks>
        ///   When using this constructor, the datasource must be set prior to using the parser
        ///   (using <see cref="CsvParser.SetDataSource(string)"/>), otherwise an exception will be thrown.
        /// </remarks>
        public CsvParserAdapter()
        {
            IncludeFileLineNumber = DefaultIncludeFileLineNumber;
            SkipEndingDataRows = DefaultSkipEndingDataRows;
        }

        /// <summary>
        ///   Constructs an instance of a <see cref="CsvParserAdapter"/> and sets
        ///   the initial datasource as the file referenced by the string passed in.
        /// </summary>
        /// <param name="strFileName">The file name to set as the initial datasource.</param>
        public CsvParserAdapter(string strFileName)
            : this()
        {
            SetDataSource(strFileName);
        }

        /// <summary>
        ///   Constructs an instance of a <see cref="CsvParserAdapter"/> and sets
        ///   the initial datasource as the file referenced by the string passed in with
        ///   the provided encoding.
        /// </summary>
        /// <param name="strFileName">The file name to set as the initial datasource.</param>
        /// <param name="encoding">The <see cref="Encoding"/> of the file being referenced.</param>
        public CsvParserAdapter(string strFileName, Encoding encoding)
            : this()
        {
            SetDataSource(strFileName, encoding);
        }

        /// <summary>
        ///   Constructs an instance of a <see cref="CsvParserAdapter"/> and sets
        ///   the initial datasource as the <see cref="TextReader"/> passed in.
        /// </summary>
        /// <param name="txtReader">
        ///   The <see cref="TextReader"/> containing the data to be parsed.
        /// </param>
        public CsvParserAdapter(TextReader txtReader)
            : this()
        {
            SetDataSource(txtReader);
        }

        #endregion Constructors

        #region Public Code

        /// <summary>
        ///   Gets or sets whether or not the <see cref="CsvParser.FileRowNumber"/> from where
        ///   the data was retrieved should be included as part of the result set.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Default: <see langword="false"/> 
        ///   </para>
        ///   <para>
        ///     If parsing has started, this value cannot be updated.
        ///   </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Attempting to modify the configuration, while parsing.</exception>
        public bool IncludeFileLineNumber
        {
            get { return m_blnIncludeFileLineNumber; }
            set
            {
                if (m_ParserState == ParserState.Parsing)
                    throw new InvalidOperationException("Parsing has already begun, close the existing parse first.");

                m_blnIncludeFileLineNumber = value;
            }
        }

        /// <summary>
        ///   Gets or sets the number of rows of data to ignore at the end of the file.
        /// </summary>
        /// <value>The number of data rows to skip at the end of the datasource</value>
        /// <remarks>
        ///   <para>
        ///     A value of zero will ensure no rows are ignored.
        ///   </para>
        ///   <para>
        ///     Default: 0 
        ///   </para>
        ///   <para>
        ///     If parsing has started, this value cannot be updated.
        ///   </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Attempting to modify the configuration, while parsing.</exception>
        public int SkipEndingDataRows
        {
            get { return m_intSkipEndingDataRows; }
            set
            {
                if (m_ParserState == ParserState.Parsing)
                    throw new InvalidOperationException("Parsing has already begun, close the existing parse first.");

                m_intSkipEndingDataRows = value;

                if (m_intSkipEndingDataRows < 0)
                    m_intSkipEndingDataRows = 0;
            }
        }

        /// <summary>
        ///   Generates an <see cref="XmlDocument"/> based on the data stored within
        ///   the entire data source after it was parsed.
        /// </summary>
        /// <returns>
        ///   The <see cref="XmlDocument"/> containing all of the data in the data
        ///   source.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   Attempting to read without properly setting up the <see cref="CsvParserAdapter"/>.
        /// </exception>
        /// <exception cref="ParsingException">
        ///   Thrown in the situations where the <see cref="CsvParserAdapter"/> cannot continue
        ///   due to a conflict between the setup and the data being parsed.
        /// </exception>
        /// <example>
        ///   <code lang="C#" escaped="true">
        ///     using (CsvParserAdapter p = new CsvParserAdapter(@"C:\MyData.txt"))
        ///       XmlDocument xmlDoc = p.GetXml();
        ///   </code>
        /// </example>
        public XmlDocument GetXml()
        {
            DataSet dsData;
            XmlDocument xmlDocument = null;

            dsData = GetDataSet();

            if (dsData != null)
            {
                xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(dsData.GetXml());
            }

            return xmlDocument;
        }

        /// <summary>
        ///   Generates a <see cref="DataSet"/> based on the data stored within
        ///   the entire data source after it was parsed.
        /// </summary>
        /// <returns>
        ///   The <see cref="DataSet"/> containing all of the data in the
        ///   data source.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   Attempting to read without properly setting up the <see cref="CsvParserAdapter"/>.
        /// </exception>
        /// <exception cref="ParsingException">
        ///   Thrown in the situations where the <see cref="CsvParserAdapter"/> cannot continue
        ///   due to a conflict between the setup and the data being parsed.
        /// </exception>
        /// <example>
        ///   <code lang="C#" escaped="true">
        ///     using (CsvParserAdapter p = new CsvParserAdapter(@"C:\MyData.txt"))
        ///       DataSet dsResults = p.GetDataSet();
        ///   </code>
        /// </example>
        public DataSet GetDataSet()
        {
            DataTable dtData;
            DataSet dsData = null;

            dtData = GetDataTable();

            if (dtData != null)
            {
                dsData = new DataSet();
                dsData.Tables.Add(dtData);
            }

            return dsData;
        }

        /// <summary>
        ///   Generates a <see cref="DataTable"/> based on the data stored within
        ///   the entire data source after it was parsed.
        /// </summary>
        /// <returns>
        ///   The <see cref="DataTable"/> containing all of the data in the data
        ///   source.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   Attempting to read without properly setting up the <see cref="CsvParserAdapter"/>.
        /// </exception>
        /// <exception cref="ParsingException">
        ///   Thrown in the situations where the <see cref="CsvParserAdapter"/> cannot continue
        ///   due to a conflict between the setup and the data being parsed.
        /// </exception>
        /// <example>
        ///   <code lang="C#" escaped="true">
        ///     using (CsvParserAdapter p = new CsvParserAdapter(@"C:\MyData.txt"))
        ///       DataTable dtResults = p.GetDataTable();
        ///   </code>
        /// </example>
        public DataTable GetDataTable()
        {
            DataRow drRow;
            DataTable dtData;
            int intCreatedColumns, intSkipRowsAtEnd;

            dtData = new DataTable();
            dtData.BeginLoadData();

            intCreatedColumns = 0;

            while (Read())
            {
                // See if we have the appropriate number of columns.
                if (m_lstColumnNames.Count > intCreatedColumns)
                {
                    // Add in our column to store off the file line number.
                    if (m_blnIncludeFileLineNumber && (intCreatedColumns < 1))
                        dtData.Columns.Add(FILE_LINE_NUMBER);

                    for (int intColumnIndex = intCreatedColumns;
                         intColumnIndex < m_lstColumnNames.Count;
                         ++intColumnIndex, ++intCreatedColumns)
                        AddColumnToTable(dtData, m_lstColumnNames[intColumnIndex]);
                }

                if (!IsCurrentRowEmpty || !SkipEmptyRows)
                {
                    drRow = dtData.NewRow();

                    if (m_blnIncludeFileLineNumber)
                    {
                        drRow[0] = FileRowNumber;

                        // Now, add in the data retrieved from the current row.
                        for (int intColumnIndex = 0; intColumnIndex < m_lstData.Count; ++intColumnIndex)
                            drRow[intColumnIndex + 1] = m_lstData[intColumnIndex];
                    }
                    else
                    {
                        // Since we don't have to account for the row number, just place the value right into the data row.
                        drRow.ItemArray = m_lstData.ToArray();
                    }

                    dtData.Rows.Add(drRow);
                }
            }

            intSkipRowsAtEnd = m_intSkipEndingDataRows;

            // Remove any rows at the end that need to be skipped.
            while ((intSkipRowsAtEnd-- > 0) && (dtData.Rows.Count > 0))
                dtData.Rows.RemoveAt(dtData.Rows.Count - 1);

            dtData.EndLoadData();

            return dtData;
        }

        /// <summary>
        ///   Loads the base <see cref="CsvParser"/> class from the
        ///   <see cref="XmlDocument"/> and then retrieves additional information
        ///    from the Xml that is specific to the <see cref="CsvParserAdapter"/>.
        /// </summary>
        /// <param name="xmlConfig">
        ///   The <see cref="XmlDocument"/> containing the configuration information.
        /// </param>
        /// <exception cref="ArgumentException">In the event that the XmlConfig file contains a value that is invalid,
        /// an <see cref="ArgumentException"/> could be thrown.</exception>
        /// <exception cref="ArgumentNullException">In the event that the XmlConfig file contains a value that is invalid,
        /// an <see cref="ArgumentNullException"/> could be thrown.</exception>
        /// <exception cref="ArgumentOutOfRangeException">In the event that the XmlConfig file contains a value that is invalid,
        /// an <see cref="ArgumentOutOfRangeException"/> could be thrown.</exception>
        /// <exception cref="InvalidOperationException">Attempting to modify the configuration, while parsing.</exception>
        public override void Load(XmlDocument xmlConfig)
        {
            XmlElement xmlElement;

            // Load the base information for the CsvParser.
            base.Load(xmlConfig);

            // Initialize the value for the file line number.
            m_blnIncludeFileLineNumber = DefaultIncludeFileLineNumber;
            m_intSkipEndingDataRows = DefaultSkipEndingDataRows;

            /////////////////////////////////////////////
            // Load the rest of the information that's //
            // specific to the CsvParserAdapter.   //
            /////////////////////////////////////////////

            xmlElement = xmlConfig.DocumentElement[XML_INCLUDE_FILE_LINE_NUMBER];

            if ((xmlElement != null) && (xmlElement.InnerText != null))
                IncludeFileLineNumber = Convert.ToBoolean(xmlElement.InnerText);

            /////////////////////////////////////////////////////////////

            xmlElement = xmlConfig.DocumentElement[XML_SKIP_ENDING_DATA_ROWS];

            if ((xmlElement != null) && (xmlElement.InnerText != null))
                SkipEndingDataRows = Convert.ToInt32(xmlElement.InnerText);
        }

        /// <summary>
        ///   Saves the configuration of the <see cref="CsvParserAdapter"/>
        ///   to an <see cref="XmlDocument"/>.
        /// </summary>
        /// <returns>
        ///   The <see cref="XmlDocument"/> that will store the configuration
        ///   information of the current setup of the <see cref="CsvParserAdapter"/>.
        /// </returns>
        public override XmlDocument Save()
        {
            XmlDocument xmlConfig = base.Save();
            XmlElement xmlElement;

            ///////////////////////////////////////////////////////////////
            // Take the document and insert the additional configuration //
            // specific to the CsvParserAdapter.                     //
            ///////////////////////////////////////////////////////////////

            xmlElement = xmlConfig.CreateElement(XML_INCLUDE_FILE_LINE_NUMBER);
            xmlElement.InnerText = IncludeFileLineNumber.ToString();
            xmlConfig.DocumentElement.AppendChild(xmlElement);

            /////////////////////////////////////////////////////////////

            xmlElement = xmlConfig.CreateElement(XML_SKIP_ENDING_DATA_ROWS);
            xmlElement.InnerText = m_intSkipEndingDataRows.ToString();
            xmlConfig.DocumentElement.AppendChild(xmlElement);

            return xmlConfig;
        }

        #endregion Public Code

        #region Private Code

        private bool m_blnIncludeFileLineNumber;
        private int m_intSkipEndingDataRows;

        #endregion Private Code
    }
}