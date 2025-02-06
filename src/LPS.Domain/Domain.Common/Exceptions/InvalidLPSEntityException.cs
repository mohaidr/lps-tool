using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Domain.Common.Exceptions
{
    public class InvalidLPSEntityException : Exception
    {
        // Default constructor
        public InvalidLPSEntityException()
            : base("The LPS entity is invalid.")
        {
        }

        // Constructor that accepts a custom message
        public InvalidLPSEntityException(string message)
            : base(message)
        {
        }

        // Constructor that accepts a custom message and an inner exception
        public InvalidLPSEntityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
