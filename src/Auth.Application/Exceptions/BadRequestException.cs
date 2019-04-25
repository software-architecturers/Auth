using System;
using System.Collections.Generic;
using System.Linq;

namespace Auth.Application.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(IDictionary<string, string[]> errors) :
            base($"bad request: {string.Join(", ", errors.Values.SelectMany(s => s))}")
        {
            
        }
        
        
        public IDictionary<string, string[]> Errors { get; set; }
    }
}