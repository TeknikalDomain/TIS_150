using System;
using System.Runtime.Serialization;

namespace TIS_150
{
    [Serializable]
    internal class IDBInvalidException : Exception
    {
        public IDBInvalidException()
        {
        }

        public IDBInvalidException(string message) : base(message)
        {
        }

        public IDBInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IDBInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}