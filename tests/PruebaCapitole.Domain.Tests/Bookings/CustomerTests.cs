using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;

namespace PruebaCapitole.Domain.Tests.Bookings;

public class CustomerTests
{
    [Fact]
    public void Create_WithPaddedValues_TrimsNameAndLowercasesEmail()
    {
        // Arrange
        const string name = "  Ana Pérez ";
        const string email = "  ANA@Example.com ";

        // Act
        var customer = Customer.Create(name, email);

        // Assert
        Assert.Equal("Ana Pérez", customer.Name);
        Assert.Equal("ana@example.com", customer.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutName_ThrowsCustomerNameRequired(string? name)
    {
        // Arrange & Act
        var exception = Assert.Throws<DomainException>(() => Customer.Create(name, "ana@example.com"));

        // Assert
        Assert.Equal(DomainErrors.CustomerNameRequired, exception.Code);
    }

    [Fact]
    public void Create_WithNameOfMaximumLength_Succeeds()
    {
        // Arrange
        var name = new string('a', Customer.NameMaxLength);

        // Act
        var customer = Customer.Create(name, "ana@example.com");

        // Assert
        Assert.Equal(Customer.NameMaxLength, customer.Name.Length);
    }

    [Fact]
    public void Create_WithNameLongerThanMaximum_ThrowsCustomerNameTooLong()
    {
        // Arrange
        var name = new string('a', Customer.NameMaxLength + 1);

        // Act
        var exception = Assert.Throws<DomainException>(() => Customer.Create(name, "ana@example.com"));

        // Assert
        Assert.Equal(DomainErrors.CustomerNameTooLong, exception.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("without-at")]
    [InlineData("a@b")]
    [InlineData("a b@c.com")]
    public void Create_WithInvalidEmail_ThrowsCustomerEmailInvalid(string? email)
    {
        // Arrange & Act
        var exception = Assert.Throws<DomainException>(() => Customer.Create("Ana", email));

        // Assert
        Assert.Equal(DomainErrors.CustomerEmailInvalid, exception.Code);
    }
}
