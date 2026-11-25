using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaCapitole.Domain.Common
{
    public sealed class DomainException(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }
}
