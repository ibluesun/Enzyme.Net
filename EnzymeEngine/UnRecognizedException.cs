using System;
using System.Runtime.Serialization;

namespace Enzyme
{
    [Serializable()]
    public class UnRecognizedException : Exception
    {
        public UnRecognizedException()
        {
         // Add any type-specific logic, and supply the default message.
        }

        public UnRecognizedException(string message): base(message) 
        {
         // Add any type-specific logic.
        }
        public UnRecognizedException(string message, Exception innerException): 
         base (message, innerException)
        {
         // Add any type-specific logic for inner exceptions.
        }
        protected UnRecognizedException(SerializationInfo info, 
         StreamingContext context) : base(info, context)
        {
         // Implement type-specific serialization constructor logic.
        }

        public string ExtraData { get; set; }

    }
}
