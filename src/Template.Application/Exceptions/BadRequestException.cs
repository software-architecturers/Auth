using System;
using System.Collections.Generic;

namespace Template.Application.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(IDictionary<string, string> errors) : base()
        {
        }
    }
}