using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Linq;
using System.Data;
using SMNETCORE.Common.Enums;
using SMNETCORE.DataType.Extensions;
using System.IO;
using System.Net.Mail;
using System.Collections.Concurrent;

namespace SMNETCORE.DataType.Exceptions
{
    [Serializable]
    public class CustomException : Exception
    {
        public CustomException(string message)
            : base(message)
        { }

        public CustomException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected CustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public CustomException() { }
    }

    [Serializable()]
    public class FeedbackAsapMessage : Exception
    {
        private FeedbackAsapMessageType _messageType = FeedbackAsapMessageType.Other;
        private Exception _innerException = new Exception();
        private String _stackTrace;
        private int _hResult;
        private MethodBase _targetSite;
        public string Message2 { get; set; }

        public FeedbackAsapMessage()
            : base()
        {
        }

        public FeedbackAsapMessage(string message)
            : base(message)
        {
        }

        public FeedbackAsapMessage(string message, FeedbackAsapMessageType type)
            : base(message)
        {
            this._messageType = type;
        }

        public FeedbackAsapMessage(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public FeedbackAsapMessage(Exception exception, FeedbackAsapMessageType type = FeedbackAsapMessageType.Error)
            : base(exception.Message, exception.InnerException)
        {
            this._hResult = exception.HResult;
            this._stackTrace = exception.StackTrace;
            this.Source = exception.Source;
            this.HelpLink = exception.HelpLink;
            this._targetSite = exception.TargetSite;
            this._messageType = type;
        }

        public string Description { get; set; }

        public override string StackTrace
        {
            get
            {
                return this._stackTrace;
            }
        }


        public new int HResult
        {
            get
            {
                return this._hResult;
            }
        }

        public new MethodBase TargetSite
        {
            get
            {
                return this._targetSite;
            }
        }

        public FeedbackAsapMessageType Type
        {
            get { return this._messageType; }
            set { this._messageType = value; }
        }

        public override string ToString()
        {
            return String.Format("({0}) - {1}", this._messageType.ToString(), this.Message);
        }

        public string ToCompleteString()
        {
            return this.Extract(String.Format("({0}) - {1}", this._messageType.ToString(), this.Message));
        }
    }

    [Serializable()]
    public class FeedBackAsapErrors : IList<FeedbackAsapMessage>
    {
        private ConcurrentBag<FeedbackAsapMessage> Items
        {
            get;
            set;
        }

        public FeedbackAsapMessage this[int index]
        {
            get
            {
                return Items.ToArray()[index];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException("The specified index is out of range.");
                var tmp = Items.EnumToList();
                var oldItem = tmp[index];
                tmp[index] = value;
                Items = new ConcurrentBag<FeedbackAsapMessage>();
                Items.AddRange(tmp);
            }
        }

        public bool isError
        {
            get
            {
                if (Items == null || !Items.Any()) return false;
                return Items.Any(data => data.Type.Equals(FeedbackAsapMessageType.Error));
            }
        }

        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<FeedbackAsapMessage>)Items).IsReadOnly;
            }
        }

        public int IndexOf(FeedbackAsapMessage item)
        {
            return Items.EnumToList().IndexOf(item);
        }

        public void Add(FeedbackAsapMessage message)
        {
            this.Items.Add(message);
        }

        public FeedbackAsapMessage Add(Exception message, FeedbackAsapMessageType type = FeedbackAsapMessageType.Error)
        {
            FeedbackAsapMessage _message = new FeedbackAsapMessage(message, type);
            this.Items.Add(_message);
            return _message;
        }

        public FeedbackAsapMessage Add(String message, FeedbackAsapMessageType type = FeedbackAsapMessageType.Error)
        {
            FeedbackAsapMessage _message = new FeedbackAsapMessage(message, type);
            this.Items.Add(_message);
            return _message;
        }

        public void Add(FeedBackAsapErrors message)
        {
            this.Items.AddRange(message);
        }

        public void Insert(int index, FeedbackAsapMessage item)
        {
            ((IList)Items).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException("The specified index is out of range.");
            var tmp = Items.EnumToList();
            var oldItem = tmp[index];
            tmp.RemoveAt(index);
            Items = new ConcurrentBag<FeedbackAsapMessage>();
            Items.AddRange(tmp);
        }

        public void Clear()
        {
            this.Items = new ConcurrentBag<FeedbackAsapMessage>();
        }

        public bool Contains(FeedbackAsapMessage item)
        {
            return this.Items.Contains(item);
        }

        public void CopyTo(FeedbackAsapMessage[] array, int arrayIndex)
        {
            ((IList)Items).CopyTo(array, arrayIndex);
        }

        public bool Remove(FeedbackAsapMessage item)
        {
            var tmp = Items.EnumToList();
            var result = tmp.Remove(item);
            Items = new ConcurrentBag<FeedbackAsapMessage>();
            Items.AddRange(tmp);
            return result;
        }

        public IEnumerator<FeedbackAsapMessage> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection<FeedbackAsapMessage>.Add(FeedbackAsapMessage item)
        {
            this.Items.Add(item);
        }

        public FeedBackAsapErrors()
        {
            Items = new ConcurrentBag<FeedbackAsapMessage>();
        }
        public FeedBackAsapErrors(FeedbackAsapMessage message)
        {
            this.Items.Add(message);
        }

        public FeedBackAsapErrors(Exception message, FeedbackAsapMessageType type = FeedbackAsapMessageType.Error)
        {
            FeedbackAsapMessage _message = new FeedbackAsapMessage(message, type);
            this.Items.Add(_message);
        }

        public FeedBackAsapErrors(FeedBackAsapErrors message)
        {
            this.Items.AddRange(message);
        }

        public Stream GetOperationReportStream()
        {
           

            if (this.Items != null && this.Items.Any())
            {
                var historyStream = new MemoryStream();
                var historyWriter = new StreamWriter(historyStream);
                foreach (var itm in this.Items)
                {
                    var historyText = itm.ToCompleteString();
                    historyWriter.WriteLine(historyText);
                    historyWriter.Flush();
                }
                if (historyStream.Length > 0)
                    historyStream.Position = 0;
                return historyStream;
            }

            return new MemoryStream();
        }

        public IEnumerable<Attachment> GetAttachMentsTextFile()
        {
            if (!Items.IsValid()) return new List<Attachment>();
            var results = new List<Attachment>();
            var catSteps = Items.SplitList(1000);

            foreach (var items in catSteps)
            {
                var historyStream = new MemoryStream();
                var historyWriter = new StreamWriter(historyStream);
                int idx = 1;

                items.ForEach(itm =>
                {
                    var historyText = itm.ToCompleteString();
                    historyWriter.WriteLine(historyText);
                    historyWriter.Flush();
                });

                if (historyStream.Length > 0)
                    historyStream.Position = 0;
                results.Add(new Attachment(historyStream, string.Format("{0}-{1}.txt", "ERROR", idx)));
                idx++;
            }
        
            return results;
        }
    }
}