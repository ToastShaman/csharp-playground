using FluentAssertions;
using FluentValidation;

namespace ValueOf.Tests;

public class ValueOfTest
{
    class Firstname(string value) : NonBlankString<Firstname>(value) { }

    class Email(string value) : NonBlankString<Email>(value, new Validator())
    {
        private class Validator : AbstractValidator<string>
        {
            public Validator()
            {
                RuleFor(x => x)
                    .SetValidator(new NonBlankStringValidator())
                    .EmailAddress()
                    .WithMessage("Email must be a valid email address");
            }
        }

        public static explicit operator Email(string value) => new(value);
    }

    [Fact(DisplayName = "Equals and GetHashCode")]
    public void EqualsAndHashCode()
    {
        Firstname name1 = new("John");
        Firstname name2 = new("John");
        Firstname name3 = new("Jane");

        name1.Equals(name2).Should().BeTrue();
        name1.Equals(name3).Should().BeFalse();
        name1.GetHashCode().Should().Be(name2.GetHashCode());
    }

    [Theory(DisplayName = "Firstname should not be empty or blank")]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidatesFirstname(string input)
    {
        Action act = () => new Firstname(input);
        act.Should().Throw<ValidationException>();
    }

    [Theory(DisplayName = "Email should be validated")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("@")]
    [InlineData("foo@bar.com@")]
    public void ValidatesEmail(string input)
    {
        Action act = () => new Email(input);
        act.Should().Throw<ValidationException>();
    }

    [Fact(DisplayName = "Implicit conversion")]
    public void ImplicitConversion()
    {
        Firstname name = (Firstname)"John";

        string raw = (string)name;

        raw.Should().Be("John");
    }
}
