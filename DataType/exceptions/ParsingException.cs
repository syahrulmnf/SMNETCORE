#region Using Directives

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

#endregion Using Directives

namespace SMNETCORE.DataType.Exceptions
{
    /// <summary>
    ///   <see cref="ParsingException"/> is an exception class meant for states where
    ///   the parser can no longer continue parsing due to the data found in the
    ///   data-source.
    /// </summary>
    [Serializable]
    public class ParsingException : CustomException
    {
        #region Constants

        private const string SERIALIZATION_COLUMN_NUMBER = "ColumnNumber";
        private const string SERIALIZATION_FILE_ROW_NUMBER = "FileRowNumber";

        #endregion Constants

        #region Constructors

        /// <summary>
        ///   Creates a new <see cref="ParsingException"/> containing a message and the
        ///   file line number that the error occured.
        /// </summary>
        /// <param name="strMessage">
        ///   The message indicating the root cause of the error.
        /// </param>
        /// <param name="intFileRowNumber">The file line number the error occured on.</param>
        /// <param name="intColumnNumber">The column number the error occured on.</param>
        public ParsingException(string strMessage, int intFileRowNumber, int intColumnNumber)
            : base(strMessage)
        {
            m_intFileRowNumber = intFileRowNumber;
            m_intColumnNumber = intColumnNumber;
        }

        /// <summary>
        ///   Creates a new <see cref="ParsingException"/> with seralized data.
        /// </summary>
        /// <param name="sInfo">
        ///   The <see cref="SerializationInfo"/> that contains information
        ///   about the exception.
        /// </param>
        /// <param name="sContext">
        ///   The <see cref="StreamingContext"/> that contains information
        ///   about the source/destination of the exception.
        /// </param>
        protected ParsingException(SerializationInfo sInfo, StreamingContext sContext)
            : base(sInfo, sContext)
        {
            m_intFileRowNumber = sInfo.GetInt32(SERIALIZATION_FILE_ROW_NUMBER);
            m_intColumnNumber = sInfo.GetInt32(SERIALIZATION_COLUMN_NUMBER);
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        ///   The line number in the file that the exception was thrown at.
        /// </summary>
        public int FileRowNumber
        {
            get { return m_intFileRowNumber; }
        }

        /// <summary>
        ///   The column number in the file that the exception was thrown at.
        /// </summary>
        public int ColumnNumber
        {
            get { return m_intColumnNumber; }
        }

        #endregion Public Properties

        private readonly int m_intColumnNumber;
        private readonly int m_intFileRowNumber;

        #region Overridden Methods

        /// <summary>
        ///   When overridden in a derived class, sets the <see cref="SerializationInfo"/> 
        ///   with information about the exception.
        /// </summary>
        /// <param name="info">
        ///   The <see cref="SerializationInfo"/> that holds the serialized object data
        ///   about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///   The <see cref="StreamingContext"/> that contains contextual information about the source
        ///   or destination.
        /// </param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(SERIALIZATION_FILE_ROW_NUMBER, m_intFileRowNumber);
            info.AddValue(SERIALIZATION_COLUMN_NUMBER, m_intColumnNumber);
        }

        #endregion Overridden Methods
    }
}