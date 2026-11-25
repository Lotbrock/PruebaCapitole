using PruebaCapitole.Domain.Common;
using PruebaCapitole.Domain.Ejemplo.Bookings;
using PruebaCapitole.Domain.Ejemplo.Bookings.ValueObjects;
using PruebaCapitole.Domain.Ejemplo.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PruebaCapitole.Domain.Bookings.ValueObjects
{
    public sealed partial record Customer
    {
        public const int NameMaxLength = 100;
        public string Name { get; }
        public string Email { get; }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();

        private Customer(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public static Customer Create(string? name, string? email)
        {
            name = name?.Trim();
            email = email?.Trim();

            if (string.IsNullOrEmpty(name))
                throw new DomainException(DomainErrors.CustomerNameRequired, "El nombre del cliente es obligatorio.");

            if (name.Length > NameMaxLength)
                throw new DomainException(DomainErrors.CustomerNameTooLong,
                    $"El nombre del cliente no puede superar {NameMaxLength} caracteres.");

            if (string.IsNullOrEmpty(email) || !EmailRegex().IsMatch(email))
                throw new DomainException(DomainErrors.CustomerEmailInvalid, "El email del cliente no es válido.");

            return new Customer(name, email.ToLowerInvariant());
        }
    }
}
